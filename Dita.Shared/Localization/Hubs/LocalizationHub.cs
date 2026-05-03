using Microsoft.AspNetCore.SignalR;
using Dita.Shared.Localization.ScheduledTranslationService;

namespace Dita.Shared.Localization.Hubs;

/// <summary>
/// SignalR hub used to push real-time localization notifications to connected clients.
/// </summary>
/// <remarks>
/// Clients can subscribe to this hub to receive live updates when translation or localization data changes,
/// such as when new locale files are generated or a translation job completes.
/// </remarks>
public class LocalizationHub(ILocalizationMonitoringState? monitoringState = null) : Hub<ILocalizationHubClient>
{
    private readonly ILocalizationMonitoringState? _monitoringState = monitoringState;

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        if (_monitoringState is not null)
        {
            await Clients.Caller.ReceiveLocalizationSnapshot(_monitoringState.GetSnapshot());
        }

        await base.OnConnectedAsync();
    }
}
