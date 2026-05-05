# Summary of Changes to the Automatic Translation Service

## Overview

This document summarizes all changes made to the Dita automatic translation service, including architecture refactoring, new features, observability improvements, and localization enhancements.

## Architecture Changes

### Refactored BackendTranslationService

The monolithic `BackendTranslationService` has been decomposed into four specialized services coordinated by a lightweight orchestrator:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Benefits

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## New Features

### Live Translation Monitor

**Location**: `/Admin/LiveTranslation`

A new admin page that provides real-time visibility into the translation pipeline:

- Displays all SignalR events as they occur
- Color-coded message types (blue=started, green=completed, red=error)
- Connection status banner with auto-reconnect
- Message counter and export to JSON

### Named Placeholders

The localization system now supports named placeholders (`{name}`) for improved grammaticality in different languages:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Features:
- Placeholder values provided at runtime or stored in `placeholders.json`
- Automatic masking/restoration during translation to prevent corruption
- Backward compatible with existing positional placeholders

### Incremental Translation

Markdown files are translated incrementally:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Enhanced Retry Logic

Three levels of resilience:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### SignalR Reporting

Real-time progress reporting for all pipeline operations:

- Every stage publishes `StageStarted/StageCompleted/StageFailed` events
- Per-language progress published as `Progress` events
- Error events include detailed context (source, error code, message)
- Sequence numbers guarantee ordering within each run

## Configuration Changes

### appsettings.json

No breaking changes. Existing configuration continues to work:

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00"
  }
}
```

### New Services

Registered in `Program.cs`:

- `ISignalRPublisher` / `SignalRPublisher`
- `TranslationRetryService`
- `ICountriesTranslationService` / `CountriesTranslationService`
- `ILocalizationTranslationService` / `LocalizationTranslationService`
- `IDocumentsTranslationService` / `DocumentsTranslationService`
- `IPlaceholderService` / `PlaceholderService`

The SignalR hub is mapped at `/hubs/localization` for client connections.

## Testing

### Test Status

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- New test coverage added for:
  - PlaceholderService functionality
  - BackendTranslationService orchestration
  - JsonStringLocalizer placeholder indexers

### Known Limitations

- `SaveAndLoad_PreservesPlaceholders` test is skipped when running in parallel because multiple test instances share the same `placeholders.json` file. It passes when run in isolation.

## New File Structure

### Services in `Dita.Shared/Localization/ScheduledTranslationService/`

- `BackendTranslationService.cs` — Pipeline orchestrator
- `CountriesTranslationService.cs` — Country name translation
- `LocalizationTranslationService.cs` — JSON dictionary synchronization
- `DocumentsTranslationService.cs` — Markdown translation
- `SignalRPublisher.cs` — SignalR message publishing
- `TranslationRetryService.cs` — Retry logic with placeholder masking
- `ISignalRPublisher.cs` — Publisher interface
- `ICountriesTranslationService.cs` — Country service interface
- `ILocalizationTranslationService.cs` — Localization service interface
- `IDocumentsTranslationService.cs` — Document service interface
- `IBackendTranslationService.cs` — Orchestrator interface (updated)
- `MarkdownTranslationMetadata.cs` — Per-file translation metadata

### Updated Services in `Dita.Shared/Localization/Services/`

- `JsonStringLocalizer.cs` — Added named placeholder support
- `JsonStringLocalizerFactory.cs` — Updated for new parameter
- `PlaceholderService.cs` — Named placeholder management
- `IPlaceholderService.cs` — Placeholder interface

### New Admin Page in `Dita.Server/Pages/Admin/`

- `LiveTranslation.cshtml` — Real-time monitoring page
- `LiveTranslation.cshtml.cs` — Page model

### New Documentation in `Dita.Server/Docs/`

- `Realtime Translations/en.md` — Updated pipeline documentation
- `Placeholders/en.md` — Placeholder system guide
- `Live Translation Dashboard/en.md` — Dashboard usage guide
- `Translation Architecture/en.md` — Technical architecture overview

## Backward Compatibility

All changes are additive:

- Existing localization code (`localizer["key"]`) works unchanged
- Positional formatting (`localizer["key", arg1, arg2]`) works unchanged
- Existing JSON dictionary format is unchanged
- Existing Markdown structure is unchanged
- SignalR messages use the same `LocalizationHubMessage` format

## Migration Path

No migration required. The refactoring is internal:

1. Old `BackendTranslationService` was preserved as a reference and then replaced
2. DI registrations were updated to use new interfaces
3. All existing consumers of `IBackendTranslationService` see no changes

## Performance Improvements

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Future Enhancements

Planned improvements:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Contact

For questions or issues with the translation service, please refer to the detailed documentation in each module's `Docs/` directory or contact the development team.
