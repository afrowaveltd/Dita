using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Synchronizes Markdown documentation translations by detecting changed source files
/// and translating them into all target languages. Each target language is translated
/// and saved independently. Supports partial translation tracking via per-file metadata.
/// </summary>
public class DocumentsTranslationService(
    IMarkdownReconstructorService markdownReconstructorService,
    IMarkdownParserService markdownParserService,
    TranslationRetryService retryService,
    ISignalRPublisher signalRPublisher,
    IHostEnvironment hostEnvironment,
    ILanguageService languageService,
    AutomaticTranslationSettings settings,
    IStringLocalizer<DocumentsTranslationService> localizer,
    ILogger<DocumentsTranslationService> logger) : IDocumentsTranslationService
{
    private readonly IMarkdownReconstructorService _markdownReconstructorService = markdownReconstructorService;
    private readonly IMarkdownParserService _markdownParserService = markdownParserService;
    private readonly TranslationRetryService _retryService = retryService;
    private readonly ISignalRPublisher _signalRPublisher = signalRPublisher;
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly ILanguageService _languageService = languageService;
    private readonly AutomaticTranslationSettings _settings = settings;
    private readonly IStringLocalizer<DocumentsTranslationService> _localizer = localizer;
    private readonly ILogger<DocumentsTranslationService> _logger = logger;

    private static string TempHashDirectory => Path.Combine(Path.GetTempPath(), "dita", "localization-hashes");
    private string DefaultLanguage => _settings.DefaultLanguage ?? "en";
    private List<string> MarkdownRoots => _settings.MarkdownRoots is { Count: > 0 } ? _settings.MarkdownRoots : ["/Docs"];

    private string T(string text) => _localizer[text].Value;

    private string T(string text, object values) => _localizer[text, values].Value;

    private string GetLocalizedLanguageName(string languageCode)
    {
        string languageName = _languageService.GetLanguageDisplayName(languageCode);
        return string.IsNullOrWhiteSpace(languageName) ? languageCode : T(languageName);
    }

    /// <summary>
    /// Synchronizes Markdown translations by detecting changed source files and translating them
    /// into all required target languages. Each language is saved immediately after translation.
    /// </summary>
    public async Task RunAsync(List<string> targetLanguages, StoringReport storingReport, Guid runId)
    {
        var report = new MarkdownTranslationsReport();

        await _signalRPublisher.PublishStageAsync(
            runId,
            ProcessStage.TranslateMarkdownFiles,
            report,
            LocalizationMessageType.StageStarted,
            T("Synchronising Markdown translations."));

        try
        {
            foreach (string markdownRoot in ResolveMarkdownRoots())
            {
                if (!Directory.Exists(markdownRoot))
                {
                    _logger.LogWarning("Markdown root '{Root}' does not exist. Skipping.", markdownRoot);
                    continue;
                }

                string[] sourceFiles = Directory.GetFiles(markdownRoot, $"{DefaultLanguage}.md", SearchOption.AllDirectories);

                await _signalRPublisher.PublishMessageAsync(
                    runId,
                    LocalizationMessageType.Progress,
                    ProcessStage.TranslateMarkdownFiles,
                    T("Scanning {sourceFileCount} source files in '{markdownRoot}'.", new { sourceFileCount = sourceFiles.Length, markdownRoot }));

                foreach (string sourceFile in sourceFiles)
                {
                    report.SourceFilesDetected++;
                    await ProcessSourceFileAsync(sourceFile, targetLanguages, storingReport, report, runId);
                }
            }

            await _signalRPublisher.PublishStageAsync(
                runId,
                ProcessStage.TranslateMarkdownFiles,
                report,
                LocalizationMessageType.StageCompleted,
                T("Markdown translations synchronised."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TranslateMarkdownFiles stage failed.");
            report.Errors.Add(CreateError("markdown", ErrorCode.TranslationFailed, ex.Message));

            await _signalRPublisher.PublishStageAsync(
                runId,
                ProcessStage.TranslateMarkdownFiles,
                report,
                LocalizationMessageType.StageFailed,
                ex.Message,
                isError: true);
            throw;
        }
    }

    private async Task ProcessSourceFileAsync(
        string sourceFile,
        List<string> targetLanguages,
        StoringReport storingReport,
        MarkdownTranslationsReport report,
        Guid runId)
    {
        string sourceContent = await File.ReadAllTextAsync(sourceFile);
        string sourceHash = ComputeHash(sourceContent);

        // Load metadata for partial translation tracking
        MarkdownTranslationMetadata? metadata = MarkdownTranslationMetadata.Load(sourceFile);
        bool sourceChanged = metadata == null || metadata.IsStale(sourceContent);

        if (!sourceChanged)
        {
            // Check if any language is missing or has untranslated blocks
            bool needsTranslation = targetLanguages.Any(lang => !metadata!.IsFullyTranslated(lang));
            if (!needsTranslation)
            {
                report.SkippedFiles++;
                return;
            }
        }

        report.SourceFilesChanged++;
        _logger.LogInformation("Processing Markdown file: {SourceFile} (changed={Changed})", sourceFile, sourceChanged);

        // Extract translatable blocks
        List<MarkdownTranslatableBlock> sourceBlocks = _markdownParserService.ExtractTranslatableBlocks(sourceContent);
        string sourceDisplayPath = GetDisplayPath(sourceFile);

        await _signalRPublisher.PublishMessageAsync(
            runId,
            LocalizationMessageType.Progress,
            ProcessStage.TranslateMarkdownFiles,
            T("File '{sourcePath}' has {blockCount} translatable blocks.", new { sourcePath = sourceDisplayPath, blockCount = sourceBlocks.Count }),
            new TranslationProgressUpdate
            {
                WorkItemId = $"markdown:plan:{sourceDisplayPath}",
                Stage = ProcessStage.TranslateMarkdownFiles,
                Scope = sourceDisplayPath,
                Unit = T("markdown blocks"),
                TotalItems = sourceBlocks.Count * targetLanguages.Count,
                IsPlan = true
            });

        // Process each target language independently
        var newMetadata = new MarkdownTranslationMetadata
        {
            SourceHash = sourceHash,
            LanguageBlockStatus = []
        };

        bool anyLanguageSucceeded = false;

        foreach (string targetLanguage in targetLanguages)
        {
            string displayLanguage = GetLocalizedLanguageName(targetLanguage);
            string targetFilePath = Path.Combine(
                Path.GetDirectoryName(sourceFile) ?? string.Empty,
                $"{targetLanguage}.md");

            // Check if we can skip this language
            if (!sourceChanged && metadata != null && metadata.IsFullyTranslated(targetLanguage) && File.Exists(targetFilePath))
            {
                _logger.LogDebug("Skipping '{TargetLanguage}' for '{SourceFile}' - already fully translated.", targetLanguage, sourceFile);

                await _signalRPublisher.PublishMessageAsync(
                    runId,
                    LocalizationMessageType.Progress,
                    ProcessStage.TranslateMarkdownFiles,
                    T("Skipping '{targetLanguage}' for '{sourcePath}' - already fully translated.", new { targetLanguage = displayLanguage, sourcePath = sourceDisplayPath }),
                    new TranslationProgressUpdate
                    {
                        WorkItemId = $"markdown:{sourceDisplayPath}:{targetLanguage}",
                        Stage = ProcessStage.TranslateMarkdownFiles,
                        Scope = sourceDisplayPath,
                        TargetLanguage = targetLanguage,
                        Unit = T("markdown blocks"),
                        TotalItems = sourceBlocks.Count,
                        SkippedItems = sourceBlocks.Count
                    });

                continue;
            }

            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.Progress,
                ProcessStage.TranslateMarkdownFiles,
                T("Translating '{sourcePath}' to '{targetLanguage}'.", new { sourcePath = sourceDisplayPath, targetLanguage = displayLanguage }));

            // Check if target language is supported
            if (!await IsLanguageSupportedAsync(targetLanguage))
            {
                _logger.LogWarning("Language '{TargetLanguage}' is not supported by the translation server. Skipping.", targetLanguage);
                report.Errors.Add(CreateError(sourceFile, ErrorCode.TranslationFailed, T("Language '{targetLanguage}' is not supported.", new { targetLanguage = displayLanguage })));

                await _signalRPublisher.PublishMessageAsync(
                    runId,
                    LocalizationMessageType.Progress,
                    ProcessStage.TranslateMarkdownFiles,
                    T("Language '{targetLanguage}' is not supported by the translation server. Skipping.", new { targetLanguage = displayLanguage }),
                    new TranslationProgressUpdate
                    {
                        WorkItemId = $"markdown:{sourceDisplayPath}:{targetLanguage}",
                        Stage = ProcessStage.TranslateMarkdownFiles,
                        Scope = sourceDisplayPath,
                        TargetLanguage = targetLanguage,
                        Unit = T("markdown blocks"),
                        TotalItems = sourceBlocks.Count,
                        FailedItems = sourceBlocks.Count
                    },
                    isError: true);

                continue;
            }

            // Translate block by block with retry
            var translatedBlocks = new List<MarkdownTranslatableBlock>();
            var blockStatus = new List<bool>();

            for (int i = 0; i < sourceBlocks.Count; i++)
            {
                var block = sourceBlocks[i];
                bool blockSucceeded = false;
                string translatedText = block.OriginalText; // fallback

                try
                {
                    var response = await _retryService.TranslateWithRetryAsync(
                        block.OriginalText,
                        DefaultLanguage,
                        targetLanguage);

                    if (response.Success && response.Data != null && !string.IsNullOrWhiteSpace(response.Data.TranslatedText))
                    {
                        translatedText = response.Data.TranslatedText.Trim();

                        // Validate inline tag structure
                        if (!HasMatchingInlineTagStructure(block.OriginalText, translatedText))
                        {
                            _logger.LogWarning(
                                "Inline tag structure mismatch for block {Index} in '{SourceFile}' -> '{TargetLanguage}'. Using original.",
                                i, sourceFile, targetLanguage);
                            translatedText = block.OriginalText;
                        }
                        else
                        {
                            blockSucceeded = true;
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Block {Index} translation failed for '{SourceFile}' -> '{TargetLanguage}': {Message}",
                            i, sourceFile, targetLanguage, response.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Exception translating block {Index} for '{SourceFile}' -> '{TargetLanguage}'.",
                        i, sourceFile, targetLanguage);
                }

                translatedBlocks.Add(new MarkdownTranslatableBlock
                {
                    Key = block.Key,
                    OriginalText = block.OriginalText,
                    TranslatedText = translatedText,
                    StartLine = block.StartLine,
                    EndLine = block.EndLine,
                    BlockType = block.BlockType,
                    Metadata = block.Metadata,
                    IsTranslated = blockSucceeded
                });

                blockStatus.Add(blockSucceeded);
            }

            newMetadata.LanguageBlockStatus[targetLanguage] = blockStatus;

            // Reconstruct Markdown
            try
            {
                string reconstructed = _markdownReconstructorService.Reconstruct(sourceContent, translatedBlocks);

                // Validate structure
                if (!HasMatchingMarkdownStructure(sourceContent, reconstructed))
                {
                    _logger.LogError(
                        "Markdown structure validation failed for '{SourceFile}' -> '{TargetLanguage}'. Skipping save.",
                        sourceFile, targetLanguage);
                    report.Errors.Add(CreateError(sourceFile, ErrorCode.TranslationFailed,
                        T("Structure validation failed for '{targetLanguage}'.", new { targetLanguage = displayLanguage })));

                    await _signalRPublisher.PublishMessageAsync(
                        runId,
                        LocalizationMessageType.Progress,
                        ProcessStage.TranslateMarkdownFiles,
                        T("Structure validation failed for '{targetLanguage}' in '{sourcePath}'.", new { targetLanguage = displayLanguage, sourcePath = sourceDisplayPath }),
                        new TranslationProgressUpdate
                        {
                            WorkItemId = $"markdown:{sourceDisplayPath}:{targetLanguage}",
                            Stage = ProcessStage.TranslateMarkdownFiles,
                            Scope = sourceDisplayPath,
                            TargetLanguage = targetLanguage,
                            Unit = T("markdown blocks"),
                            TotalItems = sourceBlocks.Count,
                            FailedItems = sourceBlocks.Count
                        },
                        isError: true);

                    continue;
                }

                // Save translated file
                string normalizedMarkdown = EnsureEndsWithSingleNewline(reconstructed);
                await File.WriteAllTextAsync(targetFilePath, normalizedMarkdown, Encoding.UTF8);
                storingReport.SavedMarkdownFiles++;
                report.SavedFiles++;
                anyLanguageSucceeded = true;

                int translatedCount = blockStatus.Count(b => b);
                await _signalRPublisher.PublishMessageAsync(
                    runId,
                    LocalizationMessageType.Progress,
                    ProcessStage.TranslateMarkdownFiles,
                    T("Saved '{targetLanguage}' translation for '{sourcePath}' ({translatedCount}/{blockCount} blocks translated).", new
                    {
                        targetLanguage = displayLanguage,
                        sourcePath = sourceDisplayPath,
                        translatedCount,
                        blockCount = sourceBlocks.Count
                    }),
                    new TranslationProgressUpdate
                    {
                        WorkItemId = $"markdown:{sourceDisplayPath}:{targetLanguage}",
                        Stage = ProcessStage.TranslateMarkdownFiles,
                        Scope = sourceDisplayPath,
                        TargetLanguage = targetLanguage,
                        Unit = T("markdown blocks"),
                        TotalItems = sourceBlocks.Count,
                        CompletedItems = translatedCount,
                        SkippedItems = sourceBlocks.Count - translatedCount,
                        SavedItems = 1
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconstruct or save Markdown for '{SourceFile}' -> '{TargetLanguage}'.", sourceFile, targetLanguage);
                report.Errors.Add(CreateError(sourceFile, ErrorCode.TranslationFailed, ex.Message));

                await _signalRPublisher.PublishMessageAsync(
                    runId,
                    LocalizationMessageType.Progress,
                    ProcessStage.TranslateMarkdownFiles,
                    T("Failed to save '{targetLanguage}' translation for '{sourcePath}': {message}", new
                    {
                        targetLanguage = displayLanguage,
                        sourcePath = sourceDisplayPath,
                        message = ex.Message
                    }),
                    new TranslationProgressUpdate
                    {
                        WorkItemId = $"markdown:{sourceDisplayPath}:{targetLanguage}",
                        Stage = ProcessStage.TranslateMarkdownFiles,
                        Scope = sourceDisplayPath,
                        TargetLanguage = targetLanguage,
                        Unit = T("markdown blocks"),
                        TotalItems = sourceBlocks.Count,
                        FailedItems = sourceBlocks.Count
                    },
                    isError: true);
            }
        }

        // Save metadata
        if (anyLanguageSucceeded)
        {
            newMetadata.Save(sourceFile);
            storingReport.SavedHashFiles++;

            // Also save legacy hash for compatibility
            await WriteStoredHashAsync(sourceFile, sourceHash, storingReport, report);
        }
    }

    private async Task<bool> IsLanguageSupportedAsync(string languageCode)
    {
        // This is a simple check - in production we might want to cache this
        try
        {
            var response = await _retryService.TranslateWithRetryAsync("hello", "en", languageCode);
            return response.Success;
        }
        catch
        {
            return false;
        }
    }

    private IEnumerable<string> ResolveMarkdownRoots()
    {
        foreach (string configuredRoot in MarkdownRoots)
        {
            if (string.IsNullOrWhiteSpace(configuredRoot))
            {
                continue;
            }

            string trimmed = configuredRoot.Trim();
            string relative = trimmed.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, '/').Replace('/', Path.DirectorySeparatorChar);
            yield return Path.Combine(_hostEnvironment.ContentRootPath, relative);
        }
    }

    private string GetDisplayPath(string sourceFilePath)
    {
        string relativePath = Path.GetRelativePath(_hostEnvironment.ContentRootPath, sourceFilePath);
        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

    private static string ComputeHash(string content)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }

    private async Task WriteStoredHashAsync(string sourceFilePath, string hash, StoringReport storingReport, MarkdownTranslationsReport report)
    {
        var entry = new StoredHashEntry
        {
            SourcePath = sourceFilePath,
            Hash = hash,
            UpdatedAtUtc = DateTime.UtcNow
        };

        string json = System.Text.Json.JsonSerializer.Serialize(entry, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        string primaryPath = Path.Combine(Path.GetDirectoryName(sourceFilePath) ?? string.Empty, $"{Path.GetFileName(sourceFilePath)}.hash.json");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(primaryPath) ?? _hostEnvironment.ContentRootPath);
            await File.WriteAllTextAsync(primaryPath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary hash write failed for {SourceFilePath}; using temp fallback.", sourceFilePath);
            Directory.CreateDirectory(TempHashDirectory);
            await File.WriteAllTextAsync(GetFallbackHashPath(sourceFilePath), json, Encoding.UTF8);
            storingReport.TempFallbackWrites++;
            report.TempFallbackWrites++;
        }
    }

    private string GetFallbackHashPath(string sourceFilePath)
    {
        string relativePath = Path.GetRelativePath(_hostEnvironment.ContentRootPath, sourceFilePath);
        StringBuilder builder = new();

        foreach (char character in relativePath)
        {
            if (character is '/' or '\\')
            {
                builder.Append('.');
            }
            else if (char.IsWhiteSpace(character))
            {
                builder.Append('_');
            }
            else if (Path.GetInvalidFileNameChars().Contains(character))
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

    private static bool HasMatchingMarkdownStructure(string sourceContent, string translatedContent)
    {
        if (string.IsNullOrWhiteSpace(sourceContent) || string.IsNullOrWhiteSpace(translatedContent))
        {
            return false;
        }

        // Compare heading counts
        if (CountPattern(sourceContent, @"(?m)^\s{0,3}#{1,6}\s") != CountPattern(translatedContent, @"(?m)^\s{0,3}#{1,6}\s"))
            return false;

        // Compare list item counts
        if (CountPattern(sourceContent, @"(?m)^\s*(?:[-+*]|\d+\.)\s+") != CountPattern(translatedContent, @"(?m)^\s*(?:[-+*]|\d+\.)\s+"))
            return false;

        // Compare code block counts
        if (CountPattern(sourceContent, @"(?m)^\s*```") != CountPattern(translatedContent, @"(?m)^\s*```"))
            return false;

        // Compare blockquote counts
        if (CountPattern(sourceContent, @"(?m)^\s*>") != CountPattern(translatedContent, @"(?m)^\s*>"))
            return false;

        return true;
    }

    private static bool HasMatchingInlineTagStructure(string originalText, string translatedText)
    {
        if (string.IsNullOrEmpty(originalText) || string.IsNullOrEmpty(translatedText))
        {
            return true;
        }

        string[] originalTags = ExtractInlineTags(originalText);
        string[] translatedTags = ExtractInlineTags(translatedText);

        return originalTags.Length == translatedTags.Length;
    }

    private static string[] ExtractInlineTags(string text)
    {
        List<string> tags = [];

        // HTML tags
        foreach (Match match in Regex.Matches(text, "</?[a-zA-Z][^>]*>"))
        {
            tags.Add(match.Value);
        }

        // Markdown formatting
        foreach (string token in new[] { "**", "*", "__", "_", "~~", "`" })
        {
            int index = 0;
            while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                tags.Add(token);
                index += token.Length;
            }
        }

        // Links
        int searchStart = 0;
        while (searchStart < text.Length)
        {
            int openBracket = text.IndexOf('[', searchStart);
            if (openBracket < 0) break;

            int closeBracket = text.IndexOf(']', openBracket + 1);
            if (closeBracket < 0) break;

            int openParen = text.IndexOf('(', closeBracket + 1);
            if (openParen == closeBracket + 1)
            {
                int closeParen = text.IndexOf(')', openParen + 1);
                if (closeParen > openParen)
                {
                    bool isImage = openBracket > 0 && text[openBracket - 1] == '!';
                    tags.Add(isImage ? "![]()" : "[]()");
                    searchStart = closeParen + 1;
                    continue;
                }
            }

            searchStart = closeBracket + 1;
        }

        return [.. tags];
    }

    private static int CountPattern(string content, string pattern)
    {
        return Regex.Matches(content, pattern).Count;
    }

    private static string EnsureEndsWithSingleNewline(string content)
    {
        string normalized = content.Replace("\r\n", "\n").TrimEnd('\n');
        return normalized + "\n";
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

    private sealed class StoredHashEntry
    {
        public string SourcePath { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public DateTime UpdatedAtUtc { get; set; }
    }
}
