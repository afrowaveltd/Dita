using Dita.Shared.Localization.Models;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Orchestrates Markdown document translation by coordinating parsing, translation, and reconstruction.
/// Translates all blocks for each target language sequentially while preserving document structure.
/// </summary>
/// <param name="parserService">Service for extracting translatable blocks from Markdown.</param>
/// <param name="reconstructorService">Service for rebuilding Markdown from translated blocks.</param>
/// <param name="translateService">Service for translating individual text blocks.</param>
/// <param name="settings">Automatic translation settings containing default language and ignored languages.</param>
/// <param name="logger">Logger for diagnostic output.</param>
public class MarkdownTranslationService(
   IMarkdownParserService parserService,
   IMarkdownReconstructorService reconstructorService,
   ILibreTranslateService translateService,
   AutomaticTranslationSettings settings,
   ILogger<MarkdownTranslationService> logger) : IMarkdownTranslationService
{
   private readonly IMarkdownParserService _parserService = parserService;
   private readonly IMarkdownReconstructorService _reconstructorService = reconstructorService;
   private readonly ILibreTranslateService _translateService = translateService;
   private readonly AutomaticTranslationSettings _settings = settings;
   private readonly ILogger<MarkdownTranslationService> _logger = logger;

   /// <summary>
   /// Translates a Markdown document into all specified target languages.
   /// Returns a dictionary mapping language codes to fully translated Markdown content.
   /// </summary>
   /// <param name="markdownContent">The original Markdown content to translate.</param>
   /// <param name="sourceLanguage">The source language code (e.g., "en").</param>
   /// <param name="targetLanguages">The list of target language codes to translate into.</param>
   /// <param name="cancellationToken">Cancellation token for async operation.</param>
   /// <returns>A dictionary mapping language codes to translated Markdown content.</returns>
   public async Task<Dictionary<string, string>> TranslateMarkdownAsync(
      string markdownContent,
      string sourceLanguage,
      List<string> targetLanguages,
      CancellationToken cancellationToken = default)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(markdownContent);
      ArgumentException.ThrowIfNullOrWhiteSpace(sourceLanguage);
      ArgumentNullException.ThrowIfNull(targetLanguages);

      if (targetLanguages.Count == 0)
      {
         _logger.LogWarning("No target languages specified for Markdown translation.");
         return [];
      }

      _logger.LogInformation(
         "Starting Markdown translation from {Source} to {Count} target languages.",
         sourceLanguage, targetLanguages.Count);

      // Step 1: Parse and extract translatable blocks
      List<MarkdownTranslatableBlock> originalBlocks = _parserService.ExtractTranslatableBlocks(markdownContent);

      if (originalBlocks.Count == 0)
      {
         _logger.LogWarning("No translatable blocks found in Markdown content.");
         return targetLanguages.ToDictionary(lang => lang, _ => markdownContent);
      }

      _logger.LogDebug("Extracted {Count} translatable blocks from Markdown.", originalBlocks.Count);

      // Step 2: Translate all blocks for each target language sequentially to avoid overloading LibreTranslate.
      Dictionary<string, List<MarkdownTranslatableBlock>> translatedBlocksByLanguage = [];

      foreach (string targetLanguage in targetLanguages)
      {
         cancellationToken.ThrowIfCancellationRequested();

         List<MarkdownTranslatableBlock> translatedBlocks = await TranslateBlocksAsync(
            originalBlocks,
            sourceLanguage,
            targetLanguage,
            cancellationToken);

         translatedBlocksByLanguage[targetLanguage] = translatedBlocks;
      }

      // Step 3: Reconstruct Markdown for each language
      Dictionary<string, string> results = [];

      foreach ((string language, List<MarkdownTranslatableBlock> blocks) in translatedBlocksByLanguage)
      {
         try
         {
            string reconstructed = _reconstructorService.Reconstruct(markdownContent, blocks);
            results[language] = reconstructed;

            _logger.LogDebug("Successfully reconstructed Markdown for language {Language}.", language);
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Failed to reconstruct Markdown for language {Language}.", language);
            results[language] = markdownContent; // Fallback to original
         }
      }

      _logger.LogInformation(
         "Markdown translation completed: {SuccessCount}/{TotalCount} languages successful.",
         results.Count, targetLanguages.Count);

      return results;
   }

   private async Task<List<MarkdownTranslatableBlock>> TranslateBlocksAsync(
      List<MarkdownTranslatableBlock> originalBlocks,
      string sourceLanguage,
      string targetLanguage,
      CancellationToken cancellationToken)
   {
      List<MarkdownTranslatableBlock> translatedBlocks = [];

      foreach (MarkdownTranslatableBlock block in originalBlocks)
      {
         cancellationToken.ThrowIfCancellationRequested();

         try
         {
            var result = await _translateService.TranslateTextAsync(
               block.OriginalText,
               sourceLanguage,
               targetLanguage);

            MarkdownTranslatableBlock translatedBlock = new()
            {
               Key = block.Key,
               OriginalText = block.OriginalText,
               TranslatedText = result.Success && result.Data != null ? result.Data.TranslatedText : block.OriginalText,
               StartLine = block.StartLine,
               EndLine = block.EndLine,
               BlockType = block.BlockType,
               Metadata = block.Metadata,
               IsTranslated = result.Success
            };

            translatedBlocks.Add(translatedBlock);

            if (!result.Success)
            {
               _logger.LogWarning(
                  "Translation failed for block {Key} (line {Line}) to {Target}: {Message}",
                  block.Key, block.StartLine, targetLanguage, result.Message);
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Exception translating block {Key} to {Target}.", block.Key, targetLanguage);

            // Add untranslated block as fallback
            translatedBlocks.Add(new MarkdownTranslatableBlock
            {
               Key = block.Key,
               OriginalText = block.OriginalText,
               TranslatedText = block.OriginalText,
               StartLine = block.StartLine,
               EndLine = block.EndLine,
               BlockType = block.BlockType,
               Metadata = block.Metadata,
               IsTranslated = false
            });
         }
      }

      return translatedBlocks;
   }

   /// <summary>
   /// Translates a Markdown document into all supported languages using default settings.
   /// Automatically determines source language from <see cref="AutomaticTranslationSettings.DefaultLanguage"/>
   /// and target languages from LibreTranslate available languages, excluding the default and ignored languages.
   /// </summary>
   /// <param name="markdownContent">The original Markdown content to translate.</param>
   /// <param name="cancellationToken">Cancellation token for async operation.</param>
   /// <returns>A dictionary mapping language codes to translated Markdown content.</returns>
   public async Task<Dictionary<string, string>> TranslateMarkdownAsync(
      string markdownContent,
      CancellationToken cancellationToken = default)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(markdownContent);

      string sourceLanguage = _settings.DefaultLanguage;

      _logger.LogInformation(
         "Starting automatic Markdown translation from default language {Source}.",
         sourceLanguage);

      // Get available languages from LibreTranslate
      var languagesResponse = await _translateService.GetAvailableLanguagesAsync();

      if (!languagesResponse.Success || languagesResponse.Data == null || languagesResponse.Data.Length == 0)
      {
         _logger.LogError(
            "Failed to retrieve available languages from LibreTranslate: {Message}",
            languagesResponse.Message);
         return [];
      }

      // Build target language list: all available languages minus default and ignored
      List<string> targetLanguages = languagesResponse.Data
         .Where(lang => !string.Equals(lang, sourceLanguage, StringComparison.OrdinalIgnoreCase))
         .Where(lang => !_settings.IgnoredLanguages.Contains(lang, StringComparer.OrdinalIgnoreCase))
         .ToList();

      if (targetLanguages.Count == 0)
      {
         _logger.LogWarning(
            "No target languages available after excluding default ({Default}) and ignored languages.",
            sourceLanguage);
         return [];
      }

      _logger.LogInformation(
         "Automatically translating Markdown to {Count} target languages: {Languages}",
         targetLanguages.Count, string.Join(", ", targetLanguages));

      // Call the main translation method
      return await TranslateMarkdownAsync(markdownContent, sourceLanguage, targetLanguages, cancellationToken);
   }
}
