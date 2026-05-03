using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Defines a service that synchronizes JSON localization dictionaries across target languages.
/// </summary>
public interface ILocalizationTranslationService
{
    /// <summary>
    /// Synchronizes JSON localization dictionaries by detecting added/removed keys in the default dictionary
    /// and translating them into all target languages.
    /// Saves each target language dictionary immediately after translation completes.
    /// </summary>
    /// <param name="targetLanguages">The list of language codes to synchronize.</param>
    /// <param name="storingReport">The accumulating pipeline report to append results to.</param>
    /// <param name="runId">The current pipeline run identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync(List<string> targetLanguages, StoringReport storingReport, Guid runId);
}
