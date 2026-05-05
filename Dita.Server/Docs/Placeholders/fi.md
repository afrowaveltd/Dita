# Nimetyt paikanhaltijat

Dita tukee **nimettyjä paikanpitäjiä** lokalisointijonoissa, jolloin dynaamiset arvot voidaan lisätä ajoaikaan säilyttäen samalla oikea kielioppi eri kielillä.

## Syntaksi

Paikanhaltijat käyttävät kihara-arkku syntaksia sisällä JSON sanakirjassa arvot:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Toisin kuin paikanhaltijat (, ), nimetyt paikanhaltijat ovat **kieli-agnostikko** . Kääntäjät voivat tilata ne uudelleen vastaamaan kohdekielen kielioppia rikkomatta koodia.

## Varastointi

Nimetyillä paikanhaltijoilla on kaksi arvolähdettä:

### 1. Ajoaika-arvot (suositellaan dynaamisia tietoja varten)

Siirrä arvot suoraan, kun haet paikallista merkkijonoa:

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

### 2. Säilytetyt arvot (puolistaattisessa konfiguraatiossa)

Hakemistossa on tiedosto:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Tallennetut arvot toimivat ** olettamina** ja ohittavat ne ajoajan arvoilla.

## API-viite

### JsonStringLocalizer-indeksi

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

### IPaikanpidätyspalvelu

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

### Laajentamismenetelmät

Mukavuuden vuoksi, kun työskentelet kanssa:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Käyttö:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Käännös

Kun automaattinen käännöspalvelu kohtaa tekstin nimetty paikka haltijat:

1. **Ennen käännöstä**: Paikan haltijat ovat naamioitu turvallisia kuponkia () estää käännös moottorin muuttaa niitä.
2. **Käännöksen aikana**: Käännös moottori käsittelee vain käännettävä teksti.
3. ** Käännöksen jälkeen**: Alkuperäiset paikanhaltijan nimet () palautetaan oikeisiin asentoihinsa.

### Esimerkki

Lähde:

Valmistettu käännettäväksi:

Käännetty:

Lopputulos:

Näin varmistetaan, että
- Paikalle haltijat eivät koskaan käännetä tai vioittunut
- Kohdekielen kielioppi voi järjestää ympäröivän tekstin vapaasti
- Sama malli toimii oikein kaikilla kielillä

## Parhaat käytännöt

1. **Käytä kuvailevia nimiä**: on parempi tai
2. ** Säilytä paikat minimaalisesti**: Liian monta paikkaa haltijat tehdä käännös vaikeampaa
3. **Dokumentti-odotustyypit**: JSON-tiedoston kommentit auttavat kääntäjiä ymmärtämään kontekstia
4. ** Määrittele ajoajan arvot**: Todella dynaamisille tiedoille (käyttäjien nimet, määrät, päivämäärät), läpäisyarvot ajohetkellä
5. **Käytä tallennettuja arvoja oletusarvoille**: Asetukseen, joka harvoin muuttuu (sovelluksen nimi, tukisähköposti)
6. ** Validaattipaikan haltijat**: Käyttö tarkistaa kaikki odotetut paikanhaltijat toimitetaan

## Integrointi automaattisella kääntämisellä

Automaattisesti käsittelee paikanhaltijan säilytys aikana LibreKäännä puhelut. Lisäasetuksia ei tarvita.

Ja molemmat käyttävät uusintapalvelua, joten kaikki JSON-sanakirjan käännökset tukevat avoimesti paikanhaltijoita.

## Takautuva yhteensopivuus

Nykyiset koodit, joissa käytetään paikanhaltijoita tai joissa yksikään paikka ei toimi ennallaan:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Nimetty paikan haltija API on additive ... se ei riko olemassa olevaa käyttöä.
