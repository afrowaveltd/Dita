# Translation Architecture

This document describes the modular architecture of Dita's automatic translation system, introduced to improve maintainability, testability, and resilience.

## Design goals

The refactoring addressed several concerns with the original monolithic design:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Service decomposition

### BackendTranslationService (orchestrator)

**Responsibilities**:
- Pipeline lifecycle management (start, completion, error handling)
- Semaphore-based concurrency control (prevents overlapping runs)
- Server validation (latency, language availability, configuration)
- Delegation to sub-services

**Does NOT contain**:
- Translation logic
- File I/O for specific formats
- Retry logic

### CountriesTranslationService

**Responsibilities**:
- Read `countries.json` from `Jsons/` directory
- Synchronize country names into the default locale dictionary
- Translate missing country names per target language
- Save each target dictionary immediately after translation

**Key behaviors**:
- If default language is English: country names stored as-is
- If default language is other: English names translated to default language first
- Each language is processed independently with its own retry loop

### LocalizationTranslationService

**Responsibilities**:
- Detect added/removed keys by comparing current default dictionary with previous snapshot
- Translate added keys into each target language
- Remove deleted keys from each target language
- Save snapshot for next comparison

**Key behaviors**:
- Manual translations always take priority (never overwritten)
- Added keys are translated and saved per-language immediately
- Removed keys are deleted per-language immediately
- Snapshot is saved only after all languages complete successfully

### DocumentsTranslationService

**Responsibilities**:
- Walk configured Markdown roots recursively
- Detect changed source files using SHA-256 hashes
- Track per-block translation status in `.translation-meta.json`
- Translate block-by-block with per-block retry
- Validate Markdown structure after translation
- Save each target language file independently

**Key behaviors**:
- Block-level granularity: headings, paragraphs, list items are translated separately
- Metadata tracks which blocks succeeded/failed per language
- Failed blocks are retried on next run without re-translating successful blocks
- Structure validation ensures heading counts, lists, code blocks, etc. match source

## Retry strategy

The system implements retries at three levels:

### Level 1 — HTTP (LibreTranslateService)

- Up to 5 attempts with exponential backoff (1s, 2s, 3s, 4s, 5s)
- Handles network timeouts, 5xx errors, and transient failures
- Built into the HTTP client configuration

### Level 2 — Stage (TranslationRetryService)

- Up to 3 attempts with 30-second delays
- Re-drives the entire translation request after HTTP-level retries are exhausted
- Placeholder masking and restoration is applied at this level

### Level 3 — Block (DocumentsTranslationService)

- Individual Markdown blocks that fail are marked in metadata
- Retried automatically on the next pipeline run
- Successful blocks are never re-translated

## Data flow

### JSON dictionary translation

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### Markdown translation

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### Country name translation

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## State persistence

### Snapshots

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Hash files

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Translation metadata

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Source content hash
- Per-language block status (array of booleans)
- Last update timestamp
- **Purpose**: Enables partial re-translation of only failed blocks

### Placeholder storage

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## SignalR reporting

### Publisher abstraction

`ISignalRPublisher` decouples translation services from SignalR specifics:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sequence guarantees

- Messages within a single run are monotonically sequenced
- Sequence numbers are unique per-run via `Interlocked.Increment`
- Clients can detect gaps or reordering

### Hub mapping

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Extension points

### Adding a new translation target

1. Create a new interface `INewTranslationService` with `RunAsync(List<string>, StoringReport, Guid)`
2. Implement the interface with domain-specific logic
3. Register in DI container
4. Inject into `BackendTranslationService` constructor
5. Call from `RunAsync()` after existing stages

### Custom retry policy

Override `TranslationRetryService` constructor parameters:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### Custom placeholder handling

Implement `IPlaceholderService` to change placeholder syntax or storage:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Configuration

### appsettings.json

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### Runtime tuning

| Setting | Default | Effect |
|---------|---------|--------|
| `RequestThrottleMs` | 80 | Delay between translation requests |
| `RequestTimeoutSeconds` | 10 | HTTP timeout per request |
| `stageMaxRetries` | 3 | Stage-level retry count |
| `stageRetryDelaySeconds` | 30 | Delay between stage retries |

## Testing strategy

### Unit tests

Each sub-service is independently testable:

- Mock `ILibreTranslateService` to simulate success/failure
- Mock `ISignalRPublisher` to verify reporting
- Use temporary directories for file I/O
- Verify per-language saving behavior

### Integration tests

- Full pipeline run with real (local) LibreTranslate instance
- Verify SignalR messages are delivered to connected clients
- Test concurrent run prevention (semaphore)
- Validate Markdown structure after translation

### End-to-end tests

- Trigger translation via API or scheduler
- Verify all target language files are created/updated
- Check metadata files contain correct block status
- Confirm placeholders are preserved across translations

## Performance considerations

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migration from monolithic design

The original `BackendTranslationService` contained all logic in one class. The migration path:

1. Extract country logic → `CountriesTranslationService`
2. Extract JSON logic → `LocalizationTranslationService`
3. Extract Markdown logic → `DocumentsTranslationService`
4. Extract SignalR publishing → `SignalRPublisher`
5. Extract retry logic → `TranslationRetryService`
6. Simplify orchestrator to delegation-only

All existing interfaces (`IBackendTranslationService`) remain unchanged. Consumers of the pipeline see no breaking changes.
