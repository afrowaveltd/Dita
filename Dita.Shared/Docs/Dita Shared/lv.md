# Dita. Kopīgs pārskats

**Dita. Shared** ir starpplatformu koplietošanas bibliotēka, kas pilnvaras Dita lokalizācija, tulkošana, identitāte, un mailer apakšsistēmas. To patērē serveris, GUI un TUI priekšpuses, un tajā nav neviena paša UI koda — tikai pakalpojumi, modeļi, enums, un infrastruktūra.

## Projekta metadati

Īpašība
|---|---|
Mērķu struktūra
atceļams
Netieši izmanto
Dokumentācija

### NuGet atkarības

Pakotne
|---|---|---|
Afrowave. Koplietošanas rīki. Api
Afrowave. Koplietošanas rīki. Paraugi
pastkastīte
Markdig
Microsoft. AspNetCore. SignalR. Pamattīkls
Microsoft. Pagarinājumi. Kešošana. Kopsavilkumi
Microsoft. Pagarinājumi. Hostings. Kopsavilkumi
Microsoft. Pagarinājumi. Lokalizācija. Kopsavilkumi
Newtonsoft.Json

## Direktoriju struktūra

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

## Apakšsistēmas pārskats

Apakšsistēma
|---|---|---|
Autentificēšanas rezultātu kodi
Pilna tulkošanas cauruļvads, JSON lokalizētājs, SignalR centrmezgls, demarkācijas tulkojums, vietturi, starpprogrammatūra
SMTP konfigurācijas modelis (plānotie pakalpojumi)

## Projektēšanas principi

- **Interface-first**: katram pakalpojumam ir koncentrēta saskarne (, , , utt.)
- **Rezultātu objekti virs izņēmumiem**: visas servisa metodes atgriežas drošai kļūdu apstrādei
- **Drošība**:
- **Inkrementālā noturība**: vārdnīcas un iezīmēšanas faili tiek saglabāti par valodu uzreiz pēc tulkojuma
- **Grādošā degradācija**: trūkstošie faili rada tukšas kolekcijas, nevis izņēmumus
- **Write-through par miss**: un auto-pievienot trūkst atslēgas noklusējuma vārdnīca atliktā tulkojuma
