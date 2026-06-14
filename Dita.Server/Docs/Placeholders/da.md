# Opkaldt placeholdere i lokalisering

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Syntaks

Placeholderne bruger curly- brack syntaks inde i JSON ordbog værdier:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## Opbevaring

Navngivne pladsholdere har to værdikilder:

### 1. Køretidsværdier (anbefales til dynamiske data)

Pass værdier direkte når du henter den lokaliserede streng:

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

### 2. Lagrede værdier (for semistatisk konfiguration)

Den administrerer en fil i mappen:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Stored values act as **defaults** and are overridden by runtime values.

## API-reference

### JsonStringLokalizer-indeksering

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

### ipladsholderservice

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

### Udvidelsesmetoder

For nemheds skyld, når du arbejder med:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Brug:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Oversættelse

Når den automatiske oversættelsestjeneste møder tekst med navngivne pladsholdere:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **After translation**: Original placeholder names (`{name}`) are restored in their correct positions.

### Eksempel

Kilde (engelsk):

Tilberedt til oversættelse:

Oversat til tjekkisk:

Endeligt resultat:

Dette sikrer, at:
- Pladsholdere er aldrig oversat eller beskadiget
- Målsprog grammatik kan omarrangere den omgivende tekst frit
- Den samme skabelon fungerer korrekt på alle sprog

## Bedste praksis

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## Integration med automatisk oversættelse

Den håndterer automatisk pladsholder opbevaring under LibreTranslate opkald. Ingen yderligere konfiguration er nødvendig.

De og begge bruge rely service, så alle JSON ordbog oversættelser gennemsigtigt støtte navngivne pladsholdere.

## Kompatibilitet bagfra

Eksisterende kode ved hjælp af positionsholdere eller ingen pladsholdere fortsætter med at fungere uændret:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Den navngivne pladsholder API er additiv - det bryder ikke eksisterende brug.
