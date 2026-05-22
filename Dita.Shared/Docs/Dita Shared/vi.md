# Dita. Xem toàn cảnh chung

**Dita. Chia sẻ** là thư viện chia sẻ chữ viết chéo mà cho phép Dita sử dụng sức mạnh định vị, dịch thuật, danh tính và hệ thống điện thư của Dita. Nó được sử dụng bởi máy chủ, giao diện người dùng giao diện người dùng và TUI và không chứa mã UI nào — chỉ có dịch vụ, mô hình, danh mục và cơ sở hạ tầng.

## Comment

Thuộc tính
|---|---|
Mục tiêu
Name
Dùng đơn giản
Tập tin tài liệu

### Các phụ thuộc NuGet

Gói
|---|---|---|
Afrowave. Công cụ chia sẻ. Api
Afrowave. Công cụ chia sẻ. Mô hình
thư điện tử
Đánh dấu
Microsoft. AspNetCore. Tín hiệu cấp cứu. lõi
Microsoft. Mở rộng. Đau quá. Trừu tượng
Microsoft. Mở rộng. Tiếp đón. Trừu tượng
Microsoft. Mở rộng. Địa phương hóa. Trừu tượng
Newtonsoft.Json

## Comment

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

## Phân tích hệ thống con

Hệ thống con
|---|---|---|
**nidentity**
**Localization**
**Mailer**

## Nguyên tắc thiết kế

- ** Interface- first**: mỗi dịch vụ có một giao diện tập trung (, , , etc.)
- ** Xem xét đối tượng trên ngoại lệ**: mọi phương pháp dịch vụ trở lại để xử lý lỗi an toàn
- **Sự an toàn thứ ba**: in : ,
- ** Kiên trì gia tăng**: từ điển và tập tin Markdown được lưu ngay sau khi dịch
- **Graceful suy đồi**: thiếu tập tin tạo bộ sưu tập rỗng, không phải ngoại lệ
- ** viết qua khi bỏ lỡ**: và khóa thiếu tự động thêm vào từ điển mặc định cho việc dịch bị hoãn
