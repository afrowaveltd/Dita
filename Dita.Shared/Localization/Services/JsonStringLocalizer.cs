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
public class JsonStringLocalizer(IDistributedCache cache, ILibreTranslateService translationService, ILogger<JsonStringLocalizer> logger) : IStringLocalizer
{
   private readonly IDistributedCache _cache = cache;
   private readonly ILibreTranslateService _translationService = translationService;
   private readonly ILogger<JsonStringLocalizer> _logger = logger;

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
      string filePath = Path.Combine(LocalesPath, Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLower() + ".json");
      if(!File.Exists(filePath))
      {
         filePath = Path.Combine(LocalesPath, "en.json");
         if(!File.Exists(filePath))
         {
            _logger.LogWarning("Locale file not found for culture {culture}. Expected at {path}", Thread.CurrentThread.CurrentUICulture.Name, filePath);
            return [];
         }
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
         return pairs[key] ?? "";
      }
      catch(Exception ex)
      {
         _logger.LogWarning("Error while reading the dictionary {error}", ex);
         return string.Empty;
      }
   }

   private static string GetLocaleFilePath()
   {
      string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
      string filePath = Path.Combine(LocalesPath, culture + ".json");
      return File.Exists(filePath) ? filePath : Path.Combine(LocalesPath, "en.json");
   }

   private string? GetString(string key)
   {
      string filePath = GetLocaleFilePath();
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
      }
      else
      {
         Response<TranslateResult> translationResult = Task.Run(
            () => _translationService.TranslateTextAsync(key, "en", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName))
            .GetAwaiter()
            .GetResult();

         if(translationResult.Success && translationResult.Data != null)
         {
            value = translationResult.Data.TranslatedText;
            _cache.SetString(cacheKey, value);
         }
      }
      return value;
   }
}