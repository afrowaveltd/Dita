# Data Modelləri

The . . . . . . . . . . . . . .

## Model baxış

### Konfiqurasiya

#### Avtomatlaşdırma

Konfiqurasiya modeli də bağlıdır. Controls LibreTranslate server bağlantısı və boru yolu davranışı.

Property
|---|---|---|---|
Qeydiyyatdan keçin
Bir API əsas tələb olunur
Axtarış
Application default dil
Xüsusiyyətlər çeviriciləri
Sertifikatlaşdırma üsulları
Müasir planlaşdırılmış boru
Ilk rundan əvvəl
Minutes
LibreTranslate mövzu
LibreTranslate fayl uçpoint
LibreTranslate dillər endpoint
LibreTranslate aşkar endpoint
Müəlliflik istəyi
Yadda saxla
Oxunub:

### LibreTranslate API modelləri

#### Translate → →

**Request** — məhsul çeviri API çağrı:

Property
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Result** - çeviri cavab:

Property
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### → İmtahanları

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### Axtarış

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### Qeydiyyat

Endpoint-dən bir dil giriş:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Line hesabat modelləri

#### Bakı

Server validation məhsulunun nəticəsi:

Property
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Tarix

Səviyyə / sərgi mövzuları:

Property
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

#### qeyd

Markdown tərcümə mərkəzinin nəticəsi:

Property
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Qeydiyyat

Davamlı çıxışların son aggregation:

Property
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Stage <T>

Hesab metadata ilə hər hansı bir hesabat nömrəsini gəlir Genric konteyner:

Property
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(

### Kompüter iş modelləri

#### PhraseInQueu

Komponent sıra üçün iş maddəsi:

Property
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

#### Axtarış

Bütün hesablarda qeyd edilməsi:

Property
|---|---|
(dil kodu, fayl yolu, və ya məhsul adı)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Axtarış

Single yerli sözlər:

Property
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### Axtarış

Markdown səhifəsindən çıxarılmış blok:

Property
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Text

#### TextLocalization Yadda saxla  Response

**Request** —  dictionary-based localization (writable):

Property
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Property
|---|---|
(
(yerləşdirilmiş)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### məhsullar

**Request** — dinamik çeviri (read-only):

Property
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Property
|---|---|
(
(
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Tarix

Yerlileştirilmiş/translated dəyərinin qarşılaşdırıldığı yerləşdirilir:

Qeydiyyat
|---|---|
Qarşı dil üçün yerli sözlər tapdı
Default dil sözdə tapılmışdır
Not found; default sözlərinə əlavə
LibreTranslate
Qarışıqsız as-is

### Xüsusi növlər

#### Qeydiyyat

Yalnız giriş:

Property
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Comparison

Konfrans üçün Filter statusu:

Property
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Yadda saxla

Yadda saxla

Property
|---|---|
| `Error` | `string?` |
