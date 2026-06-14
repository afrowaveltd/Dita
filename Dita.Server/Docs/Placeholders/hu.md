# Elnevezett helyfoglalók lokalizációban

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Szintaxis

Helyzetfoglalók használja a curly- merevítő szintaxist belül JSON szótár értékek:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## Tárolás

A "placeholders" -nek két értékforrása van:

### 1. Futási idő értékek (dinamikus adatokhoz ajánlott)

Pass értékek közvetlenül a lokalizált sztring lekérdezésekor:

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

### 2. Tárolt értékek (félstatikus konfigurációhoz)

A fájl kezelése a könyvtárban:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Stored values act as **defaults** and are overridden by runtime values.

## API-hivatkozás

### JsonStringLocalizer indexer

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

### IPlaceholderService

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

### Kiterjesztési módszerek

Kényelmi célra, ha az alábbiakkal dolgozik:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Használat:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Fordítás

Amikor az automatikus fordítási szolgáltatás találkozik szöveget megnevezett plakettezők:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **After translation**: Original placeholder names (`{name}`) are restored in their correct positions.

### Példa

Forrás (angol):

Fordításra előkészítve:

Fordítás:

Végső eredmény:

Ez biztosítja, hogy:
- A helyfoglalókat soha nem fordítják vagy rontják meg
- Cél- nyelv nyelvtan lehet átrendezni a környező szöveget szabadon
- Ugyanaz a sablon működik helyesen minden nyelven

## Legjobb gyakorlatok

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## Integráció automatikus fordítással

A LibreTranslate hívások során automatikusan kezeli a helytartó megőrzését. Nincs szükség további konfigurációra.

A JSON szótár fordításai átlátható módon támogatják az úgynevezett helyfoglalókat.

## Hátrameneti kompatibilitás

A jelenlegi kód a pozicionálási táblákat használva, vagy egyetlen placeable sem működik változatlan formában:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

A megnevezett posztoperátor API adalékanyag - nem töri meg a meglévő használatot.
