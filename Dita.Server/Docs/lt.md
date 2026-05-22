# Automatinio vertimo paslaugos pakeitimų santrauka

## Comment

Šiame dokumente apibendrinami visi pakeitimai, padaryti "Dita" automatinio vertimo paslaugos, įskaitant architektūros reaktoriaus, naujų funkcijų, stebimumo gerinimo, ir lokalizacijos patobulinimai.

## Architektūros pokyčiai

### Name

Monolitinis buvo suskaidytas į keturias specializuotas paslaugas, koordinuojamas lengvo orkestro:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Išmokos

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## NAME OF TRANSLATORS

### Live Vertimo monitorius

**Location**: `/Admin/LiveTranslation`

Naujas admin puslapis, kuris suteikia realaus laiko matomumą į vertimo vamzdyną:

- Rodo visus SignalR įvykius, kai jie įvyksta
- Spalvoto kodo pranešimų tipai (mėlyna = pradėta, žalia = užbaigta, raudona = klaida)
- Jungiamosios būklės baneris su prijungimu automatiškai
- Laiško skaitiklis ir eksportas į JSON

### Name

Lokalizavimo sistema dabar palaiko pavadintą placebą () už patobulintą gramatiškumą įvairiomis kalbomis:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Savybės:
- Kelio laikiklio reikšmės, pateiktos kilimo ir tūpimo metu arba saugomos
- Automatinis maskavimas / atkūrimas vertimo metu, siekiant išvengti korupcijos
- Grįžtamasis suderinamumas su esamomis padėties nustatymo sistemomis

### Papildomas vertimas

Žymėjimo failai yra verčiami palaipsniui:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Patobulintas atnaujinimo žurnalas

Tris atsparumo lygius:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### SignalR pranešimas

Visų vamzdynų operacijų pažangos ataskaitoms:

- Kiekvienas etapas skelbia renginius
- Kalbų pažanga skelbiama kaip renginiai
- Klaidų įvykiai apima išsamų kontekstą (šaltinis, klaidos kodas, pranešimas)
- Sequence numeriai garantija užsakymas per kiekvieną paleisti

## Konfigūracijos pakeitimai

### appsettings.json

Nekeisti. Egzistuojanti konfigūracija toliau veikia:

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

### Naujosios paslaugos

Registruota:

- /
- `TranslationRetryService`
- /
- /
- /
- /

SignalR centras sudarytas klientų ryšiams.

## Testavimas

### Bandymo būsena

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Pridedama nauja bandymų aprėptis:
  - PlaceholderService funkcijos
  - BackendTranslationService orchestration
  - JsonStringLocalizer klaviatūros indeksai

### Žinomi apribojimai

- bandymas praleidžiamas, kai veikia lygiagrečiai, nes kelių bandymų atvejų tą patį failą. Ugnis pravažiuoja, kai važiuoja atskirai.

## Nauja failų struktūra

### Paslaugos

- - Vamzdynų orkestratorius
- - Šalies pavadinimo vertimas
- - JSON žodyno sinchronizavimas
- - Markdown vertimas
- - SignalR pranešimų leidyba
- - Kartojama logika su placebu
- - Leidėjo sąsaja
- Šalies paslaugų sąsaja
- Lokalizacijos paslaugos sąsaja
- - Dokumentų aptarnavimo sąsaja
- - Orkestro sąsaja (atnaujinta)
- - Pe- file vertimo metaduomenys

### Atnaujinta Paslaugos

- - Pridėta pavadintą placeholder parama
- - Atnaujinta dėl naujo parametro
- - Name
- Kameros laikiklio sąsaja

### Naujas admin puslapis

- - Laiko stebėjimo puslapis
- - Puslapio modelis

### NAME OF TRANSLATORS

- - Atnaujinta vamzdynų dokumentacija
- - Vietų laikiklių sistemos vadovas
- Dashboard naudojimo vadovas
- - Techninės architektūros apžvalga

## Grįžtamasis suderinamumas

III PRIEDAS

- NAME OF TRANSLATORS
- Padėties formatavimas () darbai nepasikeitė
- NAME OF TRANSLATORS
- @ info: tooltip
- SignalR pranešimai naudoja tą patį formatą

## Migracijos kelias

Migracijos nereikia. Rikimas yra vidinis:

1. Senasis buvo išsaugotas kaip nuoroda ir tada pakeistas
2. DI registracijos buvo atnaujintos, siekiant naudoti naujas sąsajas
3. Visi esami vartotojai nemato jokių pokyčių

## Našumas

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Tolesni veiksmai

Planuojami patobulinimai:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Kontaktai

Klausimais ar klausimais su vertimo tarnyba, prašome kreiptis į išsamią dokumentaciją kiekvieno modulio kataloge arba susisiekti su plėtros komanda.
