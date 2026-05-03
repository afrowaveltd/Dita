using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Defines a service that synchronizes country names into localization dictionaries.
/// </summary>
public interface ICountriesTranslationService
{
    /// <summary>
    /// Synchronizes country names from the canonical source file into localization dictionaries for all target languages.
    /// </summary>
    /// <param name="targetLanguages">The list of language codes to translate country names into.</param>
    /// <param name="storingReport">The accumulating pipeline report to append results to.</param>
    /// <param name="runId">The current pipeline run identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync(List<string> targetLanguages, StoringReport storingReport, Guid runId);
}
