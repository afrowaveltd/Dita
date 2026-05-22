# Mga Modelo ng Data

Binibigyang kahulugan ng pangalangspace ang lahat ng mga data structure na ginagamit sa ibayo ng lokalisasyon at sistema ng pagsasalin – mula sa API request/response pares hanggang sa mga tubong ulat at sshboard speciations.

## Huwaran

### Pagsasaayos

#### AutomaticTransationSettings

Configuration model na nakatali mula sa . Controls LibreTranslate server connection at acute behavior.

Mga ari - arian
|---|---|---|---|
libre transaksyone server url
Kung kailangan ang API key
Susi ng API
Pagkakapit ng default language
Mga wikang hindi isinasalin
Dokumentasyon ng root directories
Nakaiskedyul na takbo ng tubo
Pagpapaliban bago tumakbo
Mga minuto sa pagitan ng pagtakbo
libreng salin na endpoint ng teksto
talaan ng mga nilalaman
pinag - aaralang mga wika
natuklasang endpoint ng "brebre translate"
Pagantala sa pagitan ng mga kahilingan sa pagsasalin
HTTP timeout bawat kahilingan
Kung baga ang pagsasaayos ay may karga

### Mga modelong LibreTranlate API

#### transaksyonista → translationresult

**Request** — text translation API call:

Mga ari - arian
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Result** — tugon sa pagsasalin:

Mga ari - arian
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### Pag - unawa → Pagtuklas

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### isinalin angfilerequest → isalin angfileresult

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### wikang lila

Isang ipinasok na wika mula sa endpoint:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Mga modelo sa pag - uulat ng pipeline

#### Pagsusuri sa Report

Resulta ng yugto ng server na may bisa:

Mga ari - arian
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Pag - uulat ng Pagsasalin

Resulta ng mga yugto ng diksiyonaryo/country translation:

Mga ari - arian
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

#### Reportasyon ng mga MarkdownTransation

Resulta ng yugto ng pagsasalin sa Markdown:

Mga ari - arian
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Nakapagpapatibay na Reportasyon

Pangwakas na agregasyon ng patuloy na outputs:

Mga ari - arian
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Stage Report<T>

Generic container na nagbabalot ng anumang uri ng report na may metadata sa entablado:

Mga ari - arian
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(ipino)

### Mga modelo sa pagsasalin

#### Mabigat na Imahinasyon

Gumawa ng artikulo para sa pagsasalin:

Mga ari - arian
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

#### Tagapagsalin

Nakapangingilabot na rekord ng pagkakamali na nasa lahat ng ulat:

Mga ari - arian
|---|---|
( kodigo ng wika, landas ng talaksan, o pangalan ng entablado)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Single Transation

Isang diksyunaryong lokal:

Mga ari - arian
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### Ang MarkdownTranslatableBlock

Hinango sa isang dokumentong Markdown:

Mga ari - arian
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Mga modelo ng resolusyon sa teksto

#### TextLocalization Kahilingan → TextLocalization Pagtugon

**Request** — dictionary-based localization (writable):

Mga ari - arian
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Mga ari - arian
|---|---|
(orihinal)
(pinalo)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### pagsalin ng teksto → pagsalin ng teksto

**Request** — dynamic translation (read-only):

Mga ari - arian
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Mga ari - arian
|---|---|
(orihinal)
(isinasalin)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Tekstong Resolusyon

Mga opinyon kung saan ang isang lokalisado/isinalin na halaga ay nalutas mula sa:

Halaga
|---|---|
Matatagpuan sa diksyunaryong lokale para sa puntiryang wika
Matatagpuan sa diksyunaryo ng wikang default
Hindi nasumpungan; idinagdag sa diksiyonaryong default
Ibinalik ng LibreTranslate
Bumalik sa as-is nang walang resolusyon

### Mga kabahaging uri

#### Panggagamot sa Bansa

Basahin-lamang entry mula sa :

Mga ari - arian
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Paghahambing

Kalagayan para sa pagtatasa:

Mga ari - arian
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Pagkakamali

Simpleng sobre ng API error:

Mga ari - arian
|---|---|
| `Error` | `string?` |
