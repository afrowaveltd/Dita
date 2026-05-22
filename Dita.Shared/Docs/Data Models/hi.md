# डेटा मॉडल

नेमस्पेस स्थानीयकरण और अनुवाद प्रणाली में प्रयुक्त सभी डेटा संरचनाओं को परिभाषित करता है - एपीआई अनुरोध / प्रतिक्रिया जोड़े से पाइपलाइन रिपोर्ट और डैशबोर्ड स्नैपशॉट तक।.

## मॉडल अवलोकन

### विन्यास

#### स्वचालित अनुवाद सेटिंग

सम्पर्क करने का विवरण LibreTranslate सर्वर कनेक्शन और पाइपलाइन व्यवहार को नियंत्रित करता है।.

संपत्ति
|---|---|---|---|
LibreTranslate सर्वर URL
क्या एक API कुंजी की आवश्यकता है
एपीआई कुंजी
आवेदन डिफ़ॉल्ट भाषा
अनुवाद से बाहर करने के लिए भाषाएँ
दस्तावेज़ीकरण रूट निर्देशिका
अनुसूचित पाइपलाइन रन सक्षम करें
पहले रन से पहले विलंब
रन के बीच मिनट
LibreTranslate पाठ समापन बिंदु
LibreTranslate फ़ाइल समापन बिंदु
LibreTranslate भाषाओं endpoint
LibreTranslate डिटेक्शन एंडपॉइंट
अनुवाद अनुरोधों के बीच विलंब
अनुरोध के अनुसार HTTP टाइमआउट
क्या विन्यास लोड किया गया था

### LibreTranslate API मॉडल

#### अनुवादRequest → अनुवादResult

**Request ** - पाठ अनुवाद एपीआई कॉल:

संपत्ति
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Result** — translation response:

संपत्ति
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### पता लगाना

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### अनुवादFileRequest

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### भाषा

समापन बिंदु से एकल भाषा प्रवेश:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### पाइपलाइन रिपोर्ट मॉडल

#### जांच रिपोर्ट

सर्वर सत्यापन चरण का परिणाम:

संपत्ति
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### अनुवाद

शब्दकोश/देश अनुवाद चरणों का परिणाम:

संपत्ति
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

#### MarkdownTranslationReport

मार्कडाउन अनुवाद चरण का परिणाम:

संपत्ति
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### भंडारण

सतत उत्पादन का अंतिम एकत्रीकरण:

संपत्ति
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport\<T\>

जेनेरिक कंटेनर जो स्टेज मेटाडाटा के साथ किसी भी रिपोर्ट प्रकार को लपेटता है:

संपत्ति
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(computed)

### अनुवाद कार्य मॉडल

#### वाक्यांश

अनुवाद कतार के लिए कार्य आइटम:

संपत्ति
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

#### अनुवाद त्रुटि

सभी रिपोर्टों में किए गए संरचित त्रुटि रिकॉर्ड:

संपत्ति
|---|---|
(भाषा कोड, फ़ाइल पथ, या मंच का नाम)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### एकल स्थानांतरण

एकल स्थानीय शब्दकोश:

संपत्ति
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### मार्कडाउनट्रांसलेटेबलब्लॉक

एक मार्कडाउन दस्तावेज़ से निकाले गए ब्लॉक:

संपत्ति
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### पाठ संकल्प मॉडल

#### TextLocalization अनुरोध → TextLocalization जवाब

**Request ** - शब्दकोश आधारित स्थानीयकरण (लेखा):

संपत्ति
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

संपत्ति
|---|---|
(मूल)
(localized)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextTranslationRequest

**Request ** - गतिशील अनुवाद (केवल पढ़ना):

संपत्ति
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

संपत्ति
|---|---|
(मूल)
(translated)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### पाठ संसाधन

यह दर्शाता है कि स्थानीयकृत/translated मूल्य कहाँ से हल किया गया है:

मूल्य
|---|---|
लक्ष्य भाषा के लिए स्थानीय शब्दकोश में स्थापित
डिफ़ॉल्ट भाषा शब्दकोश में पाया
नहीं मिला; डिफ़ॉल्ट शब्दकोश में जोड़ा गया
LibreTranslate
प्रस्ताव के बिना वापसी

### साझा प्रकार

#### देश परिभाषा

केवल प्रवेश से :

संपत्ति
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### तुलना

मूल्यांकन के लिए फ़िल्टर स्थिति:

संपत्ति
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### त्रुटि

सरल एपीआई त्रुटि लिफाफा:

संपत्ति
|---|---|
| `Error` | `string?` |
