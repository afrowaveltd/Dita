using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Hubs;

/// <summary>
/// Client contract for real-time localization events delivered by <see cref="LocalizationHub"/>.
/// </summary>
public interface ILocalizationHubClient
{
    /// <summary>
    /// Receives the current server-side localization dashboard state.
    /// </summary>
    /// <param name="snapshot">Snapshot with aggregate counters and recent messages.</param>
    Task ReceiveLocalizationSnapshot(LocalizationHubSnapshot snapshot);

    /// <summary>
    /// Receives a structured localization pipeline message.
    /// </summary>
    /// <param name="message">Message envelope with stage and payload details.</param>
    Task ReceiveLocalizationMessage(LocalizationHubMessage message);
}
