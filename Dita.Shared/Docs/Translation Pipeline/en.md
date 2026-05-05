# Translation Pipeline

The `Dita.Shared.Localization.ScheduledTranslationService` namespace contains the automatic translation pipeline — a five-stage, scheduled, real-time-monitored system that keeps all target language dictionaries and Markdown documentation files up to date.

## Architecture

```
Scheduller (IHostedService, Timer)
  └─► IBackendTranslationService.RunAsync()
        ├─ Stage 1: CheckServers       (inline)
        ├─ Stage 2: ICountriesTranslationService.RunAsync()
        ├─ Stage 3: ILocalizationTranslationService.RunAsync()
        ├─ Stage 4: IDocumentsTranslationService.RunAsync()
        └─ Stage 5: StoringResults      (aggregate + publish)
```

### Interface contracts

Every sub-service shares a uniform `RunAsync` signature:

```csharp
Task RunAsync(List<string> targetLanguages, StoringReport storingReport, Guid runId);
```

The orchestrator (`IBackendTranslationService`) provides a single entry point:

```csharp
Task RunAsync(); // Idempotent — returns immediately if a run is already in progress
```

## Scheduller

A `IHostedService` that triggers the translation pipeline on a configurable schedule.

### Configuration

| Setting | Source | Default | Description |
|---|---|---|---|
| `AutomaticRun` | `appsettings.json` | `false` | Enable/disable automatic scheduling |
| `WaitingTime` | `appsettings.json` | `0` | Delay before first run (minutes or TimeSpan) |
| `CheckingPeriod` | `appsettings.json` | `30` | Interval between runs (minutes) |

### Execution model

- Uses `System.Threading.Timer` for scheduling
- `Interlocked.CompareExchange` prevents overlapping timer callbacks
- Each invocation creates a **new DI scope** to avoid captive-dependency issues with scoped services
- Resolves `IBackendTranslationService` from the scoped provider

## BackendTranslationService (Orchestrator)

Coordinates all stages sequentially. Prevents concurrent runs with a `SemaphoreSlim(1,1)`.

### Pipeline flow

1. **Acquire semaphore** — if another run is active, return immediately
2. **Generate runId** — `Guid.NewGuid()` for correlation
3. **Stage 1: CheckServers** — validate server health and configuration
4. **Stage 2: TranslateCountries** — delegate to `ICountriesTranslationService`
5. **Stage 3: TranslateJsonFiles** — delegate to `ILocalizationTranslationService`
6. **Stage 4: TranslateMarkdownFiles** — delegate to `IDocumentsTranslationService`
7. **Stage 5: StoringResults** — aggregate and publish the final `StoringReport`
8. **Release semaphore**

### Error handling

If any stage throws, the orchestrator:
1. Publishes a `StageFailed` message via SignalR
2. Publishes a `PipelineFailed` message
3. Releases the semaphore
4. The next timer tick or manual invocation can start a new run

### CheckServers stage

Inline validation (not delegated to a sub-service):

1. Verify `AutomaticTranslationSettings` is loaded (`AppsettingsLoaded`)
2. Ping the LibreTranslate server, measure latency
3. Fetch available languages from the server
4. Verify the configured default language is in the list
5. Build a filtered target language list (excluding `IgnoredLanguages`)
6. Return a `CheckContext(Report, TargetLanguages, DefaultLanguage)`

If any check fails, `StageFailed` is published and the pipeline stops.

## CountriesTranslationService

**Stage 2**: Synchronizes country names from a canonical `countries.json` file into per-language locale dictionaries.

### Process

1. Load `Jsons/countries.json` → `List<CountryDefinition>`
2. For each country, build a `key = English name` entry in the default dictionary
3. If the default language is not English, translate the country name into the default language first
4. For each target language:
   - Load the existing locale dictionary
   - Find missing country entries
   - Translate each missing entry via `TranslationRetryService`
   - **Save the dictionary immediately** after each language
5. Publish `StageCompleted` with a `TranslationsReport`

### Incremental behaviour

Only missing entries are translated. Already-present entries are never overwritten, ensuring manual corrections are preserved.

## LocalizationTranslationService

**Stage 3**: Synchronizes JSON localization dictionaries by detecting added and removed keys.

### Process

1. Load the current default dictionary
2. Load the previously-stored snapshot (`old.json` via `ILanguageService.GetLastStored`)
3. **Detect changes**:
   - **Added keys** — present in current but absent from snapshot
   - **Removed keys** — present in snapshot but absent from current
4. For each target language:
   - **Remove** deleted keys from the target dictionary
   - **Translate** added keys via `TranslationRetryService`
   - **Skip** keys that already exist in the target (manual translations always win)
   - **Save** the dictionary immediately
5. Save the current default dictionary as the new snapshot for the next diff
6. Publish `StageCompleted` with a `TranslationsReport`

### Snapshot diffing

The snapshot mechanism enables **incremental sync** — only changed keys are processed. Without a snapshot, every key would need to be compared every time.

## DocumentsTranslationService

**Stage 4**: The most complex stage — translates Markdown documentation files block-by-block with full partial/incremental support.

### Process

1. Walk configured `MarkdownRoots` directories
2. Find all `{DefaultLanguage}.md` source files recursively
3. For each source file:
   - Read content, compute SHA-256 hash
   - Compare with stored hash (`.hash.json` next to the source file)
   - If hash matches and all target files are fully translated → skip
   - Load `MarkdownTranslationMetadata` (`.translation-meta.json`)
   - Extract translatable blocks via `IMarkdownParserService`
   - For each target language:
     - Check metadata: if all blocks are already translated → skip the file
     - Check language support via `IsLanguageSupportedAsync`
     - Translate block-by-block with `TranslationRetryService`
     - For each block:
       - If metadata says already translated → skip
       - Translate text, validate inline tag structure
       - If validation passes → mark as translated in metadata
       - If validation fails → keep original text, mark as untranslated
     - Reconstruct Markdown via `IMarkdownReconstructorService`
     - Validate document structure (headings, lists, code blocks, blockquotes)
     - Save target file
     - Save metadata
   - Write new hash file (with temp-directory fallback)

### Hash-based staleness

SHA-256 hashing detects source file changes. If the hash matches the stored value, the file is considered unchanged and only blocks with failed translations are retried.

### Dual validation

1. **Inline tag validation** — HTML tags, Markdown formatting tokens (`**`, `*`, `~~`, `` ` ``), and link structures must match between original and translated blocks
2. **Document structure validation** — heading counts, list items, code blocks, blockquotes must be equal

### Metadata sidecar files

`.translation-meta.json` files track per-language, per-block translation status:

```json
{
  "SourceHash": "a1b2c3...",
  "LanguageBlockStatus": {
    "cs": [true, true, false, true],
    "de": [true, true, true, true]
  },
  "UpdatedAtUtc": "2025-05-05T12:00:00Z"
}
```

Boolean arrays indicate which blocks (by index) were successfully translated. `false` entries are retried on the next pipeline run without re-translating successful blocks.

## TranslationRetryService

Stage-level retry wrapper that adds resilience on top of LibreTranslate's internal HTTP retries.

### Retry policy

| Parameter | Default | Description |
|---|---|---|
| `stageMaxRetries` | 3 | Additional attempts after HTTP-level retries fail |
| `stageRetryDelaySeconds` | 30 | Delay between stage-level retries |

### Placeholder masking

Before each translation attempt:
1. **Mask** — replace `{placeholder}` tokens with translation-safe `⟦N⟧` tokens via `IPlaceholderService.PrepareForTranslation`
2. **Translate** — call LibreTranslate with masked text
3. **Restore** — replace `⟦N⟧` tokens with original `{placeholder}` names via the `restore` delegate

### Same-language fast path

If `sourceLanguage == targetLanguage`, the service returns a `TranslateResult` with the same text immediately, without calling the API.

## Correlation and observability

Every pipeline run is identified by a `Guid runId` that flows through all stages, SignalR messages, and reports. Combined with the monotonically-increasing `Sequence` counter, this provides complete traceability of every pipeline event.