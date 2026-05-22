# דיטה סקירה משותפת

**Dita. משותף ** הוא הספרייה המשותפת חוצה כוכבית המעצימה את ה Localization, התרגום, הזהות ומערכת הדואר. הוא נצרך על ידי Server, GUI ו-TUI Front-ends ואינו מכיל קוד UI עצמו - רק שירותים, מודלים, אנמיות ותשתיות.

## פרויקט metadata

רכוש
|---|---|
מסגרת Target
ביטול
המונחים:
קובץ

### קבל תלות

חבילה
|---|---|---|
אפרו גל משותף. Api
אפרו גל משותף. מודלים
דואר
מארקיג
Microsoft AspNetCore אות. Core Core
Microsoft הרחבה. גילוח. המונחים
Microsoft הרחבה. אירוח. המונחים
Microsoft הרחבה. מקומיות. המונחים
ניוטון ג'ונסון

## מבנה Directory

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

## המונחים:

מערכת
|---|---|---|
** זהות**
** Localization**
**Mailer**

## עקרונות עיצוב

- ** Interface-First**: לכל שירות יש ממשק ממוקד (, , וכו ')
- ** אובייקטים מורכבים מעל חריגים **: כל שיטות השירות חוזרות לטיפול בשגיאה בטוחה
- **Thread safety**: `SemaphoreSlim` in `LanguageService`, `PlaceholderService`, `LibreTranslateService`; `lock` in `TranslationQueue`
- ** התעקשות מוגברת**: דיסלקציות וקבצי גילוח נשמרים באופן מיידי לאחר התרגום
- **השפל עצום **: קבצים חסרים מייצרים אוספים ריקים, לא חריגים
- **Write-through on miss**: `JsonStringLocalizer` and `LocalizeService` auto-add missing keys to the default dictionary for deferred translation
