# Deţinătorii numiţi în localizare

Dita suportă ** nume de ocupanți** în siruri de caractere de localizare, permițând inserarea valorilor dinamice în timp ce păstrarea gramaticii corecte în limbi.

## Sintaxă

Deţinătorii folosesc sintaxa creţ-creţ în interiorul valorilor dicţionarului JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Spre deosebire de ocupanţii poziţionali (, ), deţinătorii de locuri numiţi sunt ** agnostici în limba** .

## Depozitare

Deţinătorii numiţi au două surse de valori:

### 1. Valorile timpului de execuție (recomandate pentru date dinamice)

Treceţi valorile direct la recuperarea şirului localizat:

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

### 2. Valori stocate (pentru configurare semistatica)

Gestionează un fișier în director:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Valorile stocate acţionează ca default** ** şi sunt suprapuse de valorile runtime.

## Referinţă API

### Indexer JsonStringLocalizator

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

### Serviciul de administrare a locului

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

### Metode de extindere

Pentru comoditate atunci când lucrează cu:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Utilizare:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Comportament de traducere

Atunci când serviciul automat de traducere întâlnește textul cu persoanele numite:

1. **Înainte de traducere**: Deţinătorii sunt mascaţi cu jetoane sigure () pentru a împiedica modificarea motorului de traducere.
2. **În timpul traducerii**: Motorul de traducere proceseaza doar textul traducator.
3. **După traducere**: Numele de titular original () sunt restaurate în pozițiile lor corecte.

### Exemplu

Sursa (engleză):

Pregătit pentru traducere:

Tradus în cehă:

Rezultatul final:

Aceasta garantează că:
- Placeholders nu sunt niciodată traduse sau corupte
- Gramatica în limba țintă poate rearanja liber textul din jur
- Același model funcționează corect în toate limbile

## Cele mai bune practici

1. **Folosiţi nume descriptive**: este mai bun decât sau
2. ** Păstraţi ocupanţii minimali**: Prea mulţi ocupanţi fac traducerea mai dificilă
3. ** Tipuri preconizate de documente**: Comentariile din dosarul JSON ajută traducătorii să înțeleagă contextul
4. ** Preferați valorile runtime **: Pentru date cu adevarat dinamice (nume de utilizator, conte, date), trece valorile la termen
5. ** Utilizați valorile stocate pentru implicite**: Pentru configurarea care rareori se modifică (aplică numele, suport email)
6. **Validaţi locaţiile**: Utilizați pentru a verifica toți titularii de locuri așteptați sunt furnizate

## Integrarea cu traducere automată

Manipulează automat păstrarea ocupantului în timpul LibreTranslate apeluri. Nu este necesară o configurare suplimentară.

Și ambele folosesc serviciul retry, astfel încât toate traducerile JSON dicționar transparent suport nume de ocupanți.

## Compatibilitate înapoi

Codul existent prin utilizarea deţinătorilor poziţionali sau a absenţilor de locaţii continuă să funcţioneze neschimbat:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Deţinătorul numit API este un aditiv .
