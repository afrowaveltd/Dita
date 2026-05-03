using Microsoft.AspNetCore.SignalR;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Defines the contract for the main automatic translation pipeline orchestrator.
/// Coordinates server checks, country name translations, JSON dictionary updates, and Markdown document translations.
/// </summary>
public interface IBackendTranslationService
{
    /// <summary>
    /// Executes a full automatic translation pipeline run.
    /// This operation is idempotent: if a run is already in progress, the call returns immediately.
    /// </summary>
    Task RunAsync();
}
