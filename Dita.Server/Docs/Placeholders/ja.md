# ローカリゼーションのプレースホルダー

Dita は、ローカライズ文字列で **named placeholders** をサポートしており、言語間で正しい文法を保存しながら、動的値をランタイムで差し込むことができます.

## シンタックス

プレースホルダは、JSON の辞書値の内側の curly-brace 構文を使用します

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

位置のプレースホルダ(、)とは異なり、プレースホルダは**language-agnostic**です。トランスレーターは、コードを破らずにターゲット言語文法と一致させるためにそれらを並べ替えることができます.

## ストレージ

名前付きプレースホルダーには2つの値のソースがあります

### 1.ランタイム値(動的データに推奨)

ローカライズされた文字列を取得すると、直接値を渡します

```csharp
// In a Razor page or controller
@inject JsonStringLocalizer Localizer

var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

### 2. 保存された値(半静的な構成のために)

ディレクトリにファイルを管理します

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

保存された値は、**defaults** として機能し、ランタイム値で上書きされます.

## APIリファレンス

### JsonStringLocalizer インデックス

```csharp
// Without placeholders (backward compatible)
LocalizedString text = localizer["SomeKey"];

// With positional formatting (backward compatible)
LocalizedString text = localizer["SomeKey", "arg1", "arg2"];

// With named placeholders (new)
LocalizedString text = localizer["SomeKey", new Dictionary<string, string>
{
    ["name"] = "value"
}];
```

### IPlaceholderサービス

```csharp
public interface IPlaceholderService
{
    // Get stored placeholders for a key
    Dictionary<string, string> GetPlaceholders(string key);
    
    // Set a stored placeholder value
    void SetPlaceholder(string key, string placeholderName, string value);
    
    // Remove all stored placeholders for a key
    void RemoveKey(string key);
    
    // Format a template with placeholders
    string Format(string template, Dictionary<string, string>? values = null);
    
    // Extract placeholder names from template
    string[] ExtractPlaceholders(string template);
    
    // Check if template contains placeholders
    bool HasPlaceholders(string template);
    
    // Prepare text for translation (mask placeholders)
    (string preparedText, Func<string, string> restore) PrepareForTranslation(string template);
    
    // Persist/load from disk
    Task SaveAsync();
    Task LoadAsync();
}
```

### 延長方法

協力するときの便利のため:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

使用法:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## 翻訳行動

自動翻訳サービスが名前付きプレースホルダーでテキストに遭遇したとき:

1. **翻訳前**: プレースホルダーは安全なトークン()でマスクされ、翻訳エンジンが変更されるのを防ぐことができます.
2. **翻訳中**: 翻訳エンジンは翻訳可能なテキストのみを処理します.
3. **翻訳後**: 元のプレースホルダー名()は、正しい位置で復元されます.

### 事例紹介

ソース(英語):

翻訳の準備:

チェコに翻訳:

最終的な結果:

これにより、次のことが可能になります
- プレースホルダーは、翻訳または破損することはありません
- ターゲット言語文法は周囲のテキストを自由に並べ替えることができます
- 同じテンプレートはすべての言語で正しく機能します

## ベストプラクティス

1. **記述名を使用する**:よりよいですか
2. **キーププレースホルダの最小限**: あまりにも多くのプレースホルダーは、翻訳困難を作る
3. **書類の予想タイプ**: JSONファイルへのコメントは、翻訳者が文脈を理解するのに役立ちます
4. **ランタイムの値を事前に確認**: 動的データ(ユーザ名、カウント、日付)のために、実行時に値を渡します
5. **デフォルトで保存された値を使用する**: 変更がほとんどない設定(アプリ名、サポートメール)
6. **有効なプレースホルダ**: 想定されるすべてのプレースホルダを検証するために使用

## 自動翻訳による統合

LibreTranslate 呼び出し時にプレースホルダーの保存を自動的に処理します。 追加の構成は必要ありません.

両方のリトライサービスを利用しているため、JSON辞書の全ての翻訳は、プレースホルダという名前の翻訳をサポートしています.

## 後方互換性

ポジションプレースホルダーやプレースホルダーを使用して既存のコードは変更されていない作業を続けている:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

名前付きプレースホルダーAPIは添加剤です。既存の使用状況を破棄しません.
