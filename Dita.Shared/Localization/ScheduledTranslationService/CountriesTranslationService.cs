using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Synchronizes country names from a canonical source file into per-language localization dictionaries.
/// For each target language missing a country entry, the name is translated and the dictionary is saved immediately.
/// </summary>
public class CountriesTranslationService(
    ILanguageService languageService,
    ILibreTranslateService translateService,
    ISignalRPublisher signalRPublisher,
    TranslationRetryService retryService,
    IHostEnvironment hostEnvironment,
    IStringLocalizer<CountriesTranslationService> localizer,
    ILogger<CountriesTranslationService> logger) : ICountriesTranslationService
{
    private readonly ILanguageService _languageService = languageService;
    private readonly ILibreTranslateService _translateService = translateService;
    private readonly ISignalRPublisher _signalRPublisher = signalRPublisher;
    private readonly TranslationRetryService _retryService = retryService;
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly IStringLocalizer<CountriesTranslationService> _localizer = localizer;
    private readonly ILogger<CountriesTranslationService> _logger = logger;

    private string CountriesFilePath => Path.Combine(_hostEnvironment.ContentRootPath, "Jsons", "countries.json");
    private string DefaultLanguage => "en";

    private string T(string text) => _localizer[text].Value;

    private string T(string text, object values) => _localizer[text, values].Value;

    /// <summary>
    /// Synchronizes country names into localization dictionaries for all target languages.
    /// Translates each missing country name per-language and saves the dictionary immediately.
    /// </summary>
    public async Task RunAsync(List<string> targetLanguages, StoringReport storingReport, Guid runId)
    {
        var report = new TranslationsReport();

        await _signalRPublisher.PublishStageAsync(
            runId,
            ProcessStage.TranslateCountries,
            report,
            LocalizationMessageType.StageStarted,
            T("Synchronising country names into localization dictionaries."));

        try
        {
            // Load default dictionary and countries
            var defaultDictionary = await LoadDictionaryOrEmptyAsync(DefaultLanguage, report);
            var countries = await LoadCountriesAsync();

            report.DefaultDictionaryExists = defaultDictionary.Count > 0;

            // Build set of country keys to translate
            HashSet<string> countryKeys = [];
            bool defaultChanged = false;

            foreach (var country in countries.OrderBy(c => c.Name, StringComparer.Ordinal))
            {
                string? defaultPhrase = await ResolveCountryDefaultPhraseAsync(country, report, runId);
                if (string.IsNullOrWhiteSpace(defaultPhrase))
                {
                    continue;
                }

                countryKeys.Add(defaultPhrase);

                if (!defaultDictionary.ContainsKey(defaultPhrase))
                {
                    defaultDictionary[defaultPhrase] = defaultPhrase;
                    report.AddedCount++;
                    defaultChanged = true;
                }
            }

            // Save default dictionary if new countries were added
            if (defaultChanged)
            {
                await SaveDictionaryAsync(DefaultLanguage, defaultDictionary, storingReport, report, runId, "countries/default");
            }

            report.DefaultDictionaryCount = defaultDictionary.Count;

            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.Progress,
                ProcessStage.TranslateCountries,
                T("Found {countryCount} country names. Default dictionary changed: {defaultChanged}.", new { countryCount = countryKeys.Count, defaultChanged }),
                new TranslationProgressUpdate
                {
                    WorkItemId = "countries:plan",
                    Stage = ProcessStage.TranslateCountries,
                    Scope = T("Country names"),
                    Unit = T("country names"),
                    TotalItems = countryKeys.Count * targetLanguages.Count,
                    IsPlan = true
                });

            // Process each target language independently
            foreach (string targetLanguage in targetLanguages)
            {
                var languageReport = new TranslationsReport
                {
                    DefaultDictionaryExists = report.DefaultDictionaryExists,
                    DefaultDictionaryCount = report.DefaultDictionaryCount
                };

                await _signalRPublisher.PublishMessageAsync(
                    runId,
                    LocalizationMessageType.Progress,
                    ProcessStage.TranslateCountries,
                    T("Starting country translations for '{targetLanguage}'.", new { targetLanguage }));

                var dictionaryResponse = await _languageService.GetDictionaryAsync(targetLanguage);
                var dictionary = dictionaryResponse.Success && dictionaryResponse.Data != null
                    ? dictionaryResponse.Data
                    : new Dictionary<string, string>();

                int translatedInLanguage = 0;
                int skippedInLanguage = 0;

                foreach (string countryKey in countryKeys)
                {
                    if (dictionary.ContainsKey(countryKey))
                    {
                        skippedInLanguage++;
                        continue;
                    }

                    var translationResponse = await _retryService.TranslateWithRetryAsync(countryKey, DefaultLanguage, targetLanguage);

                    if (!translationResponse.Success || translationResponse.Data == null || string.IsNullOrWhiteSpace(translationResponse.Data.TranslatedText))
                    {
                        languageReport.Errors ??= [];
                        languageReport.Errors.Add(CreateError($"{targetLanguage}:{countryKey}", ErrorCode.TranslationFailed, translationResponse.Message));
                        _logger.LogWarning("Country translation failed for '{CountryKey}' to '{TargetLanguage}'.", countryKey, targetLanguage);
                        continue;
                    }

                    dictionary[countryKey] = translationResponse.Data.TranslatedText.Trim();
                    translatedInLanguage++;
                }

                languageReport.TranslatedCount = translatedInLanguage;
                languageReport.SkippedCount = skippedInLanguage;
                languageReport.ToTranslateCount = countryKeys.Count;

                // Save dictionary immediately after this language is done
                await SaveDictionaryAsync(targetLanguage, dictionary, storingReport, languageReport, runId, $"countries/{targetLanguage}");

                await _signalRPublisher.PublishMessageAsync(
                    runId,
                    LocalizationMessageType.Progress,
                    ProcessStage.TranslateCountries,
                    T("Country translations for '{targetLanguage}' completed. Translated: {translatedCount}, Skipped: {skippedCount}, Errors: {errorCount}.", new
                    {
                        targetLanguage,
                        translatedCount = translatedInLanguage,
                        skippedCount = skippedInLanguage,
                        errorCount = languageReport.Errors?.Count ?? 0
                    }),
                    new TranslationProgressUpdate
                    {
                        WorkItemId = $"countries:{targetLanguage}",
                        Stage = ProcessStage.TranslateCountries,
                        Scope = T("Country names"),
                        TargetLanguage = targetLanguage,
                        Unit = T("country names"),
                        TotalItems = countryKeys.Count,
                        CompletedItems = translatedInLanguage,
                        FailedItems = languageReport.Errors?.Count ?? 0,
                        SkippedItems = skippedInLanguage,
                        SavedItems = 1
                    });
            }

            await _signalRPublisher.PublishStageAsync(
                runId,
                ProcessStage.TranslateCountries,
                report,
                LocalizationMessageType.StageCompleted,
                T("Country names synchronised."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TranslateCountries stage failed.");
            report.Errors ??= [];
            report.Errors.Add(CreateError("countries", ErrorCode.TranslationFailed, ex.Message));

            await _signalRPublisher.PublishStageAsync(
                runId,
                ProcessStage.TranslateCountries,
                report,
                LocalizationMessageType.StageFailed,
                ex.Message,
                isError: true);
            throw;
        }
    }

    private async Task<Dictionary<string, string>> LoadDictionaryOrEmptyAsync(string language, TranslationsReport report)
    {
        var response = await _languageService.GetDictionaryAsync(language);
        if (response.Success && response.Data != null)
        {
            return response.Data;
        }

        report.Errors ??= [];
        report.Errors.Add(CreateError(language, ErrorCode.DictionaryNotFound, response.Message));
        return new Dictionary<string, string>();
    }

    private async Task<List<CountryDefinition>> LoadCountriesAsync()
    {
        if (!File.Exists(CountriesFilePath))
        {
            throw new FileNotFoundException("countries.json was not found.", CountriesFilePath);
        }

        string json = await File.ReadAllTextAsync(CountriesFilePath);
        var countries = JsonSerializer.Deserialize<List<CountryDefinition>>(json);
        return countries ?? new List<CountryDefinition>();
    }

    private async Task<string?> ResolveCountryDefaultPhraseAsync(CountryDefinition country, TranslationsReport report, Guid runId)
    {
        if (DefaultLanguage.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            return country.Name;
        }

        var response = await _retryService.TranslateWithRetryAsync(country.Name, "en", DefaultLanguage);

        if (!response.Success || response.Data == null || string.IsNullOrWhiteSpace(response.Data.TranslatedText))
        {
            report.Errors ??= [];
            report.Errors.Add(CreateError(country.Code, ErrorCode.TranslationFailed, response.Message));

            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.Progress,
                ProcessStage.TranslateCountries,
                T("Failed to resolve default phrase for country '{countryName}': {message}", new { countryName = country.Name, message = response.Message }),
                isError: true);

            return null;
        }

        return response.Data.TranslatedText.Trim();
    }

    private async Task SaveDictionaryAsync(
        string language,
        Dictionary<string, string> dictionary,
        StoringReport storingReport,
        TranslationsReport report,
        Guid runId,
        string source)
    {
        var result = await _languageService.SaveDictionaryAsync(new SingleTranslation
        {
            Language = language,
            Translations = dictionary
        });

        if (result.Success)
        {
            storingReport.SavedDictionaryFiles++;

            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.Progress,
                ProcessStage.TranslateCountries,
                T("Saved dictionary for '{language}' ({entryCount} entries).", new { language, entryCount = dictionary.Count }));

            return;
        }

        report.Errors ??= [];
        var error = CreateError(source, ErrorCode.StorageWriteFailed, result.Message);
        report.Errors.Add(error);
        storingReport.Errors.Add(error);

        await _signalRPublisher.PublishMessageAsync(
            runId,
            LocalizationMessageType.Progress,
            ProcessStage.TranslateCountries,
            T("Failed to save dictionary for '{language}': {message}", new { language, message = result.Message }),
            isError: true);
    }

    private TranslationError CreateError(string source, ErrorCode code, string? details = null)
    {
        string errorText = T(ErrorCodeText.ErrorText(code));

        return new TranslationError
        {
            Source = source,
            Code = code,
            ErrorMessage = string.IsNullOrWhiteSpace(details)
                ? errorText
                : T("{error}: {details}", new { error = errorText, details })
        };
    }
}
