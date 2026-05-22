# veri modelleri

Namespace yerelleşme ve çeviri sistemi boyunca kullanılan tüm veri yapıları tanımlar - API istek/response çiftlerinden boru hattı raporları ve pano anlıkları.

## Model genel bakış

### Yapı oluşturma

#### OtomatikTranslationSettings

Yapı modeli .. Kontroller LibreTranslate server bağlantısı ve boru hattı davranışı.

Emlak
|---|---|---|---|
LibreTranslate server URL
Bir API anahtarı gerekli olsun
API anahtar anahtarı
Uygulama varsayılan dil
Çeviriden dışlanmak için diller
Dokümantasyon kök yönetmenleri
Enable planlanan boru hatları çalışır
İlk önce gecikme
Mesafeler arasında koşmak
LibreTranslate text endpoint
LibreTranslate file endpoint
LibreTranslate dilleri endpoint
LibreTranslate algılama endpoint
Çeviri talepleri arasında gecikme
HTTP zamanout per request
Yapının yüklenmesi

### LibreTranslate API modelleri

#### TranslateRequest

**Request** - metin çevirisi API çağrısı:

Emlak
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
–
–
–
| `ApiKey` | `string?` | `"api_key"` | `null` |
–

**Result** - çeviri yanıtı:

Emlak
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### ReRequest → Analizler

**Request**: **Response**:
**Response**: `{ Language, Confidence }`

#### TranslateFileRequest

**Request**: **Response**:
**Response**: `{ TranslatedFileUrl }`

#### Libredil

Tek dil uç noktasından giriş:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Boru raporlama modelleri

#### kontrol

Sunucu doğrulama aşamasının sonucu:

Emlak
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### ÇevirilerReport

Sözlük / ülke çeviri aşamalarının sonucu:

Emlak
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

Markdown çeviri aşamasının sonucu:

Emlak
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### depolama

Kalıcı çıktıların son aggregasyonu:

Emlak
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### AşamaReport<T>

Herhangi bir rapor türünü sahne metadata ile sarmalayan Genric konteyner:

Emlak
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(i̇slâm)

### Çeviri iş modelleri

#### cümle

Çeviri kuyruğu için iş öğesi:

Emlak
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

#### TercümeErroror

Tüm raporlarda yapılan yapısal hata kaydı:

Emlak
|---|---|
(i̇ngilizce kodu, dosya yolu veya sahne adı)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### tek geçiş

Tek yerel sözlük:

Emlak
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownTranslatableBlock

Bir Markdown belgesinden alıntılanan blok:

Emlak
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Text Çözüm modelleri

#### TextLocalization → TextLocalization Yanıt

**Request** - sözlük tabanlı yerelleştirme (writable):

Emlak
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Emlak
|---|---|
(orijin)
(yerelleştirilmiş)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### metin çevirisi

**Request** – dinamik çeviri (yalnızca):

Emlak
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Emlak
|---|---|
(orijin)
(translated)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextResolution Source

Yerelleştirilmiş/translated değerin nereden çözüldüğünü belirtir:

Değer değeri
|---|---|
Hedef dil için yerel sözlükte bulundu
Varsayılan dil sözlüğünde bulundu
Bulunamadı; varsayılan sözlüğe eklendi
LibreTranslate tarafından geri döndü
Karar olmadan geri döndü

### Ortak türleri

#### ÜlkeDefinition

Sadece giriş:

Emlak
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### KarşılaştırmaCondition

Değerlendirme için filtre durumu:

Emlak
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### HataResponse

Basit API hatası:

Emlak
|---|---|
| `Error` | `string?` |
