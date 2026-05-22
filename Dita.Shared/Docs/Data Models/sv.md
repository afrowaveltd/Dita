# Datamodeller

Namnrymden definierar alla datastrukturer som används över hela lokaliserings- och översättningssystemet - från API-förfrågan / svarspar till pipelinerapporter och instrumentbräda ögonblicksbilder.

## Modellöversikt

### Konfiguration

#### automatiska översättningar

Konfigurationsmodell bunden från . Kontrollerar LibreTranslate serveranslutning och pipeline beteende.

Fastighet
|---|---|---|---|
LibreTranslate server URL
Om en API-nyckel krävs
API nyckel
Ansökan standard språk
Språk att utesluta från översättning
Dokumentation root kataloger
Aktivera schemalagda pipeline körningar
Fördröjning före första körningen
Minuter mellan runs
LibreTranslate text endpoint
LibreTranslate fil endpoint
LibreTranslate språk endpoint
LibreTranslate detektion endpoint
Fördröjning mellan översättningsförfrågningar
HTTP timeout per förfrågan
Om konfig laddades

### LibreTranslate API-modeller

#### TranslateRequest → TranslateResultat

**Begäran** - textöversättning API-anrop:

Fastighet
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
——
——
——
| `ApiKey` | `string?` | `"api_key"` | `null` |
——

**Result** – översättningsrespons:

Fastighet
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → Detektering

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### TranslateFileRequest → TranslateFileResultat

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### LibreLanguage

Enstaka språkinmatning från slutpunkten:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Pipeline rapport modeller

#### CheckingReport

Resultatet av serverns valideringsstadium:

Fastighet
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Översättningarrapport

Resultat av ordbok/landsöversättningsstadier:

Fastighet
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

#### MarkdownTranslationsRapport

Resultatet av Markdown översättningsstadiet:

Fastighet
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StoringReport

Slutlig aggregation av bestående utgångar:

Fastighet
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### scenreport<t>

Generisk behållare som lindrar någon rapporttyp med stegmetadata:

Fastighet
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(dator)

### Översättningsarbete modeller

#### fraseinqueue

Arbetspunkt för översättningskö:

Fastighet
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

#### ÖversättningError

Strukturerad felrekord utförd i alla rapporter:

Fastighet
|---|---|
(språkkod, filväg eller scennamn)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### SingleTranslation

Enskild lokal ordbok:

Fastighet
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownTranslatableBlock

Utdraget block från ett Markdown-dokument:

Fastighet
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Textupplösningsmodeller

#### Textlokalisering Begär → Textlokalisering Svar

** Begär** – ordboksbaserad lokalisering (skrivbar):

Fastighet
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Svar**:

Fastighet
|---|---|
(original)
(lokaliserad)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### texttranslationrequest → textöversättningsrespons

** Begär** – dynamisk översättning (läs endast):

Fastighet
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Svar**:

Fastighet
|---|---|
(original)
(översatt)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextResolutionSource

Identifierar var ett lokaliserat/översatt värde löstes från:

Värde
|---|---|
Hittades i lokal ordbok för målspråket
Finns i standard språkordboken
Inte hittades; tillsätts till standardordboken
Återvänd av LibreTranslate
Returnerad som-är utan resolution

### Delade typer

#### landdefinition

Read-only entry från:

Fastighet
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Jämförelsevillkor

Filtrera tillstånd för utvärdering:

Fastighet
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Fel svar

Enkelt API-felkuvert:

Fastighet
|---|---|
| `Error` | `string?` |
