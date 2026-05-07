# Nosaukti vietturi lokalizācijai

Dita atbalsta **nosauktus vietturus** lokalizācijas virknēs, ļaujot ierakstīt dinamiskās vērtības darba laikā, vienlaikus saglabājot pareizu gramatiku visās valodās.

## Sintakse

Vietas turētāji izmanto cirtaini-brace sintakses JSON vārdnīcas robežās vērtības:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Atšķirībā no pozicionāliem vietturiem (, ), nosaukti vietturi ir ** valoda-agnostika** — tulkotāji var tos pārkārtot, lai atbilstu mērķvalodas gramatikai, nepārkāpjot kodu.

## Glabāšana

Nosauktajiem vietturiem ir divi vērtību avoti:

### 1. Darbības laika vērtības (ieteicams dinamiskajiem datiem)

Izlaiž vērtības tieši, ielādējot lokalizēto virkni:

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

### 2. Glabātās vērtības (pusstatiskai konfigurācijai)

Pārvalda failu direktorijā:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Saglabātās vērtības darbojas kā ** noklusējumi** un tiek aizstātas ar runtime vērtībām.

## Atsauce uz API

### JsonStringLocalizer indeksētājs

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

### iplacerservice

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

### Paplašināšanas metodes

Ērtības labad, strādājot ar:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Lietošana:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Tulkošanas uzvedība

Kad automātiskais tulkošanas dienests saskaras ar tekstu ar nosauktajiem vietturiem:

1. **Pirms tulkošanas**: Vietas turētāji ir maskēti ar drošiem žetoniem (), lai tulkošanas dzinējs nevarētu tos modificēt.
2. **Tulkojuma laikā**: Tulkošanas dzinējs apstrādā tikai tulkojamo tekstu.
3. **Pēc tulkojuma**: Oriģinālie vietturu vārdi () tiek atjaunoti pareizās pozīcijās.

### Piemērs

Avots (angļu valodā):

Sagatavots tulkošanai:

Tulkots čehu valodā:

Galīgais rezultāts:

Tas nodrošina, ka:
- Vietnieki nekad netiek tulkoti vai bojāti
- Mērķvalodas gramatika var brīvi pārkārtot apkārtējo tekstu
- Viena un tā pati sagatave darbojas pareizi visās valodās

## Paraugprakse

1. **Izmantot aprakstošus vārdus**: ir labāks par vai
2. ** Paturēt vietas turētājus minimālus**: Pārāk daudzi vietturi padara tulkojumu grūtāk
3. **Dokumentu gaidāmie veidi**: Komentāri JSON failā palīdz tulkotājiem saprast kontekstu
4. ** Prefer runtime vērtības**: Patiesi dinamiskiem datiem (lietotāju vārdi, skaits, datumi), pasi vērtības palaišanas laikā
5. ** Izmantot noklusētās vērtības**: Konfigurācijai, kas reti mainās (app name, support email)
6. **Novērtēti vietturi**: Tiek nodrošināta izmantošana visu paredzamo vietturu pārbaudei

## Integrācija ar automātisko tulkošanu

Automātiski apstrādā vietturis saglabāšanu LibreTranslate zvanu laikā. Nav nepieciešama papildu konfigurācija.

Gan izmantot retritry pakalpojumu, tāpēc visi JSON vārdnīca tulkojumi caurspīdīgi atbalsta nosaukto vietturi.

## Aizmugurējā savietojamība

Esošais kods, kurā izmantoti pozicionālie vietturi vai neviens vietturis, turpina darboties nemainīgs:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Nosauktais vietturis API ir piedeva — tas nepārkāpj esošo lietojumu.
