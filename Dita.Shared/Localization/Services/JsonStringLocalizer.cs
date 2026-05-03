using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Provides localization using JSON files. Implements <see cref="IStringLocalizer"/> with support
/// for named placeholders ({name}), distributed caching, and fallback to LibreTranslate.
/// </summary>
public class JsonStringLocalizer(
    IDistributedCache cache,
    ILibreTranslateService translationService,
    AutomaticTranslationSettings automaticTranslationSettings,
    IPlaceholderService placeholderService,
    ILogger<JsonStringLocalizer> logger) : IStringLocalizer
{
    private readonly IDistributedCache _cache = cache;
    private readonly ILibreTranslateService _translationService = translationService;
    private readonly AutomaticTranslationSettings _automaticTranslationSettings = automaticTranslationSettings;
    private readonly IPlaceholderService _placeholderService = placeholderService;
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
    /// Gets the localized string for the specified key and formats it with positional arguments.
    /// Uses <see cref="string.Format(string, object[])"/> for formatting. For named placeholders use
    /// the indexer with Dictionary parameter instead.
    /// </summary>
    /// <param name="name">The key of the localized string.</param>
    /// <param name="arguments">Positional arguments to format the localized string.</param>
    /// <returns>
    /// A <see cref="LocalizedString"/> containing the formatted localized value, or the key if not found.
    /// </returns>
    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            Dictionary<string, string> placeholderValues = ConvertArgumentsToPlaceholderValues(name, arguments);
            if (placeholderValues.Count > 0)
            {
                string? template = GetString(name, placeholderValues);
                if (template == null)
                {
                    return new LocalizedString(name, name, resourceNotFound: true);
                }

                string formatted = _placeholderService.Format(name, template, placeholderValues);
                return new LocalizedString(name, formatted, resourceNotFound: false);
            }

            LocalizedString actualValue = this[name];
            return !actualValue.ResourceNotFound && arguments.Length > 0
                ? new LocalizedString(name, string.Format(actualValue.Value, arguments), false)
                : actualValue;
        }
    }

    /// <summary>
    /// Gets the localized string for the specified key and formats named placeholders.
    /// Placeholders use the syntax {name} and are replaced from the provided values dictionary.
    /// Falls back to stored placeholder values if a named placeholder is not in the runtime values.
    /// </summary>
    /// <param name="name">The key of the localized string.</param>
    /// <param name="placeholderValues">Runtime values for named placeholders.</param>
    /// <returns>
    /// A <see cref="LocalizedString"/> containing the formatted localized value, or the key if not found.
    /// </returns>
    public LocalizedString this[string name, Dictionary<string, string> placeholderValues]
    {
        get
        {
            string? template = GetString(name, placeholderValues);
            if (template == null)
            {
                return new LocalizedString(name, name, resourceNotFound: true);
            }

            string formatted = _placeholderService.Format(name, template, placeholderValues);
            return new LocalizedString(name, formatted, resourceNotFound: false);
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
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
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
        catch (Exception ex)
        {
            _logger.LogError("Error while reading the dictionary {error}", ex);
            return [];
        }
    }

    private string? GetValueFromJson(string key, string filePath)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return default;
        }

        try
        {
            string jsonDictionary = File.ReadAllText(filePath);
            Dictionary<string, string> pairs = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonDictionary) ?? [];
            return pairs.TryGetValue(key, out string? value) ? value : string.Empty;
        }
        catch (Exception ex)
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

        foreach (string candidate in candidates.Where(static candidate => !string.IsNullOrWhiteSpace(candidate)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string candidatePath = Path.Combine(LocalesPath, candidate + ".json");
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        return null;
    }

    private string GetDefaultLanguageCode()
    {
        string configuredDefaultLanguage = _automaticTranslationSettings.DefaultLanguage;
        if (string.IsNullOrWhiteSpace(configuredDefaultLanguage))
        {
            return "en";
        }

        string normalized = configuredDefaultLanguage.Trim();
        try
        {
            return CultureInfo.GetCultureInfo(normalized).Name;
        }
        catch (CultureNotFoundException)
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
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        EnsureLocalesInfrastructureExists();
        string defaultFilePath = GetDefaultLocaleFilePath();

        DefaultDictionaryLock.Wait();
        try
        {
            if (!File.Exists(defaultFilePath))
            {
                File.WriteAllText(defaultFilePath, "{}");
            }

            string json = File.ReadAllText(defaultFilePath);
            Dictionary<string, string> dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];

            if (dict.ContainsKey(key))
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
        catch (Exception ex)
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
            if (!Directory.Exists(LocalesPath))
            {
                Directory.CreateDirectory(LocalesPath);
                _logger.LogInformation("Created locales directory at {Path}.", LocalesPath);
            }

            string defaultLocalePath = GetDefaultLocaleFilePath();
            if (!File.Exists(defaultLocalePath))
            {
                File.WriteAllText(defaultLocalePath, "{}");
                _logger.LogInformation("Created default locale file at {Path}.", defaultLocalePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not ensure locales infrastructure exists.");
        }
    }

    private string? GetString(string key, Dictionary<string, string>? translationReferenceValues = null)
    {
        EnsureLocalesInfrastructureExists();

        string? filePath = ResolveLocaleFilePath(CultureInfo.CurrentUICulture);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogWarning("No locale file could be resolved for culture {Culture}.", CultureInfo.CurrentUICulture.Name);
            return key;
        }

        string cacheKey = $"locale_{CultureInfo.CurrentUICulture.Name}_{key}";
        string? cachedValue = _cache.GetString(cacheKey);

        if (!string.IsNullOrEmpty(cachedValue))
        {
            return cachedValue;
        }

        _logger.LogDebug("Value for key {key} not found in cache. Attempting to read from JSON file.", key);
        string? value = GetValueFromJson(key, filePath);
        if (!string.IsNullOrEmpty(value))
        {
            _cache.SetString(cacheKey, value);
            return value;
        }

        EnsureKeyExistsInDefaultDictionary(key);

        string defaultLanguage = GetDefaultLanguageCode();
        string targetLanguage = string.IsNullOrWhiteSpace(Thread.CurrentThread.CurrentUICulture.Name)
            ? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName
            : Thread.CurrentThread.CurrentUICulture.Name;

        if (targetLanguage.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))
        {
            _cache.SetString(cacheKey, key);
            return key;
        }

        (string preparedText, Func<string, string> restorePlaceholders) = _placeholderService.PrepareForTranslation(key, translationReferenceValues);
        preparedText ??= key;
        restorePlaceholders ??= translated => translated;

        Response<TranslateResult> translationResult = Task.Run(
              () => _translationService.TranslateTextAsync(preparedText, defaultLanguage, targetLanguage))
           .GetAwaiter()
           .GetResult();

        if (translationResult?.Success == true && translationResult.Data != null)
        {
            value = restorePlaceholders(translationResult.Data.TranslatedText);
            _cache.SetString(cacheKey, value);
        }

        return value;
    }

    private Dictionary<string, string> ConvertArgumentsToPlaceholderValues(string key, object[] arguments)
    {
        if (arguments.Length == 0 || !_placeholderService.HasPlaceholders(key))
        {
            return [];
        }

        if (arguments.Length == 1)
        {
            Dictionary<string, string> singleArgumentValues = ConvertSingleArgumentToPlaceholderValues(arguments[0]);
            if (singleArgumentValues.Count > 0)
            {
                return singleArgumentValues;
            }
        }

        string[] placeholders = _placeholderService.ExtractPlaceholders(key);
        if (placeholders.Length == 0)
        {
            return [];
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int i = 0; i < Math.Min(placeholders.Length, arguments.Length); i++)
        {
            values[placeholders[i]] = Convert.ToString(arguments[i], CultureInfo.CurrentCulture) ?? string.Empty;
        }

        return values;
    }

    private static Dictionary<string, string> ConvertSingleArgumentToPlaceholderValues(object? argument)
    {
        if (argument is null)
        {
            return [];
        }

        if (argument is Dictionary<string, string> stringDictionary)
        {
            return new Dictionary<string, string>(stringDictionary, StringComparer.Ordinal);
        }

        if (argument is IReadOnlyDictionary<string, string> readOnlyStringDictionary)
        {
            return readOnlyStringDictionary.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        }

        if (argument is IEnumerable<KeyValuePair<string, object?>> objectPairs)
        {
            return objectPairs.ToDictionary(
                item => item.Key,
                item => Convert.ToString(item.Value, CultureInfo.CurrentCulture) ?? string.Empty,
                StringComparer.Ordinal);
        }

        Type argumentType = argument.GetType();
        if (argumentType.IsPrimitive || argument is string or decimal or DateTime or DateTimeOffset or Guid)
        {
            return [];
        }

        return argumentType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetIndexParameters().Length == 0)
            .ToDictionary(
                property => property.Name,
                property => Convert.ToString(property.GetValue(argument), CultureInfo.CurrentCulture) ?? string.Empty,
                StringComparer.Ordinal);
    }
}
