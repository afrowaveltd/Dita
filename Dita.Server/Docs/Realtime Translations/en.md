# Real-time translations

This document exists as a live test input for the automatic translation pipeline. Any change to this file triggers re-translation of all target language files on the next scheduled run.

## Architecture overview

The translation pipeline has been restructured into a modular architecture with four specialized sub-services coordinated by a lightweight orchestrator:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Each sub-service operates independently and reports progress via SignalR in real time.

## What the service does

The service runs on a schedule and executes a five-stage pipeline: server validation, country synchronisation, JSON dictionary synchronisation, Markdown file translation, and persisting the results. Each stage emits structured real-time progress events over SignalR so that connected clients can follow along as work proceeds.

## Pipeline stages

### Stage 1 — CheckServers

Before any translation work begins, the service verifies that all preconditions are satisfied:

- The `AutomaticTranslationSettings` configuration section must be present and valid.
- The LibreTranslate server must respond within an acceptable latency.
- The list of languages available on the translation server is fetched.
- The configured default language must be present in that list.
- Missing locale JSON files for any supported language are created automatically.

If any check fails, the pipeline stops immediately and a `StageFailed` message is emitted.

### Stage 2 — TranslateCountries

Country names are kept in sync from a read-only catalog (`countries.json`) into the localization JSON dictionaries.

- If the application default language is English, each country name is stored as `key = value` without translation.
- If the default language is any other language, the English country name is first translated into that language, and the result becomes the `key = value` entry in the default dictionary.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Already-translated entries are preserved without modification.
- If a translation fails, the service retries up to 3 times with 30-second delays before moving to the next language.

### Stage 3 — TranslateJsonFiles

The service compares the current default localization dictionary with a snapshot stored from the previous run:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Manual translations always take priority. If a target dictionary already contains a value for a key, that entry is left unchanged regardless of what the source says.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- If a translation fails for a specific language, the service retries automatically. Only persistent errors (e.g., unsupported language) cause that language to be skipped.
- After the run, the current default dictionary is saved as the new snapshot for the next comparison.

All dictionaries are always stored with alphabetically sorted keys and indented JSON for human readability.

### Stage 4 — TranslateMarkdownFiles

The service walks the configured documentation roots (default: `/Docs`) and processes every `{defaultLanguage}.md` source file recursively:

1. The source file content is read and a SHA-256 hash is computed.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. The stored hash from the previous run (kept in a `.hash.json` file next to the source file, or in a temporary fallback location) is compared with the current hash.
4. For each target language, the corresponding `{targetLanguage}.md` file is also checked for structural integrity.
5. Any target file that is missing, has an outdated hash, fails structure validation, or contains untranslated blocks is queued for re-translation.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Successfully translated files are validated for structural parity with the source (equal heading counts, list items, code blocks, blockquotes, links, bold/italic markers, and HTML tags) before they are written to disk.
8. If all target files for a source succeed, the new hash is stored next to the source. If writing next to the source fails (for example in read-only deployments), the hash falls back to the temporary directory.
9. If any target translation fails validation, the metadata marks those blocks as untranslated so they are retried on the next run.

### Stage 5 — StoringResults

A consolidated `StoringReport` is assembled and published. It includes:

- UTC run start and completion timestamps.
- Counts of saved locale JSON files, saved Markdown files, saved hash files, and fallback hash writes.
- Any storage errors collected during the run.
- Per-language translation statistics (translated count, skipped count, error count).

## SignalR message envelope

Every progress event is delivered as a `LocalizationHubMessage` with the following fields:

| Field | Type | Description |
|-------|------|-------------|
| `RunId` | `Guid` | Correlation identifier for the current pipeline run |
| `Sequence` | `long` | Monotonic counter within a run, starting at 1 |
| `Type` | `LocalizationMessageType` | Semantic type of the message |
| `Stage` | `ProcessStage` | Pipeline stage the message belongs to |
| `TimestampUtc` | `DateTime` | UTC time when the message was emitted |
| `IsError` | `bool` | Whether the message represents an error condition |
| `Message` | `string` | Human-readable summary |
| `Data` | `object?` | Stage-specific payload (report object or null) |

### Message types

| Value | Name | Meaning |
|-------|------|---------|
| 0 | `StageStarted` | A pipeline stage began execution |
| 1 | `StageCompleted` | A pipeline stage finished successfully |
| 2 | `StageFailed` | A pipeline stage encountered a fatal error |
| 3 | `PipelineCompleted` | All stages completed successfully |
| 4 | `PipelineFailed` | The pipeline encountered an unrecoverable error |
| 5 | `Progress` | An informational progress update |
| 6 | `Warning` | A non-fatal warning |

### Pipeline stages

| Value | Name | Description |
|-------|------|-------------|
| 0 | `Iddle` | No active processing |
| 1 | `CheckServers` | Environment and translation server validation |
| 2 | `TranslateCountries` | Country name synchronisation |
| 3 | `TranslateJsonFiles` | JSON localization dictionary synchronisation |
| 4 | `TranslateMarkdownFiles` | Markdown documentation translation |
| 5 | `StoringResults` | Final result aggregation and persistence |

### Typical message flow

```text
StageStarted  / CheckServers
Progress / CheckServers — Server latency: 42ms
StageCompleted / CheckServers
StageStarted  / TranslateCountries
Progress / TranslateCountries — Found 195 country names
Progress / TranslateCountries — Starting translations for 'cs'...
Progress / TranslateCountries — Saved dictionary for 'cs' (198 entries)
StageCompleted / TranslateCountries
StageStarted  / TranslateJsonFiles
Progress / TranslateJsonFiles — Detected 3 added and 0 removed keys
Progress / TranslateJsonFiles — Starting JSON translations for 'cs'...
Progress / TranslateJsonFiles — Saved dictionary for 'cs' (201 entries)
StageCompleted / TranslateJsonFiles
StageStarted  / TranslateMarkdownFiles
Progress / TranslateMarkdownFiles — Scanning 2 source files in '/Docs'
Progress / TranslateMarkdownFiles — File 'en.md' has 12 translatable blocks
Progress / TranslateMarkdownFiles — Translating 'en.md' to 'cs'...
Progress / TranslateMarkdownFiles — Saved 'cs' translation for 'en.md' (12/12 blocks)
StageCompleted / TranslateMarkdownFiles
StageCompleted / StoringResults
PipelineCompleted / StoringResults
```

If any stage fails, the remaining stages are skipped, a `StageFailed` message is emitted, and finally a `PipelineFailed` message closes the run.

## Translation retry logic

The pipeline implements two levels of resilience:

### Stage-level retry (TranslationRetryService)

- If a translation request fails after LibreTranslate's internal retries, the `TranslationRetryService` performs up to 3 additional stage-level retries with 30-second delays.
- Placeholder masking: Named placeholders (`{name}`) in text are temporarily replaced with safe tokens (`___PH_0___`) before translation and restored afterward, ensuring correct grammar in target languages.

### Language validation

- Before translating to a target language, the service verifies the language is supported by the translation server.
- Unsupported languages are skipped with a warning, preventing repeated failed attempts.

### Markdown block-level retry

- Markdown translations are performed block-by-block (headings, paragraphs, list items).
- If an individual block fails translation, it is marked as untranslated in the metadata file and retried on the next pipeline run.
- The service tracks per-language, per-block status in `.translation-meta.json` files next to each source Markdown file.

## Error codes

Errors are reported using a unified `ErrorCode` enum grouped into ranges:

| Range | Category |
|-------|----------|
| 1000–1999 | Network errors |
| 2000–2999 | Storage errors |
| 3000–3999 | Translation errors |
| 4000–4999 | Configuration and argument errors |
| 5000–5999 | Internal errors |

Each error in a report carries the source identifier (language code, file path, or stage name), the error code, and a human-readable message.

## Live Translation Dashboard

The Server project includes an admin page at `/Admin/LiveTranslation` that connects to the SignalR hub at `/hubs/localization` and displays all pipeline events in real time.

- Displays connection status, message count, and a live-updating table of all events.
- Color-coded rows: blue for stage start, green for completion, red for errors.
- Supports clearing the feed and exporting all messages to JSON.
- Auto-reconnects with exponential backoff if the connection drops.

## Design principles

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
