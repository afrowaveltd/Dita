# Datenmodelle

Der Namespace definiert alle Datenstrukturen, die über das Lokalisierungs- und Übersetzungssystem hinweg verwendet werden – von API-Anfrage/Responsepaaren bis hin zu Pipeline-Berichten und Dashboard-Snapshots.

## Modellübersicht

### Konfiguration

#### Automatische Übersetzungen

Konfigurationsmodell aus . Kontrolliert LibreTranslate Serververbindung und Pipelineverhalten.

Eigentum
|---|---|---|---|
LibreTranslate Server URL
Ob ein API-Schlüssel erforderlich ist
API Schlüssel
Standardsprache der Anwendung
Sprachen, die von der Übersetzung ausschließen
Dokumentation Wurzelverzeichnisse
Geplante Pipelineläufe aktivieren
Verzögerung vor dem ersten Lauf
Minuten zwischen den Strecken
LibreÜbersetzen Textendpunkt
LibreTranslate Datei Endpoint
LibreÜbersetzen Sprachen Endpoint
LibreTransfert Erkennung Endpunkt
Verzögerung zwischen Übersetzungsanfragen
HTTP Timeout pro Anfrage
Ob config geladen wurde

### LibreTranslate API Modelle

#### ÜbersetzenRequest → ÜbersetzenErgebnis

**Request** — Textübersetzung API-Aufruf:

Eigentum
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Ergebnis** — Übersetzungsantwort:

Eigentum
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → Erkennungen

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### ÜbersetzenFileRequest → TranslateFileResult

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### LibreLangument

Ein Spracheintrag vom Endpunkt:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Pipeline-Berichtsmodelle

#### ÜberprüfungBericht

Ergebnis der Servervalidierung:

Eigentum
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### ÜbersetzungenBericht

Ergebnis der Übersetzungsstufen von Wörterbuch/Land:

Eigentum
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

Ergebnis der Markdown-Übersetzungsphase:

Eigentum
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Artikel zum Artikel

Endaggregation der fortbestehenden Outputs:

Eigentum
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport\<T\>

Generika-Container, der jeden Report-Typ mit Stage-Metadaten umschließt:

Eigentum
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(gemeldet)

### Übersetzungsmodelle

#### phrasen in folge

Artikel für die Übersetzungswarte:

Eigentum
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

#### ÜbersetzungError

Strukturierter Fehlerrekord in allen Berichten:

Eigentum
|---|---|
(sprachcode, dateipfad oder bühnenname)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Einzelübersetzung

Wörterbuch der einzelnen Lokale:

Eigentum
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownÜbersetzbarBlock

Auszug aus einem Markdown-Dokument:

Eigentum
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Modelle der Textauflösung

#### TextLokalisierung Anfrage → TextLocaling Antwort

**Request** — wörterbuchbasierte Lokalisierung (schreibbar):

Eigentum
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Eigentum
|---|---|
(original)
(lokalisiert)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextTranslationRequest → TextTranslationResponse

**Request** — dynamische Übersetzung (nur lesen):

Eigentum
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Eigentum
|---|---|
(original)
(übersetzt)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextResolutionSource

Ist ein lokalisierter/translatierter Wert aus:

Wert
|---|---|
Gefunden in locale Wörterbuch für die Zielsprache
Gefunden im Standardwörterbuch
Nicht gefunden; zum Standardwörterbuch hinzugefügt
Zurückgegeben von LibreTranslate
Rückgabe ohne Auflösung

### Gemeinsame Typen

#### LandDefinition

Nur auf Vorlesung von :

Eigentum
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Vergleichsbedingungen

Filterbedingung für die Auswertung:

Eigentum
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Fehlerbeantwortung

Einfache API-Fehlerhülle:

Eigentum
|---|---|
| `Error` | `string?` |
