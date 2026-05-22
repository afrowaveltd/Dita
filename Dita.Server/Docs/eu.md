# Itzulpen automatikoko zerbitzuan egindako aldaketen laburpena

## Ikuspegi orokorra

Dokumentu honek Dita itzulpen automatikoko zerbitzuan egindako aldaketa guztiak laburbiltzen ditu, arkitektura birfabrikatzea, ezaugarri berriak, obserbagarritasunaren hobekuntzak eta lokalizazioaren hobekuntzak barne.

## Arkitekturaren aldaketak

### Atzeratutako zerbitzua

Monolitoa lau zerbitzu espezializatutan banatu da, orkestratzaile arin batek koordinatua:

- **BackendTranslationService** - Pipeline orkestratzailea (zerbitzariaren balidazioa, agertokiaren delegazioa, erroreen kudeaketa)
- **TranslationService** — Herrialde-izenen sinkronizazioa (ingelesa → helburu-hizkuntza)
- **LocalizationTranslationService** — JSON hiztegiaren sinkronizazioa (gakoak gehitu/kendu)
- **DokumentuakTranslationService** - Markdown dokumentazio itzulpena blokeen mailako jarraipenarekin
- **SignalRPublisher** - Denbora errealeko aurrerapena seinalearen bidez
- **TranslationRetryService** - Eszena-mailako saiakera leku-markaren kontserbazioarekin

### Onurak

- ** Kezkak bereiztea**: Zerbitzu bakoitzak itzulpen-domeinu bakarra kudeatzen du
- **Maintainability**: Klase txikiak errazago ulertzen eta probatzen dira
- ** Hedapena**: Itzulpen-helburu berriak gehi daitezke interfazearen inplementazioaren bidez
- **Fidagarritasuna**: zerbitzu independenteek hutsegiteen isolamendu hobea eskaintzen dute

## Eginbide berriak

### zuzeneko itzulpenaren monitorea

**Helbidea**:

Administratzaile-orri berri bat, denbora errealeko ikusgaitasuna eskaintzen duena itzulpen-kanalizazioan:

- Gertatzen diren seinale-gertaera guztiak bistaratzen ditu
- Kolorez kodetutako mezu motak (urdina=hasiera, berdea=osoa, gorria=errorea)
- Konexio-egoeraren bandera automatikoki berriro konektatzeko
- Mezu-kontagailua eta JSON-era esportatzea

### Izendatutako lekuak

Lokalizazio-sistemak orain leku-marka izendatuak onartzen ditu hizkuntza ezberdinetan gramatika hobetzeko:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Ezaugarriak:
- Leku-markaren balioak exekuzio-denboran edo gordeta
- Maskara automatikoa/atzerapena itzulpenean ustelkeria saihesteko
- Atzera egitea bateragarria da leku-marka posizionalekin

### Itzulpen inkrementala

Markdown fitxategiak goitik behera itzulita daude:

- **Hizkuntza-aurrezpena**: Helburuko hizkuntza bakoitza berehala gordetzen da itzulpenaren ondoren, memoriaren presioa murriztuz
- **Block-level tracking**: pistak itzultzeko egoera blokeko
- **Hautatutako saiakera**: Huts egin duten blokeak bakarrik itzultzen dira hurrengo lasterketan
- **Metadataren iraupena**: Itzulpen-egoerak bizirik irauten du aplikazioa berrabiarazten

### Erretorearen logika hobetua

Hiru erresilientzia maila:

1. **HTTP retry** (LibreTranslateService): 5 saiakera atzeraldi esponentzialarekin (1s-5s)
2. **Stage retry** (TranslationRetryService): 3 saiakera gehiago 30eko atzerapenarekin
3. **Block retry** (DokumentuakTranslationService): Huts egin du Markdowneko blokeek hurrengo exekuzioan

### Seinale-informazioa

Denbora errealeko progresioa hodi-eragiketa guztietarako:

- Etapa bakoitzak gertaerak argitaratzen ditu
- Hizkuntzaren progresioa gertaera gisa argitaratuta
- Errorearen gertaerek testuinguru zehatza dute (iturburua, errore-kodea, mezua)
- Sekuentzia-zenbakiek ordena bermatzen dute lasterketa bakoitzean

## Konfigurazio-aldaketak

### appsettings.json

Aldaketarik ez. Existitzen den konfigurazioak funtzionatzen jarraitzen du:

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

### Zerbitzu berriak

Hemen erregistratua:

- /
- `TranslationRetryService`
- /
- /
- /
- /

SignalR zentroa bezeroen konexioetarako mapatuta dago.

## Proba

### Probako egoera

- **243/244 probak pasatzen** (1 saltatuta probako inguruneko fitxategi-sarbidea dela eta)
- Probaren estaldura berria gehitu zaio:
  - PlaceholderService funtzionalitatea
  - Orkesta zerbitzua
  - JsonStringLocalizer leku-marka indexatzaileak

### Muga ezagunak

- proba gainditu egiten da paraleloan exekutatzen denean, hainbat instantziak fitxategi bera partekatzen dutelako. Isolamenduan igarotzen da.

## Fitxategi-egitura berria

### Zerbitzuak

- - Pipeline orkestratzailea
- - Herrialde-izenen itzulpena
- - JSON hiztegiaren sinkronizazioa
- - Markdown itzulpena
- - SignalR mezua argitaratuta
- - Probatu logika leku-markaren maskararekin
- - Argitaratzailearen interfazea
- — Herrialdeko zerbitzu interfazea
- Lokalizazio-zerbitzuaren interfazea
- - Dokumentu-zerbitzuaren interfazea
- - Orkestra-interfazea (eguneratua)
- - Fitxategi bakoitzeko itzulpen metadatuak

### Zerbitzu eguneratuak

- - Leku-markaren euskarria gehituta
- - Parametro berrietarako eguneratua
- - Izendatutako leku-markaren kudeaketa
- - Placeholder interfazea

### Admin-orri berria

- - Denbora errealeko monitorizazio-orria
- - Orri-eredua

### Dokumentazio berria

- - kanalizazioaren dokumentazio eguneratua
- -Jarduera-sistemaren gida
- - Arbel-erabileraren gida
- - Arkitektura teknikoaren ikuspegi orokorra

## Atzerako bateragarritasuna

Aldaketa guztiak gehigarriak dira:

- Lokalizazio-kodea () aldatu gabe funtzionatzen du
- Posizioaren formatua () aldatu gabe funtzionatzen du
- JSON hiztegi-formatua ez da aldatu
- Dagoen Markdown egitura aldatu gabe dago
- Seinale-mezuek formatu bera erabiltzen dute

## Migrazio-bidea

Ez da migraziorik behar. Berreraikitzea barnekoa da:

1. Antzinakoa erreferentzia gisa gorde zen eta gero ordeztu egin zen
2. DI erregistroak eguneratu dira interfaze berriak erabiltzeko
3. Existitzen diren kontsumitzaile guztiek ez dute aldaketarik ikusten

## Errendimenduaren hobekuntzak

- **Memoriaren erabilera murriztua**: Fitxategiak berehala gordetzen dira memorian gorde ordez
- **Faster igoera-eskerrak**: Markdowneko bloke aldatuak edo hondatuak bakarrik itzultzen dira
- **Ikuspen hobea**: Denbora errealeko aurrerapenak fase motelak diagnostikatzen laguntzen du

## Etorkizuneko hobekuntzak

Hobekuntza planifikatuak:

1. **AI fine-tuning** — Post-machine itzulpenaren berrikuspena esaldientzat > 5 hitz
2. **Admin autentifikazioa** - Mugatu admin orriak baimendutako erabiltzaileentzat
3. **Egunkaria** — Web UI lokalizazio-gakoak kudeatzeko
4. **Itzulpen-estatistikak** - Itzulpen-kopuruak eta errore-tasak erakusten dituzten diagramak denboran zehar
5. **Leku-marka pertsonalizatua** - Leku-markaren formatu alternatiboen euskarria

## Kontaktua

Itzulpen-zerbitzuaren galdera edo gaietarako, irakurri modulu bakoitzaren direktorioko dokumentazio zehatza edo jarri harremanetan garapen-taldearekin.
