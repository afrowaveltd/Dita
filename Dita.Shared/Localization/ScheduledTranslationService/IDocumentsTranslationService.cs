using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Defines a service that synchronizes Markdown documentation translations.
/// Translates source Markdown files language-by-language and saves each target file immediately.
/// Supports partial translation tracking via per-file metadata.
/// </summary>
public interface IDocumentsTranslationService
{
    /// <summary>
    /// Synchronizes Markdown translations by detecting changed source files and translating them
    /// into all required target languages. Each language is saved immediately after translation.
    /// </summary>
    /// <param name="targetLanguages">The list of language codes to translate into.</param>
    /// <param name="storingReport">The accumulating pipeline report to append results to.</param>
    /// <param name="runId">The current pipeline run identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync(List<string> targetLanguages, StoringReport storingReport, Guid runId);
}
