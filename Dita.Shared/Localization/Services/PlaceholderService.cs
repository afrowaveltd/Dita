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

    /// <summary>
    /// Strict match for the canonical token format ⟦N⟧ (U+27E6 … U+27E7).
    /// Used for identity checks only — restore uses the flexible regex below.
    /// </summary>
    private static readonly Regex TranslationSafePlaceholderRegex = new(
        @"\u27e6\d+\u27e7",
        RegexOptions.Compiled);

    /// <summary>
    /// Flexible regex that matches placeholder tokens in both the canonical format
    /// and in forms corrupted by machine-translation engines.
    /// <para>
    /// New token format:  ⟦N⟧  (mathematical left/right white lenticular brackets).
    /// These Unicode characters are outside normal text ranges and are typically
    /// left intact by translation engines.</para>
    /// <para>
    /// Legacy format:  ___PH_N___  — still recognised with tolerance for spaces
    /// inserted by MT (e.g. "___ PH _ 0 ___", "_ _ _ P H _ 0 _ _ _").</para>
    /// <para>Capture group 1 = new-token index, group 2 = legacy-token index.</para>
    /// </summary>
    private static readonly Regex TokenRestoreRegex = new(
        @"\u27e6(\d+)\u27e7|(?:_\s*){3,}P\s*H\s*(?:_\s*)*(\d+)(?:\s*_){3,}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaceholderService"/> class.
    /// </summary>
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
        if (string.IsNullOrWhiteSpace(template)) return template;

        EnsureLoaded();
        var mergedValues = new Dictionary<string, string>(StringComparer.Ordinal);

        if (_placeholders.TryGetValue(key, out var storedPlaceholders))
            foreach (var (name, val) in storedPlaceholders)
                mergedValues[$"{{{name}}}"] = val;

        if (values != null)
            foreach (var (name, val) in values)
                mergedValues[$"{{{name}}}"] = val;

        return PlaceholderRegex.Replace(template, match =>
            mergedValues.TryGetValue(match.Value, out string? value) ? value : match.Value);
    }

    /// <inheritdoc />
    public string Format(string template, Dictionary<string, string>? values = null)
    {
        if (string.IsNullOrWhiteSpace(template)) return template;

        EnsureLoaded();
        var mergedValues = new Dictionary<string, string>(StringComparer.Ordinal);

        if (values != null)
            foreach (var (name, val) in values)
                mergedValues[$"{{{name}}}"] = val;

        return PlaceholderRegex.Replace(template, match =>
            mergedValues.TryGetValue(match.Value, out string? value) ? value : match.Value);
    }

    /// <inheritdoc />
    public string[] ExtractPlaceholders(string template)
    {
        if (string.IsNullOrWhiteSpace(template)) return Array.Empty<string>();
        return PlaceholderRegex.Matches(template)
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    /// <inheritdoc />
    public bool HasPlaceholders(string template)
        => !string.IsNullOrWhiteSpace(template) && PlaceholderRegex.IsMatch(template);

    /// <inheritdoc />
    public (string preparedText, Func<string, string> restore) PrepareForTranslation(string template)
    {
        if (string.IsNullOrWhiteSpace(template) || !HasPlaceholders(template))
            return (template, translated => translated);

        var placeholders = new List<string>();
        int counter = 0;

        string prepared = PlaceholderRegex.Replace(template, match =>
        {
            string placeholderName = match.Groups[1].Value;
            placeholders.Add(placeholderName);
            return $"\u27e6{counter++}\u27e7"; // ⟦N⟧
        });

        var placeholdersSnapshot = placeholders.ToArray();

        Func<string, string> restore = translated =>
        {
            if (string.IsNullOrWhiteSpace(translated)) return translated;

            return TokenRestoreRegex.Replace(translated, match =>
            {
                string? indexStr = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                if (int.TryParse(indexStr, out int index) && index >= 0 && index < placeholdersSnapshot.Length)
                    return $"{{{placeholdersSnapshot[index]}}}";
                return match.Value;
            });
        };

        return (prepared, restore);
    }

    /// <inheritdoc />
    public (string preparedText, Func<string, string> restore) PrepareForTranslation(
        string template,
        Dictionary<string, string>? referenceValues)
    {
        if (string.IsNullOrWhiteSpace(template) || !HasPlaceholders(template))
            return (template, translated => translated);

        var placeholders = new List<(string Name, string Token, string? ReferenceValue)>();
        int counter = 0;

        string prepared = PlaceholderRegex.Replace(template, match =>
        {
            string placeholderName = match.Groups[1].Value;
            string token = $"\u27e6{counter++}\u27e7"; // ⟦N⟧
            string? referenceValue = ResolveReferenceValue(referenceValues, placeholderName);
            placeholders.Add((placeholderName, token, referenceValue));
            return string.IsNullOrWhiteSpace(referenceValue) ? token : referenceValue;
        });

        // Record reference-value positions in the prepared text for positional restore.
        var referencePositions = new List<(string Token, string ReferenceValue, int PreparedIndex)>();
        foreach ((string name, string token, string? referenceValue) in placeholders)
        {
            if (string.IsNullOrWhiteSpace(referenceValue)) continue;
            int occurrenceIndex = referencePositions.Count(rv => rv.ReferenceValue.Equals(referenceValue, StringComparison.Ordinal));
            int pos = FindNthOccurrence(prepared, referenceValue!, occurrenceIndex + 1);
            if (pos >= 0)
                referencePositions.Add((token, referenceValue!, pos));
        }

        var referencePositionsSnapshot = referencePositions.ToArray();
        var placeholdersSnapshot = placeholders.ToArray();

        Func<string, string> restore = translated =>
        {
            if (string.IsNullOrWhiteSpace(translated)) return translated;

            string result = translated;

            // Phase 1: locate reference values positionally, replace with unique tokens (right-to-left).
            if (referencePositionsSnapshot.Length > 0)
            {
                var locatedReplacements = new List<(int Position, int Length, string Token)>();
                foreach (var group in referencePositionsSnapshot.GroupBy(rv => rv.ReferenceValue, StringComparer.Ordinal))
                {
                    string refVal = group.Key;
                    var members = group.OrderBy(m => m.PreparedIndex).ToList();
                    var allOccurrences = new List<int>();
                    int searchFrom = 0;
                    while (searchFrom < result.Length)
                    {
                        int idx = result.IndexOf(refVal, searchFrom, StringComparison.Ordinal);
                        if (idx < 0) break;
                        allOccurrences.Add(idx);
                        searchFrom = idx + refVal.Length;
                    }
                    for (int i = 0; i < members.Count && i < allOccurrences.Count; i++)
                        locatedReplacements.Add((allOccurrences[i], refVal.Length, members[i].Token));
                }
                foreach ((int position, int length, string token) in locatedReplacements.OrderByDescending(r => r.Position))
                {
                    if (position <= result.Length - length)
                        result = string.Concat(result.AsSpan(0, position), token, result.AsSpan(position + length));
                }
            }

            // Phase 2: replace all tokens (new ⟦N⟧ or legacy ___PH_N___) with placeholder names.
            result = TokenRestoreRegex.Replace(result, match =>
            {
                string? indexStr = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                if (int.TryParse(indexStr, out int index) && index >= 0 && index < placeholdersSnapshot.Length)
                    return $"{{{placeholdersSnapshot[index].Name}}}";
                return match.Value;
            });

            return result;
        };

        return (prepared, restore);
    }

    private static int FindNthOccurrence(string text, string value, int n)
    {
        if (n < 1 || string.IsNullOrEmpty(value)) return -1;
        int index = -1;
        for (int i = 0; i < n; i++)
        {
            index = text.IndexOf(value, index + 1, StringComparison.Ordinal);
            if (index < 0) return -1;
        }
        return index;
    }

    /// <inheritdoc />
    public async Task SaveAsync()
    {
        await _fileLock.WaitAsync().ConfigureAwait(false);
        try
        {
            string? directory = Path.GetDirectoryName(_placeholdersFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(_placeholders, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(_placeholdersFilePath, json).ConfigureAwait(false);
            _logger.LogDebug("Saved {Count} placeholder entries to {Path}", _placeholders.Count, _placeholdersFilePath);
        }
        finally { _fileLock.Release(); }
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
                json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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
        finally { _fileLock.Release(); }
    }

    private void EnsureLoaded()
    {
        if (!_loaded) LoadAsync().GetAwaiter().GetResult();
    }

    private static string? ResolveReferenceValue(Dictionary<string, string>? referenceValues, string placeholderName)
    {
        if (referenceValues is null) return null;
        if (referenceValues.TryGetValue(placeholderName, out string? value)) return value;
        return referenceValues.TryGetValue($"{{{placeholderName}}}", out value) ? value : null;
    }
}