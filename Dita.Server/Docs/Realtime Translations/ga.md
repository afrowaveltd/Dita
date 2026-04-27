# Real-am aistriúcháin

Tá an doiciméad seo mar ionchur tástála beo don phíblíne aistriúcháin uathoibríoch. Spreagann aon athrú ar an gcomhad seo ath-aistriú de gach comhad teanga sprioc ar an chéad reáchtáil sceidealta eile.

## Cad a dhéanann an tseirbhís

Ritheann an tseirbhís ar sceideal agus forghníomhaíonn píblíne cúig chéim: bailíochtú freastalaí, sioncrónaithe tír, sioncrónaithe foclóir JSON, aistriúchán comhad Markdown, agus fós na torthaí. Gach céim astaíonn struchtúrtha imeachtaí dul chun cinn fíor-ama thar Signal R ionas gur féidir le cliaint nasctha a leanúint chomh maith le fáltais oibre.

## Céimeanna Pipeline

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
- After the default dictionary is updated, each missing country entry in every target language dictionary is queued for translation.
- Already-translated entries are preserved without modification.

### Stage 3 — TranslateJsonFiles

The service compares the current default localization dictionary with a snapshot stored from the previous run:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Manual translations always take priority. If a target dictionary already contains a value for a key, that entry is left unchanged regardless of what the source says.
- After the run, the current default dictionary is saved as the new snapshot for the next comparison.

All dictionaries are always stored with alphabetically sorted keys and indented JSON for human readability.

### Stage 4 — TranslateMarkdownFiles

The service walks the configured documentation roots (default: `/Docs`) and processes every `{defaultLanguage}.md` source file recursively:

1. The source file content is read and a SHA-256 hash is computed.
2. The stored hash from the previous run (kept in a `.hash.json` file next to the source file, or in a temporary fallback location) is compared with the current hash.
3. For each target language, the corresponding `{targetLanguage}.md` file is also checked for structural integrity and for the presence of known untranslated sentinel phrases.
4. Any target file that is missing, has an outdated hash, fails structure validation, or contains untranslated content is queued for re-translation.
5. Successfully translated files are validated for structural parity with the source (equal heading counts, list items, code blocks, blockquotes, links, bold/italic markers, and HTML tags) before they are written to disk.
6. If all target files for a source succeed, the new hash is stored next to the source. If writing next to the source fails (for example in read-only deployments), the hash falls back to the temporary directory.
7. If any target translation fails validation, the stored hash is deliberately cleared so that the source is unconditionally re-translated on the next run.

### Stage 5 — StoringResults

A consolidated `StoringReport` is assembled and published. It includes:

- UTC run start and completion timestamps.
- Counts of saved locale JSON files, saved Markdown files, saved hash files, and fallback hash writes.
- Any storage errors collected during the run.

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
StageCompleted / CheckServers
StageStarted  / TranslateCountries
StageCompleted / TranslateCountries
StageStarted  / TranslateJsonFiles
StageCompleted / TranslateJsonFiles
StageStarted  / TranslateMarkdownFiles
StageCompleted / TranslateMarkdownFiles
StageCompleted / StoringResults
PipelineCompleted / StoringResults
```

If any stage fails, the remaining stages are skipped, a `StageFailed` message is emitted, and finally a `PipelineFailed` message closes the run.

## Translation validation and retry logic

Text translations go through intelligent validation before being accepted:

1. If the translated text is empty or whitespace, the translation is retried automatically.
2. If the translated text equals the source text (case-insensitive comparison) and the source contains mixed casing, the translation is retried using a fully lowercase version of the source.
3. If the lowercase retry produces a result that differs from the original source (case-insensitive), that result is accepted.
4. If the lowercase retry still matches the source, the original translation with correct casing is returned as-is.

All text translation calls use exponential backoff (up to five attempts, with delays of 1 s, 2 s, 3 s, 4 s, 5 s). Retry translations use up to three additional attempts.

File translations do not go through translation validation because the output is a file URL rather than a text value.

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

## Design principles

- Translations are processed sequentially to avoid overloading the LibreTranslate server.
- Localization JSON dictionaries are always stored with alphabetically sorted keys and indented JSON for easier maintenance.
- The previous default dictionary snapshot is stored persistently so that a restart of the application does not lose change tracking.
- Hash files are stored next to the source Markdown file; if that location is not writable, a sanitised path in the system temporary directory is used as a fallback.
- Structure validation for translated Markdown files compares heading counts, list item counts, code fence pairs, blockquote markers, hyperlink counts, bold and italic markers, and HTML tag counts between the source and translated output.

**Manual translations always have priority over automatic additions.**
