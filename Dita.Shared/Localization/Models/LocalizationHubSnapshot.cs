using Dita.Shared.Localization.Enums;

namespace Dita.Shared.Localization.Models;

#pragma warning disable CS1591

/// <summary>
/// Current server-side snapshot for the live localization dashboard.
/// </summary>
public class LocalizationHubSnapshot
{
    public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;
    public LocalizationDashboardSummary Summary { get; set; } = new();
    public List<LocalizationStageSnapshot> Stages { get; set; } = [];
    public List<LocalizationHubMessage> RecentMessages { get; set; } = [];
}

/// <summary>
/// Aggregate counters displayed by the live localization dashboard.
/// </summary>
public class LocalizationDashboardSummary
{
    public Guid? ActiveRunId { get; set; }
    public bool IsRunning { get; set; }
    public string CurrentStage { get; set; } = ProcessStage.Iddle.ToString();
    public string LastMessage { get; set; } = string.Empty;
    public DateTime? RunStartedUtc { get; set; }
    public DateTime? RunCompletedUtc { get; set; }
    public DateTime? LastEventUtc { get; set; }
    public long LastSequence { get; set; }
    public int MessagesReceived { get; set; }
    public int TotalTranslations { get; set; }
    public int CompletedTranslations { get; set; }
    public int FailedTranslations { get; set; }
    public int SkippedTranslations { get; set; }
    public int ProgressPercent { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int SavedDictionaryFiles { get; set; }
    public int SavedMarkdownFiles { get; set; }
    public int SavedHashFiles { get; set; }
    public int TempFallbackWrites { get; set; }
    public int AvailableLanguageCount { get; set; }
    public string DefaultLanguage { get; set; } = string.Empty;
    public int TranslationServerLatencyMs { get; set; }
    public bool TranslationServerReady { get; set; }
}

/// <summary>
/// Per-stage aggregate counters for the live localization dashboard.
/// </summary>
public class LocalizationStageSnapshot
{
    public ProcessStage Stage { get; set; }
    public string Status { get; set; } = "Waiting";
    public LocalizationMessageType LastType { get; set; } = LocalizationMessageType.Progress;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime? StartedUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public int FailedItems { get; set; }
    public int SkippedItems { get; set; }
    public int SavedItems { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int ProgressPercent { get; set; }
}

/// <summary>
/// Structured progress payload used by translation stages to update dashboard totals.
/// </summary>
public class TranslationProgressUpdate
{
    public string WorkItemId { get; set; } = Guid.NewGuid().ToString("N");
    public ProcessStage Stage { get; set; } = ProcessStage.Iddle;
    public string Scope { get; set; } = string.Empty;
    public string? TargetLanguage { get; set; }
    public string Unit { get; set; } = "items";
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public int FailedItems { get; set; }
    public int SkippedItems { get; set; }
    public int SavedItems { get; set; }
    public bool IsPlan { get; set; }
}
