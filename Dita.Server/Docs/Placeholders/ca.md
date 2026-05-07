# Variables de substitució amb nom a la localització

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Sintaxi

Les variables de substitució usen la sintaxi de la cadena de fitxers entre els valors del diccionari JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## Emmagatzematge

Les variables de substitució amb nom tenen dues fonts de valors:

### 1. Valors d' execució (recomanat per dades dinàmiques)

Passa els valors directament en recuperar la cadena local:

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

### 2. Valors emmagatzemats (per configuració semi- astàtic)

El fitxer gestiona un fitxer en el directori:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Stored values act as **defaults** and are overridden by runtime values.

## Referència API

### JsonStringLocalzer indexador

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

### Servei de correu IPlace

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

### Mètodes d' extensió

Per comoditat en treballar amb:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Ús:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Comportament de la traducció

Quan el servei de traducció automàtic troba text amb variables de nom:

1. **Abans de traducció **: Els paràmetres de substitució són emmascalats amb fitxes segures () per evitar que el motor de traducció els modifica.
2. **Dinir traducció **: Els processos de motor de traducció només són el text translatable.
3. **After translation**: Original placeholder names (`{name}`) are restored in their correct positions.

### Exemple

Origen (anglès):

S' ha preparat per a la traducció:

Traduït a txec:

Resultat final:

Això assegura que:
- Els paràmetres de substitució mai es tradueixen o s' han corromput
- La gramàtica de l' idioma objectiu pot col· locar el text que envolta lliurement
- La mateixa plantilla funciona correctament en tots els idiomes

## Les millors pràctiques

1. **Usa noms descriptius ** És millor que o
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## Integració amb traducció automàtica

La conservació de substitució gestiona automàticament durant les crides de Libretrate. No cal configuració addicional.

Les i ambdós usen el servei reintentar- ho, de manera que totes les traduccions del diccionari JSON suport transparentment amb marcadors de substitució.

## Compatibilitat enrere

Codi existent usant marcadors de posició o no es continua cap marcador de posició:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

L' API de posició de nom és additiu libno es trenca l' ús existent.
