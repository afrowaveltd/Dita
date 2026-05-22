# Dita. Paylaşılan Genel Bakış

**Dita. Paylaşılan**, Dita'nın yerelleşmesi, çeviri, kimlik ve postacı alt sistemlere sahip olan kütüphaneyi paylaştı. Server, GUI ve TUI ön uçları tarafından tüketilir ve hiçbir UI kodunın kendisi yoktur - sadece hizmetler, modeller, enums ve altyapı.

## proje metadata

Emlak
|---|---|
Hedef çerçevesi
Nullable
Kapalı kullanımları
Dokümantasyon dosyası

### NuGet bağımlılık

Paket Paketi
|---|---|---|
Afro dalgası. PaylaşılanTools. Api
Afro dalgası. PaylaşılanTools. Model modelleri
posta
işaret
Microsoft. AspNetCore. SignalR. Core Core Core Core
Microsoft. Hazırlanmalar. Caching. Abstractions
Microsoft. Hazırlanmalar. Hosting. Abstractions
Microsoft. Hazırlanmalar. Yerelleşme. Abstractions
Newtonyu.Json

## Rehberlik yapısı

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

## Subsystem overview

Subsystem
|---|---|---|
**Identity**
Full çeviri hatları, JSON yerelleştirici, SignalR merkezi, işaret çeviri, yer sahipleri, ortaware
**Mailer**

## Tasarım ilkeleri

- ** Interface-ilk**: Her hizmet odaklanmış bir arayüze sahiptir (, , vs.)
- ** istisnalar üzerindeki nesneler **: tüm hizmet yöntemleri güvenli hata işleme için geri döner
- **Gread security**: in , ,; in
- **Incremental Continuence**: sözlükler ve Markdown dosyaları hemen çeviriden sonra kaydedilir
- **Graceful revision **: Eksik dosyalar boş koleksiyonlar üretir, istisnalar istisna değil
- **Write-through on miss**: `JsonStringLocalizer` and `LocalizeService` auto-add missing keys to the default dictionary for deferred translation
