# Dita. Ringkasan Kongsi

**Dita. Shared** adalah perpustakaan berbagi cross-platform yang memberikan kekuatan lokalisasi, terjemahan, identitas, dan subsistem mailer kepada Dita. Ini dikonsumsi oleh server, GUI, dan TUI front-ends dan tidak mengandung kode UI sendiri — hanya layanan, model, enum, dan infrastruktur.

## Data meta Projek Projek data meta

Ciri-ciri
|---|---|
Rangka kerja sasaran Rangka kerja Rangka kerja
diaktifkan
Penggunaan implisit
Berkas dokumentasi Dokumentasi Dokumentasi Dokumentasi Dokumentasi

### Ketergantungan NuGet NuGet

Paket Ubuntu
|---|---|---|
Afrowave. SharedTools. ♪Api
Afrowave. SharedTools. Model
ukiran surat
Makeda Markdig
Microsoft. AspNetCore. SignalR. Core
Microsoft. Extension. Caching. Abstrak
Microsoft. Extension. Hosting. Abstrak
Microsoft. Extension. Olokalisasi. Abstrak
newstonsoft.json

## Struktur direktori

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

## Sekilas pandang Subsistem

Subsistem
|---|---|---|
Kode hasil pengesahihan kode autentikasi
Terjemahan lengkap, lokalizer JSON, sinyalR hub, terjemahan markdown, placeholders, middleware
Model konfigurasi SMTP (services direncanakan)

## Prinsip desain

- **Interface-first**: every service has a focused interface (`ILanguageService`, `IPlaceholderService`, `ISignalRPublisher`, etc.)
- **Result objects over exceptions**: all service methods return `Response<T>` for safe error handling
- **Thread safety**: `SemaphoreSlim` in `LanguageService`, `PlaceholderService`, `LibreTranslateService`; `lock` in `TranslationQueue`
- **Incremental persistence**: dictionaries and Markdown files are saved per-language immediately after translation
- **Graceful degradation**: missing files produce empty collections, not exceptions
- **Write-through on miss**: `JsonStringLocalizer` and `LocalizeService` auto-add missing keys to the default dictionary for deferred translation
