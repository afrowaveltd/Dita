# 在本地化中被命名的占位符

Dita 在本地化字串中支持 ** 取名的占位符 **, 在執行時可以插入活性值, 同时保留不同語言的正則文法 .

## 語法

在 JSON 字典值中, 位址使用者使用卷曲- brace 語法:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

有名的占位符是**語言-不可知性**-翻譯者可以重新排序以匹配目標語言語法而不需要被破解出碼.

## 儲存

已命名的占位符有兩個取自:

### 1. runtime 值 (被推荐為活性相關資料)

在取回本地化字串后直接傳出數值:

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

### 2. 有 已儲存值 (半靜態配置)

在目錄中管理檔案:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

被儲存的數值起於**defaults **被运行時數值所覆蓋 .

## API 參考

### JsonString 本地化索引器

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

### 接取器伺服器

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

### 延伸方法

在同:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

用法:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## 翻譯行為

自動翻譯服務遇到已命名的占位符后:

1. ** 在翻译前**: 有安全符號被遮住以阻止翻譯引擎修改.
2. ** 在翻譯中**: 翻譯引擎只處理可翻譯的文字 .
3. ** 在翻譯后**:原占地者姓名()以正确位置被恢复.

### 示例

出自 (英文) :

出自:

翻譯成克羅地亞:

最后成果:

它能确保:
- 有位符從未被翻譯或損壞
- 目標語言語法可以自由重排相圍的文字
- 有同樣樣的樣本能用同樣的語言做得到

## 最佳做法

1. ** 使用描述性名称**:好于或
2. ** 保持最低占位符**: 有太多的占位符使翻譯更難做
3. ** 文件需要類型**: JSON 文件中的註解能幫助翻譯者理解上下文
4. ** 优先跑取值**: 对于真正的活性資料 (用戶名、 數目、 日期) , 在跑取時傳出數值
5. ** 为預設使用已儲存的值**: 对于很少變更的設定 (app name, 支援電子郵件)
6. ** 空缺占位符**: 已提供所有需要的占位符以驗證

## 与自動翻譯相整合

在 Libre Translate 呼叫中自動處理占位符保存 。 不需要附加配置 .

因此所有 JSON 字典的翻譯都透明地支援了已命名的占位符 .

## 后向相容性

使用位置占位符或沒有占位符的已存在的代碼仍舊有效:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

有名的占位符 API 有相通相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相相接相接相接相接相接相接相接相相相相接相接相接相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相.
