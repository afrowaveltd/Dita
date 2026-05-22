# Gegevensmodellen

De namespace definieert alle datastructuren die gebruikt worden in het lokalisatie- en vertaalsysteem van API-verzoek/antwoordparen naar pijpleidingrapporten en dashboard snapshots.

## Modeloverzicht

### Instellingen

#### Automatische vertalingInstellingen

Configuratie model gebonden van . Controleert LibreVertaal server verbinding en leidinggedrag.

Eigenschap
|---|---|---|---|
LibreTranslate server-URL
Of een API-sleutel vereist is
API-sleutel
De standaardtaal van de toepassing
Talen die van vertaling moeten worden uitgesloten
Documentatie-hoofdmappen
Geplande pijpleidingen inschakelen
Vertraging voor eerste run
Minuten tussen runs
LibreVertaal tekst eindpunt
LibreVertaal bestandseindpunt
LibreVertaal taal eindpunt
LibreVertaal detectie eindpunt
Vertraging tussen vertaalverzoeken
HTTP timeout per verzoek
Of de configuratie geladen is

### LibreVertaal API modellen

#### VertalenRequest → VertalenResult

**Request** Tekstvertaling API-oproep:

Eigenschap
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
Wat
Wat
Wat
| `ApiKey` | `string?` | `"api_key"` | `null` |
Wat

**Result** Vertaalrespons:

Eigenschap
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → Detecties

**Request**: **Rerespons**:
**Response**: `{ Language, Confidence }`

#### VertalenFileRequest → VertalenFileResult

**Request**: **Rerespons**:
**Response**: `{ TranslatedFileUrl }`

#### LibreTaal

Enkele taal ingang vanaf het eindpunt:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Modellen van het pijpleidingrapport

#### Controlerapport

Resultaat van de servervalidatiefase:

Eigenschap
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### VertalingenMelden

Resultaat van woordenboek/landvertalingsstadia:

Eigenschap
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

#### MarkdownVertalingenMelden

Resultaat van de vertaalfase Markdown:

Eigenschap
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### OpslaanMelden

Definitieve samenvoeging van aanhoudende outputs:

Eigenschap
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport<T>

Generieke container die elk rapporttype met fasemetadata omwikkelt:

Eigenschap
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(berekend)

### Vertaalwerkmodellen

#### zinsinqueue

Werkitem voor de vertaalwachtrij:

Eigenschap
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

#### Vertalingfout

Gestructureerde foutrecord in alle rapporten:

Eigenschap
|---|---|
(taalcode, bestandspad of podiumnaam)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Enkele vertaling

Enig lokaal woordenboek:

Eigenschap
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownTranslatableBlock

Uitgepakt blok uit een Markdown-document:

Eigenschap
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Tekstresolutiemodellen

#### Tekstlokalisatie Verzoek → Tekstlokalisatie Respons

**Request** Op woordenboek gebaseerde localisatie (schrijfbaar):

Eigenschap
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Respons**:

Eigenschap
|---|---|
(origineel)
(gelokaliseerd)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TekstvertalingRequest → TekstvertalingRespons

**Request** dynamische vertaling (alleen-lezen):

Eigenschap
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Respons**:

Eigenschap
|---|---|
(origineel)
(vertaald)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TekstResolutieBron

Identificeert waar een gelokaliseerde/vertaalde waarde werd opgelost uit:

Waarde
|---|---|
Gevonden in lokaal woordenboek voor de doeltaal
Gevonden in het standaardtaalwoordenboek
Niet gevonden; toegevoegd aan het standaard woordenboek
geretourneerd door libretranslate
Teruggekeerd als-is zonder resolutie

### Gedeelde typen

#### Landdefinitie

Alleen-lezen ingang van:

Eigenschap
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Vergelijking

Filterconditie voor evaluatie:

Eigenschap
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Foutrespons

Eenvoudige API-foutenvelop:

Eigenschap
|---|---|
| `Error` | `string?` |
