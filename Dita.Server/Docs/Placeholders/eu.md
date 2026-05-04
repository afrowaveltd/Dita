# Lokalizazioko izendun lekuak

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Sintaxia

Leku-jabeek 'curly-brace' sintaxia erabiltzen dute JSON hiztegi-balioen barruan:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Leku-marka posizionalak ez bezala (, ), leku-marka izendatuak **hizkuntza-agnostikoa** dira; itzultzaileek helburuko gramatikarekin bat egin dezakete kodea hautsi gabe.

## Biltegia

Izendatutako leku-markak bi balio-iturri ditu:

### 1. Denboraren balioak (datu dinamikoentzat gomendatuak)

Eman balioak zuzenean lokaleko katea berreskuratzean:

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

### 2. Gordetako balioak (konfigurazio erdi estatikoarentzat)

Direktorioko fitxategi bat kudeatzen du:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Gordetako balioek **defaults** gisa jokatzen dute eta exekuzio-denboraren balioek gainjartzen dituzte.

## API erreferentzia

### JsonStringLocalizer indexatzailea

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

### Hedapen-metodoak

Erosotasunerako:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Erabilera:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Itzulpenaren portaera

Itzulpen automatikoko zerbitzuak leku-markadun testua aurkitzen duenean:

1. **Itzulpena baino lehen**: Leku-jabeek token seguruekin () maskaratzen dituzte itzulpen-motorrak aldatzea saihesteko.
2. **Itzulpen iraunkorra**: Itzulpen-motorrak testu itzulgarria bakarrik prozesatzen du.
3. **Itzulpenaren ondoren**: Jatorrizko leku-izenak () berrezarri egiten dira beren kokaleku egokietan.

### Adibidea

Iturburua (ingelesa):

Itzulpenerako prest:

Txekierara itzulita:

Azken emaitza:

Horrek ziurtatzen du:
- Leku-jabeak ez dira inoiz itzuli edo hondatu
- Helburuko gramatikak inguruko testua libreki berrantola dezake
- Txantiloi berak zuzen funtzionatzen du hizkuntza guztietan

## Praktika onenak

1. **Erabili izen deskriptiboak**
2. **Gorde leku-markak gutxienekoak**: Leku-marka gehiegik zaildu egiten dute itzulpena
3. **Dokumentua espero zen motak**: JSON fitxategi-laguntzako itzultzaileek testuingurua ulertzen dute
4. **Prefer runtime balioak**: Datu benetan dinamikoentzat (erabiltzaile-izenak, zenbaketak, datak), gainditu balioak exekuzio-denboran
5. **Erabili gordetako balioak lehenespenez**: Oso gutxitan aldatzen den konfigurazioa (apl-izena, euskarri-posta)
6. **Validate leku-markak**: Erabili espero zen leku-marka guztiak egiaztatzeko

## Integrazioa itzulpen automatikoarekin

Leku-markaren kontserbazioa automatikoki kudeatzen du LibreTranslate deietan. Ez da konfigurazio gehigarririk behar.

Eta biek erabiltzen dute saiakera-zerbitzua, beraz JSON hiztegi-itzulpen guztiek leku-marka izendatuak onartzen dituzte gardenki.

## Atzeko bateragarritasuna

Leku-marka posizionalak edo leku-markarik gabeko kodeak ez du funtzionatzen

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Etiketa-markaren APIa gehigarria da, ez du existitzen den erabilera hausten.
