# ผู้ถือตําแหน่งต่าง ๆ ที่มีชื่อในท้องถิ่น

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## ไวยากรณ์

ตําแหน่งผู้ถือครองจะใช้ไวยากรณ์แบบบิดเบี้ยวภายในพจนานุกรม Json:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## สื่อ

ผู้ถือครองสถานที่มีชื่อ มีสองแหล่งของค่า:

### 1 ค่าเวลาทํางาน (แนะนําให้ใช้กับข้อมูลไดนามิก)

ผ่านค่าโดยตรง เมื่อมีการดึงข้อมูลข้อความภายในเครื่อง:

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

### 2 เก็บค่าที่ตั้งไว้ (สําหรับการปรับแต่งกึ่งปกติ)

จัดการแฟ้มในไดเรกทอรี:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Stored values act as **defaults** and are overridden by runtime values.

## อ้างอิง API

### ตัวทําดัชนี Localuer ของ JsonString

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

### ผู้ถือใบรับรอง IPluservice

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

### วิธีการส่วนขยาย

เพื่อความสะดวกเมื่อทํางานกับ

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

วิธีใช้:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## พฤติกรรม การ แปล

เมื่อบริการแปลภาษาอัตโนมัติ พบกับผู้ถือครองชื่อ:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. ** ภายหลังการแปล **: ชื่อเดิมของผู้ถือสถานที่ () ถูกนํากลับมาในตําแหน่งที่ถูกต้องอีกครั้ง.

### ตัวอย่าง

แหล่ง (อังกฤษ):

เตรียมสําหรับการแปล:

แปลเป็นเช็ก:

ผลสุดท้าย:

นี่รับประกันได้ว่า
- ตําแหน่งผู้ถือครองไม่เคยถูกแปลหรือทุจริต
- ไวยากรณ์เป้าหมายสามารถจัดเรียงข้อความรอบ ๆ ได้อย่างอิสระ
- แม่แบบเดียวกันนี้ใช้ได้ถูกต้องทุกภาษา

## การฝึกที่ดีที่สุด

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## การ แปล แบบ อัตโนมัติ

การรักษาตําแหน่งอัตโนมัติ ระหว่างการโทรแบบ LibreTransate ไม่จําเป็นต้องมีการปรับแต่งเพิ่มเติม.

ทั้ง สอง ฉบับ นี้ และ ทั้ง ใช้ การ บริการ ใหม่ ดัง นั้น ฉบับ แปล ทั้ง หมด ของ เจ สัน จึง ให้ การ สนับสนุน อย่าง โปร่งใส แก่ ผู้ ถือ ตําแหน่ง.

## ความเข้ากันได้แบบย้อนกลับ

การมีรหัสอยู่โดยใช้ผู้ถือตําแหน่งหรือไม่มีเจ้าของ ยังทํางานไม่เปลี่ยนแปลง:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

มี การ เพิ่ม ชื่อ สถาน ที่ ที่ มี ชื่อ ว่า เอ พี ไอ เข้า มา — มัน ไม่ ได้ ทําลาย การ ใช้ อยู่ แล้ว.
