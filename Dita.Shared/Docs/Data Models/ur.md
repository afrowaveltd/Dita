# ڈیٹا ماڈلز

اس نام سے پتہ چلتا ہے کہ اِن تمام معلومات کو استعمال کرنے اور ترجمہ کرنے کے نظام میں استعمال کِیا جاتا ہے ۔.

## اِس سلسلے میں ایک مثال پر غور کریں ۔

### مصر

#### خود کار آرام دہ

موبائل ماڈل بند سے کنٹرولز لبرٹی سرور اتصال اور اصلاحی سلوک پر کنٹرول کرتا ہے۔.

پرنٹ
|---|---|---|---|
لبرٹی سرور %d
چاہے کوئی ایپی کلید درکار ہو۔
کلید
جگہ
ترجمے کے ذریعے زبانیں
دستاویزی فلم ڈائریکٹرز
مقررہ پائپ لائنیں چلاتی ہیں۔
پہلی دوڑ سے پہلے
چلاو
لبرٹی متن اختتام پزیر ہوتا ہے۔
لبرٹی فائل پوائنٹ ختم کرتا ہے۔
زبانوں کی زبانوں کا نقطۂ‌نظر ختم ہوتا ہے
اگر آپ کو پتہ چلتا ہے کہ آپ کا بچہ آپ سے بات کرنا چاہتا ہے تو آپ کیا کر سکتے ہیں ؟
ترجمے کی درخواستوں کے درمیان فرق
ایچ ٹی‌ٹی‌ٹی‌پی وقت کو استعمال کرنے کے لئے استعمال کریں
یو

### لبرٹی ایپی ماڈل

#### انتقالِ‌خون کی علامات

**Request** — text translation API call:

پرنٹ
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Result** — translation response:

پرنٹ
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### دُنیا کا نظارہ کرنا

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### فولڈرز دوبارہ شروع کریں

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### لبرٹی

اختتامی نقطہ سے واحد زبان داخلی:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### پائپ لائن رپورٹ ماڈل

#### جانچنا

سرور صدیقی سٹیج کے نتائج:

پرنٹ
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### ترجمے

نتیجہ ترجمہ/ ترجمہ کے مراحل:

پرنٹ
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

#### مارک ڈاؤن‌لوڈز کارپوریشن

مارک ڈاؤن ترجمہ کے نتیجہ:

پرنٹ
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### جگہ

جاری کردہ برآمدات کی حتمی تقسیم:

پرنٹ
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport\<T\>

جینیریکل سانچہ جو کسی بھی رپورٹ کی نوعیت کو اسٹیج میٹاداتا سے لپیٹ دیتی ہے:

پرنٹ
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(مقام)۔

### ترجمے کا کام ماڈل

#### چین

ترجمے کے لیے ضروری کام :

پرنٹ
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

#### ترجمہ

تمام رپورٹوں میں دائر شدہ غلطی ریکارڈ:

پرنٹ
|---|---|
( لغت کوڈ، فائل راہ یا سٹیج نام)۔
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### تنہائی

تنہائی‌پسندانہ الفاظ :

پرنٹ
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### مارک ڈاؤن‌ناسلابل بلاک

مارک ڈاؤن کی ایک دستاویز سے اخذ کردہ بلاک:

پرنٹ
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### متن دوبارہ حل شدہ ماڈل

#### متن درخواست برائے نام دوبارہ شروع

**Request** — dictionary-based localization (writable):

پرنٹ
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

پرنٹ
|---|---|
( اصل میں)
(localed)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Text Translation Reformation Textslation Response -

**Request** — dynamic translation (read-only):

پرنٹ
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

پرنٹ
|---|---|
( اصل میں)
(ترجمہ:
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### متن دوبارہ نصب کریں

جہاں مقامی طور پر قابل قدر اقدار کا تعین کیا گیا:

قیمت
|---|---|
مقصدی زبان کیلئے ایک لغت میں پائی جاتی ہے
کھوار زبان کی لغت میں پایا جاتا ہے۔
اِس کی بجائے اِس میں درج الفاظ شامل ہوتے ہیں ۔
لیبر ٹرینٹ سے واپسی
واپس آئے بغیر-نام کے

### شیئر کردہ اقسام

#### ملک فہرست

صرف داخلے کو پڑھیں:

پرنٹ
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### غیر متصل

تجزیے کے لیے شرائط:

پرنٹ
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### خامی

سادہ ایپی غلطی غلاف:

پرنٹ
|---|---|
| `Error` | `string?` |
