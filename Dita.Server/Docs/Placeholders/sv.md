# Namngivna platshållare i lokalisering

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Syntax

Placeholders använder den lockiga syntaxen i JSON-ordboksvärden:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## Lagring

Namngivna platsägare har två värderingskällor:

### 1. Runtime-värden (rekommenderas för dynamiska data)

Passera värden direkt när du hämtar den lokaliserade strängen:

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

### Lagrade värden (för halvstatisk konfiguration)

Hanterar en fil i katalogen:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Lagrade värden fungerar som ** standarder** och är överbelagda av runtime-värden.

## API referens

### JsonStringLocalizer indexerare

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

### Extensionsmetoder

För bekvämlighet när du arbetar med:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Användning:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Översättningsbeteende

När den automatiska översättningstjänsten möter text med namngivna platshållare:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. ** Under översättning**: Översättningsmotorn behandlar endast den översättbara texten.
3. **After translation**: Original placeholder names (`{name}`) are restored in their correct positions.

### Exempel

Källa (engelska):

Förberedd för översättning:

Översatt till tjeckiska:

Slutresultat:

Detta säkerställer att:
- Platsägare översätts aldrig eller korrumperas
- Målspråkig grammatik kan omorganisera den omgivande texten fritt
- Samma mall fungerar korrekt på alla språk

## Bästa praxis

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: För verkligt dynamiska data (användarnamn, räknas, datum), passvärden vid runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Använd för att verifiera alla förväntade platshållare tillhandahålls

## Integration med automatisk översättning

Den hanterar automatiskt placeholder bevarande under LibreTranslate samtal. Ingen ytterligare konfiguration behövs.

De och båda använder retry-tjänsten, så alla JSON-ordboksöversättningar stöder öppet namngivna platshållare.

## Bakåtkompatibilitet

Befintlig kod med hjälp av positionsägare eller ingen platshållare fortsätter att fungera oförändrad:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Den namngivna platshållaren API är tillsats - det bryter inte befintlig användning.
