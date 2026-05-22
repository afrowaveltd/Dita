# Datu modeļi

Vārdtelpa definē visas datu struktūras, ko izmanto visā lokalizācijas un tulkošanas sistēmā — no API pieprasījumu/atbildes pāriem līdz cauruļveida atskaitēm un paneļa momentuzņēmumiem.

## Parauga pārskats

### Konfigurācija

#### Automātiskā tulkošanaIestatījumi

Konfigurācijas modelis piesaistīts no . Vadība LibreTulkojiet servera pieslēgumu un cauruļvadu uzvedību.

Īpašība
|---|---|---|---|
LibreTulkot servera URL
Vai nepieciešama API atslēga
API atslēga
Programmas noklusētā valoda
Valodas, kas jāizslēdz no tulkojuma
Dokumentācija
Ieslēgt plānotās cauruļvada darbības
Aizture pirms pirmās palaišanas
Minūtes starp braucieniem
LibreTulkot tekstu
LibreTulkot faila beigu punktu
LibreTulkot valodas galarezultāts
LibreTulkošanas noteikšanas beigu punkts
Aizture starp tulkošanas pieprasījumiem
HTTP noildze uz vienu pieprasījumu
Vai tika ielādēta konfigurācija

### LibreTulkot API modeļus

#### TulkotRequest → TulkotResult

**Pieprasījums** – teksta tulkošanas API izsaukums:

Īpašība
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Rezultāts** – tulkošanas atbilde:

Īpašība
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### NoteiktRequest → Detections

**Pieprasījums**: **Atbilde**:
**Response**: `{ Language, Confidence }`

#### TulkotFileRequest → TulkotFileResult

**Pieprasījums**: **Atbilde**:
**Response**: `{ TranslatedFileUrl }`

#### LibreLanguage

Viens ieraksts no galapunkta:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Cauruļvadu atskaites modeļi

#### Pārbaudesziņojums

Servera apstiprināšanas posma rezultāts:

Īpašība
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### tulkošanas ziņojums

Vārdnīcas/valsts tulkošanas posmu rezultāts:

Īpašība
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

#### AtzīmēšanaTranslationsReport

Nosaka tulkošanas posma rezultāts:

Īpašība
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Saglabāt atskaiti

Noturīgo rezultātu galīgā apkopošana

Īpašība
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport <T>

Vispārējs konteiners, kas aptin ziņojumu tipu ar posma metadatiem:

Īpašība
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(skaitlis)

### Tulkošanas darbu modeļi

#### FrāzeInQueue

Darba postenis tulkošanas rindai:

Īpašība
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

#### Tulkošana

Strukturēts kļūdu ieraksts visos ziņojumos:

Īpašība
|---|---|
(valodas kods, faila ceļš vai posma nosaukums)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Vienots tulkojums

Viena lokalizācijas vārdnīca:

Īpašība
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### AtzīmēšanaTransplaterableBlock

Izspiests bloks no iezīmēšanas dokumenta:

Īpašība
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Teksta izšķirtspējas modeļi

#### TekstsLocalization Pieprasījums → Teksta lokalizācija Atbildes reakcija

**Pieprasījums** – uz vārdnīcu balstīta lokalizācija (rakstāms):

Īpašība
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Response**:

Īpašība
|---|---|
(oriģināls)
(lokalizēta)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TekstsTulkojumsPieprasījums → TekstsTulkojumsAtbilde

**Pieprasījums** – dinamisks tulkojums (tikai lasāms):

Īpašība
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Response**:

Īpašība
|---|---|
(oriģināls)
(tulkots)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Avots

Identificē, kur lokalizēta/tulkota vērtība ir atrisināta no:

Vērtība
|---|---|
Atrasta mērķa valodas locale vārdnīca
Atrasts noklusētajā valodas vārdnīcā
Nav atrasts; pievienots noklusētajai vārdnīcai
LibreTulkojis
Atpakaļ nodotas bez rezolūcijas

### Dalītie tipi

#### ValstsDefinīcija

Tikai lasāms ieraksts no:

Īpašība
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### SalīdzinājumsNosacījums

Filtra stāvoklis vērtēšanai:

Īpašība
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### KļūdaResponse

Vienkārša API kļūdas aploksne:

Īpašība
|---|---|
| `Error` | `string?` |
