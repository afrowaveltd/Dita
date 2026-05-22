# Yerlileştirmenin adlanmış yerləri

Dita dəstəkləyir ** xüsusiləşdirilmiş yerləşdiriciləri ** yerlileştirme səfərlərində, dinamik dəyərlər dillər arasında doğru dilləşdirilməsi zamanı işləməyə imkan verir.

## Axtar

Placeholders JSON sözləri dəstəklərində curly-brace sözləri istifadə edin:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Mövcudlar (, ), adlandırılmış yerləşdiricilər ** dil-agnostic** — kompaniyalar onları kodu aradan qaldırmadan həyata keçirməyə bilər.

## Axtarış

Add yerləşdiricilərin qiymətləri iki mövzu var:

### 1. Runtime qiymətləri ( dinamik məlumat üçün silinir)

Yerliləşdirilmiş string retrieving zaman qiymətlərini aparın:

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

### 2. Mağazalı qiymətlər (iki effekt üçün)

Kataloqda bir fayl idarə edir:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

**defaults** və runtime dəyərlər tərəfindən overridden.

## Axtarış

### JsonStringLocalizer indekser

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

### Qeydiyyat

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

### Daxili üsullar

İşlə işləyən rahatlığı üçün:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

istifadə:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Təhsil davranışı

Avtomatik çeviri xidməti yerləşdiriciləri ilə məhsulla görüşür:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **Axtarışdan sonra**: Orijinal yerləşdirici adlar () onların düzgün mövcuddur.

### Axtarış

Yadda saxla

Tarix üçün hazırlıq:

Çempioniya:

Son versiya:

Bu bunu təsdiq edir:
- Qalereya
- Ətraflı dil xəstəlik məhsulları daxil edə bilər
- Eyni şablon bütün dillərdə düzgün çalışır

## Best proqramlar

1. **Fablaşdırma adları**: daha yaxşıdır və yaxşıdır
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document gözəl növü**: JSON faylında məsləhətçilər konfrans anlamaq
4. **Prefer runtime qiymətləri**: Xüsusi dinamik məlumatlar üçün (kullanıcı adları, sayı, tarixləri), runtime dəyişikliklərini keçmək
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate yer sahibləri**: Bütün gözəl mövcudları doğrulamak üçün istifadə edin

## Avtomatik çeviri ilə inteqrasiya

LibreTranslate çağırışları zamanı avtomatik olaraq yerləşdirici saxlama. Əlavə konfiqurasiya lazımdır.

Və hər ikisi retry xidməti istifadə, belə bütün JSON sözlər yerləşdiricilər adlanır.

## Qeydiyyat

Mövcudları istifadə edərək mövcudluq və ya heç bir yer sahibi işləməyə davam edir:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Adı yerholder API əlavə edir - mövcud istifadə deyil.
