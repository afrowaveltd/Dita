# שמות בעלי מקומות מקומיים

Dita תומך **שם בעלי מקומות** בחוזים מקומיים, ומאפשר ערכים דינמיים להיות מוכנס בזמן ריצה תוך שמירה על דקדוק נכון בשפות.

## Syntax

בעלי מקומות משתמשים בסנכר בתוך ערכי מילון JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

שלא כמו בעלי מקומות מיקום (, ), שמות בעלי מקומות הם **-agnostic ** - מתרגמים יכולים להזמין אותם כדי להתאים את הדקדוק בשפה היעד מבלי לשבור את הקוד.

## אחסון

לבעלי המקום יש שני מקורות ערכים:

### 1.1 1. ערכי Runtime (recommend for דינמיות)

להעביר ערכים ישירות בעת החזרת המיתרים המקומיים:

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

### 2. ערכים מאוחסנים (עבור תצורה חצי סטטית)

מנהל קובץ:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

ערכים מאוחסנים פועלים כ- **defaults** והם מרושנים על ידי ערכים רצופים.

## API

### JsonString Localizer Indexer

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

### חברת IPlace

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

### שיטות הרחבה

נוחות בעת עבודה עם:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

שימוש:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## התנהגות תרגום

כאשר שירות התרגום האוטומטי נתקל בטקסט עם שמות:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **After translation**: Original placeholder names (`{name}`) are restored in their correct positions.

### דוגמא

מקור (אנגלית):

מוכן לתרגום:

תורגם לצ'כיה:

התוצאה הסופית:

זה מבטיח את זה:
- בעלי מקומות לעולם לא יתורגמו או מושחתים
- דקדוק בשפה ממוקדת יכול לארגן מחדש את הטקסט שמסביב בחופשיות
- אותה תבנית עובדת נכון בכל השפות

## שיטות הטוב ביותר

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. ** שמור על בעלי המקום מינימלי ** יותר מדי בעלי מקומות עושים תרגום חזק יותר
3. **הטיפול הצפוי לסוגים**: הערות בקובץ JSON מסייעות למתרגמים להבין את ההקשר
4. ** ערכים זמניים מראש**: עבור נתונים דינמיים באמת (שמות משתמשים, ספירות, תאריכים), להעביר ערכים בזמן ריצה
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. ** בעלי מקומות פנויים ** שימוש כדי לאמת את כל בעלי המקום הצפויים מסופקים

## אינטגרציה עם תרגום אוטומטי

באופן אוטומטי מטפלים בשמירת בעלי המקום במהלך שיחות ליבריג. אין צורך בתצורה נוספת.

שניהם משתמשים בשירות השבירה, כך שכל תרגומים של מילון JSON תומכים באופן שקוף בשם בעלי המקום.

## תאימות אחורית

קוד קיים באמצעות בעלי מיקום או לא בעלי מקומות ממשיך לעבוד ללא שינוי:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

ה- API של בעל המקום הוא תוסף – הוא לא שובר את השימוש הקיים.
