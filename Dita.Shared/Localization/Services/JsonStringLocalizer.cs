using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Provides localization using JSON files for Blazor applications. Implements <see cref="IStringLocalizer"/> and
/// supports distributed caching.
/// </summary>
public class JsonStringLocalizer(
   IDistributedCache cache,
   ILibreTranslateService translationService,
   AutomaticTranslationSettings automaticTranslationSettings,
   ILogger<JsonStringLocalizer> logger) : IStringLocalizer
{
   private readonly IDistributedCache _cache = cache;
   private readonly ILibreTranslateService _translationService = translationService;
   private readonly AutomaticTranslationSettings _automaticTranslationSettings = automaticTranslationSettings;
   private readonly ILogger<JsonStringLocalizer> _logger = logger;
   private static readonly SemaphoreSlim DefaultDictionaryLock = new(1, 1);

   private static string LocalesPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory
[..AppDomain.CurrentDomain.BaseDirectory
      .IndexOf("bin")], "Locales");

   /// <summary>
   /// Gets the localized string for the specified key.
   /// </summary>
   /// <param name="name">The key of the localized string.</param>
   /// <returns>
   /// A <see cref="LocalizedString"/> containing the localized value, or the key if not found.
   /// </returns>
   public LocalizedString this[string name]
   {
      get
      {
         var value = GetString(name);
         return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
      }
   }

   /// <summary>
   /// Gets the localized string for the specified key and formats it with the provided arguments.
   /// </summary>
   /// <param name="name">The key of the localized string.</param>
   /// <param name="arguments">Arguments to format the localized string.</param>
   /// <returns>
   /// A <see cref="LocalizedString"/> containing the formatted localized value, or the key if not found.
   /// </returns>
   public LocalizedString this[string name, params object[] arguments]
   {
      get
      {
         LocalizedString actualValue = this[name];
         return !actualValue.ResourceNotFound
              ? new LocalizedString(name, string.Format(actualValue.Value, arguments), false)
              : actualValue;
      }
   }

   /// <summary>
   /// Returns all localized strings for the current culture.
   /// </summary>
   /// <param name="includeParentCultures">Indicates whether to include parent cultures.</param>
   /// <returns>An <see cref="IEnumerable{LocalizedString}"/> of all localized strings.</returns>
   public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
   {
      EnsureLocalesInfrastructureExists();

      string? filePath = ResolveLocaleFilePath(CultureInfo.CurrentUICulture);
      if(string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
      {
         _logger.LogWarning("Locale file not found for culture {culture}.", Thread.CurrentThread.CurrentUICulture.Name);
         return [];
      }

      try
      {
         var json = File.ReadAllText(filePath);
         Dictionary<string, string>? dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
         return dict == null ? [] : dict.Select(kvp => new LocalizedString(kvp.Key, kvp.Value, resourceNotFound: false));
      }
      catch(Exception ex)
      {
         _logger.LogError("Error while reading the dictionary {error}", ex);
         return [];
      }
   }

   private string? GetValueFromJson(string key, string filePath)
   {
      if(string.IsNullOrEmpty(key) || string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
      {
         return default;
      }

      try
      {
         string jsonDictionary = File.ReadAllText(filePath);
         Dictionary<string, string> pairs = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonDictionary) ?? [];
         return pairs.TryGetValue(key, out string? value) ? value : string.Empty;
      }
      catch(Exception ex)
      {
         _logger.LogWarning("Error while reading the dictionary {error}", ex);
         return string.Empty;
      }
   }

   private string? ResolveLocaleFilePath(CultureInfo culture)
   {
      List<string> candidates =
      [
         culture.Name,
         culture.TwoLetterISOLanguageName,
         GetDefaultLanguageCode(),
         "en"
      ];

      foreach(string candidate in candidates.Where(static candidate => !string.IsNullOrWhiteSpace(candidate)).Distinct(StringComparer.OrdinalIgnoreCase))
      {
         string candidatePath = Path.Combine(LocalesPath, candidate + ".json");
         if(File.Exists(candidatePath))
         {
            return candidatePath;
         }
      }

      return null;
   }

   private string GetDefaultLanguageCode()
   {
      string configuredDefaultLanguage = _automaticTranslationSettings.DefaultLanguage;
      if(string.IsNullOrWhiteSpace(configuredDefaultLanguage))
      {
         return "en";
      }

      string normalized = configuredDefaultLanguage.Trim();
      try
      {
         return CultureInfo.GetCultureInfo(normalized).Name;
      }
      catch(CultureNotFoundException)
      {
         return normalized;
      }
   }

   private string GetDefaultLocaleFilePath()
   {
      return Path.Combine(LocalesPath, GetDefaultLanguageCode() + ".json");
   }

   /// <summary>
   /// Ensures that the configured default dictionary contains the specified key as key=value.
   /// </summary>
   /// <param name="key">The translation key to ensure in the default dictionary.</param>
   private void EnsureKeyExistsInDefaultDictionary(string key)
   {
      if(string.IsNullOrWhiteSpace(key))
      {
         return;
      }

      EnsureLocalesInfrastructureExists();
      string defaultFilePath = GetDefaultLocaleFilePath();

      DefaultDictionaryLock.Wait();
      try
      {
         if(!File.Exists(defaultFilePath))
         {
            File.WriteAllText(defaultFilePath, "{}");
         }

         string json = File.ReadAllText(defaultFilePath);
         Dictionary<string, string> dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];

         if(dict.ContainsKey(key))
         {
            return;
         }

         dict[key] = key;
         Dictionary<string, string> sorted = dict
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);

         string updatedJson = JsonSerializer.Serialize(sorted);
         File.WriteAllText(defaultFilePath, updatedJson);
      }
      catch(Exception ex)
      {
         _logger.LogWarning(ex, "Could not add missing key '{Key}' to default dictionary file '{Path}'.", key, defaultFilePath);
      }
      finally
      {
         DefaultDictionaryLock.Release();
      }
   }

   /// <summary>
   /// Ensures that the Locales directory and configured default locale file exist.
   /// </summary>
   private void EnsureLocalesInfrastructureExists()
   {
      try
      {
         if(!Directory.Exists(LocalesPath))
         {
            Directory.CreateDirectory(LocalesPath);
            _logger.LogInformation("Created locales directory at {Path}.", LocalesPath);
         }

         string defaultLocalePath = GetDefaultLocaleFilePath();
         if(!File.Exists(defaultLocalePath))
         {
            File.WriteAllText(defaultLocalePath, "{}");
            _logger.LogInformation("Created default locale file at {Path}.", defaultLocalePath);
         }
      }
      catch(Exception ex)
      {
         _logger.LogWarning(ex, "Could not ensure locales infrastructure exists.");
      }
   }

   private string? GetString(string key)
   {
      EnsureLocalesInfrastructureExists();

      string? filePath = ResolveLocaleFilePath(CultureInfo.CurrentUICulture);
      if(string.IsNullOrWhiteSpace(filePath))
      {
         _logger.LogWarning("No locale file could be resolved for culture {Culture}.", CultureInfo.CurrentUICulture.Name);
         return key;
      }

      string cacheKey = $"locale_{CultureInfo.CurrentUICulture.Name}_{key}";
      string? cachedValue = _cache.GetString(cacheKey);

      if(!string.IsNullOrEmpty(cachedValue))
      {
         return cachedValue;
      }

      _logger.LogDebug("Value for key {key} not found in cache. Attempting to read from JSON file.", key);
      string? value = GetValueFromJson(key, filePath);
      if(!string.IsNullOrEmpty(value))
      {
         _cache.SetString(cacheKey, value);
         return value;
      }

      EnsureKeyExistsInDefaultDictionary(key);

      string defaultLanguage = GetDefaultLanguageCode();
      string targetLanguage = string.IsNullOrWhiteSpace(Thread.CurrentThread.CurrentUICulture.Name)
         ? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName
         : Thread.CurrentThread.CurrentUICulture.Name;

      Response<TranslateResult> translationResult = Task.Run(
            () => _translationService.TranslateTextAsync(key, defaultLanguage, targetLanguage))
         .GetAwaiter()
         .GetResult();

      if(translationResult?.Success == true && translationResult.Data != null)
      {
         value = translationResult.Data.TranslatedText;
         _cache.SetString(cacheKey, value);
      }

      return value;
   }
}