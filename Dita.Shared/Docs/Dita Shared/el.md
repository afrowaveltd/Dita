# Ντίτα. Κοινόχρηστη επισκόπηση

**Dita.Shared** is the cross-platform shared library that powers Dita's localization, translation, identity, and mailer subsystems. It is consumed by the Server, GUI, and TUI front-ends and contains no UI code itself — only services, models, enums, and infrastructure.

## Μεταδεδομένα έργου

Ιδιότητα
|---|---|
Πλαίσιο-στόχος
Αναλώσιμα
Έμμεσες χρήσεις
Αρχείο τεκμηρίωσης

### Εξαρτήσεις NuGet

Συσκευασία
|---|---|---|
Αφρώδη κύματα. Τα κοινά εργαλεία. Απι
Αφρώδη κύματα. Τα κοινά εργαλεία. Μοντέλα
MailKit
Μάρκντιγκ
Η Microsoft. AspNetCore (στα Αγγλικά). Σήμα R. Κεντρικό
Η Microsoft. Επεκτάσεις. Καψόνι. Αποσπάσεις
Η Microsoft. Επεκτάσεις. Φιλοξενία. Αποσπάσεις
Η Microsoft. Επεκτάσεις. Εντοπισμός. Αποσπάσεις
Νιούτον-Μάκ

## Δομή καταλόγου

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

## Επισκόπηση υποσυστήματος

Υποσύστημα
|---|---|---|
**Ταυτότητα **
** Εντοπισμός **
** Ταχυδρομείο **

## Αρχές σχεδιασμού

- **Interface-first**: κάθε υπηρεσία έχει μια εστιασμένη διασύνδεση (, , , κλπ.)
- ** Αντικείμενα αποτελεσμάτων σε σχέση με εξαιρέσεις **: όλες οι μέθοδοι εξυπηρέτησης επιστρέφουν για ασφαλή χειρισμό σφαλμάτων
- ** Η εξέλιξη της ασφάλειας **: in , , ; in
- **Εμπνευστική επιμονή **: τα λεξικά και τα αρχεία Markdown αποθηκεύονται ανά γλώσσα αμέσως μετά τη μετάφραση
- **Ευχάριστη υποβάθμιση **: τα ελλείποντα αρχεία παράγουν άδειες συλλογές, όχι εξαιρέσεις
- **Write-through on miss**: και αυτόματη προσθήκη ελλειπόντων κλειδιών στο προεπιλεγμένο λεξικό για αναβαλλόμενη μετάφραση
