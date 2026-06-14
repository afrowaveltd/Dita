# نام: Placeholder in Localization

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## نحو

دارندگان از سینتی-براس در ارزش های فرهنگ لغت JSON استفاده می کنند:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## ذخیره سازی

صاحبان نام دارای دو منبع ارزش هستند:

### ۱- مقدار زمان (برای داده های پویا)

ارزش های Pass به طور مستقیم هنگامی که رشته محلی را خراب کنید:

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

### ۲- ارزش های ذخیره شده (برای پیکربندی نیمه استاتیک)

فایل را در دایرکتوری مدیریت می کند:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

ارزش های ذخیره شده به عنوان ** اشتباهات ** عمل می کنند و با ارزش های زمان اجرا می شوند.

## API مرجع

### jsonstring localizer indexer

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

### خدمات IPlace

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

### روش های تمدید

برای راحتی در هنگام کار با:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

استفاده:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## رفتار ترجمه

هنگامی که سرویس ترجمه خودکار با متن با نام دارندگان مواجه می شود:

1. ** قبل از ترجمه ** صاحبان مکان با توکن های امن () ماسک می شوند تا از تغییر موتور ترجمه جلوگیری کنند.
2. ** در ترجمه ** موتور ترجمه فقط متن قابل ترجمه را پردازش می کند.
3. ** پس از ترجمه ** نام های سهامدار اصلی () در موقعیت های صحیح خود بازسازی می شوند.

### مثال

منبع (انگلیسی):

آماده برای ترجمه:

ترجمه به چک:

نتیجه نهایی:

این تضمین می کند که:
- صاحبان مکان هرگز ترجمه یا فاسد نمی شوند
- گرامر زبان هدف می تواند متن اطراف را آزادانه تنظیم کند
- همان قالب به درستی در تمام زبان ها کار می کند

## بهترین شیوه ها

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## ادغام با ترجمه خودکار

به طور خودکار حفظ مکان دارنده را در طول تماس های LibreTranslate کنترل می کند. هیچ پیکربندی اضافی لازم نیست.

و هر دو از سرویس retry استفاده می کنند، بنابراین تمام ترجمه های فرهنگ لغت JSON به طور شفاف از نام Placeholder پشتیبانی می کنند.

## بازگشت به سازگاری

کد موجود با استفاده از سهامداران موقعیت مکانی یا هیچ سهامدار همچنان بدون تغییر کار می کند:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

API Placeholder افزودنی است – استفاده موجود را از بین نمی برد.
