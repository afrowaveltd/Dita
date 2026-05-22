# Sealbhóirí Áite Ainmnithe in Localization

Tacaíonn Dita ** ainmnithe placeholders ** i teaghráin logánaithe, rud a ligeann luachanna dinimiciúla a chur isteach ag runtime agus gramadaí ceart a chaomhnú ar fud na dteangacha.

## Syntax

Úsáideann páirtithe leasmhara an chomhréir curly-brace taobh istigh luachanna foclóir JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Murab ionann agus sealbhóirí áite suite (, ), tá sealbhóirí áite ainmnithe ** teanga-agnostic ** - Is féidir le haistritheoirí iad a athordú a mheaitseáil gramadach sprioc-teanga gan briseadh an cód.

## Stóráil

Tá dhá fhoinse luachanna ag sealbhóirí áite ainmnithe:

### 1. luachanna runtime (molta le haghaidh sonraí dinimiciúla)

Pá luachanna go díreach nuair a retrieving an teaghrán localized:

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

### 2. Luachanna a stóráil (do chumraíocht leath-statach)

An Bainistíonn comhad san eolaire:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Feidhmíonn luachanna a stóráil mar ** réamhshocraithe ** agus tá siad ró-bhródaithe ag luachanna runtime.

## Tagairt API

### JsonStringLocalizer innéacs

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

### seirbhís do chustaiméirí

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

### Modhanna leathnú

Chun áise nuair a bhíonn siad ag obair le:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Úsáid:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Iompar aistriúcháin

Nuair a bhíonn an tseirbhís aistriúcháin uathoibríoch téacs le sealbhóirí áite ainmnithe:

1. **Roghnaigh aistriúchán **: Tá sealbhóirí Áiteanna maisithe le comharthaí sábháilte () chun cosc a chur ar an inneall aistriúcháin ó iad a mhodhnú.
2. **During translation**: Próisis an t-inneall aistriúcháin ach an téacs inaistrithe.
3. **Tar éis aistriúchán **: Ainmneacha sealbhóirí áite bunaidh () a chur ar ais ina seasamh ceart.

### Samplaí

Plandaí faoi dhíon

Ullmhaithe le haghaidh aistriúcháin:

Aistrithe go dtí an tSeicse:

Toradh deiridh:

Cinntíonn sé seo:
- Ní sealbhóirí Áiteanna aistrithe nó truaillithe
- Is féidir le gramadach na Spriocanna an téacs máguaird a athshocrú faoi shaoirse
- Oibríonn an teimpléad céanna i gceart i ngach teanga

## Na cleachtais is fearr

1. ** Ainmneacha tuairisciúla a úsáid **: Is fearr ná nó
2. ** sealbhóirí áite beag **: Too go leor sealbhóirí áit a dhéanamh aistriúcháin níos deacra
3. **Cáipéisí a bhfuiltear ag súil leo **: Comments in JSON comhad cabhrú aistritheoirí tuiscint comhthéacs
4. ** Luachanna runtime **: Le haghaidh sonraí fíor dinimiciúil (ainmneacha úsáideora, comhaireamh, dátaí), luachanna pas ag runtime
5. ** Luachanna stóráilte a úsáid le haghaidh mainneachtainí **: I gcás cumraíochta a athraíonn annamh (ainm iarratais, r-phost tacaíochta)
6. ** sealbhóirí áite incháilithe **: Soláthraítear úsáid chun na sealbhóirí áite a bhfuiltear ag súil leo a fhíorú

## Comhtháthú le haistriúchán uathoibríoch

Láimhseálann an chaomhnú sealbhóirí go huathoibríoch le linn glaonna LibreTranslate. Níl aon chumraíocht breise ag teastáil.

An agus an dá úsáid an tseirbhís retry, mar sin gach aistriúcháin foclóir JSON tacaíocht trédhearcach sealbhóirí áite ainmnithe.

## Comhoiriúnacht ar ais

Cód atá ann cheana ag baint úsáide as sealbhóirí áite suite nó gan aon sealbhóirí áite ag obair gan athrú:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Is é an sealbhóir API ainmnithe breiseán — ní bhriseann sé úsáid atá ann cheana féin.
