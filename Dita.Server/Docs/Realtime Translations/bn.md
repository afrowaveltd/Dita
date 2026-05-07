# প্রকৃত অনুবাদ

এই ডকুমেন্টটি একটি লাইভ পরীক্ষা ইনপুট যার সাহায্যে স্বয়ংক্রিয় অনুবাদ পাইপ-লাইনের জন্য লেখা হয়। কোনো পরিবর্তন করা হলে, পরবর্তী নির্বাচনের মধ্যে উপস্থিত সকল ভাষার ফাইল পুনরায় প্রকাশ করা হবে ।.

## স্থাপত্য ধারণা

এই অনুবাদের পাইপ পুন:স্থাপন করা হয়েছে একটি স্বায়ত্তশাসনের স্থাপত্যে যার চারটি বিশেষ সাব- সার্ভিস রয়েছে।

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

প্রত্যেক সাব সার্ভিস স্বাধীনভাবে কাজ করে আর সিগন্যালআরের মাধ্যমে রিপোর্ট করে।.

## সার্ভিস কি কাজ করে

এই সার্ভিসটি একটি শিডিউলে পরিচালনা করে এবং একটি ৫-টি পাইপ চালু করে। এটি কার্যকরকরণ, দেশ সুসংগতি, JSON অভিধান সুসংগতি, মার্ক্‌স ফাইল অনুবাদ এবং ক্রমাগত ফলাফল প্রদান করে। সংকেতের উপর প্রকৃত কালানুক্রমিক ইভেন্টের প্রতিটি পর্যায় সমান করে যাতে সংযুক্ত ক্লায়েন্টরা কাজ করে।.

## পাইপ-লাইন থ্রেড

### পর্যায় ১ - su

যে কোন অনুবাদের কাজ শুরু হওয়ার আগে, সার্ভিসটি নিশ্চিত করে যে সকল নিয়ম-কানুন এতে সন্তুষ্ট:

- কনফিগারেশন বিভাগটি উপস্থিত এবং বৈধ হওয়া আবশ্যক ।.
- LibreTranslate সার্ভারের একটি গ্রহণযোগ্য স্বচ্ছতার মধ্যে সাড়া দিতে হবে।.
- অনুবাদ সার্ভারে উপলব্ধ ভাষার তালিকা প্রাপ্ত হয়েছে।.
- কনফিগার করা ডিফল্ট ভাষাকে তালিকার মধ্যে উপস্থিত থাকা আবশ্যক।.
- কোনো সমর্থিত ভাষার জন্য  পর্লগ JSON ফাইল পাওয়া যায়নি । স্বয়ংক্রিয়ভাবে নির্মিত হয় ।.

যদি পরীক্ষা করা হয়, পাইপ বন্ধ করে দেওয়া হবে... ...আর একটা মেসেজ দেয়া হবে.

### শোক ২ — অনুবাদ

শুধুমাত্র পাঠযোগ্য ক্যাটালগের মধ্যে দেশ পুনরায় স্থাপন করা হবে (শুধুমাত্র পাঠযোগ্য)।.

- অ্যাপ্লিকেশনের ডিফল্ট ভাষা ইংরেজী হলেও, প্রতিটি দেশের নাম অনুবাদ বিনা সংরক্ষণ করা হয় ।.
- ভাষা যদি অন্য কোন ভাষা হয়, তাহলে ইংরেজী ভাষার নাম প্রথমে সেই ভাষায় অনুবাদ করা হয়, এবং এর ফলে ডিফল্ট অভিধানের মধ্যে অন্তর্ভুক্তি হয়ে পড়ে।.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- ইতোমধ্যে পরিবর্তিত এন্ট্রিগুলো মুছে ফেলা হয়েছে।.
- যদি একটি অনুবাদ ব্যর্থ হয়, তাহলে সার্ভিসটি পরবর্তী ভাষায় যাওয়ার আগে ৩০ সেকেন্ডের বিলম্বের সাথে ৩ বার ব্যর্থ হয়।.

### স্থায়ী ৩ — Jojon ফাইল অনুবাদ করুন

সার্ভিসটি বর্তমান ডিফল্টাইজেশন অভিধানের সাথে আগের রান থেকে সংরক্ষিত একটি ছবি তুলনা করেছে:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- নিজে থেকেই অনুবাদ করা হয়। কোনো লক্ষ্য অভিধানে যদি ইতিমধ্যেই একটি কী এর মান থাকে, তাহলে যে এন্ট্রিটি অপরিবর্তিত থাকবে না তা যদি উল্লেখ করা হয়।.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- অনুবাদ যদি কোনো নির্দিষ্ট ভাষার জন্য ব্যর্থ হয়, তাহলে সার্ভিস স্বয়ংক্রিয়ভাবে ব্যর্থ হয় । শুধুমাত্র স্থায়ী ত্রুটি দেখা দিয়েছে (যেমন, অসমর্থিত ভাষা) ।.
- দৌড়ের পরে, বর্তমান ডিফল্ট অভিধানটি পরবর্তী তুলনার জন্য নতুন করে সংরক্ষণ করা হয়।.

সব অভিধানই মানুষের পড়ার যোগ্যতার জন্য লিখিত চাবি ও ওপেনপিএলপিআর চালু করে ।.

### Stage 4 - উপর থেকে অনুমোদন ফাইল অনুবাদ করুন

এই পরিসেবা দ্বারা ডাউনলোডযোগ্য নথিপত্রগুলি root সহ ফাইলগুলি লেখা যাবে: (ডিফল্ট)

1. সোর্স ফাইলের বিষয়বস্তু পড়া ও SHA-5 হ্যাশের মধ্যে উপস্থিত রয়েছে।.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. পূর্ববর্তী সঞ্চালিত হ্যাশগুলি (যেমন উৎস ফাইলের পরবর্তী একটি ফাইল) দ্বারা সংরক্ষণ করা হয়, অথবা বর্তমান হ্যাশের সাথে এর তুলনা করা হয় ।.
4. প্রতিটি লক্ষ্যের জন্য সংশ্লিষ্ট ভাষার জন্য সংশ্লিষ্ট ফাইল পরীক্ষা করা হয় ।.
5. যে কোন লক্ষ্য ফাইল অনুপস্থিত, এমন একটি পৃথক হ্যাশplacements, ব্যর্থকরণ, অথবা বিচ্ছিন্ন ব্লকের মধ্যে পুনরায় স্থাপন করা হচ্ছে ।.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. সাফল্যের সাথে প্যারিটিসহ ফাইল একত্রিত করার জন্য বৈধ করা হয় (অর্থাৎ উৎসের উপর ফোকাস করার জন্য, কোড ব্লক, লিঙ্কিং, লিঙ্কিং চিহ্ন, গাঢ় এবং HTML ট্যাগের আগে থেকেই)।.
8. একটি সোর্স- এর সফল হলে, নতুন হ্যাশগুলি উৎসের পরবর্তী অংশে সংরক্ষণ করা হবে। সোর্সের পাশে লেখার সময় (শুধুমাত্র পাঠ করার জন্য), অস্থায়ী ডিরেক্টরিতে এই হ্যাশটি ফেরত দেওয়া হয়।.
9. যদি কোনো লক্ষ্য অনুবাদ বৈধ না হয়, তাহলে মিটা চিহ্নগুলো মুছে ফেলা হবে, যাতে পরবর্তী রানে আবার পরীক্ষা করা হয় ।.

### স্লাইড ৫ - সংরক্ষণ করুন

একটি বিস্তৃতির আয়োজন করা হয়েছে এবং প্রকাশ করা হয়েছে। এর মধ্যে রয়েছে:

- UTC শুরু এবং সমাপ্তি চিহ্ন।.
- সংরক্ষিত Scle JSON ফাইলের গণনা, সংরক্ষিত ফাইল, হ্যাশ তালিকা, এবং ফল-ব্যাক হ্যাশ-এর মাধ্যমে সংরক্ষিত।.
- চালানো হলে যে কোনো সময় সংগ্রহে ত্রুটি দেখা দিয়েছে।.
- পের-ভাষার অনুবাদ পরিসংখ্যান (স্বৈজ্ঞানিক গণনা, গণনা, ভুল গণনা)।.

## ক্লাস নতুন বার্তার খাম

প্রত্যেক অগ্রগতির ইভেন্ট নিম্নলিখিত ক্ষেত্র সহ একটি অংশ হিসাবে পাঠানো হয়:

যে কোনো ক্ষেত্র
|-------|------|-------------|
বর্তমানে চলমান পাইপলাইনের জন্য অগ্রাহ্য করা হবে
এককেণ্ডিক কাউন্টার আরম্ভ করে 1 থেকে আরম্ভ হয়
বার্তার ধরন
বার্তার মূল অংশে সীমা
সময় ধার্য করা হয়নি
অবস্থা সীচক আইকনটি ঝলকানো হবে কি না
পাঠযোগ্য সারসংক্ষেপ
Age (বাজার অবজেক্ট অথবা unable)

### বার্তার ধরন

মান
|-------|------|---------|
০
১
২
৩
৪
৫
৬

### পাইপ-লাইন থ্রেড

মান
|-------|------|-------------|
০
১
২
৩
৪
৫

### সাধারণ বার্তা প্রবাহ

```text
StageStarted  / CheckServers
Progress / CheckServers — Server latency: 42ms
StageCompleted / CheckServers
StageStarted  / TranslateCountries
Progress / TranslateCountries — Found 195 country names
Progress / TranslateCountries — Starting translations for 'cs'...
Progress / TranslateCountries — Saved dictionary for 'cs' (198 entries)
StageCompleted / TranslateCountries
StageStarted  / TranslateJsonFiles
Progress / TranslateJsonFiles — Detected 3 added and 0 removed keys
Progress / TranslateJsonFiles — Starting JSON translations for 'cs'...
Progress / TranslateJsonFiles — Saved dictionary for 'cs' (201 entries)
StageCompleted / TranslateJsonFiles
StageStarted  / TranslateMarkdownFiles
Progress / TranslateMarkdownFiles — Scanning 2 source files in '/Docs'
Progress / TranslateMarkdownFiles — File 'en.md' has 12 translatable blocks
Progress / TranslateMarkdownFiles — Translating 'en.md' to 'cs'...
Progress / TranslateMarkdownFiles — Saved 'cs' translation for 'en.md' (12/12 blocks)
StageCompleted / TranslateMarkdownFiles
StageCompleted / StoringResults
PipelineCompleted / StoringResults
```

যদি কোন পর্যায় ব্যর্থ হয়, তাহলে অবশিষ্ট পর্যায়কে বাদ দেওয়া হবে, একটা বার্তা দেওয়া হচ্ছে, আর শেষ পর্যন্ত একটা বার্তা এসে যায়।.

## অনুবাদ সার্ভার যুক্তি

পাইপ-লাইন আবার শুরু করার দুটি মাত্রা প্রয়োগ করে:

### লগ- ইন পরিচালন ব্যবস্থা (ট্রান্স-টেলেশন পুনঃপ্রচেষ্টার অনুকরণ)

- যদি লিরট্রাস্ট্রেটের অভ্যন্তরীণ সমালোচনার পর যদি কোন অনুবাদ অনুরোধ ব্যর্থ হয়, তাহলে ৩০ সেকেন্ডের বিলম্বের সাথে আরো তৃতীয় পর্যায় পার হতে হবে।.
- মাস্ক নির্ধারণ করুন: পরে অনুবাদ করার আগে সাময়িকভাবে ডাটার নাম চিহ্নিত করা হয় (এবং তারপর আবার অনুবাদ করা হয়), সঠিক ব্যাকরণ নিশ্চিত করা হয়।.

### ভাষা সংক্রান্ত বৈধ

- অনুবাদ করার আগে অনুবাদটি অনুবাদ করা হয় অনুবাদ করা হয়।.
- অসমর্থিত ভাষা সতর্কবার্তার সঙ্গে এড়িয়ে যাওয়া হচ্ছে না, পুনরায় প্রচেষ্টা ব্যর্থ প্রচেষ্টার প্রচেষ্টা প্রতিরোধ করা হচ্ছে।.

### ট্যাব বন্ধ করার পুনরায় চেষ্টা করা হবে

- চিহ্ন বাদ দেওয়া হয়েছে - ব্লক-ব- ব্লক দেওয়া (তালিকা, অনুচ্ছেদ, তালিকা ইত্যাদি) ।.
- যদি একজন ব্যক্তি কোন ব্লক করা অনুবাদকে ব্যর্থ করে, তাহলে এটিকে মিটা-ডাটা ফাইল হিসেবে চিহ্নিত করা হয় এবং পরবর্তী পাইপলাইনে পুনরায় স্থাপন করা হয়।.
- টি নির্বাচন প্রতি প্রতিটি সোর্স মার্ক- এর পাশে প্রতিটি ফাইল একত্রিত করার জন্য টি সার্ভিস প্রতি প্রতি টি নথী.

## ভুল কোড

এক যৌথ দল দ্বারা বিভাজিত দল ব্যবহারের তথ্য প্রাপ্ত করতে ত্রুটি:

সীমা
|-------|----------|
১০০০-১৯৯৯
২০০০-২৯৯৯
৩.৩-৯৯৯
৪-৪৯৯
৫০০০-৫৯-৯৯

প্রতিবেদনের প্রতিটি ত্রুটি উৎস কোড, ফাইল পাথ অথবা মঞ্চ-এর নাম, এবং ত্রুটি কোড, এবং একটি মানুষের লেখা বার্তা।.

## লাইভ অনুবাদ ড্যাশবোর্ড

এই প্রকল্পের মধ্যে একটি প্রশাসক পেজ অন্তর্ভুক্ত করা হয়েছে যা সিগন্যালআর হাবের সাথে সংযোগ স্থাপন করে এবং বাস্তব সময়ে সকল পাইপলাইন ইভেন্ট প্রদর্শন করে।.

- বর্তমান অবস্থার বার্তা, ইনস্ট্যান্ট বার্তার মাধ্যমে প্রদর্শন করা হবে।.
- রঙের কোড: মঞ্চের জন্য নীল, সবুজ, ত্রুটির জন্য লাল রং।.
- ফিড পরিষ্কার এবং JSON বার্তা এক্সপোর্ট করা হয় ।.
- সংযোগ না থাকলে, স্বয়ংক্রিয় ভাবে ব্যাক-আপ করা হবে।.

## ডিজাইন নীতিগুলো

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Meternet অনুবাদ সর্বদা স্বয়ংক্রিয়রূপে অগ্রাধিকার আছে. **
