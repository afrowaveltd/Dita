# Dita Shared

**Dita Shared ** یک کتابخانه مشترک متقابل پلتفرم است که قادر به محلی سازی، ترجمه، هویت و زیر سیستم های پستی است. این توسط سرور، GUI و TUI Front-ends مصرف می شود و حاوی هیچ کد UI نیست – تنها خدمات، مدل ها، enum ها و زیرساخت ها.

## پروژه metadata

مالکیت املاک
|---|---|
چارچوب هدف
دانلود بازی Nullable
درخواست استفاده از
فایل مستندات

### عدم وابستگی

بسته بسته بندی
|---|---|---|
Afrowave SharedTools Api
Afrowave SharedTools مدل ها
ایمیل
علامت گذاری شده
مایکروسافت AspNetCore سیگنالR Core Core
مایکروسافت افزونه ها Caching انتزاع
مایکروسافت افزونه ها میزبانی انتزاع
مایکروسافت افزونه ها محلی سازی انتزاع
نیوتن نرم

## ساختار دایرکتوری

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

## نمای سیستم Subsystem

سیستم فرعی
|---|---|---|
** عدم تحرک **
** حذف کردن **
**mailer **

## اصول طراحی

- **Interface-first**: every service has a focused interface (`ILanguageService`, `IPlaceholderService`, `ISignalRPublisher`, etc.)
- **Result objects over exceptions**: all service methods return `Response<T>` for safe error handling
- **Thread safety**: `SemaphoreSlim` in `LanguageService`, `PlaceholderService`, `LibreTranslateService`; `lock` in `TranslationQueue`
- **Incremental persistence**: dictionaries and Markdown files are saved per-language immediately after translation
- **Graceful degradation**: missing files produce empty collections, not exceptions
- ** نوشتن از طریق از دست دادن ** و کلیدهای از دست رفته خودکار به دیکشنری پیش فرض برای ترجمه انتقالی
