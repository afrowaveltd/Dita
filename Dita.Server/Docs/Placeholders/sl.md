# Imenovani imetniki v lokalizaciji

Dita podpira **imenovane imetnike** v nizih lokalizacije, kar omogoča vstavljanje dinamičnih vrednosti med izvajanjem in ohranjanje pravilne slovnice med jeziki.

## Skladnja

Plačniki uporabljajo kodrasto-brace sintax znotraj vrednosti slovarja JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Za razliko od pozicijskih imetnikov (, ), imenovani kraji so **jezikovno-agnostični** – prevajalci jih lahko prerazporedijo, da se ujemajo ciljno-jezikovno slovnico, ne da bi kršili kodo.

## Shranjevanje

Imenovani imetniki imajo dva vira vrednosti:

### 1. Vrednosti časa delovanja (priporočene za dinamične podatke)

Prenesi vrednosti neposredno pri pridobivanju lokaliziranega niza:

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

### 2. Shranjene vrednosti (za polstatično konfiguracijo)

Upravlja datoteko v imeniku:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Shranjene vrednosti delujejo kot **privzete** in so razveljavljene z vrednostmi časa delovanja.

## Sklic na API

### JsonStringLocalizer

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

### Storitev IPlaceholder

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

### Metode razširitve

Za udobje pri delu z:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Uporaba:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Obnašanje prevodov

Ko avtomatska prevajalska služba naleti na besedilo z imenovanimi imetniki:

1. ** Pred prevajanjem**: Plačniki so zamaskirani z varnimi žetoni (), da bi preprečili, da bi jih prevajalni motor spremenil.
2. ** Med prevajanjem**: Translacijski motor obdeluje samo prekladljivo besedilo.
3. ** Po prevodu**: Izvirna imena imetnikov () so obnovljena v pravilnih položajih.

### Primer

Vir (angleščina):

Pripravljena za prevajanje:

Prevedeno v češčini:

Končni rezultat:

To zagotavlja, da:
- Plačniki niso nikoli prevedeni ali poškodovani
- Ciljno-jezik slovnica lahko prosto preuredi okoliško besedilo
- Ista predloga deluje pravilno v vseh jezikih

## Najboljše prakse

1. **Uporabite opisna imena**: je boljša od ali
2. ** Ohranjajte mecen minimalen**: Preveč ljudi otežuje prevajanje
3. **Pričakovane vrste dokumentov**: Komentarji v datoteki JSON pomagajo prevajalcem razumeti kontekst
4. **Prefer vrednosti časa delovanja**: Za resnično dinamične podatke (uporabniška imena, številke, datumi), vrednosti podajanja ob času delovanja
5. **Uporabi shranjene vrednosti za privzete vrednosti**: Za nastavitve, ki se redko spremenijo (app name, podpora email)
6. **Validate placembers**: Zagotovi se uporaba za preverjanje vseh pričakovanih imetnikov

## Integracija s samodejnim prevajanjem

Samodejno ravna z ohranjanjem imetnika med LibrePrevajanje klicev. Dodatne nastavitve niso potrebne.

In tako uporabljajo storitev ponovnega poskusa, tako da vsi prevodi slovarja JSON pregledno podpirajo imenovane imetnike.

## Združljivost nazaj

Obstoječa koda z uporabo pozicijskih zamenjav ali brez njih še naprej nespremenjeno deluje:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Imenovani kraj API je aditiv – ne prekine obstoječe uporabe.
