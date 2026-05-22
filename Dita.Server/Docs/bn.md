# স্বয়ংক্রিয় অনুবাদ সার্ভিসের পরিবর্তন সম্বন্ধে সংক্ষিপ্ত তথ্য

## সারসংক্ষেপ

এই ডকুমেন্টটি ডিটা স্বয়ংক্রিয় অনুবাদ সার্ভিসের সব পরিবর্তনকে সংক্ষেপে তুলে ধরেছে, যার মধ্যে স্থাপত্যের নতুন নতুন, নতুন বৈশিষ্ট্য, উন্নতি এবং স্থানীয় উন্নতির বর্ণনা রয়েছে।.

## পরিবর্তন সংরক্ষণ করো

### মুছে ফেলা ব্যাক- এন্ডের জন্য

একটি লাইটওয়েট অর্কেস্ট্রার দ্বারা চিহ্নিত চারটি বিশেষ পরিষেবাতে মনোলিথিকে চিহ্নিত করা হয়েছে:

- **Backend- র্‌যান্ডেন্সার** - পাইপ-লাইন অর্কেস্ট্রাট (server, স্থায়ী প্রতিনিধি, সমস্যার সমাধান)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **KasalR প্রকাশকারী** - রিয়েল টাইম রিপোর্ট দ্বারা রিপোর্ট
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### উপকার

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## নতুন বৈশিষ্ট্য

### লাইভ অনুবাদ মনিটর

**শূণ্য**

একটি নতুন অ্যাডমিনস্ট্রেশন পাতা যা অনুবাদ লাইনের আসল সময় প্রদর্শন করে:

- সময় উৎপন্ন সমস্ত সংকেত প্রদর্শন করা হবে
- রঙ-কোডকৃত বার্তার ধরন (নীল=০, সবুজ=Pard, লাল, লাল=title)
- স্বয়ংক্রিয়-সংযোগ সহ সংযোগের অবস্থা চিহ্নকারী ব্যানার
- বার্তা প্রতিক্রিয়া এবং JSON হতে এক্সপোর্ট করুন

### নাম আড়াল করো

স্থানীয়ভাবেকরণ ব্যবস্থা বর্তমানে places (উন্নয়নের) বিভিন্ন ভাষায় বর্হিভূত বাগ সংশোধনের জন্য ব্যবহার করা হচ্ছে:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

IMAP-র বৈশিষ্ট্য
- স্লাইড-শোতে প্রদত্ত মান উন্নত হবে অথবা সংরক্ষণ করা হবে
- দুর্নীতি রোধ করার জন্য অনুবাদের সময় স্বয়ংক্রিয় মাস্ক/পুনরাবৃত্তি
- সুনির্দিষ্ট অবস্থানের সাথে সুসংগত সুসংগত সুসংগত স্থান পরিবর্তন করুন

### বর্ধিত অনুবাদ

ছোট করে দেখার ক্ষেত্রে সব ফাইল চিহ্নিত করা হয়েছে:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **শূণ্য**: অনুবাদের অবস্থা পুনরারম্ভের উদ্দেশ্যে অ্যাপ্লিকেশন পুনরারম্ভ করা হয়েছে

### উন্নত পুনরায় গণনাকারী লজিক

আবার তিনটি স্তর:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### সিগনাল রিপোর্ট

সকল পাইপলাইন অপারেশনের জন্য রিয়েল টাইম রিপোর্ট:

- প্রতিটি পর্যায়ে ইভেন্ট প্রকাশ করা হয়
- কোনো ইভেন্ট সনাক্তকরণ করা হবে
- ঘটনার বিস্তারিত তথ্য, ত্রুটির কোড, বার্তা
- প্রতিটি সংঘটনের মধ্য দিয়ে অনুক্রম

## কনফিগারেশন পরিবর্তন

### অ্যাপ্লিকেশন নিয়ন্ত্রণ ব্যবস্থাname

কোনো পরিবর্তন হয়নি । উপস্থিত কনফিগারেশন এখনো কাজ করছে:

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00"
  }
}
```

### নতুন পরিসেবা

নিবন্ধিত:

- /
- `TranslationRetryService`
- /
- /
- /
- /

ক্লায়েন্ট সংযোগের জন্য সিগন্যালআর হাব সংযুক্ত করা হয়েছে।.

## পরীক্ষা

### অবস্থা পরীক্ষা করুন

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- এই বিষয়ে নতুন পরীক্ষার সংবাদ:
  - Strigi পরিসেবা পরিচালনা করুন
  - ব্যাক- এন্ড সার্ভিসName
  - Joonsbrougher কনফিগারকারী

### পরিচয়

- সুনির্দিষ্টভাবে সঞ্চালিত হলে, একাধিক পরীক্ষা দ্বারা একই ফাইল প্রদর্শন করা হয়। বিচ্ছিন্ন হয়ে গেলে এটা চলে যায়।.

## নতুন ফাইল কাঠামো

### পরিসেবা

- — পাইপলাইন অর্কেস্ট্রা
- — দেশ নাম অনুবাদ
- — JSON অভিধান সুসংগতি
- — মার্ক অনুবাদ
- — সিগনাল প্রকাশ
- — পদাশ্রিত নির্দেশক
- — প্রকাশনাকারী ইন্টারফেস
- — দেশ সার্ভিস ইন্টারফেস
- - স্থানীয় পরিসেবা ইন্টারফেস
- — নথি ইন্টারফেস
- — স্টর্সট্রাড ইন্টারফেস (চেস্টার)
- — প্রতি উইজেটের মিটা-ডাটা অনুবাদ

### আপডেট করা পরিসেবা

- যোগ করা হয়েছে
- নতুন পরামিতির জন্য আপডেট করা হয়েছে
- — আবর্জনার বাক্স
- ইন্টারফেস রাখুন

### নতুন অ্যাডমিন পেজ

- — বাস্তব সময় নিরীক্ষণ
- — পৃষ্ঠা মডেল

### নথিপত্রের মধ্যে নতুন ডকুমেন্টেশন অনুসন্ধান করা হবে

- আপডেট করা হয়েছে
- নির্দেশনা খুঁজুন
- - ড্যাশবোর্ড ব্যবহার গাইড
- — প্রযুক্তিগত স্থাপত্য ধারণা

## পূর্ববর্তী সামঞ্জস্য

সকল পরিবর্তন যোগ করা হয়েছে:

- উপস্থিত স্থানীয়করণ সংক্রান্ত কোড (মূল পরিবর্তন)
- পার্টিশন সংক্রান্ত তথ্য পরিবর্তন করা হয়নি
- বিদ্যমান JSON অভিধান ফরম্যাট পরিবর্তন করা হয়নি
- বিদ্যমান মার্কুস গঠন অপরিবর্তিত রয়েছে
- বার্তা একই বিন্যাস ব্যবহার করে

## মাইগ্রেশন পাথ

অভিবাসনের প্রয়োজন নেই। পুনর্নবীকরণ অভ্যন্তরীণ:

1. প্রাচীন রেফারেন্স হিসাবে সংরক্ষিত ছিল এবং পরে প্রতিস্থাপন করা হয়েছিল
2. ডি- আই নিবন্ধনের নতুন ইন্টারফেস প্রয়োগ করা হয়েছে
3. সকল বিদ্যমান ক্রেতারা কোন পরিবর্তন দেখতে পান না

## উন্নতি

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## ব্যয়িত উন্নতি

পরিকল্পনা করা উন্নতি:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## পরিচিতি

এই অনুবাদ সার্ভিসের সাথে প্রশ্ন বা প্রশ্ন করার জন্য প্রতিটি মডিউলের ডিরেক্টরী বা উন্নয়ন দলের সাথে যোগাযোগ করুন।.
