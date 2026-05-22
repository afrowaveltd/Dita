# ดิตะ ภาพรวมที่ใช้ร่วมกัน

**Dita.Shared** is the cross-platform shared library that powers Dita's localization, translation, identity, and mailer subsystems. It is consumed by the Server, GUI, and TUI front-ends and contains no UI code itself — only services, models, enums, and infrastructure.

## ข้อมูลกํากับภาพของโครงการ

คุณสมบัติ
|---|---|
กรอบเป้าหมาย
ไม่สามารถเปิดได้
ปิดการใช้งานการใช้
แฟ้มเอกสาร

### การขึ้นต่อกันระหว่างแพกเกจ

แพกเกจ
|---|---|---|
แอโฟรเวฟ กล่องใช้ร่วมกัน Api
แอโฟรเวฟ กล่องใช้ร่วมกัน รุ่น
จดหมาย Kit
ทําเครื่องหมาย
ไมโครซอฟท์ แอปเน็ต โคร์ สัญญาณ แกนหลัก
ไมโครซอฟท์ ส่วนขยาย จี้ เลเยอร์ถัดไป
ไมโครซอฟท์ ส่วนขยาย โฮสต์ เลเยอร์ถัดไป
ไมโครซอฟท์ ส่วนขยาย ท้องถิ่น เลเยอร์ถัดไป
นิ ว ตัน โซล เจ สัน

## โครงสร้างไดเรกทอรี

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

## ภาพรวมของระบบย่อย

ระบบย่อย
|---|---|---|
การตรวจสอบสิทธิ์
พิมพ์แบบเต็มระบบท่อส่งเสียง Json localer, CentralR head, Countown translations, placess, Senterware
รุ่นปรับแต่ง SMTP (วางแผนบริการ)

## หลักการการออกแบบ

- **Interface-first**: every service has a focused interface (`ILanguageService`, `IPlaceholderService`, `ISignalRPublisher`, etc.)
- **Result objects over exceptions**: all service methods return `Response<T>` for safe error handling
- **Thread safety**: `SemaphoreSlim` in `LanguageService`, `PlaceholderService`, `LibreTranslateService`; `lock` in `TranslationQueue`
- **Incremental persistence**: dictionaries and Markdown files are saved per-language immediately after translation
- ** ความเสื่อมโทรมของภาพ **: แฟ้มสูญหาย จะสร้างคลังภาพที่ว่างเปล่า ไม่ใช่ข้อยกเว้น
- **Write-through on miss**: `JsonStringLocalizer` and `LocalizeService` auto-add missing keys to the default dictionary for deferred translation
