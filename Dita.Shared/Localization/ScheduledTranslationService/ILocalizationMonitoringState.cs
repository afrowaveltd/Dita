using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Stores the latest automatic localization events and aggregate dashboard state.
/// </summary>
public interface ILocalizationMonitoringState
{
    /// <summary>
    /// Adds a localization hub message to the current monitoring state.
    /// </summary>
    /// <param name="message">Message to record.</param>
    void RecordMessage(LocalizationHubMessage message);

    /// <summary>
    /// Creates a point-in-time dashboard snapshot from recorded messages.
    /// </summary>
    /// <returns>Current dashboard snapshot.</returns>
    LocalizationHubSnapshot GetSnapshot();
}
