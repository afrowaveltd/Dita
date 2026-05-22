# Automaatse tõlke teenuse muudatuste kokkuvõte

## Ülevaade

See dokument võtab kokku kõik Dita automaattõlketeenuses tehtud muudatused, sealhulgas arhitektuuri refaktoreerimine, uued funktsioonid, jälgimisparandused ja lokaliseerimistäiustused.

## Arhitektuuri muudatused

### refactored backendtranslation service

Monoliit on lagunenud neljaks eriteenistuseks, mida koordineerib kergekaaluline orkestraator:

- **BackendTranslationService** — torujuhtme orkestraator (serveri valideerimine, lavadelegatsioon, veakäsitlus)
- **RiigidTranslationService** – Riiginimede sünkroniseerimine (inglise → sihtkeel)
- **LocalizationTranslationService** — JSON sõnastiku sünkroniseerimine (lisatud/eemaldatud klahvid)
- **DocumentsTranslationService** – Markdown dokumentatsiooni tõlge koos plokitasandi jälgimisega
- **SignalRPublisher** – Reaalajas edenemise aruandlus SignalR-i kaudu
- **TranslationRetryService** – etapitaseme proovimine kohahoidja säilitamisega

### Hüved

- ** Murede lahusus**: Iga teenus tegeleb ühe tõlkedomeeniga
- **Hooldusvõime**: Väiksemaid klasse on lihtsam mõista ja testida
- ** Laiendamine**: Uusi tõlkeeesmärke saab lisada liidese rakendamise kaudu
- **Reliability**: Independent services provide better fault isolation

## Uued omadused

### Otsetõlke monitor

** Asukoht**:

Uus administraatori lehekülg, mis tagab reaalajas nähtavuse tõlketorustikus:

- Näitab kõiki SignalR sündmusi nende toimumise ajal
- Värvikoodiga sõnumitüübid (blue=käivitatud, green=lõpetatud, red=viga)
- Ühenduse oleku bänner automaatse taasühendamisega
- Sõnumiloendur ja eksport JSON- i

### Nimelised kohatäitjad

Lokaliseerimise süsteem toetab nüüd nimelisi kohahoidjaid () grammatilisuse parandamiseks erinevates keeltes:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Omadused:
- Kohatäitja väärtused, mis on esitatud käitamise ajal või salvestatud
- Automaatne maskeerimine/taastamine tõlkimise ajal korruptsiooni vältimiseks
- Tagasiühilduv olemasolevate kohatäituritega

### Järkjärguline tõlge

Märkimisfailid tõlgitakse järk-järgult:

- **Keelesäästmine**: Iga sihtkeel salvestatakse kohe pärast tõlkimist, vähendades mälurõhku
- **Plokitaseme jälgimine**: radade tõlkimise olek ploki kohta
- **Valikproovimine**: Ainult ebaõnnestunud plokid tõlkitakse uuesti järgmisel käivitamisel
- **Metaandmete püsivus**: Tõlkeolek säilib rakenduse taaskäivitamisel

### Täiustatud katsetamisloogika

Kolm vastupidavuse taset:

1. **HTTP retry** (LibreTranslateService): 5 katset eksponentsiaalse tagasilöögiga (1s–5s)
2. ** Lava proovimine ** (TranslationRetryService): 3 täiendavat katset 30-ndate viivitustega
3. **Ploki uuesti proovimine ** (DocumentsTranslationService): Failed Markdown plokid uuesti proovitud järgmisel käivitamisel

### SignaaliR aruandlus

Reaalajas esitatav eduaruanne kõigi torujuhtmete käitamise kohta:

- Iga lava avaldab sündmusi
- Sündmustena avaldatud keeleline edu
- Veasündmused hõlmavad üksikasjalikku konteksti (allikas, veakood, sõnum)
- Järjekorranumbrid garanteerivad tellimuse iga jooksu jooksul

## Seadistuste muudatused

### appsettings.json

Ei mingeid muutusi. Olemasolev konfiguratsioon töötab edasi:

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

### Uued teenused

Registreeritud :

- /
- `TranslationRetryService`
- /
- /
- /
- /

SignalR jaotur on kaardistatud kliendiühenduste jaoks.

## Testimine

### Katseseisund

- **243/244 testide läbimine ** (1 vahele jäetud samaaegse failide juurdepääsu tõttu katsekeskkonnas)
- Lisatud on uus katseala:
  - Kohapealse teenuse funktsioonid
  - BackendTranslationService orkestreerimine
  - JsonStringLocalizeri kohaomanike indekseerijad

### Tuntud piirangud

- katse jäetakse paralleelselt töötamisel vahele, sest mitu katseeksemplari jagavad sama faili. See möödub, kui jookseb isolatsioonis.

## Uus failistruktuur

### Teenused

- — Torujuhtmete orkestraator
- — riigi nime tõlge
- — JSON sõnastiku sünkroniseerimine
- — Turuväärtuse tõlge
- — SignalR-sõnumite avaldamine
- – Tagurpidine loogika kohahoidja maskiga
- — kirjastaja liides
- — Riigiteenuste liides
- — Lokaliseerimise teenuse liides
- — Dokumenditeenuse liides
- — Orkestri liides (ajakohastatud)
- – failide kaupa tõlke metaandmed

### Ajakohastatud teenused

- — Lisatud nimega kohahoidja toetus
- — Uuendatud uue parameetri jaoks
- — Nimeline kohatäitja juhtkond
- — Kohatäitja liides

### Uus administraatori lehekülg

- — Reaalajalise seire leht
- — Lehekülje mudel

### Uus dokumentatsioon

- — Torujuhtme ajakohastatud dokumentatsioon
- — Kohapealse süsteemi juhend
- — Armatuurlaua kasutusjuhend
- — Tehnilise arhitektuuri ülevaade

## Tagasiühilduvus

Kõik muudatused on täiendavad:

- Olemasolev lokaliseerimiskood () töötab muutmata kujul
- Positsiooni vormindamine () toimib muutmata kujul
- Olemasolev JSON sõnaraamatu vorming ei muutu
- Olemasolev allahindluse struktuur ei muutu
- SignalR-sõnumid kasutavad sama vormingut

## Migratsioonirada

Migratsioon ei ole vajalik. Refaktoreerimine on sisemine:

1. Vana säilitati viitena ja seejärel asendati
2. DI registreerimist uuendati uute liideste kasutamiseks
3. Kõik olemasolevad tarbijad ei näe muutusi

## Tulemuslikkuse parandamine

- ** Vähendatud mälukasutus**: Failid salvestatakse keele kohta kohe, selle asemel, et hoida kõik mälus
- **Kiired juurdekasvud**: Ainult muudetud/ebaõnnestunud Markdowni plokid tõlgitakse uuesti
- **Parem nähtavus**: Reaalajas progress aitab diagnoosida aeglaseid etappe

## Tulevased täiustused

Kavandatud parandused:

1. **AI peenhäälestus** – masinajärgne tõlkeülevaade fraasidele > 5 sõna
2. **Admin autentimine ** – Admin-lehtede piiramine volitatud kasutajatele
3. ** Sõnastikuredaktor** – veebi kasutajaliides lokaliseerimise võtmete haldamiseks
4. ** Tõlkestatistika** – diagrammid, mis näitavad tõlkimiste arvu ja veamäära aja jooksul
5. ** Kohatäitja süntaks ** – toetus alternatiivsete kohatäitja vormingutele

## Kontakt

Tõlketeenusega seotud küsimuste või probleemide korral vaadake iga mooduli kataloogi üksikasjalikku dokumentatsiooni või võtke ühendust arendusmeeskonnaga.
