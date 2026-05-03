using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Models;
using System.Reflection;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// In-memory state store for the live translation dashboard.
/// </summary>
public class LocalizationMonitoringState : ILocalizationMonitoringState
{
    private const int MaxRecentMessages = 250;
    private readonly object _syncRoot = new();
    private readonly List<LocalizationHubMessage> _recentMessages = [];
    private readonly Dictionary<ProcessStage, MutableStageState> _stages = CreateStageMap();
    private readonly Dictionary<string, TranslationProgressUpdate> _progressUpdates = new(StringComparer.Ordinal);
    private LocalizationDashboardSummary _summary = new();

    /// <inheritdoc />
    public void RecordMessage(LocalizationHubMessage message)
    {
        lock (_syncRoot)
        {
            if (StartsNewRun(message))
            {
                ResetForRun(message);
            }

            _summary.ActiveRunId ??= message.RunId;
            _summary.LastSequence = Math.Max(_summary.LastSequence, message.Sequence);
            _summary.MessagesReceived++;
            _summary.LastEventUtc = message.TimestampUtc;
            _summary.LastMessage = message.Message;
            _summary.CurrentStage = message.Stage.ToString();

            if (message.IsError || message.Type is LocalizationMessageType.StageFailed or LocalizationMessageType.PipelineFailed)
            {
                _summary.ErrorCount++;
            }

            if (message.Type == LocalizationMessageType.Warning)
            {
                _summary.WarningCount++;
            }

            UpdateRunState(message);
            UpdateStageState(message);
            ApplyPayload(message.Data, message.Stage);
            AddRecentMessage(message);
            RebuildProgressCounters();
        }
    }

    /// <inheritdoc />
    public LocalizationHubSnapshot GetSnapshot()
    {
        lock (_syncRoot)
        {
            return new LocalizationHubSnapshot
            {
                SnapshotUtc = DateTime.UtcNow,
                Summary = CopySummary(_summary),
                Stages = _stages.Values
                    .OrderBy(stage => stage.Stage)
                    .Select(stage => stage.ToSnapshot())
                    .ToList(),
                RecentMessages = _recentMessages
                    .OrderByDescending(message => message.Sequence)
                    .ToList()
            };
        }
    }

    private bool StartsNewRun(LocalizationHubMessage message)
        => message.Type == LocalizationMessageType.StageStarted
            && message.Stage == ProcessStage.CheckServers
            && _summary.ActiveRunId != message.RunId;

    private void ResetForRun(LocalizationHubMessage message)
    {
        _recentMessages.Clear();
        _progressUpdates.Clear();
        _stages.Clear();

        foreach (var stage in CreateStageMap())
        {
            _stages[stage.Key] = stage.Value;
        }

        _summary = new LocalizationDashboardSummary
        {
            ActiveRunId = message.RunId,
            IsRunning = true,
            CurrentStage = message.Stage.ToString(),
            RunStartedUtc = message.TimestampUtc
        };
    }

    private void UpdateRunState(LocalizationHubMessage message)
    {
        if (message.Type == LocalizationMessageType.StageStarted && _summary.RunStartedUtc is null)
        {
            _summary.RunStartedUtc = message.TimestampUtc;
        }

        if (message.Type == LocalizationMessageType.PipelineCompleted)
        {
            _summary.IsRunning = false;
            _summary.RunCompletedUtc = message.TimestampUtc;
        }
        else if (message.Type == LocalizationMessageType.PipelineFailed)
        {
            _summary.IsRunning = false;
            _summary.RunCompletedUtc = message.TimestampUtc;
        }
        else if (message.Type is LocalizationMessageType.StageStarted or LocalizationMessageType.Progress)
        {
            _summary.IsRunning = true;
        }
    }

    private void UpdateStageState(LocalizationHubMessage message)
    {
        if (!_stages.TryGetValue(message.Stage, out MutableStageState? stage))
        {
            stage = new MutableStageState(message.Stage);
            _stages[message.Stage] = stage;
        }

        stage.LastType = message.Type;
        stage.LastMessage = message.Message;

        if (message.IsError || message.Type is LocalizationMessageType.StageFailed or LocalizationMessageType.PipelineFailed)
        {
            stage.ErrorCount++;
        }

        if (message.Type == LocalizationMessageType.Warning)
        {
            stage.WarningCount++;
        }

        switch (message.Type)
        {
            case LocalizationMessageType.StageStarted:
                stage.Status = "Running";
                stage.StartedUtc ??= message.TimestampUtc;
                stage.CompletedUtc = null;
                break;
            case LocalizationMessageType.StageCompleted:
                stage.Status = "Completed";
                stage.CompletedUtc = message.TimestampUtc;
                break;
            case LocalizationMessageType.StageFailed:
            case LocalizationMessageType.PipelineFailed:
                stage.Status = "Failed";
                stage.CompletedUtc = message.TimestampUtc;
                break;
            case LocalizationMessageType.Progress:
            case LocalizationMessageType.Warning:
                if (stage.Status is "Waiting" or "Completed")
                {
                    stage.Status = "Running";
                }
                break;
            case LocalizationMessageType.PipelineCompleted:
                if (stage.Status != "Failed")
                {
                    stage.Status = "Completed";
                    stage.CompletedUtc ??= message.TimestampUtc;
                }
                break;
        }
    }

    private void ApplyPayload(object? data, ProcessStage fallbackStage)
    {
        object? payload = UnwrapStageData(data);

        switch (payload)
        {
            case null:
                return;
            case TranslationProgressUpdate update:
                if (update.Stage == ProcessStage.Iddle)
                {
                    update.Stage = fallbackStage;
                }

                _progressUpdates[update.WorkItemId] = update;
                return;
            case CheckingReport checkingReport:
                _summary.AvailableLanguageCount = checkingReport.AvailableLanguages.Length;
                _summary.DefaultLanguage = checkingReport.DefaultLanguage;
                _summary.TranslationServerLatencyMs = checkingReport.ServerLatencyMs;
                _summary.TranslationServerReady = checkingReport.TranslationServerReady;
                return;
            case StoringReport storingReport:
                _summary.SavedDictionaryFiles = Math.Max(_summary.SavedDictionaryFiles, storingReport.SavedDictionaryFiles);
                _summary.SavedMarkdownFiles = Math.Max(_summary.SavedMarkdownFiles, storingReport.SavedMarkdownFiles);
                _summary.SavedHashFiles = Math.Max(_summary.SavedHashFiles, storingReport.SavedHashFiles);
                _summary.TempFallbackWrites = Math.Max(_summary.TempFallbackWrites, storingReport.TempFallbackWrites);
                _summary.ErrorCount = Math.Max(_summary.ErrorCount, storingReport.Errors.Count);
                return;
            case TranslationsReport translationsReport:
                ApplyReportTotals(fallbackStage, translationsReport.ToTranslateCount, translationsReport.TranslatedCount, translationsReport.Errors?.Count ?? translationsReport.ErrorsCount, translationsReport.SkippedCount);
                return;
            case MarkdownTranslationsReport markdownReport:
                ApplyReportTotals(fallbackStage, markdownReport.SourceFilesChanged, markdownReport.SavedFiles, markdownReport.Errors.Count, markdownReport.SkippedFiles);
                return;
        }
    }

    private void ApplyReportTotals(ProcessStage stageKey, int totalItems, int completedItems, int failedItems, int skippedItems)
    {
        if (!_stages.TryGetValue(stageKey, out MutableStageState? stage))
        {
            return;
        }

        stage.ReportTotalItems = Math.Max(stage.ReportTotalItems, totalItems);
        stage.ReportCompletedItems = Math.Max(stage.ReportCompletedItems, completedItems);
        stage.ReportFailedItems = Math.Max(stage.ReportFailedItems, failedItems);
        stage.ReportSkippedItems = Math.Max(stage.ReportSkippedItems, skippedItems);
    }

    private static object? UnwrapStageData(object? data)
    {
        if (data is null)
        {
            return null;
        }

        Type dataType = data.GetType();
        if (!dataType.IsGenericType || dataType.GetGenericTypeDefinition() != typeof(StageReport<>))
        {
            return data;
        }

        PropertyInfo? stageDataProperty = dataType.GetProperty(nameof(StageReport<object>.StageData));
        return stageDataProperty?.GetValue(data);
    }

    private void AddRecentMessage(LocalizationHubMessage message)
    {
        _recentMessages.Add(message);

        if (_recentMessages.Count <= MaxRecentMessages)
        {
            return;
        }

        _recentMessages.RemoveRange(0, _recentMessages.Count - MaxRecentMessages);
    }

    private void RebuildProgressCounters()
    {
        foreach (MutableStageState stage in _stages.Values)
        {
            stage.TotalItems = stage.ReportTotalItems;
            stage.CompletedItems = stage.ReportCompletedItems;
            stage.FailedItems = stage.ReportFailedItems;
            stage.SkippedItems = stage.ReportSkippedItems;
            stage.SavedItems = 0;
        }

        foreach (IGrouping<ProcessStage, TranslationProgressUpdate> stageGroup in _progressUpdates.Values.GroupBy(update => update.Stage))
        {
            if (!_stages.TryGetValue(stageGroup.Key, out MutableStageState? stage))
            {
                continue;
            }

            int plannedTotal = stageGroup.Where(update => update.IsPlan).Sum(update => update.TotalItems);
            int itemTotal = stageGroup.Where(update => !update.IsPlan).Sum(update => update.TotalItems);
            stage.TotalItems = Math.Max(stage.TotalItems, Math.Max(plannedTotal, itemTotal));
            stage.CompletedItems = Math.Max(stage.CompletedItems, stageGroup.Where(update => !update.IsPlan).Sum(update => update.CompletedItems));
            stage.FailedItems = Math.Max(stage.FailedItems, stageGroup.Where(update => !update.IsPlan).Sum(update => update.FailedItems));
            stage.SkippedItems = Math.Max(stage.SkippedItems, stageGroup.Where(update => !update.IsPlan).Sum(update => update.SkippedItems));
            stage.SavedItems = Math.Max(stage.SavedItems, stageGroup.Where(update => !update.IsPlan).Sum(update => update.SavedItems));
            stage.ProgressPercent = CalculateProgress(stage.TotalItems, stage.CompletedItems, stage.SkippedItems, stage.FailedItems);
        }

        _summary.TotalTranslations = _stages.Values.Sum(stage => stage.TotalItems);
        _summary.CompletedTranslations = _stages.Values.Sum(stage => stage.CompletedItems);
        _summary.FailedTranslations = _stages.Values.Sum(stage => stage.FailedItems);
        _summary.SkippedTranslations = _stages.Values.Sum(stage => stage.SkippedItems);
        _summary.ProgressPercent = CalculateProgress(
            _summary.TotalTranslations,
            _summary.CompletedTranslations,
            _summary.SkippedTranslations,
            _summary.FailedTranslations);
    }

    private static int CalculateProgress(int totalItems, int completedItems, int skippedItems, int failedItems)
    {
        if (totalItems <= 0)
        {
            return 0;
        }

        int processedItems = Math.Clamp(completedItems + skippedItems + failedItems, 0, totalItems);
        return (int)Math.Round(processedItems * 100d / totalItems, MidpointRounding.AwayFromZero);
    }

    private static LocalizationDashboardSummary CopySummary(LocalizationDashboardSummary summary)
        => new()
        {
            ActiveRunId = summary.ActiveRunId,
            IsRunning = summary.IsRunning,
            CurrentStage = summary.CurrentStage,
            LastMessage = summary.LastMessage,
            RunStartedUtc = summary.RunStartedUtc,
            RunCompletedUtc = summary.RunCompletedUtc,
            LastEventUtc = summary.LastEventUtc,
            LastSequence = summary.LastSequence,
            MessagesReceived = summary.MessagesReceived,
            TotalTranslations = summary.TotalTranslations,
            CompletedTranslations = summary.CompletedTranslations,
            FailedTranslations = summary.FailedTranslations,
            SkippedTranslations = summary.SkippedTranslations,
            ProgressPercent = summary.ProgressPercent,
            ErrorCount = summary.ErrorCount,
            WarningCount = summary.WarningCount,
            SavedDictionaryFiles = summary.SavedDictionaryFiles,
            SavedMarkdownFiles = summary.SavedMarkdownFiles,
            SavedHashFiles = summary.SavedHashFiles,
            TempFallbackWrites = summary.TempFallbackWrites,
            AvailableLanguageCount = summary.AvailableLanguageCount,
            DefaultLanguage = summary.DefaultLanguage,
            TranslationServerLatencyMs = summary.TranslationServerLatencyMs,
            TranslationServerReady = summary.TranslationServerReady
        };

    private static Dictionary<ProcessStage, MutableStageState> CreateStageMap()
        => Enum.GetValues<ProcessStage>()
            .Where(stage => stage != ProcessStage.Iddle)
            .ToDictionary(stage => stage, stage => new MutableStageState(stage));

    private sealed class MutableStageState(ProcessStage stage)
    {
        public ProcessStage Stage { get; } = stage;
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
        public int ReportTotalItems { get; set; }
        public int ReportCompletedItems { get; set; }
        public int ReportFailedItems { get; set; }
        public int ReportSkippedItems { get; set; }

        public LocalizationStageSnapshot ToSnapshot()
            => new()
            {
                Stage = Stage,
                Status = Status,
                LastType = LastType,
                LastMessage = LastMessage,
                StartedUtc = StartedUtc,
                CompletedUtc = CompletedUtc,
                TotalItems = TotalItems,
                CompletedItems = CompletedItems,
                FailedItems = FailedItems,
                SkippedItems = SkippedItems,
                SavedItems = SavedItems,
                ErrorCount = ErrorCount,
                WarningCount = WarningCount,
                ProgressPercent = ProgressPercent
            };
    }
}
