# Bezeichnete Platzhalter in der Lokalisierung

Dita unterstützt **named placeholders** in Lokalisierungsstrings, so dass dynamische Werte zur Laufzeit eingefügt werden können, während korrekte Grammatik über Sprachen erhalten bleibt.

## Syntax

Platzhalter verwenden die Curly-Brace-Syntax in JSON Wörterbuch-Werten:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Im Gegensatz zu Positions-Platzhaltern (, ), benannte Platzhalter sind **sprach-agnostisch** — Übersetzer können sie neu anordnen, um die Zielsprache Grammatik anzupassen, ohne den Code zu brechen.

## Lagerung

Namete Platzhalter haben zwei Wertequellen:

### 1. Laufzeitwerte (empfohlen für dynamische Daten)

Passwerte direkt beim Abrufen der lokalisierten Zeichenfolge:

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

### 2. Gespeicherte Werte (für halbstatische Konfiguration)

Die Verwaltung einer Datei im Verzeichnis:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Stored values act as **defaults** and are overridden by runtime values.

## API Referenz

### JsonStringLocalizer Index

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

### Methoden der Erweiterung

Für den Komfort bei der Arbeit mit :

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Verwendung:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Übersetzungsverhalten

Wenn der automatische Übersetzungsservice auf Text mit benannten Platzhaltern trifft:

1. **Vorübersetzung**: Platzhalter werden mit sicheren Token () maskiert, um zu verhindern, dass die Übersetzungsmaschine sie ändert.
2. **During translation**: The translation engine processes only the translatable text.
3. **Nach der Übersetzung**: Original-Platzhalternamen () werden in ihren richtigen Positionen wiederhergestellt.

### Beispiel

Quelle (Englisch):

Vorbereitet für Übersetzung:

Übersetzt nach Tschechische:

Endergebnis:

Dies sorgt dafür, dass
- Platzhalter werden nie übersetzt oder beschädigt
- Zielsprach Grammatik kann den umliegenden Text frei umstellen
- Die gleiche Vorlage funktioniert in allen Sprachen richtig

## Best Practices

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. ** Laufzeitwerte vorgeben**: Für wirklich dynamische Daten (Benutzernamen, Zählungen, Termine), Passwerte zur Laufzeit
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## Integration mit automatischer Übersetzung

Die Platzhalterkonservierung erfolgt automatisch während der LibreTranslate-Anrufe. Es ist keine zusätzliche Konfiguration erforderlich.

Die und beide nutzen den Retry-Service, so dass alle JSON Wörterbuch Übersetzungen transparent benannte Platzhalter unterstützen.

## Rückwärtskompatibilität

Vorhandener Code mit Platzhaltern oder Platzhaltern funktioniert weiterhin unverändert:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Die benannte Placeholder API ist additiv — es bricht nicht die bestehende Nutzung.
