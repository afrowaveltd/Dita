# 翻訳アーキテクチャ

この文書は、Ditaの自動翻訳システムのモジュラーアーキテクチャを記述し、保守性、テスト性、および回復性を改善するために導入しました.

## 設計目標

オリジナルのモノリシックなデザインにいくつかの懸念を提唱:

- **懸念の分離**: 各翻訳ドメイン(国、JSON辞書、Markdown)は分離されています.
- **増加の持続**: ファイルの保存は、翻訳直後に保存され、メモリ使用量を減らし、以前の結果を提供します.
- **弾性**:複数の再試行レベルは、パイプライン全体をブロックすることなく、過渡障害を処理します.
- **保守性**: リアルタイム監視用に、SignalR を介して重要な操作が報告されます.
- **拡張性**: 1つのインターフェイスを実装することで、新しい翻訳ターゲットを追加できます.

## サービス分解

### BackendTranslationService(オルチェット)

**責任**:
- パイプラインのライフサイクル管理(スタート、完了、エラー処理)
- Semaphore ベースの対立制御(オーバーラップランを防止)
- サーバー検証(レイテンシー、言語の可用性、設定)
- サブサービスへの委任

**含まれないもの**:
- 翻訳ロジック
- 特定のフォーマットのファイルI / O
- 再試行ロジック

### トランスレーションサービス

**責任**:
- ディレクトリから読む
- 国名をデフォルトローカル辞書に同期
- ターゲット言語ごとに不足している国名を翻訳する
- 翻訳直後に各ターゲット辞書を保存

**主な行動**:
- デフォルト言語が英語の場合: として保存されている国名
- デフォルト言語が他の言語の場合: デフォルト言語に翻訳される英語名
- 各言語は、独自のリトライループで独立して処理されます

### ローカリゼーション翻訳サービス

**責任**:
- 以前のスナップショットで現在のデフォルトの辞書を比較することにより、追加/削除されたキーを検出する
- 各ターゲット言語にキーを追加
- 各ターゲット言語から削除されたキーを削除
- 次の比較でスナップショットを保存

**主な行動**:
- マニュアル翻訳は常に優先します(過度に)
- 追加されたキーは、翻訳され、言語ごとにすぐに保存されます
- 削除されたキーはすぐに言語ごとに削除されます
- スナップショットは、すべての言語が正常に完了した後にのみ保存されます

### ドキュメント翻訳サービス

**責任**:
- セットアップされたMarkdownの根を再帰的に歩く
- SHA-256ハッシュを使用して変更されたソースファイルを検出
- パーブロックの翻訳ステータスを追跡
- ブロックごとにブロックを変換する
- 翻訳後のマークダウン構造の検証
- 各ターゲット言語ファイルを独立して保存する

**主な行動**:
- ブロックレベルの粒度:見出し、段落、リスト項目は別々に翻訳されます
- 言語ごとに成功/失敗したブロックのメタデータトラック
- 失敗したブロックは、成功したブロックを再翻訳することなく、次の実行時に取得されます
- 構造検証により、見出しのカウント、リスト、コードブロックなどのマッチソースが確保されます

## リトリー戦略

システムは3つのレベルでretriesを実装します

### レベル1 — HTTP (LibreTranslateService)

- 指数関数的なバックオフ(1秒、2秒、3秒、4秒、5秒)で最大5回の試み
- ネットワークのタイムアウト、5xx エラー、および一時的な失敗を処理します
- HTTP クライアント構成に組み込まれる

### レベル2 — ステージ(TranslationRetryService)

- 30秒の遅延で最大3件の試み
- HTTP レベルのレトリーが排出された後、リクエスト全体をリドライブ
- プレースホルダーのマスクと修復は、このレベルで適用されます

### レベル3 — ブロック(ドキュメント翻訳サービス)

- 失敗する個々のマークダウンブロックはメタデータにマークされます
- 次のパイプラインの実行時に自動的に取得
- 成功したブロックは、決して再翻訳されません

## データフロー

### JSON辞書翻訳

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### マークダウン翻訳

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### 国名翻訳

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## 状態の永続性

### スナップショット

- **JSON**: デフォルト辞書の横にあるファイルに保存されます(名前はストレージプロバイダによって変わります)
- **Purpose**: 前の実行に存在するものを追跡することにより、増分同期を有効にします

### ハッシュファイル

- **Markdown**: ソースファイルの隣に
- **Fallback**: 第一次位置が読み取り専用の場合
- **注力**: 不要な再変換を避けるためのソースの変更を検知

### 翻訳メタデータ

- **マークダウン**:
- **内容**:
  - ソースコンテンツハッシュ
- 言語ブロックの状態(ボリアンの配列)
- 最終更新時刻
- **Purpose**: 失敗したブロックだけの一部再変換を有効にします

### プレースホルダーの保管

- **ファイル**:
- **コンテンツ**: プレースホルダー名値のペアにキーの辞書
- **Purpose**: アプリケーションを渡る名前付きプレースホルダのデフォルト値を提供します

## シグナル Rレポート

### 出版者抽象化

signalR の特異的な翻訳サービス:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### シーケンス保証

- 単一の実行内のメッセージは、単調にシーケンスされます
- シーケンス番号は、経由して一意です
- クライアントは、ギャップや再注文を検出することができます

### ハブマッピング

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## エクステンションポイント

### 新しい翻訳ターゲットを追加する

1. 新しいインターフェイスを作成する
2. ドメイン固有のロジックでインターフェイスを実装する
3. DIコンテナに登録
4. コンストラクタに注入
5. 既存の段階からのコール

### カスタムリトライポリシー

オーバーライドコンストラクタパラメータ:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### カスタムプレースホルダーの取り扱い

プレースホルダーの構文やストレージを変更する実装:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## 仕様

### appsettings.json の使い方

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### ランタイムチューニング

セットアップ
|---------|---------|--------|
電話番号
10月10日
3
30日

## テスト戦略

### ユニットテスト

各サブサービスが独立してテスト可能です

- 成功/失敗をシミュレートするモック
- 報告を確認するモック
- ファイルI/Oの一時的なディレクトリを使用する
- 言語保存の動作を検証

### 統合テスト

- 実際の(ローカル) LibreTranslate インスタンスでフルパイプラインを実行
- 信号を検証 Rメッセージは、接続されたクライアントに配信されます
- 同時実行防止テスト(浮腫)
- 翻訳後のマークダウン構造の検証

### エンドツーエンドのテスト

- APIまたはスケジューラによるトリガー翻訳
- すべてのターゲット言語ファイルの作成/更新
- メタデータファイルには、正しいブロックステータスが含まれていることを確認してください
- 翻訳を通じてプレースホルダーが保存されていることを確認します

## 性能検討

- **記憶**:言語保存は、メモリ内のすべての辞書を保持するのを防ぎます
- **ディスクI/O**: メタデータファイルには小さなオーバーヘッドを追加し、増分作業が可能
- **ネットワーク**: スロットリングによるシーケンシャル処理は、圧倒的なLibreTranslateを防ぎます
- ** CPU**: SHA-256 のハッシュおよび正規表現の検証は翻訳の遅延に速くあります
- **SignalR**:典型的なレポートに必要な軽量メッセージ、ペイロード圧縮なし

## モノリシックデザインからの移行

オリジナルのロジックは1つのクラスに含まれる。 移行パス:

1. 抽出国の論理 →
2. 抽出 JSON ロジック →
3. エキスマークダウンロジック →
4. 抽出信号 R出版 →
5. 抽出の試行ロジック →
6. オーケストラを委任専用に簡素化

すべての既存のインターフェイス()は変更されません。 パイプラインの消費者は、変更を中断しないを参照してください.
