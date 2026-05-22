# الملاجئ المسماة في موقع محلي

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Syntax

ويستخدم أصحاب الملاجئ النسيج العنيف في القيم القامسية للشركة:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## التخزين

ولحاملي المسكن الاسمي مصدران للقيم:

### 1 - قيم الدوام (الموصاة بالبيانات الدينامية)

تمرر القيم مباشرة عند استرجاع الخيوط المحلية:

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

### 2 - القيم المسروقة (للتشكيل شبه الإحصائي)

ويدير الملف في الدليل:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

وتتصرف القيم المخزنة على أنها ** قسائم** وتتجاوزها القيم غير المتكررة.

## مرجع API

### مؤشر JsonStringLocalizer

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

### الخدمة

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

### أساليب التمديد

للراحة عند العمل مع:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

الاستخدام:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## سلوك الترجمة التحريرية

عندما تلتقي دائرة الترجمة الآلية بنص مع أصحاب الأماكن المسمّين:

1. ** قبل الترجمة**: ويُخفى الملاجئ بعلامات آمنة () لمنع محرك الترجمة من تعديلها.
2. ** ترجمة**: محرك الترجمة يعمل فقط النص القابل للترجمة.
3. ** بعد الترجمة**: وتعاد أسماء المسكن الأصلية () إلى مواقعها الصحيحة.

### مثال

المصدر (الإنكليزية):

معد للترجمة:

ترجمة:

النتيجة النهائية:

ويضمن ذلك ما يلي:
- الملاجئ لا تترجم أو تفسد
- يمكن أن يعيد ترتيب النص المحيط بحرية
- يعمل النموذج نفسه بشكل صحيح بجميع اللغات

## أفضل الممارسات

1. ** استخدام الأسماء الوصفية**: هو أفضل من أو
2. ** حافظوا على المسكنات عند الحد الأدنى**: الكثير من المسكنات يجعلون الترجمة أكثر صعوبة
3. ** الأنواع المتوقعة من الوثائق** Comments in the JSON file help translators understand context
4. ** القيم غير المتكررة للإحالة**: بالنسبة للبيانات الدينامية حقاً (أسماء المستخدمين، العد، التواريخ)، قيم المرور في الوقت الحاضر
5. ** استخدام القيم المخزنة للمتخلفين**: بالنسبة للتشكيلات التي نادرا ما تتغير (الاسم التطبيقي، البريد الإلكتروني الداعمة)
6. ** مالكو الأماكن المرشحون**: الاستفادة من التحقق من جميع المساكن المتوقعة

## التكامل مع الترجمة الآلية

The automatically handles placeholder preservation during LibreTranslate calls. ولا حاجة إلى تشكيل إضافي.

The and both use the retry service, so all JSON dictionary translations transparently support named placeholders.

## التوافق الرجعي

لا تزال المدونة القائمة التي تستخدم حاملي أماكن العمل أو لا يوجد ملاجئ تعمل دون تغيير:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

واسم صاحب الموقع " API " مضاف - فهو لا يكسر الاستخدام القائم.
