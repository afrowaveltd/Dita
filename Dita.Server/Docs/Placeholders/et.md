# Lokaliseerimisel määratud kohatäitjad

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Süntaks

Kohatäitjad kasutavad JSONi sõnastiku väärtustes lokkis trakside süntaksit:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Erinevalt positsioonilistest kohahoidjatest (, ) on nimelised kohahoidjad ** keele-agnostilised ** - tõlkijad saavad neid ümber korraldada, et need vastaksid sihtkeele grammatikale koodi lõhkumata.

## Hoidla

Nimetatud kohaomanikel on kaks väärtuste allikat:

### 1. Käitusaja väärtused (soovitatav dünaamiliste andmete puhul)

Lokaliseeritud stringi hankimisel edastatakse väärtused otse:

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

### 2. Salvestatud väärtused (poolstaatilise konfiguratsiooni jaoks)

Faili haldamine kataloogis:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Salvestatud väärtused toimivad ** vaikeväärtustena ** ja neid tühistavad käitusaja väärtused.

## API viide

### JsonStringLocalizer indexerName

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

### asendajateenus

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

### Laiendusmeetodid

Mugavuse huvides töötades :

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Kasutamine:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Tõlkekäitumine

Kui automaattõlketeenus kohtab teksti nimega kohahoidjatega:

1. ** Enne tõlkimist**: Kohahoidjad on maskeeritud turvamärkidega (), et tõlkemootor ei saaks neid muuta.
2. ** Tõlke ajal**: Tõlkemootor töötleb ainult tõlgitavat teksti.
3. **Pärast tõlkimist**: Algsed kohatäitjate nimed () taastatakse õigetes asukohtades.

### Näide

Allikas (inglise keeles):

Valmistatud tõlkimiseks:

Tõlgitud tšehhi keelde:

Lõpptulemus:

Sellega tagatakse, et:
- Kohahoidjaid ei tõlgita ega rikuta kunagi
- Sihtkeelne grammatika võib ümbritsevat teksti vabalt ümber korraldada
- Sama mall töötab õigesti kõigis keeltes

## Parimad tavad

1. ** Kasuta kirjeldavaid nimetusi **: on parem kui või
2. **Hoida kohatäitjad minimaalsed**: Liiga palju kohatäitjaid muudab tõlkimise raskemaks
3. ** Dokumendi eeldatavad liigid**: JSON-faili kommentaarid aitavad tõlkijatel konteksti mõista
4. ** Käitamisaja väärtused**: Tõeliselt dünaamiliste andmete (kasutajanimed, loendused, kuupäevad) puhul läbimisväärtused tööajal
5. ** Kasuta vaikeväärtuste puhul salvestatud väärtusi**: Seadistamine, mis harva muutub (rakenduse nimi, e-posti tugi)
6. **Kindlaksmääratud kohatäitjad**: Kasutamine, et kontrollida kõigi eeldatavate kohaomanike olemasolu

## Integreerumine automaatse tõlkega

Automaatselt tegeleb kohahoidja säilitamisega LibreTranslate'i kõnede ajal. Täiendavat konfiguratsiooni ei ole vaja.

Mõlemad kasutavad kordusproovimise teenust, nii et kõik JSON sõnastiku tõlked toetavad läbipaistvalt nimelisi kohahoidjaid.

## Tagasiühilduvus

Olemasolev kood, mis kasutab positsioonikohahoidjaid või mitte ühtegi kohatäitjat, töötab jätkuvalt muutumatult:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Nimega kohahoidja API on lisand - see ei katkesta olemasolevat kasutamist.
