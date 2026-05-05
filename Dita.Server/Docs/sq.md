# Përmbledhje e ndryshimeve në shërbimin e përkthimit automatik

## Pamja e parë

Ky dokument përmbledh të gjitha ndryshimet që i janë bërë shërbimit automatik të përkthimit të Ditës, duke përfshirë riprodhimin e arkitekturës, veçoritë e reja, përmirësimet e observabilitetit dhe përmirësimet e lokalizimit.

## Ndryshime arkitekture

### Transferim i dobishëm

Monolitike është dekompozuar në katër shërbime të specializuara të koordinuara nga një orkestrator i lehtë:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Dobitë

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Veçori të reja

### Vëzhguesi i Përkthimit Live

**Location**: `/Admin/LiveTranslation`

Një faqe e re admin që ofron dukshmëri në kohë reale në tubacionin e përkthimit:

- Shfaq të gjithë ngjarjet e Sinjalit kur ndodhin
- Llojet e mesazheve të koduara me ngjyrë (foot=filluar, e gjelbër=e plotë, e kuqe=error)
- Lidhja me file auto- lidhur
- Mesazhi

### Mikpritës të emëruar

Sistemi i lokalizimit tani mbështet vendshënuesit () për përmirësimin e gramatikës në gjuhë të ndryshme:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Veçoritë:
- File në
- Mbulim automatik i maskimit/retoracionit gjatë përkthimit për të parandaluar korrupsionin
- Në përputhje me vendshënuesit ekzistues

### Përkthimi i brendshëm

Dosjet e shënuara janë përkthyer në rritje:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Logjika e vazhdueshme e përpjekjeve

Tre nivele elasticiteti:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### Reporting

Përparimi në kohë reale për të gjitha operacionet e tubacionit:

- Çdo fazë publikon ngjarjet
- Për-gjuhë progresi botuar si ngjarje
- Gabim mesazh
- Numrat sekuenca garantojnë urdhërimin brenda secilit prej tyre

## Ndryshimet e konfigurimit

### programe.json

Nuk ka ndryshime thyerje. Konfigurimi ekzistues vazhdon të funksionojë:

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

### Shërbime të reja

I regjistruar në:

- /
- `TranslationRetryService`
- /
- /
- /
- /

Qendra e Sinjalit është e pajisur për lidhjet e klientëve.

## Prova

### Gjëndja

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Për:
  - Funksioni i vendit
  - Orkestra e dytë e interfaqes
  - Treguesit vendshënues JsonString

### Kufizime të njohura

- është në. Kalon në izolim.

## Struktura e re e skedarëve

### Shërbime në

- Arkiparent i Pipepolit
- Përkthimi i emrit të vendit
- Sinkronizimi i fjalorit JSON
- Përkthimi
- Mesazhi
- ⇩ Përpiqu të provosh logjikën me maska vendshënuese
- Ndërfaqe Editor
- Ndërfaqe e shërbimit lokal
- Ndërfaqe e shërbimit lokal
- Ndërfaqe e shërbimit të dokumentit
- Ndërfaqe e Orkestrave (u ndryshuar)
- ⇩ Metadata për përkthimin Per-file

### Shërbimet e përditësuara

- ⇩ Shtoi mbështetje vendshënuese me emër
- U rifreskua për parametrin e ri
- Menaxhues vendshënues i emëruar
- Ndërfaqe

### E re Faqja

- hynë në faqen e monitorimit në kohë reale
- ⇩ Modeli i faqes

### Dokumentë i ri

- Dokumentimi i ri i tubacionit
- Udhëzues i sistemit të vendeve
- ⇩ Udhëzues përdorimi i Dashboard
- Pamje e arkitekturës teknike

## Compatibiliteti

Të gjitha ndryshimet janë shtesë:

- Kodi aktual () funksionon i pandryshuar
- Formati () i pozicionit
- Formati ekzistues i fjalorit JSON është i pandryshuar
- Struktura ekzistuese e shënimit është e pandryshuar
- Sinjale

## Shtegu i emigracionit

Nuk ka nevojë për migracionin. Riprodhimi është i brendshëm:

1. Vjetër është ruajtur si një referim dhe pastaj zëvendësuar
2. Regjistrimet DI u përditësuan për të përdorur interfaqe të reja
3. Të gjithë konsumatorët ekzistues nuk shohin ndryshime

## Përmirësime përformance

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Shtimi i ardhshëm

Përmirësimet e planifikuara:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Kontakti

Për pyetje ose për çështje me shërbimin e përkthimit, ju lutemi t'i referoheni dokumentacionit të hollësishëm të directory së çdo moduli ose të kontaktoni ekipin e zhvillimit.
