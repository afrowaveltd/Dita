# Dita. Yhteinen yleiskatsaus

**Dita.Shared** is the cross-platform shared library that powers Dita's localization, translation, identity, and mailer subsystems. It is consumed by the Server, GUI, and TUI front-ends and contains no UI code itself — only services, models, enums, and infrastructure.

## Projektin metatiedot

Omaisuus
|---|---|
Tavoitekehys
Haihtuvat
Implisiittiset käyttötavat
Asiakirja-aineisto

### NuGet-riippuvuudet

Pakkaus
|---|---|---|
Afrowave. Jaetut työkalut. Api
Afrowave. Jaetut työkalut. Mallit
PostiKit
mardig
Microsoft. AspNetCore. Signaali. Ydin
Microsoft. Laajennukset. - Caching. Tiivistelmät
Microsoft. Laajennukset. Isäntä. Tiivistelmät
Microsoft. Laajennukset. Lokalisointi. Tiivistelmät
Newtonsoft

## Kansiorakenne

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

## Osajärjestelmän yleiskatsaus

Osajärjestelmä
|---|---|---|
Tunnistamisen tuloskoodit
Täysi käännös putki, JSON lokalizer, SignalR napa, markdown käännös, paikkaholkit, väliohjelmat
SMTP-konfiguraatiomalli (suunnittelupalvelut)

## Suunnitteluperiaatteet

- **Yhteys ensin**: jokaisella palvelulla on keskitetty käyttöliittymä (, , , jne.)
- **Kirjoita esineet poikkeusten sijaan**: kaikki palvelumenetelmät palaavat turvalliseen virhekäsittelyyn
- **Tread safety**: in , , ; in
- **Kyseinen pysyvyys**: sanakirjat ja Markdown-tiedostot tallennetaan kielellä välittömästi käännöksen jälkeen
- **Hyvä huonontuminen**: puuttuvat tiedostot tuottavat tyhjiä kokoelmia, ei poikkeuksia
- ** Kirjoita läpi miss**: ja automaattisesti lisää puuttuvat avaimet oletussanakirjaan lykätty käännös
