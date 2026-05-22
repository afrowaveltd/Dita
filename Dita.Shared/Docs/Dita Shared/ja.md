# ディタ。 共有概要

**ディタ. Shared** は、Dita のローカリゼーション、翻訳、アイデンティティ、メーラーサブシステムに電力を供給するクロスプラットフォーム共有ライブラリです。 サーバ、GUI、TUI のフロントエンドによって消費され、UI コード自体は含まれません。サービス、モデル、列、インフラのみです.

## プロジェクトメタデータ

プロパティ
|---|---|
ターゲットフレームワーク
メニュー
インペリシットの使用
ドキュメントファイル

### NuGet の依存関係

パッケージ
|---|---|---|
アフロウェーブ。 共有ツール。 アピ
アフロウェーブ。 共有ツール。 モデル
メールキット
マークディグ
マイクロソフト。 AspNetCoreの特長 シグナルR コアコア
マイクロソフト。 エクステンション キャッシュ。 アブストラクション
マイクロソフト。 エクステンション ホスティング。 アブストラクション
マイクロソフト。 エクステンション ローカル化。 アブストラクション
ニュートンソフトジェイソン

## ディレクトリ構造

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

## サブシステムの概要

サブシステム
|---|---|---|
**アイデンティティ**
**ローカリゼーション**
**メーラー**

## デザイン原則

- **インターフェイス優先**:すべてのサービスは、フォーカスインターフェイス(、、など)を持っています
- **例外を超えた結果オブジェクト**:すべてのサービスメソッドは、安全なエラー処理のために返します
- **3つの安全**:、;で
- **包括的な永続**:辞書とMarkdownファイルは、直後に翻訳された直後に保存されます
- **素晴らしい劣化**:欠落したファイルは例外ではなく、空のコレクションを生成します
- **Write-through on miss**: と 偽造翻訳のためのデフォルトの辞書に欠落キーを自動追加
