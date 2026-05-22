# Дита. Общий обзор

**Дита. Shared** - это кроссплатформенная общая библиотека, которая поддерживает подсистемы локализации, перевода, идентификации и почтовой рассылки Dita. Он потребляется сервером, графическим интерфейсом и интерфейсом TUI и не содержит сам код пользовательского интерфейса — только службы, модели, перечни и инфраструктуру.

## Метаданные проекта

Собственность
|---|---|
Целевые рамки
Нулевой
Неявное использование
Файл документации

### NuGet зависимостей

Пакет
|---|---|---|
Афроволна. SharedTools. Апи
Афроволна. SharedTools. Модели
почтовый ящик
Маркдиг
Microsoft. AspNetCore. Сигнал. ядро
Microsoft. Продление. Кэширование. Абстракции
Microsoft. Продление. Хостинг. Абстракции
Microsoft. Продление. Локализация. Абстракции
newtonsft.json

## Структура директорий

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

## Обзор подсистем

Подсистема
|---|---|---|
** Идентичность**
**Локализация**
** Почтальон**

## Принципы проектирования

- **Интерфейс-первый**: каждый сервис имеет сфокусированный интерфейс (, , , и т.д.)
- **Объекты результатов по сравнению с исключениями**: все методы обслуживания возвращаются для безопасной обработки ошибок
- **Безопасность**: в, ; в
- **Повышенная настойчивость**: словари и файлы Markdown сохраняются на одном языке сразу после перевода
- **Грейсовая деградация**: недостающие файлы создают пустые коллекции, а не исключения
- **Прочтите на miss**: и автоматически добавьте недостающие ключи в словарь по умолчанию для отложенного перевода
