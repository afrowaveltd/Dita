# Modele de date

Spaţiul de nume defineşte toate structurile de date folosite în sistemul de localizare şi traducere .

## Prezentare generală model

### Configurare

#### Setări de traducere automată

Model de configurare legat de la . Controls LibreTraduceți conexiunea serverului și comportamentul conductei.

Proprietate
|---|---|---|---|
LibreTraduce URL server
Dacă este necesară o cheie API
Cheia API
Limba implicită de aplicare
Limbi de exclus din traducere
Dosare rădăcină documentare
Activează rulările de conducte programate
Întârziere înainte de prima cursă
Procese-verbale între curse
LibreTraduceți obiectivul textului
LibreTraduceți obiectivul final al fișierului
Limbi libreTraduceți criteriul final
LibreTraduceți obiectivul de detectare
Întârziere între cererile de traducere
Timeout HTTP per cerere
Dacă configuraţia a fost încărcată

### LibreTraduce modele API

#### Tradu Cerere → TraduceResult

**Request**

Proprietate
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Result**

Proprietate
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### Detectează cererea → Detectări

**Request**: **Response**:
**Response**: `{ Language, Confidence }`

#### Tradu Cerere → TraducereResult

**Request**: **Response**:
**Response**: `{ TranslatedFileUrl }`

#### librelimbă

Intrarea în limba unică din obiectivul final:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Modele de raport de conductă

#### Raport de verificare

Rezultatul etapei de validare a serverului:

Proprietate
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Raport de traduceri

Rezultatul etapei de traducere a dicţionarului/ţării:

Proprietate
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

#### MarkdownTranslations Report

Rezultatul etapei de traducere Markdown:

Proprietate
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Raport de stocare

Agregarea finală a realizărilor persistente:

Proprietate
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### SceneReport<T>

Container generic care împachetează orice tip de raport cu metadate de etapă:

Proprietate
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(computat)

### Modele de lucru de traducere

#### frazainqueue

Punct de lucru pentru coada de traducere:

Proprietate
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

#### TraducereError

Înregistrarea erorilor structurate efectuată în toate rapoartele:

Proprietate
|---|---|
(cod de limbă, cale de fișier sau nume de scenă)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Transformare unică

Dicţionar unic local:

Proprietate
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### bloc markdowntranslatable

Bloc extras dintr-un document Markdown:

Proprietate
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Modele de rezoluție text

#### Localizare text Cerere → Localizarea textului Răspuns

**Request**

Proprietate
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Responsa**:

Proprietate
|---|---|
(original)
(localizat)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Traducerea textului Cerere → TextTranslationResponse

**Request**

Proprietate
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Responsa**:

Proprietate
|---|---|
(original)
(tradus)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### sursă de rezoluție text

Identifică unde a fost rezolvată o valoare localizată/tradusă de la:

Valoare
|---|---|
Găsit în dicționar local pentru limba țintă
Găsit în dicționarul de limbă implicită
Negăsit; adăugat dicționarului implicit
Returnat de libreTranslate
Returnat ca fiind fără rezoluție

### Tipuri partajate

#### Definirea țării

Numai citire de la:

Proprietate
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### ComparațieCondiție

Stare filtru pentru evaluare:

Proprietate
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### EroareResponsă

Plicul de eroare simplu API:

Proprietate
|---|---|
| `Error` | `string?` |
