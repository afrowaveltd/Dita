# Dita. Comhroinnte Forbhreathnú

**Dita. Shared ** Is é an leabharlann tras-ardán roinnte go cumhachtaí logánú Dita, aistriúchán, féiniúlacht, agus fochórais mailer. Tá sé ól ag an Freastalaí, GUI, agus TUI tosaigh-deireadh agus tá aon chód Chomhéadain féin - seirbhísí amháin, samhlacha, enums, agus bonneagar.

## Meiteashonraí tionscadail

Díroghnaigh gach rud
|---|---|
Creat Sprioc
Núicléach
Ag baint úsáide as
Comhaid doiciméadú

### Spleáchas núicléach

Pacáiste
|---|---|---|
Afrawave. Comhroinnte Tools. Uisce agus Séarachas
Afrawave. Comhroinnte Tools. Múnlaí
Déan Teagmháil Linn
Amharc ar gach eolas
Microsoft. AspNetCore. Comhartha. Croí
Microsoft. Leathnú. Caching. Abstractions
Microsoft. Leathnú. Óstáil. Abstractions
Microsoft. Leathnú. Localization. Abstractions
Cliceáil grianghraf a mhéadú

## Plean Gníomhaíochta don Oideachas

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

## Forbhreathnú ar an gcóras

Fochóras
|---|---|---|
**Féiniúlacht **
**Laghdú **
**Mailer **

## Prionsabail deartha

- **Interface-first**: Tá comhéadan dírithe ag gach seirbhís (, , etc.)
- **Cuspóirí a shaothrú thar eisceachtaí**: gach modh seirbhíse ar ais le haghaidh láimhseáil earráide sábháilte
- **Trí sábháilteacht **: i, ,; i
- ** Fanacht Incriminteach **: Tá dialanna agus comhaid Markdown shábháil in aghaidh na teanga díreach tar éis an aistriúcháin
- ** díghrádú cineachúil **: comhaid ar iarraidh a tháirgeadh bailiúcháin folamh, ní eisceachtaí
- **Cuir trí cinn ar a chailleann **: agus cuir eochracha ar iarraidh chuig an bhfoclóir réamhshocraithe le haghaidh aistriúcháin iarchurtha
