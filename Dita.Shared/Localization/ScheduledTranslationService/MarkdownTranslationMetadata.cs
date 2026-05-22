using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Tracks per-block translation status for Markdown files to enable incremental/partial re-translation.
/// </summary>
public class MarkdownTranslationMetadata
{
    /// <summary>
    /// SHA256 hash of the source Markdown content at the time of the last successful translation.
    /// </summary>
    public string SourceHash { get; set; } = string.Empty;

    /// <summary>
    /// Dictionary mapping language codes to per-block translation status.
    /// Each entry contains a list of booleans indicating whether each translatable block was translated.
    /// </summary>
    public Dictionary<string, List<bool>> LanguageBlockStatus { get; set; } = [];

    /// <summary>
    /// UTC timestamp of the last metadata update.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Loads metadata for the specified source Markdown file if it exists.
    /// </summary>
    /// <param name="sourceFilePath">Path to the source Markdown file.</param>
    /// <returns>Deserialized metadata or an empty instance if no file exists.</returns>
    public static MarkdownTranslationMetadata? Load(string sourceFilePath)
    {
        string metaPath = GetMetadataPath(sourceFilePath);
        if (!File.Exists(metaPath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(metaPath);
            return JsonSerializer.Deserialize<MarkdownTranslationMetadata>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Saves metadata for the specified source Markdown file.
    /// </summary>
    /// <param name="sourceFilePath">Path to the source Markdown file.</param>
    public void Save(string sourceFilePath)
    {
        UpdatedAtUtc = DateTime.UtcNow;
        string metaPath = GetMetadataPath(sourceFilePath);
        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(metaPath, json);
    }

    /// <summary>
    /// Checks whether all blocks are translated for the given language.
    /// </summary>
    /// <param name="language">The language code to check.</param>
    /// <returns>True if all blocks are marked translated; otherwise false.</returns>
    public bool IsFullyTranslated(string language)
    {
        if (!LanguageBlockStatus.TryGetValue(language, out List<bool>? blocks))
        {
            return false;
        }

        return blocks.Count > 0 && blocks.All(b => b);
    }

    /// <summary>
    /// Checks whether all expected blocks are translated for the given language.
    /// </summary>
    /// <param name="language">The language code to check.</param>
    /// <param name="expectedBlockCount">Current number of translatable blocks in the source document.</param>
    /// <returns>True if the tracked block count matches and every block is marked translated.</returns>
    public bool IsFullyTranslated(string language, int expectedBlockCount)
    {
        if (!LanguageBlockStatus.TryGetValue(language, out List<bool>? blocks))
        {
            return false;
        }

        return blocks.Count == expectedBlockCount && blocks.All(b => b);
    }

    /// <summary>
    /// Returns the number of untranslated blocks for the given language.
    /// </summary>
    /// <param name="language">The language code to check.</param>
    /// <returns>Count of untranslated blocks.</returns>
    public int GetUntranslatedBlockCount(string language)
    {
        if (!LanguageBlockStatus.TryGetValue(language, out List<bool>? blocks))
        {
            return 0;
        }

        return blocks.Count(b => !b);
    }

    /// <summary>
    /// Determines whether the metadata is stale compared to the current source content.
    /// </summary>
    /// <param name="currentSourceContent">Current content of the source Markdown file.</param>
    /// <returns>True if the source has changed since the metadata was created.</returns>
    public bool IsStale(string currentSourceContent)
    {
        string normalizedHash = ComputeSourceHash(currentSourceContent);
        if (string.Equals(SourceHash, normalizedHash, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Backward compatibility for metadata written before line-ending normalization.
        string rawHash = ComputeHash(currentSourceContent);
        if (string.Equals(SourceHash, rawHash, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string crlfHash = ComputeHash(NormalizeLineEndings(currentSourceContent, "\r\n"));
        return !string.Equals(SourceHash, crlfHash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Computes the stable source hash used for Markdown change detection.
    /// Line endings are normalized before hashing so Windows/Linux checkouts compare equal.
    /// </summary>
    /// <param name="content">The source Markdown content.</param>
    /// <returns>A SHA256 hash of the normalized content.</returns>
    public static string ComputeSourceHash(string content)
    {
        return ComputeHash(NormalizeLineEndings(content, "\n"));
    }

    private static string GetMetadataPath(string sourceFilePath)
    {
        string directory = Path.GetDirectoryName(sourceFilePath) ?? string.Empty;
        string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
        return Path.Combine(directory, $"{fileName}.translation-meta.json");
    }

    private static string ComputeHash(string content)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }

    private static string NormalizeLineEndings(string content, string newline)
    {
        return content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("\n", newline, StringComparison.Ordinal);
    }
}
