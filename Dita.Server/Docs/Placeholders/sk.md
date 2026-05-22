# Pomenovaní držitelia miest v lokalizácii

Dita podporuje ** pomenovaných umiestovateľov** v lokalizačných reťazcoch, čo umožňuje vkladanie dynamických hodnôt v čase behu pri zachovaní správnej gramatiky v jazykoch.

## Syntax

Umiestnenia používajú syntax kučeravého race vnútri slovníkových hodnôt JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Na rozdiel od polohových držiteľov miest (, ), pomenovaní lokátori sú ** jazykovo-agnostik** .

## Skladovanie

Pomenovaní držitelia miest majú dva zdroje hodnôt:

### 1. Runtime hodnoty (odporúčané pre dynamické dáta)

Prejdite hodnoty priamo pri získavaní lokalizovaného reťazca:

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

### 2. Uložené hodnoty (pre polostatickú konfiguráciu)

Správa súboru v adresári:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Uložené hodnoty účinkujú ako ** predvolené hodnoty** a sú prekročené hodnotami času.

## Odkaz na API

### JsonStringLocalizer indexer

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

### Name

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

### Metódy rozšírenia

Pre pohodlie pri práci s:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Použitie:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Prekladové správanie

Keď sa automatická prekladateľská služba stretne s textom s pomenovanými usadlíkmi:

1. **Pred prekladom**: Držiaky sú maskované bezpečnými žetónmi (), aby sa prekladateľský motor nemenil.
2. **Počas prekladu**: Prekladateľský nástroj spracúva iba prekladateľný text.
3. **Po preklade**: Pôvodné mená držiteľa () sú obnovené v ich správnej pozícii.

### Príklad

Zdroj (anglický):

Pripravené na preklad:

Preložené do čeština:

Konečný výsledok:

Tým sa zabezpečí, že:
- Držitelia miesta nie sú nikdy preložený alebo poškodený
- Cieľový jazyk gramatika môže prestaviť okolitý text voľne
- Rovnaká šablóna funguje správne vo všetkých jazykoch

## Osvedčené postupy

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. ** Udržujte minimálne pozície**: Príliš veľa prekladateľov sťažuje preklad
3. ** Očakávané typy dokumentu**: Komentáre v súbore JSON pomôcť prekladateľom pochopiť kontext
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. ** Používajte uložené hodnoty pre predvolené hodnoty**: Pre konfiguráciu, ktorá sa zriedka mení (meno aplikácie, e-mail podpory)
6. ** Overiť držiteľov miest**: Použiť na overenie všetkých predpokladaných umiestnení

## Integrácia s automatickým prekladom

Automaticky manipuluje s uchovaním hráča počas hovorov LibreTranslate. Nie je potrebná ďalšia konfigurácia.

Obaja používajú retry službu, takže všetky preklady JSON slovníka transparentne podporujú menovaných lokátorov.

## Spätná kompatibilita

Existujúci kód s použitím pozičných držiteľov miest alebo žiadnych držiteľov miest naďalej funguje nezmenený:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Pomenovaný miestodržiteľ API je doplnkovou látkou, ktorá neporušuje existujúce použitie.
