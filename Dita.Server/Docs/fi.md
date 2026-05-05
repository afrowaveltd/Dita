# Yhteenveto automaattisen käännöspalvelun muutoksista

## Yleiskatsaus

Tässä asiakirjassa esitetään yhteenveto kaikista muutoksista, jotka on tehty Ditan automaattiseen käännöspalveluun, mukaan lukien arkkitehtuurin refaktorointi, uudet ominaisuudet, havaintokyvyn parannukset ja lokalisointiparannukset.

## Arkkitehtuurin muutokset

### refaktoroitu backendtranslation-palvelu

Monoliitti on hajotettu neljään erikoispalveluun, joita koordinoi kevyt orkesteri:

- **BackendTranslationService**
- **CountriesTranslationService**
- **LocalizationTranslationService** JSONin sanakirjan synkronointi (lisätyt/poistetut avaimet)
- **DocumentsTranslationService**
- **SignalRPublisher ** ..
- **TranslationRetryService**

### Hyödyt

- ** Huolenjako**: Jokainen palvelu käsittelee yhden käännös domain
- ** Kestävyys**: Pienemmät luokat ovat helpompi ymmärtää ja testata
- ** Laajuus**: Uusia käännöstavoitteita voidaan lisätä käyttöliittymän käyttöönoton kautta
- ** Luotettavuus**: Riippumattomat palvelut parantavat vikaeristämistä

## Uudet ominaisuudet

### Live käännösmonitori

** Sijainti**:

Uusi admin sivu, joka tarjoaa reaaliaikaista näkyvyyttä käännös putki:

- Näyttää kaikki SignalR-tapahtumat niiden ilmaantuessa
- Värikoodatut viestityypit (sininen=aloitettu, vihreä=valmis, punainen=virhe)
- Connection status banner auto-reconnect
- Viestilaskuri ja vienti JSONille

### Nimetyt paikanhaltijat

Lokalisointijärjestelmä tukee nyt nimettyä paikkaa () parantaa kieliopillisuutta eri kielillä:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Ominaisuudet:
- Paikanhaltijan arvot, jotka on annettu ajoaikana tai tallennettu
- Automaattinen naamiointi/entisöinti käännöksen aikana korruption estämiseksi
- Taaksepäin yhteensopiva nykyisten sijoituspaikkojen haltijoiden kanssa

### Käännös

Markdown-tiedostot käännetään asteittain:

- **Kielen säästäminen**: Jokainen kohdekieli tallennetaan heti käännöksen jälkeen, mikä vähentää muistipainetta
- **Lock-tason seuranta**: kappaleita käännös tila lohko
- ** Valikoiva uusintatutkimus**: Vain epäonnistuneet lohkot käännetään uudelleen seuraavalla juoksulla
- ** Metatietojen pysyvyys**: Käännöstila kestää sovelluksen uudelleenkäynnistykset

### Parannettu uudelleen-logiikka

Kestävyyden kolme tasoa:

1. **HTTP uusintayritys** (LibreTranslateService): 5 yritystä, joilla on eksponentiaalinen backoff (1s
2. **Stage retry** (TranslationRetryService): 3 uutta yritystä 30-luvun viiveellä
3. **Kellon uudelleenyrittäminen** (DocumentsTranslationService): Epäonnistuneet Markdown-lohkot löytyivät seuraavalla juoksulla

### SignalR-raportointi

Reaaliaikainen raportointi kaikista putkijohtotoiminnoista:

- Jokainen vaihe julkaisee tapahtumia
- Tapahtumina julkaistu per kieli
- Virhetapahtumat sisältävät yksityiskohtaisen kontekstin (lähde, virhekoodi, viesti)
- Sarjanumerot takaavat tilauksen kunkin ajon aikana

## Asetuksen muutokset

### appsetings.json

Ei muutoksia. Nykyiset asetukset toimivat edelleen:

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00"
  }
}
```

### Uudet palvelut

Kirjattu:

- /
- `TranslationRetryService`
- /
- /
- /
- /

SignalR-keskus on kartoitettu asiakasyhteyksiä varten.

## Testaus

### Testitila

- **243/244 koetta ** (1 ohitettu tiedostojen samanaikaisen käytön vuoksi)
- Uusi testin kattavuus lisätty:
  - PlaceholderService -toiminto
  - BackendTranslationService orkesteri
  - JsonStringLocalizer-paikkahakemistot

### Tunnetut rajoitukset

- testi ohittaa, kun ajaa rinnakkain, koska useita testi tapauksia jakaa saman tiedoston. Se kulkee, kun ajaa eristyksissä.

## Uusi tiedostorakenne

### Palvelut

- Pipeline orkesteri
- Maa
- JSONin sanakirjan synkronointi
- Markdown käännös
- SignalR-viestien julkaiseminen
- Yritä logiikkaa paikanpidin naamiointi
- Julkaisijan käyttöliittymä
- — Country service interface
- Lokalisointipalvelun käyttöliittymä
- — Document service interface
- Orkesterin käyttöliittymä (päivitetty)
- — Per-file translation metadata

### Päivitetyt palvelut

- Lisätty nimetty paikka haltija tuki
- Uusi parametri päivitetty
- — Named placeholder management
- ..

### Uusi admin- sivu

- Reaaliaikainen seuranta sivu
- Sivumalli

### Uusi dokumentaatio

- päivitetyt putkijohtoasiakirjat
- paikanpidinjärjestelmän opas
- Dashboard-käyttöohje
- Tekninen arkkitehtuuri

## Takautuva yhteensopivuus

Kaikki muutokset ovat lisäaineita:

- Olemassa oleva lokalisointikoodi () toimii ennallaan
- Sijaintimuoto () toimii ennallaan
- JSONin nykyinen sanakirjamuoto ei muutu
- Olemassa oleva Markdown-rakenne ei muutu
- SignalR-viestit käyttävät samaa muotoa

## Muuttopolku

Siirtoa ei tarvita. Korjauskerroin on sisäinen:

1. Vanha säilytettiin viitteenä ja korvattiin
2. DI-rekisteröinnit päivitettiin käyttämään uusia rajapintoja
3. Kaikki nykyiset kuluttajat eivät näe muutoksia

## Suorituskyvyn parantaminen

- ** Muistinkäyttö**: Tiedostot tallennettu per kieli heti sen sijaan, että pitäisit kaikki muistissa
- **Nopeat kiihdytysajot**: Vain muutetut tai epäonnistuneet merkintälohkot käännetään uudelleen
- ** Näkyvyyden parantaminen**: Reaaliaikainen kehitys auttaa diagnosoimaan hitaita vaiheita

## Tulevaisuuden parannukset

Suunnitellut parannukset:

1. **AI hienosäätö**
2. **Admin-tunnistus**
3. ** Dictionary editor ** .....
4. ** Kääntämistilastot** ..
5. ** Oma paikanhaltija syntaksi**

## Yhteystiedot

Käännöspalvelun kysymyksiä tai ongelmia varten katso kunkin moduulin hakemiston yksityiskohtaiset asiakirjat tai ota yhteyttä kehitystiimiin.
