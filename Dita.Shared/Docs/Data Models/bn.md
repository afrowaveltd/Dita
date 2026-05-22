# ডাটা মডেল

এই সমস্ত তথ্য স্থানীয়ীকরণ এবং অনুবাদ পদ্ধতি জুড়ে ব্যবহার করা সকল ধরনের তথ্য কাঠামোকে সংজ্ঞায়িত করে- API অনুরোধ থেকে প্রাপ্ত অনুরোধ থেকে প্রাপ্ত/respons জোড়া থেকে প্রাপ্ত সংবাদ এবং ড্যাশবোর্ড-এর দৃশ্য।.

## মডেল বিশ্লেষণ

### কনফিগারেশন

#### স্বয়ংক্রিয় নিক্তি

কনফিগারেশনের ক ্ ষেত ্ রে । LibrereTranslate সার্ভারের সংযোগ এবং পাইপলাইনের আচরণ নিয়ন্ত্রণ করা হয়।.

বৈশিষ্ট্য
|---|---|---|---|
LibreTranslate সার্ভার URL
API কী আবশ্যক কিনা
API-কি
ডিফল্ট অ্যাপ্লিকেশন
ভাষার অনুবাদ থেকে বাদ দাও
নথিপত্রের মূল ডিরেক্টরি
পাইপ-লাইন আটক করা হবে
প্রথমবার সঞ্চালনের পূর্বে বিলম্ব
মিনিট শেষ
LibreTranslate টেক্সট বিন্দু শেষ করে
LibreTranslate ফাইল শেষ করে
LibraryTranslate বর্তনী শেষ হয়
Libreate srandet কার্যকর করুন
অনুবাদের অনুরোধের মধ্যে অন্তর্বর্তী বিরতি
অনুরোধের জন্য HTTP-র অনুরোধ
কনফিগারেশন লোড করা হয়েছে কি না

### LibreTlate API মডেল

#### অনুবাদ

**Request** — text translation API call:

বৈশিষ্ট্য
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Sorting** — অনুবাদ প্রতিক্রিয়া:

বৈশিষ্ট্য
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### স্বয়ংক্রীয় সনাক্তকরণ

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### অনুবাদ

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### লিব্রেয়ান

শেষাংশ থেকে একটি ভাষা এন্ট্রি ।

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### পাইপ-লাইন প্রতিবেদন মডেল

#### পরীক্ষার অনুরোধ

সার্ভার পরিচালনার বৈধ ধাপ:

বৈশিষ্ট্য
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### অনুবাদ

অভিধান/পার্দশা অনুবাদ কালের ফলাফল:

বৈশিষ্ট্য
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

#### গুরুত্বপূর্ণ বর্ণনামূলক ঘোষণা

মার্কেজের অনুবাদ মঞ্চের ফলাফল:

বৈশিষ্ট্য
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### বাগ রিপোর্ট প্রেরণ করা হচ্ছে

শেষ সমষ্টি:

বৈশিষ্ট্য
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport\<T\>

সাধারণ ফাইল যে কোনো ধরনের রিপোর্ট ধারণ করে :

বৈশিষ্ট্য
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(কম)

### অনুবাদ কাজের মডেল

#### কা. পূ

অনুবাদের সারির জন্য কাজ বস্তু:

বৈশিষ্ট্য
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

#### অনুবাদ

সকল রিপোর্টে কাঠামোকৃত ত্রুটি রেকর্ড করা হয়েছে:

বৈশিষ্ট্য
|---|---|
(ভাষার কোড, ফাইল পাথ, বা মঞ্চ নাম)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### এককমিটার

একটি লোকেইল অভিধান:

বৈশিষ্ট্য
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### মার্কট পরিত্যাগকারী বাক্স

মার্ক-আপ ডকুমেন্ট থেকে সরবরাহ করা হয়েছে:

বৈশিষ্ট্য
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### টেক্সট রেজল্যুশন মডেল

#### টেক্সট ক্যাশে  ‘% 1 লেখা হচ্ছে প্রতিক্রিয়া

**Request** — dictionary-based localization (writable):

বৈশিষ্ট্য
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Repson**:

বৈশিষ্ট্য
|---|---|
(বড়ি)
(স্থানীয়)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### টেক্সটের পরিমাণ প্রকাশ

**Request** — dynamic translation (read-only):

বৈশিষ্ট্য
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Repson**:

বৈশিষ্ট্য
|---|---|
(বড়ি)
(দীর্ঘশ্বাস)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### টেক্সট রেজোলিউশন

স্থানীয়/মুণ্ডু কোথায় একটি স্থানীয় মূল্য নির্ধারণ করা হয়েছে:

মান
|---|---|
লক্ষ্যবস্তুর জন্য লোকেইলের অভিধান পাওয়া গিয়েছে
ডিফল্ট ভাষার অভিধানের মধ্যে পাওয়া গিয়েছে
পাওয়া যায়নি; ডিফল্ট অভিধানে যোগ করা হয়েছে
BabreTranslate দ্বারা প্রাপ্ত
রিসোলিউশন ছাড়াই ফেরত দেওয়া হয়েছে

### শেয়ার করা ধরন

#### দেশ নিরাপদ

শুধুমাত্র পাঠযোগ্য ফলাফল থেকে পড়া যাবে :

বৈশিষ্ট্য
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### তুলনা

মূল্যায়নের জন্য অবস্থা:

বৈশিষ্ট্য
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### ফাইল পড়তে সমস্যা হয়েছে

সাধারণ API ত্রুটির খাম:

বৈশিষ্ট্য
|---|---|
| `Error` | `string?` |
