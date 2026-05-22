# Дита. Бирдиктүү жалпы көрүнүш

** Дита. Shared** - Дитанын локализациясын, котормосун, идентификациясын жана почта жөнөтүүчү подсистемаларын камсыз кылган платформалар аралык бөлүшүлгөн китепкана. Ал Server, GUI жана TUI алдыңкы чекиттери тарабынан керектелет жана UI кодунун өзүн камтыбайт - кызматтар, моделдер, энумдар жана инфраструктура гана.

## Долбоордун метамаалыматтары

Мүлк мүлк
|---|---|
Максаттык алкактар
Жокко чыгарылат
Имплициттик пайдалануу
Документация файлы

### NuGet көз карандылыктар

Пакет пакети
|---|---|---|
Афроув. Биргелешкен куралдар. Апи Апи
Афроув. Биргелешкен куралдар. Моделдер
MailKit почта
Марксиг
Microsoft. AspNetCore. SignalR. Негизги негизги негизги
Microsoft. Кеңейтүү. Кэшинг. Абстракциялар
Microsoft. Кеңейтүү. Хостинг. Абстракциялар
Microsoft. Кеңейтүү. Локализация. Абстракциялар
Ньютонсофт, Джсон

## Каталогдун түзүлүшү

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

## Субсистеманын жалпы көрүнүшү

Субсистема
|---|---|---|
** Идентификация**
** Жергиликтүү **
** Почта**

## Дизайн принциптери

- ** Интерфейс биринчи**: ар бир кызматтын багытталган интерфейси бар (,, ж.б.)
- ** Жыйынтык объектилери өзгөчө учурларга караганда**: бардык кызмат ыкмалары каталарды коопсуз иштетүү үчүн кайтарылат
- ** Үч тараптуу коопсуздук**: in,, in
- ** Инкременталдык туруктуулук**: сөздүктөр жана Markdown файлдары котормодон кийин дароо тилге сакталат
- ** Ырайымдуу бузулуу**: жетишпеген файлдар бош коллекцияларды пайда кылат, өзгөчө учурлар эмес
- ** Туура эмес котормо жөнүндө жазуу**: жана кийинкиге калтырылган котормо үчүн демейки сөздүккө жетишпеген ачкычтарды автоматтык түрдө кошуу
