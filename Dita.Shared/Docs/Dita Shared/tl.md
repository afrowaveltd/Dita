# Dita. Ibahagi ang Overview

**Dita. Bahagi** ang cross-platform na kabahaging aklatan na nagbibigay kapangyarihan sa lokalisasyon, pagsasalin, pagkakakilanlan, at mga subsistema ng koreo ni Dita. Ito ay kinokonsumo ng Server, GUI, at TUI front-ends at naglalaman ng walang kodigo ng UI mismo — mga serbisyo lamang, modelo, enum, at imprastraktura.

## Metadata ng Proyekto

Mga ari - arian
|---|---|
Target frame
Maisasara
Di - angkop na mga gamit
Imbalidong pangalan ng programa

### Nu Kunin ang mga dependensiya

Pakete
|---|---|---|
Afrowave. Ibahagi ang mga Tol. Api
Afrowave. Ibahagi ang mga Tol. Mga Modelo
Sulat
marka
Microsoft. ApNetCore. Ang SignalR. Baryo
Microsoft. Mga ekstensyon. Pagdusa. Mga Pagbabago
Microsoft. Mga ekstensyon. Pag - aartista. Mga Pagbabago
Microsoft. Mga ekstensyon. Lokalisasyon. Mga Pagbabago
Newtonsoft.Json

## Direktor na kayarian

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

## Subsystem Bilang sumaryo

subsistema
|---|---|---|
Ang pagiging totoo ay nagbubunga ng mga kodigo
Buong translation pipeline, JSON localizer, SignalR center, markdown translation, placeholders, middleware
SMTP configuration model (mga plano)

## Magdisenyo ng mga simulain

- **Interface-first**: bawat serbisyo ay may nakatutok na interface (, , , atbp.)
- ** Ilagay ang mga bagay sa mga eksepsiyon**: lahat ng paraan ng paglilingkod ay bumabalik para sa ligtas na pagkakamali sa paghawak
- **Thread safety**: sa , , ; sa
- ** Ang inkremental na pagtitiyaga**: ang mga diksyunaryo at mga talaksang Markdown ay natitipid sa bawat-wika karaka-agad pagkatapos ng pagsasalin
- **Graceful destruct**: Ang nawawalang mga file ay gumagawa ng walang laman na mga koleksiyon, hindi ng mga eksepsiyon
- ** Isulat-rough sa miss**: at auto-add na nawawalang susi sa default dictionary para sa ipinagpaliban na salin
