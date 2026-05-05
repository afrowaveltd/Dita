# অনুবাদের নকশা

এই ডকুমেন্টটি ডিটারটার স্বয়ংক্রিয় অনুবাদের পদ্ধতি সম্পর্কে বর্ণনা করে, যার মাধ্যমে উন্নতি, দক্ষতা, পরীক্ষা এবং দৃঢ়তা বজায় রাখা যায়।.

## ডিজাইন লক্ষ্য

এই পুনরায় সমর্থনের বিষয়টি মূল মনোলিথির ডিজাইনের সাথে বেশ কিছু উদ্বেগের বিষয় তুলে ধরেছে:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## সার্ভিস ডি রিডার

### ব্যাক- এন্ড সার্ভিস (বাচিয়ার)

**REConsits**:
- পাইপ-লাইন সাইকেল পরিচালন ব্যবস্থা (আরম্ভ, সমাপ্তি, সংশোধন, সমস্যার সমাধান)
- Smaprohole-ভিত্তিক কনফিউজেন্সি কন্ট্রোল (প্রাথমিক কাজ করে)
- সার্ভার সংক্রান্ত বৈধতা (হালকা, ভাষা, কনফিগারেশন)
- সাব-সেবার জন্য কর্ম

**শূণ্য**
- অনুবাদ যুক্তি
- সুনির্দিষ্ট বিন্যাসের জন্য ফাইল / ইনপুট/ আউটপুট
- পুনরায় যুক্তি করুন

### বিভিন্ন দেশ

**REConsits**:
- ডিরেক্টরি থেকে পড়া হবে
- ডিফল্ট লোকেইলের অভিধানের মধ্যে নাম সুসংগত করা হবে
- লক্ষ্য বিশিষ্ট দেশের নাম অনুবাদ করা হচ্ছে
- অনুবাদের পর প্রতিটি লক্ষ্য অভিধান সংরক্ষণ করো

**Key behaviors**:
- ডিফল্ট ভাষা যদি ইংরেজী হয়, তাহলে দেশ হিসেবে সংরক্ষণ করা হবে
- যদি ডিফল্ট ভাষা অন্য হয়, তবে ইংরেজী ভাষার নাম প্রথমে ডিফল্ট ভাষা থেকে অনুবাদ করা হবে
- প্রতিটি ভাষা স্বাধীনভাবে নিজেদের ভিন্ন পদ্ধতিতে পরিচালিত হচ্ছে

### স্থানীয়করণ

**REConsits**:
- পূর্বে ব্যবহৃত ডিফল্ট অভিধানের সাথে তুলনা করে, সনাক্ত করুন
- প্রতিটি লক্ষ্য ভাষায় কী যোগ করো
- প্রতিটি চিহ্নিত ভাষার তালিকা থেকে মুছে ফেলা হবে
- পরবর্তী তুলনার জন্য পর্দা সংরক্ষণ করুন

**Key behaviors**:
- নিজে হাতে অনুবাদ সব সময় গুরুত্ব গ্রহণ করো (কখনো না)
- কী যোগ করা হয়েছে এবং অবিলম্বে যোগ করা
- মুছে ফেলা কী অবিলম্বে মুছে ফেলা হবে
- সব ভাষা সাফল্যের সাথে Scround করা হয়ে গেছে

### দস্তাবেজ সংরক্ষণ করা হচ্ছে

**REConsits**:
- দেখাও ক্ষেত্রের & উপর
- SHA-২৫৬ সহযোগে উৎস ফাইল রূপান্তর করুন
- আপডেটের সময়
- টার্মিন্যালের মাধ্যমে একত্রিত করার জন্য ব্লক-কে প্রস্তুত করা হবে
- অনুবাদের পর মার্কুস গঠন করুন
- প্রতিটি লক্ষ্য ফাইলের পৃথকরূপে সংরক্ষণ করুন

**Key behaviors**:
- ব্লক- লেভেল গ্যাব্রলারিটি: শিরোনাম, বস্তুর তালিকা আলাদা করে লেখা আছে
- গ্রাফিকাল ট্র্যাক যার ফলে পড়া/ স্থানীয় ভাষাতে সফল হতে পারে
- সফল ব্লক না করে পরবর্তী রানে পুনরায় স্থাপন করতে ব্যর্থ
- কাঠামোর মাত্রা নিশ্চিতকরণ, তালিকা, কোড ব্লক, ইত্যাদি নিশ্চিত করুন।

## পুনরায় প্রচেষ্টার নীতি

সিস্টেম দ্বারা তিনটি স্তর থেকে :

### স্তর ১ — HTTP Herible Medialates (অভিব্যক্তি)

- ৫টি প্রচেষ্টা করার পর, ১, ২, ৩, ৪, ৫)
- নেটওয়ার্ক অগ্রসর হতে পারে, ৫x ভেতর আঘাতকারী নেটওয়ার্ক, এবং সাময়িক ত্রুটির সূচনাName
- HTTP ক্লায়েন্ট কনফিগারেশনের মধ্যে সনাক্ত করা হয়েছে

### স্তর ২ - স্ট্রং (ট্রান্সট্রান্স অফ গ্রেঞ্জ)

- 3 সেকেন্ডের বিলম্বের পরে
- HTTP-level ধইরা ক্লান্ত হওয়ার পর সম্পূর্ণ অনুবাদ অনুরোধ
- এই স্তরে মাস্ক ও পুনর্স্থাপনের ক্ষেত্রে প্রয়োগ করা হবে

### স্তর ৩ — ব্লক (ডিট্রান্সমিটার সার্ভিস)

- বিচ্ছিন্ন ব্লকের মধ্যে হওয়া বিচ্ছিন্ন ব্লক
- পরবর্তী পাইপে স্বয়ংক্রিয়ভাবে চালু করা হবে
- সফল ব্লক কখনো পুনরায় স্থাপন করা হয়নি

## ডাটা প্রবাহ

### JSON অভিধান অনুবাদ

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### মাইন অনুবাদ

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### দেশের নাম

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## অবস্থা স্ব-পরীক্ষণ

### কর্কোভাডোস

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Pros**: পূর্বে সঞ্চালিত উপস্থিত লগের তালিকা সক্রিয় করুন

### হ্যাশ ফাইল

- **মার্কেট**: উৎস ফাইলের পাশে
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Prous**: অপ্রয়োজনীয় পুনঃসংযোগ এড়ানোর জন্য esport উৎস পরিবর্তন করা হচ্ছে

### মিটা-ডাটা

- **মার্কেট**:
- **শূণ্য**:
  - উৎস বিষয়বস্তুর হ্যাশ
- পের-ভাষার ব্লক (legens)
- সর্বশেষ আপডেটের সময়
- **Props**: শুধুমাত্র ব্লকের পুনরায় বিভক্ত করার অনুমতি প্রদান করা হয় না

### অবস্থান সংগ্রহ

- **শূণ্য**
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## ক্লাস রি রিপোর্টিং

### Shrites প্রকাশ করুন

সিগন্যাল ডিকোক্‌স থেকে অনুবাদ সার্ভিস:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### ক্রমপর্যায়

- একক মাত্র শব্দ হিসাবে বার্তা নির্বাচন করুন
- ক্রমপর্যায়ের মাধ্যমে সংখ্যাগুলো একক-প্রতিরোধ ব্যবস্থা
- ক্লায়েন্টের দূরত্ব সনাক্ত অথবা বিচ্ছিন্ন করা যায়নি

### হাব ম্যাপ

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## এক্সটেনশন

### নতুন অনুবাদ লক্ষ্য যোগ করা হচ্ছে

1. ইন্টারফেস সহ একটি নতুন ইন্টারফেস তৈরি করুন
2. ডোমেন-নির্দিষ্ট যুক্তির মাধ্যমে ইন্টারফেসটিকে ছেদ করুন
3. কনটেইনারে নিবন্ধন করুন
4. প্রবেশ করানো
5. বিদ্যমান পর্যায় থেকে কল

### স্বনির্বাচিত পুনরায় পলিসি

নতুন শব্দভাণ্ডার তৈরি করো

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### স্বনির্বাচিত সংগঠিত সময়

প্রতি:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## কনফিগারেশন

### অ্যাপ্লিকেশন নিয়ন্ত্রণ ব্যবস্থাname

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### সঞ্চালনার সময় সঞ্চালনার প্রণালী

মানসমূহ
|---------|---------|--------|
৮০
১০
৩
৩০

## প্ল্যান পরীক্ষা করা হচ্ছে

### একক পরীক্ষা

প্রতিটি সাব-সেবা স্বাধীনভাবে পরীক্ষা করা যায়:

- সফল/ফিগার অনুকরণ করতে Mock
- রিপোর্ট পরীক্ষার জন্য Mock
- I/O-র জন্য অস্থায়ী ডিরেক্টরি ব্যবহার করুন
- নেটওয়ার্ক পরিবেশে সংরক্ষণ করুন

### একত্রিত পরীক্ষা

- স্থানীয় (স্থানীয়) Library (স্থানীয়) Partiles ইনস্ট্যান্স সহ সম্পূর্ণ পাইপলাইন
- সিগনাল যাচাই করো ক্লায়েন্টদের সাথে সংযোগ করা হয়েছে
- পরীক্ষা চলছেComment
- অনুবাদের পর মার্কুস গঠন করুন

### শেষ- পরীক্ষা

- API অথবাউলer দ্বারা অনুবাদ ব্যবস্থা
- সব লক্ষ্য ভাষার ফাইল/ফোল্ডার নির্মাণ করুন
- মিটা-ডাটা ফাইল সঠিক কিনা পরীক্ষা করুন
- নিশ্চিত placesss অনুবাদ দ্বারা সংরক্ষিত হয়

## কর্মক্ষমতার বিশ্লেষণ

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Con**: ricationing sprespicationing powerty strication supports
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## মোনোলিথিক নকশা থেকে মাইগ্রেশন

মূল যুক্তি এক ক্লাসে ছিল. অভিবাসন পাথ:

1. দেশের যুক্তি হাইলাইট করুন
2. JSON যুক্তি এক্সট্র্যাক্ট করো
3. Latting যুক্তি এক্সট্র্যাক্ট করো
4. সিগনাল সম্প্রসারণ করা হবে মুক্ত/ব্যস্ত প্রকাশনা
5. Countrydir পুনরায় আরম্ভ করুন
6. শুধু প্রতিনিধি দলনেতা করার জন্যplippeor

সকল বিদ্যমান ইন্টারফেস অপরিবর্তিত রয়েছে। পাইপের কনস্যুলেটরা কোন পরিবর্তন দেখতে পাচ্ছে না।.
