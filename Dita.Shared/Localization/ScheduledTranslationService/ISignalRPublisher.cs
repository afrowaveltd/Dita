using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Abstraction for publishing real-time localization pipeline messages via SignalR.
/// </summary>
public interface ISignalRPublisher
{
    /// <summary>
    /// Publishes a pipeline stage report to all connected SignalR clients.
    /// </summary>
    /// <typeparam name="T">The type of the stage report payload.</typeparam>
    /// <param name="runId">The current pipeline run identifier.</param>
    /// <param name="stage">The pipeline stage being reported.</param>
    /// <param name="data">The stage-specific report data.</param>
    /// <param name="type">The message type (started, completed, failed, etc.).</param>
    /// <param name="message">Human-readable description of the event.</param>
    /// <param name="isError">Whether the message represents an error condition.</param>
    Task PublishStageAsync<T>(
        Guid runId,
        ProcessStage stage,
        T data,
        LocalizationMessageType type,
        string message,
        bool isError = false)
        where T : class;

    /// <summary>
    /// Publishes a simple pipeline message to all connected SignalR clients.
    /// </summary>
    /// <param name="runId">The current pipeline run identifier.</param>
    /// <param name="type">The message type.</param>
    /// <param name="stage">The pipeline stage associated with the message.</param>
    /// <param name="message">Human-readable description.</param>
    /// <param name="data">Optional payload object.</param>
    /// <param name="isError">Whether the message represents an error condition.</param>
    Task PublishMessageAsync(
        Guid runId,
        LocalizationMessageType type,
        ProcessStage stage,
        string message,
        object? data = null,
        bool isError = false);
}
