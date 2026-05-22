# Dita. Prezentare generală partajată

**Dita.Shared** is the cross-platform shared library that powers Dita's localization, translation, identity, and mailer subsystems. It is consumed by the Server, GUI, and TUI front-ends and contains no UI code itself — only services, models, enums, and infrastructure.

## Metadatele proiectului

Proprietate
|---|---|
Cadrul-țintă
Nulabile
Utilizare implicită
Fișier documentație

### NuGet dependențe

Ambalaj
|---|---|---|
Afrowave. SharedTools. Api
Afrowave. SharedTools. Modele
kit poştal
Markdig
Microsoft. AspNetCore. Semnal. Nucleu
Microsoft. Extensii. Caching. Abstractii
Microsoft. Extensii. Gazduire. Abstractii
Microsoft. Extensii. Localizare. Abstractii
newtonsoft. json

## Structura directorului

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

## Prezentare generală a subsistemului

Subsistemul
|---|---|---|
**Identitatea**
**Localizare**
**mailer **

## Principii de proiectare

- ** Interfață-primul**: fiecare serviciu are o interfață concentrată (, , , etc.)
- **Resultaţi obiecte peste excepţii**: toate metodele de service se întorc pentru manipularea în siguranţă a erorilor
- **Thread safety**: in , ; in
- ** Persistența incrementală**: dicționarele și fișierele Markdown sunt salvate pe limbă imediat după traducere
- ** Degradare spectaculoasă**: fișierele lipsă produc colecții goale, nu excepții
- ** Write-through on miss**: and auto-add lipsess keys to the implicit dicţionar for latented translation
