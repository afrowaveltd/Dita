# ライブ翻訳ダッシュボード

ライブ翻訳ダッシュボードは、自動翻訳パイプラインにリアルタイムの可視性を提供する管理者ページです。 シグナルR ハブに接続し、すべてのパイプラインイベントが発生したときに表示します.

## サイトマップ

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## 特徴:

### リアルタイムイベントストリーム

すべての信号 翻訳パイプラインからのRイベントは、ライブアップテーブルに表示されます

- **シーケンス番号** — 各パイプラインの実行中のモノトニックカウンター
- **Timestamp** — イベントの受信時にローカル時間
- **Run ID** — 相関のためのGUIDを短縮
- **Stage** — パイプラインステージバッジ(CheckServers、TranslateCountriesなど)
- **タイプ** — メッセージタイプバッジ(ステージ開始、進捗、ステージ完了など)
- **メッセージ** — 人為的な説明
- **Details** — イベントデータのフルJSONペイロード

### カラーコーディング

カラー
|-------|---------|
ブルー ()
グリーン ()
赤 ()
ホワイト (デフォルト)

### 接続状況

トップショーのステータスバナー:
- **Connecting** — SignalR 接続の確立
- **Connected** — 通常イベントを受け取る
- **再接続** — 接続が失われ、再接続しようとする
- **接続解除** — 接続を閉じる

接続は、指数関数的なバックオフで自動再接続を使用する: 0s、2s、5s、10s、30s.

### コントロール

- **Clear Feed** — 表示されるすべてのメッセージを削除し、カウンターをリセット
- **JSON のエクスポート** — JSONファイルとして受け取ったすべてのメッセージをダウンロード
- **メッセージカウンター** — このセッションで受けたイベントの総数を表示

## シグナル Rハブ

ダッシュボードは次のように接続します

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### メッセージ契約

```typescript
interface LocalizationHubMessage {
    runId: string;        // Guid
    sequence: number;     // long
    type: LocalizationMessageType;
    stage: ProcessStage;
    timestampUtc: string; // ISO 8601
    isError: boolean;
    message: string;
    data: object | null;
}
```

### イベントの種類

ダッシュボードは、すべての値を処理します

タイプ:
|------|---------|
ブルーバッジ
緑のバッジ
赤いバッジ
緑のバッジ
赤いバッジ
情報バッジ
警告バッジ

## 技術的な実装

### バックエンド

- **ローカリゼーション Hub**() — すべての接続されたクライアントにメッセージを放送するSignalRハブ
- **ISignalRPublisher** — 翻訳サービスで使用するハブ上の抽象
- **SignalRPublisher** — 単調なシーケンスと放送を増加させるデフォルト実装

### フロントエンド

- 純粋な HTML/JS と Bootstrap 5 スタイリング
- Microsoft SignalR JavaScript クライアントライブラリ (CDN からロード) を使用する
- イベントフィードのサーバー側レンダリングは不要です

### ページ構造

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## 開発中の使用法

1. Dita を起動します。 サーバーアプリケーション
2. ナビゲート
3. 翻訳実行をトリガーする(スケジューラを待ち、APIを呼び出す)
4. リアルタイムでイベントを見る
5. エクスポートボタンを使用して、デバッグのための完全なトレースをキャプチャします

## 未来の強化

ダッシュボードの計画的な改善:

- **Authentication** — ユーザーの権限を制限する
- **Filtering** — Filter events by stage, type, or run ID
- **ヒステリカルラン** — データベースまたはログファイルからの完全な実行を表示
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **手動トリガー** — 手動で特定のパイプラインステージを開始するボタン
- **構成** — ダッシュボードから直接編集
- **言語管理** — サポートされている言語の表示と編集
- **辞書プレビュー** — ローカライゼーション辞書の閲覧と検索

## トラブルシューティング

### ダッシュボードは「接続できない」を示しています

1. サーバが実行されていることを確認し、アクセス可能
2. CORSまたはネットワークエラーのブラウザコンソールをチェックする
3. お問い合わせ
4. ファイアウォールがWebSocket接続をブロックされていないことを確認してください

### イベントが表示されない

1. サーバ () とクライアント () の間で、SignalR ハブ URL がマッチしていることを確認します
2. スケジューラが有効になっていることを確認します
3. 翻訳パイプラインエラーのサーバーログを見る
4. ブラウザをチェック WebSocket メッセージのネットワークタブ

### 注文からメッセージが出る

フィールドは、単一の実行内で注文することを保証します。 注文からメッセージが表示された場合は、次のことを示します
- 複数のパイプラインはオーバーラップを実行します(semaphoreロックのために起こらない)
- ブラウザのレンダリングの問題(ページをリフレッシュしてください)
