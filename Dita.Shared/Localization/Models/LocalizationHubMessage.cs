using Dita.Shared.Localization.Enums;

namespace Dita.Shared.Localization.Models;

/// <summary>
/// Generic SignalR message envelope for automatic localization updates.
/// </summary>
public class LocalizationHubMessage
{
    /// <summary>
    /// Correlation identifier for the current pipeline run.
    /// </summary>
    public Guid RunId { get; set; }

    /// <summary>
    /// Monotonic sequence number within a pipeline run.
    /// </summary>
    public long Sequence { get; set; }

    /// <summary>
    /// Semantic type of the message.
    /// </summary>
    public LocalizationMessageType Type { get; set; } = LocalizationMessageType.Progress;

    /// <summary>
    /// Pipeline stage associated with the message.
    /// </summary>
    public ProcessStage Stage { get; set; } = ProcessStage.Iddle;

    /// <summary>
    /// UTC time when the message was created.
    /// </summary>
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether this message represents an error condition.
    /// </summary>
    public bool IsError { get; set; }

    /// <summary>
    /// Human-readable message summary.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional stage-specific payload.
    /// </summary>
    public object? Data { get; set; }
}