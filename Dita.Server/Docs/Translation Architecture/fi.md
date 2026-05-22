# Käännösarkkitehtuuri

Tässä asiakirjassa kuvataan Ditan automaattisen käännösjärjestelmän modulaarista arkkitehtuuria, joka on otettu käyttöön ylläpidettävyyden, testaavuuden ja sietokyvyn parantamiseksi.

## Suunnittelutavoitteet

Korjauksessa käsiteltiin useita alkuperäisen monoliittisen mallin ongelmia:

- ** Huolenjako**: Kukin käännösalue (maat, JSON-sanakirjat, Markdown) on eristetty.
- ** Perusjatkuvuus**: Tiedostot tallennetaan kielellä välittömästi kääntämisen jälkeen, mikä vähentää muistin käyttöä ja tuottaa aikaisempia tuloksia.
- ** Kestävyys**: Useat uusintatasot käsittelevät ohimeneviä epäonnistumisia sulkematta koko putkistoa.
- ** Havaintokyky**: Kaikki merkittävät operaatiot raportoidaan SignalR:n kautta reaaliaikaista seurantaa varten.
- ** Laajuus**: Uusia käännöstavoitteita voidaan lisätä yhden käyttöliittymän avulla.

## Palvelun hajoaminen

### BackendTranslationService (orkesteri)

** Vastuut**:
- Putkiston elinkaaren hallinta (käynnistys, valmistuminen, virheiden käsittely)
- Semafore-pohjainen valuutan valvonta (ehkäisee päällekkäisiä juoksuja)
- Palvelimen validointi (viivästys, kielen saatavuus, kokoonpano)
- Osapalvelujen siirto

**Does NOT contain**:
- Käännöksen logiikka
- Tiedosto I/O tietyissä muodoissa
- Yritä uudelleen logiikkaa

### MaatKäännöspalvelu

** Vastuut**:
- Lue kansiosta
- Synkronisoi maan nimet oletuslocale sanakirjaan
- Käännä puuttuvat maanimet kohdekieltä kohti
- Tallenna jokainen kohde sanakirja heti käännöksen jälkeen

**Key behaviors**:
- Jos oletuskieli on englanti: maan nimet tallennettu nimellä
- Jos oletuskieli on muu: Englanti nimet käännetty oletuskieli ensin
- Jokainen kieli käsitellään itsenäisesti oman retry silmukan

### lokalizationtranslationservice

** Vastuut**:
- Havaitse lisätty tai poistettu avaimet vertaamalla nykyistä oletussanakirjaa edelliseen tilannekuvaan
- Käännä lisättyjä avaimia jokaiselle kohdekielelle
- Poista poistetut avaimet jokaisesta kohdekielestä
- Tallenna kuvakuva seuraavaa vertailua varten

**Key behaviors**:
- Manuaaliset käännökset ovat aina etusijalla (ei koskaan ylikirjoitettu)
- Lisänäppäimet käännetään ja tallennetaan kielellä välittömästi
- Poistetut avaimet poistetaan heti kielellä
- Snapshot tallennetaan vasta, kun kaikki kielet valmistuvat onnistuneesti

### asiakirjakäännöspalvelu

** Vastuut**:
- Kävele konfiguroituja Markdown-juuret rekursiivisesti
- Tunnista muuttuneet lähdekooditiedostot käyttäen SHA-256 hashes
- Kappale lohkoa kohti käännöstila
- Käännä lohko kerrallaan ja yritä uudelleen
- Validoidaan Markdown rakenne käännöksen jälkeen
- Tallenna jokainen kohdekielitiedosto itsenäisesti

**Key behaviors**:
- Ryhmätason rakeisuus: otsikot, kohdat, luettelon kohdat käännetään erikseen
- Metadataraidat, jotka estävät onnistuneen/hylätyn kielen
- Epäonnistuneet lohkot yritetään uudelleen seuraavalla juoksulla ilman uudelleentranslatointi onnistuneita lohkoja
- Rakennevalidointi takaa otsakemäärät, luettelot, koodilohkot jne

## Uudelleen

Järjestelmä toimii kolmella tasolla:

### Taso 1 HTTP (LibreTransateService)

- Enintään 5 yrittää eksponentiaalinen backoff (1s, 2s, 3s, 4s, 5s)
- Käsittelevät verkon aikakatkaisut, 5xx virheet ja ohimenevät viat
- Rakennettu HTTP-asiakasasetuksiin

### Taso 2

- Enintään 3 yritystä 30 sekunnin viiveellä
- Uudelleen ajaa koko käännöspyyntö jälkeen HTTP-tason retries on käytetty
- Paikan pidin naamiointia ja restaurointia sovelletaan tällä tasolla

### Taso 3

- Virheelliset yksittäiset merkintälohkot on merkitty metatietoihin
- Noudettu automaattisesti seuraavalla putkijohdolla
- Onnistuneita lohkoja ei koskaan käännetä uudelleen

## Tietovirta

### JSONin sanakirja

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### Markdown käännös

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### Maan nimi käännös

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## Valtion pysyvyys

### Kuvat

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Mahdollistaa inkrementaalisen synkronoinnin seuraamalla edellisen ajon tapahtumia

### Hash-tiedostot

- **Markdown**: lähdetiedoston vieressä
- **Fallback**: jos ensisijainen sijainti lukee vain
- **Purpose**: Havaitsee lähteen muutoksia välttää tarpeettoman uudelleenkäännöksen

### Käännösmetatiedot

- ** merkintä**:
- ** Sisältö**:
  - Lähde:
- kieli-blokin tila (array of booleans)
- Viimeisin päivitysaikaleima
- **Purpose**: Mahdollistaa vain epäonnistuneiden lohkojen osittaisen uudelleenkäännöksen

### Varasto

- **File**: `Locales/placeholders.json`
- **Sisältö**: Sanakirja avaimista paikanhaltijan nimi-arvo paria
- **Purpose**: Tarjoaa oletusarvot nimetyille paikanhaltijoille koko sovelluksessa

## SignalR-raportointi

### Julkaisijan abstraktio

decouples translation services from SignalR specifications:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sekvenssitakeet

- Yhden ajon viestit ovat yksitoikkoisia
- Sarjanumerot ovat yksilöllisiä per-ajo kautta
- Asiakkaat voivat havaita aukkoja tai tilata uudelleen

### Napakartoitus

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Laajennuspisteet

### Uuden käännöstavoitteen lisääminen

1. Luo uusi käyttöliittymä
2. Toteuta rajapinta verkkoaluekohtaisen logiikan kanssa
3. Rekisteröidy DI-säiliössä
4. Ruiskuta rakennukseen
5. Puhelu nykyisten vaiheiden jälkeen

### Oma uusintakäytäntö

Ohita rakentajan parametrit:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### Oma paikkakäsittely

Toteuta vaihtaaksesi paikanhaltija syntaksi tai tallennus:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Asetukset

### apsetations.json

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### Käynnistys

Asetukset
|---------|---------|--------|
80
10
3
30

## Testausstrategia

### Yksikkötestit

Jokainen osapalvelu voidaan testata itsenäisesti:

- Mock simuloida menestys / epäonnistuminen
- Mock raportoinnin todentamiseksi
- Käytä väliaikaishakemistoja tiedostoon I/ O
- Varmista kielikohtainen tallennus

### Integrointitestit

- Täysi putkisto käynnissä todellinen (paikallinen) LibreKäännä esimerkki
- Varmista SignalR-viestit toimitetaan liitetyille asiakkaille
- Kokeile samanaikaista ajon estoa (semafore)
- Validoidaan Markdown rakenne käännöksen jälkeen

### Päästä päähän -testit

- Trigger käännös kautta API tai aikataulu
- Tarkista kaikki kohdekielitiedostot luodaan/päivitetään
- Tarkista metadatatiedostot sisältävät oikean lohkon tilan
- Vahvistettu paikka haltijat säilytetään kaikissa käännöksissä

## Suorituskykyä koskevat näkökohdat

- **Muisti**: Kielitallennus estää kaikkien sanakirjojen tallentamisen muistiin
- **Disk I/O**: Metadatatiedostot lisäävät pieniä kustannuksia, mutta mahdollistavat lisätyön
- **Verkko**: Sequential käsittely trottling estää ylivoimainen LibreTranslate
- **CPU**: SHA-256 hashing ja regex validointi ovat nopeita suhteessa käännös latenssi
- **SignalR**: Kevyet viestit, tyypillisiin raportteihin ei tarvita hyötykuormakompressiota

## Siirtyminen monoliittisesta suunnittelusta

Alkuperäinen sisälsi kaiken logiikan yhdessä luokassa. Muuttopolku

1. Pura maan logiikka →
2. Pura JSONin logiikka →
3. Pura merkintälogiikka →
4. Pura SignalR julkaisu →
5. Pura retry logiikka →
6. Yksinkertaistetaan orkesteria vain delegaatioon

Kaikki nykyiset rajapinnat () pysyvät muuttumattomina. Putkiston kuluttajat eivät näe muutoksia.
