# Data Models

The `Dita.Shared.Localization.Models` namespace defines all data structures used across the localization and translation system — from API request/response pairs to pipeline reports and dashboard snapshots.

## Model overview

### Configuration

#### AutomaticTranslationSettings

Configuration model bound from `appsettings.json`. Controls LibreTranslate server connection and pipeline behaviour.

| Property | Type | Default | Description |
|---|---|---|---|
| `Address` | `string` | `http://localhost:5000` | LibreTranslate server URL |
| `NeedsKey` | `bool` | `false` | Whether an API key is required |
| `Key` | `string` | `""` | API key |
| `DefaultLanguage` | `string` | `"en"` | Application default language |
| `IgnoredLanguages` | `List<string>` | `[]` | Languages to exclude from translation |
| `MarkdownRoots` | `List<string>` | `["/Docs"]` | Documentation root directories |
| `AutomaticRun` | `bool` | `false` | Enable scheduled pipeline runs |
| `WaitingTime` | `TimeSpan` | `00:00:00` | Delay before first run |
| `CheckingPeriod` | `int` | `30` | Minutes between runs |
| `TranslateEndpoint` | `string` | `"/translate"` | LibreTranslate text endpoint |
| `TranslateFileEndpoint` | `string` | `"/translate_file"` | LibreTranslate file endpoint |
| `LanguagesEndpoint` | `string` | `"/languages"` | LibreTranslate languages endpoint |
| `DetectLanguageEndpoint` | `string` | `"/detect"` | LibreTranslate detection endpoint |
| `RequestThrottleMs` | `int` | `80` | Delay between translation requests |
| `RequestTimeoutSeconds` | `int` | `10` | HTTP timeout per request |
| `AppsettingsLoaded` | `bool` | `false` | Whether config was loaded |

### LibreTranslate API models

#### TranslateRequest → TranslateResult

**Request** — text translation API call:

| Property | Type | JSON key | Default |
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
| `Source` | `string` | — | `"auto"` |
| `Target` | `string` | — | `"en"` |
| `Format` | `string?` | — | `"text"` |
| `ApiKey` | `string?` | `"api_key"` | `null` |
| `Alternatives` | `int` | — | `0` |

**Result** — translation response:

| Property | Type |
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → Detections

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### TranslateFileRequest → TranslateFileResult

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### LibreLanguage

Single language entry from the `/languages` endpoint:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Pipeline report models

#### CheckingReport

Result of the server validation stage:

| Property | Type |
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### TranslationsReport

Result of dictionary/country translation stages:

| Property | Type |
|---|---|
| `DefaultDictionaryExists` | `bool` |
| `DefaultDictionaryCount` | `int` |
| `ToTranslateCount` | `int` |
| `AddedCount` | `int` |
| `RemovedCount` | `int` |
| `SkippedCount` | `int` |
| `TranslatedCount` | `int` |
| `ErrorsCount` | `int` |
| `Errors` | `List<TranslationError>?` |

#### MarkdownTranslationsReport

Result of the Markdown translation stage:

| Property | Type |
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StoringReport

Final aggregation of persisted outputs:

| Property | Type |
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport\<T\>

Generic container that wraps any report type with stage metadata:

| Property | Type |
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
| `StageDuration` | `TimeSpan?` (computed) |

### Translation work models

#### PhraseInQueue

Work item for the translation queue:

| Property | Type |
|---|---|
| `Target` | `TranslationTarget` |
| `Key` | `string?` |
| `Phrase` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string` |
| `ChangeRequired` | `PhraseChange` |
| `AddedToList` | `DateTime` |
| `TranslationStart` | `DateTime?` |
| `TranslationEnds` | `DateTime?` |
| `IsTranslated` | `bool` |
| `TranslatedText` | `string?` |

#### TranslationError

Structured error record carried in all reports:

| Property | Type |
|---|---|
| `Source` | `string` (language code, file path, or stage name) |
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### SingleTranslation

Single locale dictionary:

| Property | Type |
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownTranslatableBlock

Extracted block from a Markdown document:

| Property | Type |
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Text resolution models

#### TextLocalizationRequest → TextLocalizationResponse

**Request** — dictionary-based localization (writable):

| Property | Type |
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

| Property | Type |
|---|---|
| `Text` | `string` (original) |
| `LocalizedText` | `string` (localized) |
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextTranslationRequest → TextTranslationResponse

**Request** — dynamic translation (read-only):

| Property | Type |
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

| Property | Type |
|---|---|
| `Text` | `string` (original) |
| `TranslatedText` | `string` (translated) |
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextResolutionSource

Identifies where a localized/translated value was resolved from:

| Value | Meaning |
|---|---|
| `TargetDictionary` | Found in locale dictionary for the target language |
| `DefaultDictionary` | Found in the default language dictionary |
| `DefaultDictionaryCreated` | Not found; added to the default dictionary |
| `TranslationServer` | Returned by LibreTranslate |
| `OriginalText` | Returned as-is without resolution |

### Shared types

#### CountryDefinition

Read-only entry from `countries.json`:

| Property | Type | JSON key |
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### ComparisonCondition

Filter condition for evaluation:

| Property | Type |
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### ErrorResponse

Simple API error envelope:

| Property | Type |
|---|---|
| `Error` | `string?` |