# ديتا عرض عام مشترك

** ديتا. المتقاسمة** هي المكتبة المشتركة عبر المزيجات التي تتحكّم موقع ديتا، الترجمة، الهوية، النظم الفرعية البريدية وهو يستهلكه الخادم، وغواي، وشركة توتي للخطوط الأمامية، ولا يحتوي على أي رمز للوحدة وحدها - أي الخدمات، والنماذج، والأوراق، والهياكل الأساسية.

## Project metadata

الممتلكات
|---|---|
الإطار المستهدف
لاغي
استخدامات غير مشروعة
ملف الوثائق

### المعالين من النواة

التعبئة
|---|---|---|
(أفروايف) مُشاركة. Api
(أفروايف) مُشاركة. النماذج
البريد
Markdig
مايكروسوفت (أسبينيت كور) سينالر Core
مايكروسوفت تمديدات الصراخ الخلاصات
مايكروسوفت تمديدات استضافة. الخلاصات
مايكروسوفت تمديدات التمركز الخلاصات
Newtonsoft.Json

## هيكل الدليل

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

## لمحة عامة عن النظام الفرعي

النظام الفرعي
|---|---|---|
** الهوية**
** التخصص**
** مالي**

## مبادئ التصميم

- ** غير مباشر**: لدى كل خدمة واجهة مركزة (، إلخ)
- ** Result objects over exceptions**: all service methods return for safe error handling
- ** خيط الأمان**: في،
- ** الثبات التدريجي**: يُحتفظ بملفات القاموس والملفات المميزة لكل لغة مباشرة بعد الترجمة التحريرية
- ** التدهور المتبصر**: تنتج الملفات المفقودة مجموعات فارغة، لا استثناءات
- ** شطب البيانات المتعلقة بالخطأ**: ومفاتيح المفقودة آليا للقاموس الافتراضي للترجمة المؤجلة
