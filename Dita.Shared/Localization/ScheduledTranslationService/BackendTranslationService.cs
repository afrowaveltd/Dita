using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Hubs;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Orchestrates scheduled validation, JSON synchronization, country synchronization, and Markdown translation.
/// </summary>
public class BackendTranslationService(
   ILanguageService languageService,
   ITranslationQueue translationQueue,
   IHubContext<LocalizationHub, ILocalizationHubClient> hub,
   IConfiguration configuration,
   IHostEnvironment hostEnvironment,
   IMarkdownTranslationService markdownTranslationService,
   ILibreTranslateService translateService,
   ILogger<BackendTranslationService> logger) : IBackendTranslationService
{
   private readonly AutomaticTranslationSettings _settings = configuration.GetSection("AutomaticTranslationSettings").Get<AutomaticTranslationSettings>() ?? new AutomaticTranslationSettings();
   private readonly ILogger<BackendTranslationService> _logger = logger;
   private readonly ILanguageService _languageService = languageService;
   private readonly ITranslationQueue _translationQueue = translationQueue;
   private readonly IHubContext<LocalizationHub, ILocalizationHubClient> _hubContext = hub;
   private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
   private readonly IMarkdownTranslationService _markdownTranslationService = markdownTranslationService;
   private readonly ILibreTranslateService _translateService = translateService;
   private string DefaultLanguage => _settings.DefaultLanguage ?? "en";
   private List<string> IgnoredLanguages => _settings.IgnoredLanguages ?? [];

   private string CountriesFilePath => Path.Combine(_hostEnvironment.ContentRootPath, "Jsons", "countries.json");
   private static string TempHashDirectory => Path.Combine(Path.GetTempPath(), "dita", "localization-hashes");
   private Guid _runId;
   private long _messageSequence;

   /// <summary>
   /// Executes a full automatic translation pipeline run.
   /// </summary>
   public async Task RunAsync()
   {
      _runId = Guid.NewGuid();
      _messageSequence = 0;

      StoringReport storingReport = new()
      {
         RunStartedUtc = DateTime.UtcNow
      };

      _translationQueue.Clear();
      await PublishMessageAsync(LocalizationMessageType.StageStarted, ProcessStage.CheckServers, "Automatic translation pipeline started.");

      try
      {
         CheckContext checkContext = await RunCheckServersStageAsync();
         await RunCountriesStageAsync(checkContext.TargetLanguages, storingReport);
         await RunJsonStageAsync(checkContext.TargetLanguages, storingReport);
         await RunMarkdownStageAsync(checkContext.TargetLanguages, storingReport);

         storingReport.RunCompletedUtc = DateTime.UtcNow;
         await PublishStageAsync(ProcessStage.StoringResults, storingReport, LocalizationMessageType.StageCompleted, "Localization artifacts stored.");
         await PublishMessageAsync(LocalizationMessageType.PipelineCompleted, ProcessStage.StoringResults, "Automatic translation pipeline completed successfully.", storingReport);
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Automatic translation pipeline failed.");
         storingReport.Errors.Add(CreateError("pipeline", ErrorCode.InternalError, ex.Message));
         storingReport.RunCompletedUtc = DateTime.UtcNow;
         await PublishMessageAsync(LocalizationMessageType.PipelineFailed, ProcessStage.StoringResults, ex.Message, storingReport, true);
      }
      finally
      {
         _translationQueue.Clear();
      }
   }

   private async Task<CheckContext> RunCheckServersStageAsync()
   {
      CheckingReport report = new()
      {
         AppsettingsLoaded = _settings.AppsettingsLoaded,
         DefaultLanguage = DefaultLanguage
      };

      try
      {
         Response<int> latencyResponse = _translateService.ServerLatency();
         report.ServerLatencyMs = latencyResponse.Data;
         report.TranslationServerReady = latencyResponse.Success;

         var languagesResponse = await _translateService.GetAvailableLanguagesAsync();
         if(!languagesResponse.Success || languagesResponse.Data is null || languagesResponse.Data.Length == 0)
         {
            throw new InvalidOperationException(languagesResponse.Message);
         }

         report.AvailableLanguages = languagesResponse.Data;

         if(!_settings.AppsettingsLoaded)
         {
            throw new InvalidOperationException("AutomaticTranslationSettings were not loaded.");
         }

         if(!languagesResponse.Data.Contains(DefaultLanguage, StringComparer.OrdinalIgnoreCase))
         {
            throw new InvalidOperationException($"Default language '{DefaultLanguage}' is not supported by the translation server.");
         }

         await _languageService.CreateMissingLanguageFilesAsync([.. languagesResponse.Data.Distinct(StringComparer.OrdinalIgnoreCase)]);

         List<string> targetLanguages = [.. languagesResponse.Data
            .Where(language => !language.Equals(DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            .Where(language => !IgnoredLanguages.Contains(language, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)];

         await PublishStageAsync(ProcessStage.CheckServers, report, LocalizationMessageType.StageCompleted, "Translation server and configuration validated.");
         return new CheckContext(report, targetLanguages);
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "CheckServers stage failed.");
         await PublishMessageAsync(LocalizationMessageType.StageFailed, ProcessStage.CheckServers, ex.Message, report, true);
         throw;
      }
   }

   private async Task RunCountriesStageAsync(List<string> targetLanguages, StoringReport storingReport)
   {
      TranslationsReport report = new();
      await PublishStageAsync(ProcessStage.TranslateCountries, report, LocalizationMessageType.StageStarted, "Synchronising country names into localization dictionaries.");

      try
      {
         Dictionary<string, string> defaultDictionary = await LoadDictionaryOrEmptyAsync(DefaultLanguage, report);
         List<CountryDefinition> countries = await LoadCountriesAsync();

         report.DefaultDictionaryExists = defaultDictionary.Count > 0;

         HashSet<string> countryKeys = [];
         bool defaultChanged = false;

         foreach(CountryDefinition country in countries.OrderBy(country => country.Name, StringComparer.Ordinal))
         {
            string? defaultPhrase = await ResolveCountryDefaultPhraseAsync(country, report);
            if(string.IsNullOrWhiteSpace(defaultPhrase))
            {
               continue;
            }

            countryKeys.Add(defaultPhrase);

            if(!defaultDictionary.ContainsKey(defaultPhrase))
            {
               defaultDictionary[defaultPhrase] = defaultPhrase;
               report.AddedCount++;
               defaultChanged = true;
            }
         }

         if(defaultChanged)
         {
            await SaveDictionaryAsync(DefaultLanguage, defaultDictionary, storingReport, report, "countries/default");
         }

         Dictionary<string, Dictionary<string, string>> targetDictionaries = await LoadTargetDictionariesAsync(targetLanguages);

         _translationQueue.Clear();
         foreach(string targetLanguage in targetLanguages)
         {
            Dictionary<string, string> dictionary = targetDictionaries[targetLanguage];
            foreach(string countryKey in countryKeys)
            {
               if(!dictionary.ContainsKey(countryKey))
               {
                  _translationQueue.Enqueue(new PhraseInQueue
                  {
                     Target = TranslationTarget.Languages,
                     Key = countryKey,
                     Phrase = countryKey,
                     SourceLanguage = DefaultLanguage,
                     TargetLanguage = targetLanguage,
                     ChangeRequired = PhraseChange.Added
                  });
               }
            }
         }

         report.ToTranslateCount = _translationQueue.GetPendingAdditions().Count;
         await ProcessQueueAsync(report, targetDictionaries, storingReport);
         report.DefaultDictionaryCount = defaultDictionary.Count;

         foreach((string language, Dictionary<string, string> dictionary) in targetDictionaries)
         {
            await SaveDictionaryAsync(language, dictionary, storingReport, report, $"countries/{language}");
         }

         await PublishStageAsync(ProcessStage.TranslateCountries, report, LocalizationMessageType.StageCompleted, "Country names synchronised.");
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "TranslateCountries stage failed.");
         report.Errors ??= [];
         report.Errors.Add(CreateError("countries", ErrorCode.TranslationFailed, ex.Message));
         await PublishMessageAsync(LocalizationMessageType.StageFailed, ProcessStage.TranslateCountries, ex.Message, report, true);
         throw;
      }
   }

   private async Task RunJsonStageAsync(List<string> targetLanguages, StoringReport storingReport)
   {
      TranslationsReport report = new();
      await PublishStageAsync(ProcessStage.TranslateJsonFiles, report, LocalizationMessageType.StageStarted, "Synchronising JSON localization dictionaries.");

      try
      {
         Dictionary<string, string> currentDefault = await LoadDictionaryOrEmptyAsync(DefaultLanguage, report);
         report.DefaultDictionaryExists = currentDefault.Count > 0;
         report.DefaultDictionaryCount = currentDefault.Count;

         var oldResponse = await _languageService.GetLastStored();
         Dictionary<string, string> previousDefault = oldResponse.Success && oldResponse.Data != null ? oldResponse.Data : [];

         string[] addedKeys = [.. currentDefault.Keys.Except(previousDefault.Keys, StringComparer.Ordinal).OrderBy(key => key, StringComparer.Ordinal)];
         string[] removedKeys = [.. previousDefault.Keys.Except(currentDefault.Keys, StringComparer.Ordinal).OrderBy(key => key, StringComparer.Ordinal)];

         report.AddedCount = addedKeys.Length;
         report.RemovedCount = removedKeys.Length;

         Dictionary<string, Dictionary<string, string>> targetDictionaries = await LoadTargetDictionariesAsync(targetLanguages);
         _translationQueue.Clear();

         foreach(string targetLanguage in targetLanguages)
         {
            foreach(string removedKey in removedKeys)
            {
               _translationQueue.Enqueue(new PhraseInQueue
               {
                  Target = TranslationTarget.JsonFiles,
                  Key = removedKey,
                  Phrase = removedKey,
                  SourceLanguage = DefaultLanguage,
                  TargetLanguage = targetLanguage,
                  ChangeRequired = PhraseChange.Removed
               });
            }

            foreach(string addedKey in addedKeys)
            {
               _translationQueue.Enqueue(new PhraseInQueue
               {
                  Target = TranslationTarget.JsonFiles,
                  Key = addedKey,
                  Phrase = currentDefault[addedKey],
                  SourceLanguage = DefaultLanguage,
                  TargetLanguage = targetLanguage,
                  ChangeRequired = PhraseChange.Added
               });
            }
         }

         report.ToTranslateCount = _translationQueue.GetAll().Count;
         await ProcessQueueAsync(report, targetDictionaries, storingReport);

         foreach((string language, Dictionary<string, string> dictionary) in targetDictionaries)
         {
            await SaveDictionaryAsync(language, dictionary, storingReport, report, $"json/{language}");
         }

         await _languageService.SaveOldTranslationAsync(currentDefault);
         await PublishStageAsync(ProcessStage.TranslateJsonFiles, report, LocalizationMessageType.StageCompleted, "JSON localization dictionaries synchronised.");
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "TranslateJsonFiles stage failed.");
         report.Errors ??= [];
         report.Errors.Add(CreateError("json", ErrorCode.TranslationFailed, ex.Message));
         await PublishMessageAsync(LocalizationMessageType.StageFailed, ProcessStage.TranslateJsonFiles, ex.Message, report, true);
         throw;
      }
   }

   private async Task RunMarkdownStageAsync(List<string> targetLanguages, StoringReport storingReport)
   {
      MarkdownTranslationsReport report = new();
      await PublishStageAsync(ProcessStage.TranslateMarkdownFiles, report, LocalizationMessageType.StageStarted, "Synchronising Markdown translations.");

      try
      {
         foreach(string markdownRoot in ResolveMarkdownRoots())
         {
            if(!Directory.Exists(markdownRoot))
            {
               continue;
            }

            foreach(string sourceFile in Directory.GetFiles(markdownRoot, $"{DefaultLanguage}.md", SearchOption.AllDirectories))
            {
               report.SourceFilesDetected++;
               string sourceContent = await File.ReadAllTextAsync(sourceFile);
               string sourceHash = ComputeHash(sourceContent);
               StoredHashEntry storedHash = await ReadStoredHashAsync(sourceFile);

               List<string> missingOrChangedTargets = [.. targetLanguages.Where(targetLanguage =>
               {
                  string targetFilePath = Path.Combine(Path.GetDirectoryName(sourceFile) ?? string.Empty, $"{targetLanguage}.md");
                  return !File.Exists(targetFilePath) || !string.Equals(storedHash.Hash, sourceHash, StringComparison.OrdinalIgnoreCase);
               })];

               if(missingOrChangedTargets.Count == 0)
               {
                  report.SkippedFiles++;
                  continue;
               }

               report.SourceFilesChanged++;
               Dictionary<string, string> translatedDocuments = await _markdownTranslationService.TranslateMarkdownAsync(
                  sourceContent,
                  DefaultLanguage,
                  missingOrChangedTargets);

               bool allSucceeded = true;
               foreach(string targetLanguage in missingOrChangedTargets)
               {
                  if(!translatedDocuments.TryGetValue(targetLanguage, out string? translatedContent) || string.IsNullOrWhiteSpace(translatedContent))
                  {
                     report.Errors.Add(CreateError(sourceFile, ErrorCode.TranslationFailed, $"Markdown translation for '{targetLanguage}' returned no content."));
                     allSucceeded = false;
                     continue;
                  }

                  string targetFilePath = Path.Combine(Path.GetDirectoryName(sourceFile) ?? string.Empty, $"{targetLanguage}.md");
                  await File.WriteAllTextAsync(targetFilePath, translatedContent, Encoding.UTF8);
                  storingReport.SavedMarkdownFiles++;
                  report.SavedFiles++;
               }

               if(allSucceeded)
               {
                  bool usedFallback = await WriteStoredHashAsync(sourceFile, sourceHash);
                  storingReport.SavedHashFiles++;
                  if(usedFallback)
                  {
                     storingReport.TempFallbackWrites++;
                     report.TempFallbackWrites++;
                  }
               }
            }
         }

         await PublishStageAsync(ProcessStage.TranslateMarkdownFiles, report, LocalizationMessageType.StageCompleted, "Markdown translations synchronised.");
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "TranslateMarkdownFiles stage failed.");
         report.Errors.Add(CreateError("markdown", ErrorCode.TranslationFailed, ex.Message));
         await PublishMessageAsync(LocalizationMessageType.StageFailed, ProcessStage.TranslateMarkdownFiles, ex.Message, report, true);
         throw;
      }
   }

   private async Task<Dictionary<string, string>> LoadDictionaryOrEmptyAsync(string language, TranslationsReport report)
   {
      var response = await _languageService.GetDictionaryAsync(language);
      if(response.Success && response.Data != null)
      {
         return response.Data;
      }

      report.Errors ??= [];
      report.Errors.Add(CreateError(language, ErrorCode.DictionaryNotFound, response.Message));
      return [];
   }

   private async Task<List<CountryDefinition>> LoadCountriesAsync()
   {
      if(!File.Exists(CountriesFilePath))
      {
         throw new FileNotFoundException("countries.json was not found.", CountriesFilePath);
      }

      string json = await File.ReadAllTextAsync(CountriesFilePath);
      List<CountryDefinition>? countries = JsonSerializer.Deserialize<List<CountryDefinition>>(json);
      return countries ?? [];
   }

   private async Task<string?> ResolveCountryDefaultPhraseAsync(CountryDefinition country, TranslationsReport report)
   {
      if(DefaultLanguage.Equals("en", StringComparison.OrdinalIgnoreCase))
      {
         return country.Name;
      }

      var response = await _translateService.TranslateTextAsync(country.Name, "en", DefaultLanguage);
      if(!response.Success || response.Data is null || string.IsNullOrWhiteSpace(response.Data.TranslatedText))
      {
         report.Errors ??= [];
         report.Errors.Add(CreateError(country.Code, ErrorCode.TranslationFailed, response.Message));
         return null;
      }

      return response.Data.TranslatedText.Trim();
   }

   private async Task<Dictionary<string, Dictionary<string, string>>> LoadTargetDictionariesAsync(List<string> targetLanguages)
   {
      Dictionary<string, Dictionary<string, string>> dictionaries = [];
      foreach(string targetLanguage in targetLanguages)
      {
         var response = await _languageService.GetDictionaryAsync(targetLanguage);
         dictionaries[targetLanguage] = response.Success && response.Data != null ? response.Data : [];
      }

      return dictionaries;
   }

   private async Task ProcessQueueAsync(
      TranslationsReport report,
      Dictionary<string, Dictionary<string, string>> targetDictionaries,
      StoringReport storingReport)
   {
      foreach(PhraseInQueue item in _translationQueue.GetAll())
      {
         Dictionary<string, string> targetDictionary = targetDictionaries[item.TargetLanguage];

         if(item.ChangeRequired == PhraseChange.Removed)
         {
            if(item.Key != null && targetDictionary.Remove(item.Key))
            {
               report.RemovedCount++;
            }

            continue;
         }

         if(string.IsNullOrWhiteSpace(item.Key))
         {
            report.Errors ??= [];
            report.Errors.Add(CreateError(item.TargetLanguage, ErrorCode.ArgumentInvalid, "Queue item key is missing."));
            continue;
         }

         if(targetDictionary.ContainsKey(item.Key))
         {
            report.SkippedCount++;
            continue;
         }

         _translationQueue.MarkTranslationStarted(item.Key);
         var response = await _translateService.TranslateTextAsync(item.Phrase, item.SourceLanguage ?? DefaultLanguage, item.TargetLanguage);

         if(!response.Success || response.Data is null || string.IsNullOrWhiteSpace(response.Data.TranslatedText))
         {
            report.Errors ??= [];
            report.Errors.Add(CreateError($"{item.TargetLanguage}:{item.Key}", ErrorCode.TranslationFailed, response.Message));
            _translationQueue.MarkTranslationFailed(item.Key);
            continue;
         }

         string translatedText = response.Data.TranslatedText.Trim();
         targetDictionary[item.Key] = translatedText;
         report.TranslatedCount++;
         _translationQueue.MarkTranslationSucceeded(item.Key, translatedText);
      }
   }

   private async Task SaveDictionaryAsync(
      string language,
      Dictionary<string, string> dictionary,
      StoringReport storingReport,
      TranslationsReport report,
      string source)
   {
      var result = await _languageService.SaveDictionaryAsync(new SingleTranslation
      {
         Language = language,
         Translations = dictionary
      });

      if(result.Success)
      {
         storingReport.SavedDictionaryFiles++;
         return;
      }

      report.Errors ??= [];
      TranslationError error = CreateError(source, ErrorCode.StorageWriteFailed, result.Message);
      report.Errors.Add(error);
      storingReport.Errors.Add(error);
   }

   private IEnumerable<string> ResolveMarkdownRoots()
   {
      List<string> configuredRoots = _settings.MarkdownRoots is { Count: > 0 }
         ? _settings.MarkdownRoots
         : ["/Docs"];

      foreach(string configuredRoot in configuredRoots)
      {
         if(string.IsNullOrWhiteSpace(configuredRoot))
         {
            continue;
         }

         string trimmed = configuredRoot.Trim();
         string relative = trimmed.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, '/').Replace('/', Path.DirectorySeparatorChar);
         yield return Path.Combine(_hostEnvironment.ContentRootPath, relative);
      }
   }

   private static string ComputeHash(string content)
   {
      byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
      return Convert.ToHexString(bytes);
   }

   private async Task<StoredHashEntry> ReadStoredHashAsync(string sourceFilePath)
   {
      string primaryPath = GetPrimaryHashPath(sourceFilePath);
      string fallbackPath = GetFallbackHashPath(sourceFilePath);

      foreach(string candidate in new[] { primaryPath, fallbackPath })
      {
         if(!File.Exists(candidate))
         {
            continue;
         }

         try
         {
            string json = await File.ReadAllTextAsync(candidate);
            StoredHashEntry? entry = JsonSerializer.Deserialize<StoredHashEntry>(json);
            if(entry != null)
            {
               return entry;
            }
         }
         catch(Exception ex)
         {
            _logger.LogWarning(ex, "Failed to read stored hash from {HashPath}", candidate);
         }
      }

      return new StoredHashEntry();
   }

   private async Task<bool> WriteStoredHashAsync(string sourceFilePath, string hash)
   {
      StoredHashEntry entry = new()
      {
         SourcePath = sourceFilePath,
         Hash = hash,
         UpdatedAtUtc = DateTime.UtcNow
      };

      string json = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true });
      string primaryPath = GetPrimaryHashPath(sourceFilePath);

      try
      {
         Directory.CreateDirectory(Path.GetDirectoryName(primaryPath) ?? _hostEnvironment.ContentRootPath);
         await File.WriteAllTextAsync(primaryPath, json, Encoding.UTF8);
         return false;
      }
      catch(Exception ex)
      {
         _logger.LogWarning(ex, "Primary hash write failed for {SourceFilePath}; using temp fallback.", sourceFilePath);
      }

      Directory.CreateDirectory(TempHashDirectory);
      await File.WriteAllTextAsync(GetFallbackHashPath(sourceFilePath), json, Encoding.UTF8);
      return true;
   }

   private static string GetPrimaryHashPath(string sourceFilePath)
      => Path.Combine(Path.GetDirectoryName(sourceFilePath) ?? string.Empty, $"{Path.GetFileName(sourceFilePath)}.hash.json");

   private string GetFallbackHashPath(string sourceFilePath)
   {
      string relativePath = Path.GetRelativePath(_hostEnvironment.ContentRootPath, sourceFilePath);
      StringBuilder builder = new();

      foreach(char character in relativePath)
      {
         if(character is '/' or '\\')
         {
            builder.Append('.');
         }
         else if(char.IsWhiteSpace(character))
         {
            builder.Append('_');
         }
         else if(Path.GetInvalidFileNameChars().Contains(character))
         {
            builder.Append('.');
         }
         else
         {
            builder.Append(character);
         }
      }

      return Path.Combine(TempHashDirectory, $"{builder}.hash.json");
   }

   private async Task PublishStageAsync<T>(ProcessStage stage, T data, LocalizationMessageType type, string message, bool isError = false) where T : class
   {
      await _hubContext.Clients.All.ReceiveLocalizationMessage(new LocalizationHubMessage
      {
         RunId = _runId,
         Sequence = Interlocked.Increment(ref _messageSequence),
         Type = type,
         Stage = stage,
         TimestampUtc = DateTime.UtcNow,
         IsError = isError,
         Message = message,
         Data = new StageReport<T>
         {
            ReportedStage = stage,
            StageData = data,
            StageStartTime = DateTime.UtcNow,
            StageEndTime = type is LocalizationMessageType.StageCompleted or LocalizationMessageType.StageFailed ? DateTime.UtcNow : null
         }
      });
   }

   private async Task PublishMessageAsync(LocalizationMessageType type, ProcessStage stage, string message, object? data = null, bool isError = false)
   {
      await _hubContext.Clients.All.ReceiveLocalizationMessage(new LocalizationHubMessage
      {
         RunId = _runId,
         Sequence = Interlocked.Increment(ref _messageSequence),
         Type = type,
         Stage = stage,
         TimestampUtc = DateTime.UtcNow,
         IsError = isError,
         Message = message,
         Data = data
      });
   }

   private static TranslationError CreateError(string source, ErrorCode code, string? details = null)
      => new()
      {
         Source = source,
         Code = code,
         ErrorMessage = string.IsNullOrWhiteSpace(details)
            ? ErrorCodeText.ErrorText(code)
            : $"{ErrorCodeText.ErrorText(code)}: {details}"
      };

   private sealed record CheckContext(CheckingReport Report, List<string> TargetLanguages);

   private sealed class StoredHashEntry
   {
      public string SourcePath { get; set; } = string.Empty;
      public string Hash { get; set; } = string.Empty;
      public DateTime UpdatedAtUtc { get; set; }
   }
}