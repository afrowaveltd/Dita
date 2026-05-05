# 本地化中命名的占位符

Dita在本地化字符串中支持**名占位符**,允许在运行时插入动态值,同时保留跨语言的正确语法.

## 语法

占位符使用 JSON 词典内卷轴语法 :

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

与位置占位符不同(,),被命名的占位符是**语言-不可知论**——翻译者可以在不突破代码的情况下重新命令它们与目标语言语法相匹配.

## 存储

命名的占位符有两个值来源:

### 1. 联合国 运行时间值( 为动态数据推荐)

获取本地化字符串时直接传递值 :

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

### 2. 联合国 存储值(用于半静态配置)

管理目录中的文件 :

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

存储值为 ** 默认值 ** 并被运行时值所覆盖 .

## API 参考

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

### IPlachers 服务

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

### 扩展方法

为方便,与下列机构合作:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

用法 :
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## 翻译行为

当自动翻译服务遇到指定的占位符时:

1. ** 在翻译前**: 占位符被用安全符口罩 ()来防止翻译引擎修改.
2. ** 在翻译过程中**: 翻译引擎只处理可变文本.
3. ** 翻译后**:原占地者姓名()按正确位置恢复.

### 示例

资料来源(英文):

编写供翻译:

译为捷克文:

最后结果:

这确保:
- 占位符从未被翻译或损坏
- 目标语言语法可以自由地重新安排周围的文字
- 相同的模板在所有语言中都正确工作

## 最佳做法

1. ** 使用描述性名称**:比或
2. ** 保持最低占位符**: 太多的占位符让翻译更难
3. ** 文件预期类型**:JSON文件中的评论有助于翻译理解上下文
4. ** 优先运行时间值**: 对于真正动态数据(用户名、计数、日期),运行时通过值
5. ** 使用存储的默认值**: 对于很少更改的配置( 应用程序名称, 支持电子邮件)
6. ** 变量占位符**: 用于验证所有预期占位符

## 与自动翻译相结合

在 Libre Translate 通话时自动处理占位符保存 。 不需要额外的配置 .

两者都使用重试服务,因此所有JSON字典翻译都透明地支持命名占位符.

## 后向兼容性

使用位置占位符或没有占位符的现有代码继续有效:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

被命名的占位符 API是添加剂——它不会打破现有的用法.
