# Datamodeller

Navnerommet definerer alle datastrukturer som brukes på tvers av lokaliserings- og oversettelsessystemet - fra API-forespørsel/responspar til pipelinerapporter og dashboard-bildebilder.

## Modelloversikt

### Konfigurasjon

#### Automatiske overføringsinnstillinger

Konfigurasjonsmodell bundet fra . Kontrollerer LibreTranslate servertilkobling og rørledningsadferd.

Eiendom
|---|---|---|---|
LibreTranslate server URL
Om det kreves en API-nøkkel
API-nøkkel
Brukerstandardspråk
Språk å utelukke fra oversettelse
Dokumentasjonsrotmapper
Aktivere planlagte rørledninger
Forsening før første løp
Minutter mellom løp
LibreTranslate tekst endpoint
LibreTranslate filendpoint
LibreTranslate språkendpoint
LibreTranslate deteksjon sluttpunkt
Forsinkelse mellom oversettelsesforespørsler
HTTP timeout per forespørsel
Om oppsett ble lastet

### LibreTranslate API-modeller

#### OversettRequest → Oversett resultat

**Request** — tekstoversettelse API-samtale:

Eiendom
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Resultat** — oversettelsesvar:

Eiendom
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectionRequest → Deteksjoner

**Request**: **Response**:
**Response**: `{ Language, Confidence }`

#### Oversett FileRequest → Oversett FileResult

**Request**: **Response**:
**Response**: `{ TranslatedFileUrl }`

#### LibreLanguage

Enspråklig oppføring fra endepunktet:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Pipeline rapportmodeller

#### Sjekk rapport

Resultat av valideringsfasen til serveren:

Eiendom
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### OversettelserReport

Resultat av ordbok/land oversettelsesstadier:

Eiendom
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

Resultat av Markdown-oversettelsesfasen:

Eiendom
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### lagringsrapport

Endelig sammenslåing av vedvarende utganger:

Eiendom
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### faserapport<t>

Generisk beholder som pakker inn enhver rapporttype med fase metadata:

Eiendom
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(komputert)

### Oversettelsesarbeid modeller

#### FraseInQueue

Arbeidselement for oversettelseskøen:

Eiendom
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

#### Oversettelsesfeil

Strukturert feilregistre som er ført i alle rapporter:

Eiendom
|---|---|
(språklig kode, filsti eller scenenavn)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Singletranslation

Single locale ordbok:

Eiendom
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### markdowntranslaterbar blokk

Pakket ut blokk fra et Markdown-dokument:

Eiendom
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Tekstoppløsningsmodeller

#### Tekstlokalisering Forespørsel → Tekstlokalisering Svar

**Request** — ordbokbasert lokalisering (skrivelig):

Eiendom
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Response**:

Eiendom
|---|---|
(opprinnelig)
(lokalisert)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TekstoverføringRequest → TekstoverføringResponse

**Request** — dynamisk oversettelse (kun lese):

Eiendom
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Response**:

Eiendom
|---|---|
(opprinnelig)
(oversatt)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TekstResolutionSource

Identifiserer hvor en lokalisert/translatert verdi ble løst fra:

Verdi
|---|---|
Funnet i locale ordbok for målspråket
Funnet i standardspråkordboka
Ikke funnet; lagt til i standardordboka
Returnert av LibreTranslate
Returnert as-is uten oppløsning

### Delte typer

#### landdefinition

Lesbar oppføring fra:

Eiendom
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### SammenligningCondition

Filterbetingelser for evaluering:

Eiendom
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Feilresponse

Enkel API-feil konvolutt:

Eiendom
|---|---|
| `Error` | `string?` |
