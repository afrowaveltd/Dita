using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Manages named placeholders in localization strings using a separate JSON file.
/// Placeholders use the syntax {name} and are stored per-key in placeholders.json.
/// </summary>
public class PlaceholderService : IPlaceholderService
{
    private readonly ILogger<PlaceholderService> _logger;
    private readonly string _placeholdersFilePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    // Internal storage: key -> { placeholderName -> value }
    private Dictionary<string, Dictionary<string, string>> _placeholders = [];
    private bool _loaded = false;

    private static readonly Regex PlaceholderRegex = new(
        @"\{(\w+)\}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex TranslationSafePlaceholderRegex = new(
        @"___PH_\d+___",
        RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaceholderService"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    public PlaceholderService(ILogger<PlaceholderService> logger)
    {
        _logger = logger;
        _placeholdersFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory[..AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin")],
            "Locales",
            "placeholders.json");
    }

    /// <inheritdoc />
    public Dictionary<string, string> GetPlaceholders(string key)
    {
        EnsureLoaded();
        return _placeholders.TryGetValue(key, out var values)
            ? new Dictionary<string, string>(values, StringComparer.Ordinal)
            : new Dictionary<string, string>();
    }

    /// <inheritdoc />
    public void SetPlaceholder(string key, string placeholderName, string value)
    {
        EnsureLoaded();
        if (!_placeholders.TryGetValue(key, out var dict))
        {
            dict = new Dictionary<string, string>(StringComparer.Ordinal);
            _placeholders[key] = dict;
        }
        dict[placeholderName] = value;
        _logger.LogDebug("Set placeholder '{Placeholder}' for key '{Key}' = '{Value}'", placeholderName, key, value);
    }

    /// <inheritdoc />
    public void RemoveKey(string key)
    {
        EnsureLoaded();
        _placeholders.Remove(key);
        _logger.LogDebug("Removed all placeholders for key '{Key}'", key);
    }

    /// <inheritdoc />
    public string Format(string key, string template, Dictionary<string, string>? values = null)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return template;
        }

        EnsureLoaded();
        var mergedValues = new Dictionary<string, string>(StringComparer.Ordinal);

        // 1. Load stored placeholders for the specific key
        if (_placeholders.TryGetValue(key, out var storedPlaceholders))
        {
            foreach (var (name, val) in storedPlaceholders)
            {
                mergedValues[$"{{{name}}}"] = val;
            }
        }

        // 2. Runtime values override stored values
        if (values != null)
        {
            foreach (var (name, val) in values)
            {
                mergedValues[$"{{{name}}}"] = val;
            }
        }

        // 3. Replace placeholders
        string result = PlaceholderRegex.Replace(template, match =>
        {
            string fullPlaceholder = match.Value; // e.g. "{name}"
            return mergedValues.TryGetValue(fullPlaceholder, out string? value)
                ? value
                : fullPlaceholder; // Leave unchanged if not found
        });

        return result;
    }

    /// <inheritdoc />
    public string Format(string template, Dictionary<string, string>? values = null)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return template;
        }

        EnsureLoaded();
        var mergedValues = new Dictionary<string, string>(StringComparer.Ordinal);

        // 1. Runtime values (no key-based lookup in this overload)
        if (values != null)
        {
            foreach (var (name, val) in values)
            {
                mergedValues[$"{{{name}}}"] = val;
            }
        }

        // 2. Replace placeholders
        string result = PlaceholderRegex.Replace(template, match =>
        {
            string fullPlaceholder = match.Value;
            return mergedValues.TryGetValue(fullPlaceholder, out string? value)
                ? value
                : fullPlaceholder;
        });

        return result;
    }

    /// <inheritdoc />
    public string[] ExtractPlaceholders(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return Array.Empty<string>();
        }

        return PlaceholderRegex.Matches(template)
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    /// <inheritdoc />
    public bool HasPlaceholders(string template)
    {
        return !string.IsNullOrWhiteSpace(template) && PlaceholderRegex.IsMatch(template);
    }

    /// <inheritdoc />
    public (string preparedText, Func<string, string> restore) PrepareForTranslation(string template)
    {
        if (string.IsNullOrWhiteSpace(template) || !HasPlaceholders(template))
        {
            return (template, translated => translated);
        }

        var placeholders = new List<string>();
        int counter = 0;

        string prepared = PlaceholderRegex.Replace(template, match =>
        {
            string placeholderName = match.Groups[1].Value;
            placeholders.Add(placeholderName);
            return $"___PH_{counter++}___";
        });

        Func<string, string> restore = translated =>
        {
            if (string.IsNullOrWhiteSpace(translated))
            {
                return translated;
            }

            string result = translated;
            for (int i = 0; i < placeholders.Count; i++)
            {
                result = result.Replace($"___PH_{i}___", $"{{{placeholders[i]}}}", StringComparison.Ordinal);
            }
            return result;
        };

        return (prepared, restore);
    }

    /// <inheritdoc />
    public (string preparedText, Func<string, string> restore) PrepareForTranslation(
        string template,
        Dictionary<string, string>? referenceValues)
    {
        if (string.IsNullOrWhiteSpace(template) || !HasPlaceholders(template))
        {
            return (template, translated => translated);
        }

        var placeholders = new List<(string Name, string Token, string? ReferenceValue)>();
        int counter = 0;

        string prepared = PlaceholderRegex.Replace(template, match =>
        {
            string placeholderName = match.Groups[1].Value;
            string token = $"___PH_{counter++}___";
            string? referenceValue = ResolveReferenceValue(referenceValues, placeholderName);

            placeholders.Add((placeholderName, token, referenceValue));
            return string.IsNullOrWhiteSpace(referenceValue) ? token : referenceValue;
        });

        Func<string, string> restore = translated =>
        {
            if (string.IsNullOrWhiteSpace(translated))
            {
                return translated;
            }

            string result = translated;
            foreach ((string name, string token, string? referenceValue) in placeholders)
            {
                string placeholder = $"{{{name}}}";
                result = result.Replace(token, placeholder, StringComparison.Ordinal);

                if (!string.IsNullOrWhiteSpace(referenceValue))
                {
                    result = result.Replace(referenceValue, placeholder, StringComparison.Ordinal);
                }
            }

            return result;
        };

        return (prepared, restore);
    }

    /// <inheritdoc />
    public async Task SaveAsync()
    {
        await _fileLock.WaitAsync().ConfigureAwait(false);
        try
        {
            string? directory = Path.GetDirectoryName(_placeholdersFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(_placeholders, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(_placeholdersFilePath, json).ConfigureAwait(false);
            _logger.LogDebug("Saved {Count} placeholder entries to {Path}", _placeholders.Count, _placeholdersFilePath);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task LoadAsync()
    {
        await _fileLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!File.Exists(_placeholdersFilePath))
            {
                _placeholders = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
                _loaded = true;
                return;
            }

            string json = await File.ReadAllTextAsync(_placeholdersFilePath).ConfigureAwait(false);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(
                json,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            _placeholders = loaded ?? new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            _loaded = true;

            _logger.LogDebug("Loaded {Count} placeholder entries from {Path}", _placeholders.Count, _placeholdersFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load placeholders from {Path}. Starting with empty collection.", _placeholdersFilePath);
            _placeholders = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            _loaded = true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Ensures placeholder data is loaded from disk.
    /// </summary>
    private void EnsureLoaded()
    {
        if (!_loaded)
        {
            LoadAsync().GetAwaiter().GetResult();
        }
    }

    private static string? ResolveReferenceValue(Dictionary<string, string>? referenceValues, string placeholderName)
    {
        if (referenceValues is null)
        {
            return null;
        }

        if (referenceValues.TryGetValue(placeholderName, out string? value))
        {
            return value;
        }

        return referenceValues.TryGetValue($"{{{placeholderName}}}", out value)
            ? value
            : null;
    }
}
