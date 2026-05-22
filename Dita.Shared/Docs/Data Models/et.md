# Andmemudelid

Nimeruum määratleb kõik lokaliseerimis- ja translatsioonisüsteemis kasutatavad andmestruktuurid - alates API päringutest / vastuste paaridest kuni torujuhtmete aruannete ja armatuurlaua hetktõmmisteni.

## Mudeli ülevaade

### Seadistamine

#### Automaattõlke seadistused

Seadistamismudel on seotud alates . Juhtib LibreTranslate serveriühendust ja torustiku käitumist.

Kinnisvara
|---|---|---|---|
LibreTranslate serveri URL
Kas API võti on vajalik
API võti
Rakenduse vaikekeel
Keeled, mis tuleb tõlkest välja jätta
Dokumentatsiooni juurkataloogid
Perioodiliste torujooksude lubamine
Viivitus enne esimest käivitamist
Käikudevahelised minutid
LibreTõlgi teksti lõpp-punkt
LibreTranslate faili lõpp- punkt
LibreTõlgi keelte lõpp-punkt
LibreTranslaadi tuvastuse lõpp-punkt
Viivitus tõlketaotluste vahel
HTTP aegumine taotluse kohta
Kas seadistus laaditi

### LibreTranslate API mudelid

#### TranslateRequest → TranslateReult

**Taotlus** – tekstitõlke API-kõne:

Kinnisvara
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

** Tulemus** – tõlkevastus:

Kinnisvara
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### detekteerimistaotlus → tuvastamine

** Taotlus**: **Vastavus**:
**Response**: `{ Language, Confidence }`

#### TranslateFileRequest → TranslateFileReult

** Taotlus**: **Vastavus**:
**Response**: `{ TranslatedFileUrl }`

#### LibreKeel

Üks keelekirje lõpp-punktist:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Torujuhtmete aruandemudelid

#### KontrollReport

Serveri valideerimise etapi tulemus:

Kinnisvara
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### TõlkedReport

Sõnaraamatu/riigitõlke etappide tulemus:

Kinnisvara
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

#### allahindlustõlkearuanne

Märgistuse tõlkeetapi tulemus:

Kinnisvara
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### SalvestamineReport

Püsivate väljundite lõplik liitmine:

Kinnisvara
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### lavaaruanne<t>

Üldine konteiner, mis ümbritseb mis tahes aruandetüübi etapi metaandmetega:

Kinnisvara
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(arvutatud)

### Tõlketöömudelid

#### PhraseInQueue

Tõlkejärjekorra tööartikkel:

Kinnisvara
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

#### tõlkija

Struktureeritud veakirje kõigis aruannetes:

Kinnisvara
|---|---|
(keelekood, faili asukoht või lava nimi)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Üksiktõlge

Üksikkoha sõnaraamat:

Kinnisvara
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownTranslatableBlock

Ploki väljavõtmine märgistusdokumendist:

Kinnisvara
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Teksti resolutsiooni mudelid

#### Teksti lokaliseerimine Nõue → teksti lokaliseerimine Vastus

**Request** – sõnastikupõhine lokaliseerimine (kirjutatav):

Kinnisvara
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Vastus**:

Kinnisvara
|---|---|
(originaal)
(lokaliseeritud)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TekstitõlgeRequest → TekstitõlgeResponse

** Taotlus** – dünaamiline tõlge (kirjutuskaitstud):

Kinnisvara
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Vastus**:

Kinnisvara
|---|---|
(originaal)
(tõlgitud)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TekstResolutionSource

Näitab, kus lokaliseeritud/tõlgitud väärtus lahendati:

Väärtus
|---|---|
Leitud sihtkeele lokaadisõnastikus
Leiti keele vaikesõnastikus
Ei leitud; lisatakse vaikimisi sõnaraamatusse
Tagastas LibreTranslate
Tagastatud nagu on ilma resolutsioonita

### Jagatud liigid

#### Riigi määratlus

Ainult lugemisõigusega kirje :

Kinnisvara
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Võrdlustingimused

Filtri tingimused hindamiseks:

Kinnisvara
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### VigaRespons

Lihtne API veaümbris:

Kinnisvara
|---|---|
| `Error` | `string?` |
