# Genoemde Plaatshouders in Localization

Dita ondersteunt **named placeholders** in lokalisatie strings, waardoor dynamische waarden kunnen worden ingevoegd op runtime met behoud van de juiste grammatica in verschillende talen.

## Syntaxis

Plaatshouders gebruiken de curly-brace syntax binnen JSON woordenboek waarden:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## Opslag

Genoemde plaatshouders hebben twee waardenbronnen:

### 1. Runtime waarden (aanbevolen voor dynamische gegevens)

Geef waarden direct door bij het ophalen van de gelokaliseerde string:

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

### 2. Opgeslagen waarden (voor semistatische configuratie)

Het beheert een bestand in de map:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Opgeslagen waarden fungeren als **defaults** en worden overschreven door runtime waarden.

## API-referentie

### JsonStringLocalizer-indexer

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

### IPlaatshouderDienst

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

### Uitbreidingsmethoden

Voor het gemak bij het werken met:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Gebruik:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Vertaalgedrag

Wanneer de automatische vertaaldienst tekst tegenkomt met genoemde plaatshouders:

1. **Voor vertaling**: Plaatshouders worden gemaskeerd met veilige tokens () om te voorkomen dat de vertaalmachine ze te wijzigen.
2. **Tijdens de vertaling**: De vertaalmachine verwerkt alleen de vertaalbare tekst.
3. **Na vertaling**: Originele plaatshouder namen () worden hersteld in hun juiste posities.

### Voorbeeld

Bron (Engels):

Voorbereid voor vertaling:

Vertaald naar het Tsjechisch:

Eindresultaat:

Dit garandeert dat:
- Plaatshouders worden nooit vertaald of beschadigd
- Doeltaal grammatica kan de omliggende tekst vrij herschikken
- Dezelfde sjabloon werkt correct in alle talen

## Beste praktijken

1. ** Gebruik beschrijvende namen**: is beter dan of
2. **Houd plaatshouders minimaal**: Te veel plaatshouders maken vertaling moeilijker
3. **Verwachte typen documenten**: Opmerkingen in het JSON-bestand helpen vertalers de context te begrijpen
4. **Voorkeur runtime waarden**: Voor werkelijk dynamische gegevens (gebruikersnamen, tellingen, data), pas waarden op runtime
5. ** Gebruik opgeslagen waarden voor standaardwaarden**: Voor configuratie die zelden verandert (app name, support email)
6. **Validatieplaatshouders**: Gebruik om alle verwachte plaatshouders te controleren worden verstrekt

## Integratie met automatische vertaling

Tijdens LibreTranslate-gesprekken wordt het behoud van de plaatshouder automatisch behandeld. Er is geen extra configuratie nodig.

De en beide maken gebruik van de retry service, dus alle JSON woordenboek vertalingen transparant ondersteunen benoemde plaatshouders.

## Compatibiliteit achteraf

Bestaande code met behulp van positiehouders of geen plaatshouders blijft ongewijzigd werken:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

De benoemde plaatshouder API is additive .
