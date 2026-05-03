using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Hubs;
using Dita.Shared.Localization.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Publishes real-time localization pipeline messages to connected SignalR clients.
/// </summary>
public class SignalRPublisher(
    IHubContext<LocalizationHub, ILocalizationHubClient> hubContext,
    ILocalizationMonitoringState? monitoringState = null) : ISignalRPublisher
{
    private readonly IHubContext<LocalizationHub, ILocalizationHubClient> _hubContext = hubContext;
    private readonly ILocalizationMonitoringState? _monitoringState = monitoringState;
    private long _messageSequence;

    /// <inheritdoc />
    public async Task PublishStageAsync<T>(
        Guid runId,
        ProcessStage stage,
        T data,
        LocalizationMessageType type,
        string message,
        bool isError = false)
        where T : class
    {
        var hubMessage = new LocalizationHubMessage
        {
            RunId = runId,
            Sequence = Interlocked.Increment(ref _messageSequence),
            Type = type,
            Stage = stage,
            TimestampUtc = DateTime.UtcNow,
            IsError = isError,
            Message = message,
            Data = new StageReport<T>
            {
                ReportedStage = stage,
                StageData = data,
                StageStartTime = DateTime.UtcNow,
                StageEndTime = type is LocalizationMessageType.StageCompleted or LocalizationMessageType.StageFailed ? DateTime.UtcNow : null
            }
        };

        _monitoringState?.RecordMessage(hubMessage);
        await _hubContext.Clients.All.ReceiveLocalizationMessage(hubMessage);
        await PublishSnapshotAsync();
    }

    /// <inheritdoc />
    public async Task PublishMessageAsync(
        Guid runId,
        LocalizationMessageType type,
        ProcessStage stage,
        string message,
        object? data = null,
        bool isError = false)
    {
        var hubMessage = new LocalizationHubMessage
        {
            RunId = runId,
            Sequence = Interlocked.Increment(ref _messageSequence),
            Type = type,
            Stage = stage,
            TimestampUtc = DateTime.UtcNow,
            IsError = isError,
            Message = message,
            Data = data
        };

        _monitoringState?.RecordMessage(hubMessage);
        await _hubContext.Clients.All.ReceiveLocalizationMessage(hubMessage);
        await PublishSnapshotAsync();
    }

    private async Task PublishSnapshotAsync()
    {
        if (_monitoringState is null)
        {
            return;
        }

        await _hubContext.Clients.All.ReceiveLocalizationSnapshot(_monitoringState.GetSnapshot());
    }
}
