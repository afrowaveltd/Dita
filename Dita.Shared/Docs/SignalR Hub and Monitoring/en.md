# SignalR Hub and Monitoring

The `Dita.Shared.Localization.Hubs` and `Dita.Shared.Localization.ScheduledTranslationService` namespaces provide real-time pipeline monitoring through ASP.NET Core SignalR.

## LocalizationHub

A typed SignalR hub that broadcasts pipeline events to connected dashboard clients.

**Route**: `/hubs/localization` (mapped in `Program.cs`)

### Hub contract

```csharp
public interface ILocalizationHubClient
{
    Task ReceiveLocalizationSnapshot(LocalizationHubSnapshot snapshot);
    Task ReceiveLocalizationMessage(LocalizationHubMessage message);
}
```

- **ReceiveLocalizationSnapshot** — sent immediately on connection, delivers the full current dashboard state
- **ReceiveLocalizationMessage** — sent for every pipeline event (stage start, progress, completion, error)

### Snapshot-on-connect pattern

When a client connects to the hub, `OnConnectedAsync` pushes the current `LocalizationHubSnapshot` to **only the caller** via `Clients.Caller`. This ensures new clients immediately have up-to-date data without polling or waiting for the next event.

```csharp
public override async Task OnConnectedAsync()
{
    if (_monitoringState is not null)
    {
        var snapshot = _monitoringState.GetSnapshot();
        await Clients.Caller.ReceiveLocalizationSnapshot(snapshot);
    }
    await base.OnConnectedAsync();
}
```

The `ILocalizationMonitoringState` dependency is optional — if not registered in DI, the hub functions without monitoring state support.

## ISignalRPublisher

Abstraction that decouples translation services from the SignalR hub:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data,
        LocalizationMessageType type, string message, bool isError = false)
        where T : class;

    Task PublishMessageAsync(Guid runId, LocalizationMessageType type,
        ProcessStage stage, string message, object? data = null,
        bool isError = false);
}
```

- **PublishStageAsync<T>** — publishes a typed stage report (e.g. `StageReport<CheckingReport>`) to all connected clients
- **PublishMessageAsync** — publishes a simple untyped message

### Sequence guarantees

`SignalRPublisher` maintains an atomically-incremented `long` sequence counter via `Interlocked.Increment`. Each message within a run gets a unique, monotonically increasing sequence number, enabling clients to detect gaps or reorder events.

---

## LocalizationMonitoringState

In-memory dashboard state store that implements a CQRS-lite pattern: `RecordMessage` is the write side, `GetSnapshot` is the read side.

### Write side: RecordMessage

When a pipeline message arrives:
1. **New-run detection** — if `StageStarted + CheckServers` with a different `RunId`, all state is reset for the new run
2. **Run state update** — tracks `IsRunning`, start/completion timestamps
3. **Stage state FSM** — per-stage state machine: `Waiting → Running → Completed/Failed`
4. **Payload application** — pattern-matches typed payloads (`TranslationProgressUpdate`, `CheckingReport`, `StoringReport`, `TranslationsReport`, `MarkdownTranslationsReport`) and updates aggregate counters
5. **Recent message buffer** — capped at 250 messages (ring buffer)
6. **Progress rebuild** — aggregates progress updates from all stages, computes overall `ProgressPercent`

### Read side: GetSnapshot

Produces a deep copy of the current dashboard state as `LocalizationHubSnapshot` for transmission to clients. Thread-safe via `lock`.

### Dashboard models

#### LocalizationHubSnapshot

| Field | Type | Description |
|---|---|---|
| `SnapshotUtc` | `DateTime` | When the snapshot was taken |
| `Summary` | `LocalizationDashboardSummary` | Aggregate counters |
| `Stages` | `List<LocalizationStageSnapshot>` | Per-stage state |
| `RecentMessages` | `List<LocalizationHubMessage>` | Last 250 messages |

#### LocalizationDashboardSummary

| Field | Type | Description |
|---|---|---|
| `ActiveRunId` | `Guid?` | Current pipeline run identifier |
| `IsRunning` | `bool` | Whether a pipeline run is active |
| `CurrentStage` | `string` | Current `ProcessStage` name |
| `LastMessage` | `string` | Most recent human-readable event |
| `TotalTranslations` | `int` | Total planned translation units |
| `CompletedTranslations` | `int` | Successfully translated units |
| `FailedTranslations` | `int` | Failed translation units |
| `SkippedTranslations` | `int` | Skipped translation units |
| `ProgressPercent` | `int` | Overall pipeline progress (0–100) |
| `ErrorCount` | `int` | Total error count |
| `WarningCount` | `int` | Total warning count |
| `SavedDictionaryFiles` | `int` | Locale JSON files saved |
| `SavedMarkdownFiles` | `int` | Translated documents saved |
| `SavedHashFiles` | `int` | Hash files saved |
| `AvailableLanguageCount` | `int` | Languages known to the server |
| `TranslationServerReady` | `bool` | Whether the translation server is reachable |
| `TranslationServerLatencyMs` | `int` | Server response latency |

#### LocalizationHubMessage

| Field | Type | Description |
|---|---|---|
| `RunId` | `Guid` | Correlation identifier for the run |
| `Sequence` | `long` | Monotonic counter within a run |
| `Type` | `LocalizationMessageType` | Message type (StageStarted, Progress, etc.) |
| `Stage` | `ProcessStage` | Pipeline stage |
| `TimestampUtc` | `DateTime` | When the message was emitted |
| `IsError` | `bool` | Error flag |
| `Message` | `string` | Human-readable summary |
| `Data` | `object?` | Stage-specific payload |

### Message flow example

```text
StageStarted  / CheckServers      (Sequence: 1)
Progress      / CheckServers       (Sequence: 2)  — Server latency: 42ms
StageCompleted/ CheckServers       (Sequence: 3)
StageStarted  / TranslateCountries (Sequence: 4)
Progress      / TranslateCountries (Sequence: 5)  — Found 195 country names
Progress      / TranslateCountries (Sequence: 6)  — Saved dictionary for 'cs'
StageCompleted/ TranslateCountries (Sequence: 7)
...
PipelineCompleted / StoringResults (Sequence: N)
```

All messages flow through `SignalRPublisher` → `LocalizationHub` → connected dashboard clients, and simultaneously through `LocalizationMonitoringState.RecordMessage` for snapshot generation.

---

## Localization Middleware

The `LocalizationMiddleware` in `Dita.Shared.Localization.Middlewares` sets the request's `CultureInfo` for every HTTP request.

### Resolution priority

1. **Cookie** — reads `Language` cookie via `ICookieService`
2. **Accept-Language header** — parses the primary language from the HTTP header
3. **Configured default** — `AutomaticTranslationSettings.DefaultLanguage`
4. **Hard-coded fallback** — `"en"`

### Behaviour

1. Resolve the default culture from settings (with `CultureInfo.GetCultureInfo` validation, falling back to `"en"`)
2. Read language preference from cookie, then header
3. Validate the resolved culture against `CultureInfo.GetCultures(CultureTypes.AllCultures)`
4. Set `Thread.CurrentThread.CurrentCulture` and `CurrentUICulture`
5. Update the `Accept-Language` request header
6. Write the language cookie back via `ICookieService`
7. Continue the middleware pipeline

The middleware implements `IMiddleware` (factory-based, registered as scoped) and uses C# 12 primary constructor syntax.