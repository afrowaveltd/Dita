# データモデル

Namespace は、API リクエスト/レスポンスペアからパイプラインレポートとダッシュボードスナップショットまで、ローカリゼーションと翻訳システムで使用されているすべてのデータ構造を定義します.

## モデル概要

### 仕様

#### 自動翻訳設定

設定モデルを . LibreTranslate サーバー接続とパイプライン動作を制御する.

プロパティ
|---|---|---|---|
LibreTranslate サーバー URL
API キーが必要かどうか
APIキー
アプリケーションのデフォルト言語
翻訳から除外する言語
ドキュメントルートディレクトリ
スケジュールされたパイプラインの実行を有効にする
最初の実行前の遅延
実行間分
LibreTranslateテキストエンドポイント
LibreTranslateファイルエンドポイント
LibreTranslate言語エンドポイント
LibreTranslate 検出エンドポイント
翻訳リクエスト間の遅延
リクエストごとのHTTPタイムアウト
Config がロードされたかどうか

### LibreTranslate APIモデル

#### 翻訳依頼 → 翻訳結果

**Request** — テキスト翻訳 API 呼び出し:

プロパティ
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**結果** — 翻訳応答:

プロパティ
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → 検出

**要求**:**応答**:
**Response**: `{ Language, Confidence }`

#### translatefilerequest → 翻訳ファイル

**要求**:**応答**:
**Response**: `{ TranslatedFileUrl }`

#### ライブラリ

エンドポイントからの単一言語エントリ:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### パイプラインレポートモデル

#### チェックレポート

サーバ検証段階の結果:

プロパティ
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### 翻訳レポート

辞書/翻訳段階の結果:

プロパティ
|---|---|
| `DefaultDictionaryExists` | `bool` |
| `DefaultDictionaryCount` | `int` |
| `ToTranslateCount` | `int` |
| `AddedCount` | `int` |
| `RemovedCount` | `int` |
| `SkippedCount` | `int` |
| `TranslatedCount` | `int` |
| `ErrorsCount` | `int` |
| `Errors` | `List<TranslationError>?` |

#### MarkdownTranslationsレポート

マークダウン翻訳段階の結果:

プロパティ
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### ストーリングレポート

持続的な出力の最終的な集計:

プロパティ
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### ステージレポート<T>

ステージメタデータでレポートタイプをラップする汎用コンテナ:

プロパティ
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(必須)

### 翻訳作業モデル

#### フレーズInQueue

翻訳キューの作業項目:

プロパティ
|---|---|
| `Target` | `TranslationTarget` |
| `Key` | `string?` |
| `Phrase` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string` |
| `ChangeRequired` | `PhraseChange` |
| `AddedToList` | `DateTime` |
| `TranslationStart` | `DateTime?` |
| `TranslationEnds` | `DateTime?` |
| `IsTranslated` | `bool` |
| `TranslatedText` | `string?` |

#### 翻訳エラー

すべてのレポートで行われる構造化されたエラーレコード:

プロパティ
|---|---|
(言語コード、ファイルパス、ステージ名)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### シングルトランスレーション

単一の局所辞書:

プロパティ
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownTranslatableブロック

Markdown文書から抽出されたブロック:

プロパティ
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### テキスト解像度モデル

#### テキストローカリゼーション リクエスト → テキストローカリゼーション ソリューション

**リクエスト** — 辞書ベースのローカリゼーション(書き込み可能):

プロパティ
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**応答**:

プロパティ
|---|---|
(オリジナル)
(ローカライズ)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### テキスト翻訳リクエスト → テキスト翻訳応答

**リクエスト** — 動的翻訳(読み込みのみ):

プロパティ
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**応答**:

プロパティ
|---|---|
(オリジナル)
(翻訳済み)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### テキストソリューションソース

Localized/translated が次の値から解決された場所を特定します

バリュー
|---|---|
ターゲット言語のローカル辞書で発見
デフォルトの言語辞書で発見
見つかりません。デフォルト辞書に追加
LibreTranslate による返送
解像度なしでas-isを返す

### 共有タイプ

#### カントリーディフェンス

読まれた記入項目:

プロパティ
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### 比較条件

評価のためのフィルター条件:

プロパティ
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### エラー応答

シンプルな API エラー envelope:

プロパティ
|---|---|
| `Error` | `string?` |
