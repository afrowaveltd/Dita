using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Orchestrates the translation of Markdown documents across multiple target languages.
/// Integrates parsing, translation queueing, and reconstruction into a single workflow.
/// </summary>
public interface IMarkdownTranslationService
{
   /// <summary>
   /// Translates a Markdown document into all supported target languages.
   /// </summary>
   /// <param name="markdownContent">The original Markdown content to translate.</param>
   /// <param name="sourceLanguage">The source language code (e.g., "en").</param>
   /// <param name="targetLanguages">The list of target language codes to translate into.</param>
   /// <param name="cancellationToken">Cancellation token for async operation.</param>
   /// <returns>A dictionary mapping language codes to translated Markdown content.</returns>
   Task<Dictionary<string, string>> TranslateMarkdownAsync(
      string markdownContent,
      string sourceLanguage,
      List<string> targetLanguages,
      CancellationToken cancellationToken = default);

   /// <summary>
   /// Translates a Markdown document into all supported languages using default settings.
   /// Automatically determines source language from <see cref="AutomaticTranslationSettings.DefaultLanguage"/>
   /// and target languages from LibreTranslate available languages, excluding the default and ignored languages.
   /// </summary>
   /// <param name="markdownContent">The original Markdown content to translate.</param>
   /// <param name="cancellationToken">Cancellation token for async operation.</param>
   /// <returns>A dictionary mapping language codes to translated Markdown content.</returns>
   Task<Dictionary<string, string>> TranslateMarkdownAsync(
      string markdownContent,
      CancellationToken cancellationToken = default);
}
