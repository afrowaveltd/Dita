# Pojmenované pamětihodnosti v lokalizaci

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Syntaxe

Placeholders use the curly- orit syntaxe inside JSON dictionary values:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## Skladování

Jmenované jmenovatele mají dva zdroje hodnot:

### 1. Hodnoty runtime (doporučené pro dynamická data)

Přejít hodnoty přímo při načtení lokalizovaného řetězce:

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

### 2. Uložené hodnoty (pro semistatickou konfiguraci)

Správa souboru v adresáři:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Stored values act as **defaults** and are overridden by runtime values.

## Odkaz na API

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

### Metody rozšíření

Pro pohodlí při práci s:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Použití:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Chování překladu

Když se automatická překladatelská služba setká s textem s jmenovanými nosiči:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **After translation**: Original placeholder names (`{name}`) are restored in their correct positions.

### Příklad

Zdroj (anglicky):

Připravené pro překlad:

Přeloženo do češtiny:

Konečný výsledek:

Tím se zajistí, že:
- Placeholders nejsou nikdy přeloženy nebo poškozeny
- Gramatika cílového jazyka může volně přeskupit okolní text
- Stejná šablona funguje správně ve všech jazycích

## Osvědčené postupy

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## Integrace s automatickým překlad

Při volání LibreTranslate automaticky zpracovává uchování místa. Není potřeba žádná další konfigurace.

A oba používají retry služby, takže všechny JSON slovník překlady transparentně podporu pojmenované jmenovatele.

## Zpětná kompatibilita

Stávající kód používající polohové jmenovatele nebo žádné jmenovatele nadále nepracuje beze změny:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Název místa API je aditivní - neporušuje stávající používání.
