# ডিটা। শেয়ার করা সারাংশ

**Dita.Shared** is the cross-platform shared library that powers Dita's localization, translation, identity, and mailer subsystems. It is consumed by the Server, GUI, and TUI front-ends and contains no UI code itself — only services, models, enums, and infrastructure.

## প্রজেক্টের মিটা-ডাটা

বৈশিষ্ট্য
|---|---|
টার্গেট কাঠামো
Null
পরোক্ষ ব্যবহার
নথিপত্র ফাইল

### Nu পর্যন্ত Gutinult

প্যাকেজ
|---|---|---|
আফ্রিকাওয়াভ. শেয়ার সরঞ্জাম। এপি
আফ্রিকাওয়াভ. শেয়ার সরঞ্জাম। মডেল
মেইল
মার্ক- আপ
মাইক্রোসফট। এপনেটর. সিগন্যাল আর. কোরে
মাইক্রোসফট। এক্সটেনশন। ক্যাশে করা হচ্ছে. নির্বস্তুক
মাইক্রোসফট। এক্সটেনশন। হোস্টিং। নির্বস্তুক
মাইক্রোসফট। এক্সটেনশন। স্থানীয়করণ। নির্বস্তুক
নিউটনসফ্ট জসন

## ডিরেক্টরির গঠন

```
Dita.Shared/
├── Identity/
│   └── Enums/
│       └── LoginResponse.cs
├── Localization/
│   ├── Enums/
│   │   ├── AuthenticationError.cs
│   │   ├── Comparison.cs
│   │   ├── ConfigurationError.cs
│   │   ├── DiskError.cs
│   │   ├── ErrorCode.cs
│   │   ├── FileSystemError.cs
│   │   ├── Gender.cs
│   │   ├── GeneralError.cs
│   │   ├── LocalizationError.cs
│   │   ├── LocalizationMessageType.cs
│   │   ├── NetworkError.cs
│   │   ├── PhraseChange.cs
│   │   ├── ProcessStage.cs
│   │   ├── StorageError.cs
│   │   ├── TranslationTarget.cs
│   │   └── ValidationError.cs
│   ├── Hubs/
│   │   ├── ILocalizationHubClient.cs
│   │   └── LocalizationHub.cs
│   ├── Middlewares/
│   │   └── LocalizationMiddleware.cs
│   ├── Models/
│   │   ├── AutomaticTranslationSettings.cs
│   │   ├── CheckingReport.cs
│   │   ├── ComparisonCondition.cs
│   │   ├── CountryDefinition.cs
│   │   ├── Detections.cs
│   │   ├── DetectRequest.cs
│   │   ├── ErrorResponse.cs
│   │   ├── LibreLanguage.cs
│   │   ├── LocalizationHubMessage.cs
│   │   ├── LocalizationHubSnapshot.cs
│   │   ├── MarkdownTranslatableBlock.cs
│   │   ├── MarkdownTranslationsReport.cs
│   │   ├── PhraseInQueue.cs
│   │   ├── SingleTranslation.cs
│   │   ├── StageReport.cs
│   │   ├── StoringReport.cs
│   │   ├── TextLocalizationModels.cs
│   │   ├── TranslateFileRequest.cs
│   │   ├── TranslateFileResult.cs
│   │   ├── TranslateRequest.cs
│   │   ├── TranslateResult.cs
│   │   ├── Translation.cs
│   │   ├── TranslationError.cs
│   │   └── TranslationsReport.cs
│   ├── Services/
│   │   ├── ILanguageService.cs
│   │   ├── ILibreTranslateHttpClientFactory.cs
│   │   ├── ILibreTranslateService.cs
│   │   ├── ILocalizeService.cs
│   │   ├── IMarkdownParserService.cs
│   │   ├── IMarkdownReconstructorService.cs
│   │   ├── IMarkdownTranslationService.cs
│   │   ├── IPlaceholderService.cs
│   │   ├── ITranslateService.cs
│   │   ├── ITranslationQueue.cs
│   │   ├── JsonStringLocalizer.cs
│   │   ├── JsonStringLocalizerFactory.cs
│   │   ├── LanguageService.cs
│   │   ├── LibreTranslateHttpClientFactory.cs
│   │   ├── LibreTranslateService.cs
│   │   ├── LocalizeService.cs
│   │   ├── MarkdownParserService.cs
│   │   ├── MarkdownReconstructorService.cs
│   │   ├── MarkdownTranslationService.cs
│   │   ├── PlaceholderService.cs
│   │   ├── TranslateService.cs
│   │   └── TranslationQueue.cs
│   └── ScheduledTranslationService/
│       ├── BackendTranslationService.cs
│       ├── CountriesTranslationService.cs
│       ├── DocumentsTranslationService.cs
│       ├── IBackendTranslationService.cs
│       ├── ICountriesTranslationService.cs
│       ├── IDocumentsTranslationService.cs
│       ├── ILocalizationMonitoringState.cs
│       ├── ILocalizationTranslationService.cs
│       ├── ISignalRPublisher.cs
│       ├── LocalizationMonitoringState.cs
│       ├── LocalizationTranslationService.cs
│       ├── MarkdownTranslationMetadata.cs
│       ├── Scheduller.cs
│       ├── SignalRPublisher.cs
│       └── TranslationRetryService.cs
└── Mailer/
    ├── Models/
    │   └── SmtpSettings.cs
    └── Services/         (planned — empty)
```

## সাব- সিস্টেম বিশ্লেষণ

সাব- সিস্টেম
|---|---|---|
অনুমোদন ব্যবস্থা
সম্পূর্ণ অনুবাদ পাইপ, JSONকাইজার, সিগন্যাল ২
**Khooser**

## ডিজাইন নীতিগুলো

- **Interface-first**: every service has a focused interface (`ILanguageService`, `IPlaceholderService`, `ISignalRPublisher`, etc.)
- **Result objects over exceptions**: all service methods return `Response<T>` for safe error handling
- **Thread safety**: `SemaphoreSlim` in `LanguageService`, `PlaceholderService`, `LibreTranslateService`; `lock` in `TranslationQueue`
- **Incremental persistence**: dictionaries and Markdown files are saved per-language immediately after translation
- **Graceful degradation**: missing files produce empty collections, not exceptions
- **- মিসিং-এ লেখা **Rected : এবং auto-Do চিহ্নিত অভিধানের জন্য ডিফল্ট অভিধানের মধ্যে অনুপস্থিত
