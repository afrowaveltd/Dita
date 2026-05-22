# Stedholdere i lokalisering

Dita støtter **navngitt plassholdere** i lokaliseringsstrenger, noe som gjør det mulig å sette inn dynamiske verdier ved kjøring samtidig som riktig grammatikk bevares på tvers av språk.

## Syntaks

Stedholdere bruker curly-brace syntaksen i JSON ordbok verdier:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

I motsetning til posisjonsinnehavere ( ), er navngitte plassinnehavere **språklig-agnostisk** — oversettere kan ombestille dem til å matche målspråklig grammatikk uten å bryte koden.

## Oppbevaring

Navngitte plassholdere har to verdikilder:

### 1. Kjøretidverdier (anbefales for dynamiske data)

Passer verdier direkte når du henter den lokaliserte strengen:

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

### 2. Oppbevarte verdier (for semistatisk konfigurasjon)

Den administrerer en fil i katalogen:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Oppbevarte verdier fungerer som **standarder** og overstyres av løpsverdier.

## API-referanse

### JsonString Localizer indekserer

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

### Utvidelsesmetoder

For bekvemmelighet når du jobber med:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Bruk:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Oversettelsesadferd

Når den automatiske oversettelsestjenesten møter tekst med navngitte plassholdere:

1. **Før oversettelse**: Plassholdere er maskert med trygge polletter () for å hindre at oversettelsesmotoren endrer dem.
2. ** Oversettelse**: Oversettelsesmotoren behandler bare den translaterbare teksten.
3. ** Etter oversettelse**: Opprinnelige stedholdernavn () er restaurert i sine riktige stillinger.

### Eksempel

Kilde (engelsk):

Forberedt på oversettelse:

Oversatt til tsjekkisk:

Sluttresultat:

Dette sikrer at:
- Stedholdere er aldri oversatt eller ødelagt
- Målspråklig grammatikk kan omorganisere teksten fritt
- Den samme malen fungerer riktig på alle språk

## Beste praksis

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Hold plassholdere minimal**: For mange plassholdere gjør oversettelse vanskeligere
3. ** Dokument forventet typer**: Kommentarer i JSON-filen hjelper oversettere å forstå kontekst
4. **Prefer løpsverdier**: For virkelig dynamiske data (brukernavn, teller, datoer), passere verdier på kjøretid
5. ** Bruk lagrede verdier for standard**: For konfigurasjon som sjelden endres (søkenavn, støtte e-post)
6. **Validate stedholdere**: Brukes til å verifisere alle forventet stedholdere er gitt

## Integrasjon med automatisk oversettelse

Den håndterer automatisk bevaring av plassholderen under LibreTranslate-samtaler. Ingen ekstra konfigurasjon er nødvendig.

De og begge bruker gjenforsøkstjenesten, så alle JSON ordbok oversettelser transparent støtte navngitte stedholdere.

## Bakoverkompatibilitet

Eksisterende kode ved bruk av posisjonsinnehavere eller ingen stedsinnehavere fortsetter å arbeide uendret:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Den navngitte plassholder API er additiv - det bryter ikke eksisterende bruk.
