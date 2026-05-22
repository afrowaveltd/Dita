# Tõlkearhitektuur

See dokument kirjeldab Dita automaatse tõlkesüsteemi modulaarset arhitektuuri, mis võeti kasutusele hooldatavuse, testitavuse ja vastupidavuse parandamiseks.

## Disaini eesmärgid

Refaktoreerimine käsitles esialgse monoliitse disainiga mitmeid probleeme:

- ** Murede lahusus**: Iga tõlkedomeen (riigid, JSON sõnastikud, Markdown) on isoleeritud.
- ** Järkjärguline püsivus**: Failid salvestatakse keele kaupa kohe pärast tõlkimist, vähendades mälukasutust ja pakkudes varasemaid tulemusi.
- ** Vastupidavus **: mitu retry-taset tegelevad mööduvate riketega, blokeerimata kogu torujuhet.
- **Observaability**: Igast olulisest operatsioonist teatatakse SignalR-i kaudu reaalajas jälgimiseks.
- ** Laiendamine**: Uusi tõlkeeesmärke saab lisada ühe liidese rakendamisega.

## Teenuse lahtipakkimine

### BackendTranslationService (orchestrator)

** Kohustused**:
- Torustiku olelusringi haldamine (käivitus, valmimine, veakäsitlus)
- Semaforipõhine konkurentsikontroll (vältib kattuvaid jooksusid)
- Serveri valideerimine (latentsus, keele kättesaadavus, konfiguratsioon)
- Delegeerimine alltalitustele

**Does NOT contain**:
- Tõlkeloogika
- Konkreetsete vormingute fail I/O
- Tagakatse loogika

### Riikide Tõlketeenistus

** Kohustused**:
- Loe kataloogist
- Riikide nimed sünkroniseeritakse vaikimisi lokaadi sõnaraamatusse
- Puuduvate riikide nimede tõlkimine sihtkeele järgi
- Salvesta iga sihtsõnaraamat kohe pärast tõlkimist

** Võtmekäitumine**:
- Kui vaikimisi keel on inglise keel, salvestatakse riiginimed nagu-is
- Kui vaikimisi keel on muu: inglisekeelsed nimed tõlgitakse vaikimisi keelde esimesena
- Iga keelt töödeldakse iseseisvalt oma retry loopiga

### LokaliseerimineTranslationService

** Kohustused**:
- Lisatud/eemaldatud klahvide tuvastamine aktiivse vaikesõnaraamatu võrdlemisel eelmise pildiga
- Lisatud võtmete tõlkimine igasse sihtkeelde
- Eemalda kustutatud võtmed igast sihtkeelest
- Pildi salvestamine järgmiseks võrdlemiseks

** Võtmekäitumine**:
- Käsitsi tõlked on alati prioriteetsed (mitte kunagi üle kirjutatud)
- Lisatud võtmed tõlgitakse ja salvestatakse kohe keele kaupa
- Eemaldatud võtmed kustutatakse keele kaupa kohe
- Snapshot salvestatakse alles pärast kõigi keelte edukat lõpetamist

### Dokumentide tõlkimise teenus

** Kohustused**:
- Jalgsi seadistatud Markdowni juured rekursiivselt
- Tuvastage muudetud lähtefailid SHA-256 räsi abil
- Plokipõhine translatsiooniolek
- Tõlgimine ploki kaupa plokipõhise proovimisega
- Märgistuse struktuuri kinnitamine pärast tõlkimist
- Salvesta iga sihtkeele fail iseseisvalt

** Võtmekäitumine**:
- Plokitaseme detailsus: pealkirjad, lõigud, loendi elemendid tõlgitakse eraldi
- Metaandmete rajad, mille plokid õnnestusid / ebaõnnestusid keele kaupa
- Ebaõnnestunud plokke otsitakse järgmisel käivitamisel ilma edukaid plokke uuesti tõlkimata
- Struktuuri valideerimine tagab rubriikide arvu, loendite, koodiplokkide jne sobivuse allikas

## Tagasiproovimise strateegia

Süsteem rakendab korduskatseid kolmel tasandil:

### Tase 1 – HTTP (LibreTranslateService)

- Kuni 5 katset eksponentsiaalse tagasilöögiga (1s, 2s, 3s, 4s, 5s)
- Käsitleb võrgu aegumisi, 5xx vigu ja ajutisi tõrkeid
- Sisseehitatud HTTP kliendi seadistusse

### Tase 2 – etapp (TranslationRetryService)

- Kuni 3 katset 30-sekundilise viivitusega
- Taaskäivitab kogu tõlkepäringu pärast HTTP taseme korduste ammendumist
- Sellel tasemel rakendatakse kohahoidja maskeerimist ja taastamist

### Tase 3 – plokk (DocumentsTranslationService)

- Ebaõnnestunud üksikud märgistusplokid on märgitud metaandmetesse
- Uuendatakse automaatselt järgmisel torujuhtmel
- Edukaid blokke ei tõlgita kunagi uuesti

## Andmevoog

### JSON sõnastiku tõlge

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

### Märgistuse tõlge

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

### Riigi nime tõlge

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

## Riigi püsivus

### Pilte

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Eesmärk**: Lubab astmelist sünkroonimist, jälgides seda, mis oli eelmises jooksus

### Räsifailid

- **Markdown**: lähtefaili kõrval
- **Fallback**: kui esmane asukoht on kirjutuskaitstud
- **Eesmärk ** Tuvastab allika muudatused, et vältida tarbetut taastõlkimist

### Tõlke metaandmed

- **Markdown**:
- **Sisu**:
  - Allika sisu räsi
- Keeleline bloki staatus (tõemärkide seeria)
- Viimase uuendamise ajatempel
- **Eesmärk**: Lubab ainult ebaõnnestunud plokkide osalist taastõlkimist

### Kohapealne hoidla

- ** Fail**:
- **Sisu**: kohatäitja nime- väärtuspaaride võtmete sõnastik
- **Eesmärk **: pakub määratud kohahoidjate vaikeväärtusi kogu rakenduses

## SignaaliR aruandlus

### Kirjastuse abstraktsioon

lahutab tõlketeenused SignalRi spetsiifikast:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Järjekorragarantiid

- Ühes reas olevad sõnumid on monotoonselt järjestatud
- Järjekorranumbrid on kordumatud jooksu kohta
- Kliendid saavad tuvastada lünki või ümber korraldada

### Hubi kaardistamine

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Pikenduspunktid

### Uue tõlkeeesmärgi lisamine

1. Uue liidese loomine
2. Liidese rakendamine domeenipõhise loogikaga
3. Registreerimine DI konteineris
4. Süstida konstruktorisse
5. Kõne pärast olemasolevaid etappe

### Kohandatud kordusproovide poliitika

Eemalda konstruktori parameetrid:

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

### Kohatäitja kohandatud käitlemine

Kohatäitja süntaksi või salvestamise muutmine:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Seadistamine

### appsettings.json

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

### Runtime häälestamine

Seadistamine
|---------|---------|--------|
80
10
3
30

## Katsetamisstrateegia

### Ühikukatsed

Iga alamteenus on iseseisvalt testitav:

- Mock edu/ebaõnne simuleerimiseks
- Mood aruandluse kontrollimiseks
- Ajutise kataloogi kasutamine failis I/ O
- Salvestamiskäitumise kontrollimine keele kaupa

### Integratsioonikatsed

- Täispikk torujuhe reaalse (kohaliku) LibreTranslate protsessiga
- Kontrollige, kas SignalR-sõnumid edastatakse ühendatud klientidele
- Samaaegse katse ärahoidmine (semafor)
- Märgistuse struktuuri kinnitamine pärast tõlkimist

### Lõpust lõpuni katsed

- Tõlke käivitamine API või planeerija kaudu
- Kontrolli, et kõik sihtkeele failid on loodud/uuendatud
- Kontrolli metaandmete faile korrektse ploki olekuga
- Kinnitage, et kohahoidjad säilitatakse kogu tõlkes

## Tulemuslikkuse kaalutlused

- ** Mälu**: keelepõhine salvestamine takistab kõigi sõnaraamatute mällu jätmist
- **Disk I/O**: metaandmete failid lisavad väikseid üldkulusid, kuid võimaldavad lisatööd
- ** Võrgustik**: Järjestikune töötlemine summutamisega hoiab ära ülekaaluka LibreTranslate'i
- **CPU**: SHA-256 räsimine ja regexi valideerimine on tõlke latentsuse suhtes kiired
- **SignalR**: Kerged sõnumid, tüüpilised teated ei vaja kasuliku koormuse tihendamist

## Migratsioon monoliitsest disainist

Originaal sisaldas kogu loogikat ühes klassis. Migratsioonitee:

1. Väljavõte riigi loogikast
2. Eemalda JSON loogika →
3. Markdowni loogika ekstrakt →
4. Väljavõte SignalR kirjastamine →
5. Proovimise loogika ekstrakt →
6. Lihtsustada orkestraatorit ainult delegatsioonile

Kõik olemasolevad liidesed () jäävad muutumatuks. Torujuhtme tarbijad ei näe murrangulisi muutusi.
