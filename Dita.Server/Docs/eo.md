# Resumo de Ŝanĝoj al la Aŭtomata Translation Servo

## Superrigardo

Tiu dokumento resumas ĉiujn ŝanĝojn faritajn al la Dita aŭtomata traduko servo, inkluzive de arkitekturreaktoro, novaj ecoj, observatorioplibonigoj, kaj lokalizo plifortigas.

## Arkitekturo ŝanĝiĝas

### Reaktoro Backend TranslationService

La monolita estis malkonstruita en kvar specialigitajn servojn kunordigitajn fare de malpeza orkestro:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Profitoj

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Novaj karakterizaĵoj

### Viva traduko ekrano

**Location**: `/Admin/LiveTranslation`

Nova admin paĝo kiu disponigas realtempan videblecon en la tradukon dukto:

- Apartigas ĉiujn SignalR-okazaĵojn kiam ili okazas
- Koloro-koditaj mesaĝspecoj (bluaj ekkomencitaj, verdaj kompletigitaj, ruĝa tero)
- Ligo statusstandardo kun aŭto-religo
- Mesaĝo kontraŭ kaj eksportado al JSON

### Nomita lokposedantoj

La lokalizo sistemo nun apogas nomitajn lokposedantojn () por plibonigita gramatikeco en malsamaj lingvoj:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Trajtoj:
- Lokulaj valoroj provizitaj je rultempo aŭ stokita en
- Aŭtomata maskado/restorado dum traduko por malhelpi korupton
- Malantaŭe kongrua kun ekzistantaj poziciigaj lokposedantoj

### Inklina traduko

Markdown dosieroj estas tradukitaj pliige:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Plifortigita Retry Logiko

Tri niveloj de respekto:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### signalo raportanta

Realtempa progreso raportanta por ĉiuj duktoperacioj:

- Ĉiu stadio publikigas la okazaĵojn
- Per-lingva progreso publikigita kiel la okazaĵoj
- Erarokazaĵoj inkludas detalan kuntekston (fonto, erarkodo, mesaĝo)
- Sequence nombroj garantias ordonadon ene de ĉiu kuro

## Konfiguracio ŝanĝiĝas

### apps.json

Ne rompi ŝanĝojn. Ekzistanta konfiguracio daŭre funkcias:

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

### Novaj servoj

Registrita en:

- /
- `TranslationRetryService`
- /
- /
- /
- /

La SignalR-nabo estas mapita ĉe por klientligoj.

## Testado

### Testa statuso

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Nova testpriraportado aldonis por:
  - Situa funkcieco
  - BackendTranslationService instrumentado
  - JsonStringLocalizer lokulo indeksuloj

### Konataj Limigoj

- testo estas transsaltita kiam kurante enen paralela ĉar multoblaj testkazoj dividas la saman dosieron. Ĝi pasas kiam ĝi kuras en izoliteco.

## Nova dosierstrukturo

### Servoj en servoj

- Pipeline orkestrotor
- Landa nomo traduko
- JSON-vortaro sinkronigado
- Markdown traduko
- SignalR-mesaĝo
- Retry logiko kun lokulo maskanta
- Publisher interfaco
- Landa servinterfaco
- Lokalizo servo interfaco
- Dokumenta servinterfaco
- Orchestrator interfaco (ĝisdatigita)
- Per-dosiero traduko metadatenoj

### Ĝisdatigitaj servoj en

- Aldonita nomita lokula subteno
- Ĝisdatigita por nova parametro
- Nomita lokula administrado
- Situa interfaco

### Nova Admin-paĝo en

- Realtempa monitora paĝo
- Paĝo modelo

### Nova dokumentado en

- Ĝisdatigita dukto dokumentaro
- Situa sistemo
- Dashboard-uzokutimo
- Teknika arkitektursuperrigardo

## Malantaŭa Compatibility

Ĉiuj ŝanĝoj estas aldonaj:

- Ekzistanta lokalizokodo () funkcias senŝanĝa
- Pozicio formatado () funkcias senŝanĝa
- Eksistanta JSON-vorta formato estas senŝanĝa
- Existing Markdown strukturo estas senŝanĝa
- Signalaj mesaĝoj uzas la saman formaton

## Migradoj

Neniu migrado postulis. La rektoro estas interna:

1. Malnova estis konservita kiel referenco kaj tiam anstataŭigita
2. DI-registradoj estis ĝisdatigitaj por uzi novajn interfacojn
3. Ĉiuj ekzistantaj konsumantoj ne vidas ŝanĝojn

## Plibonigoj

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Estontaj pliboniĝoj

Planitaj plibonigoj:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Kontaktu kontakton

Por demandoj aŭ temoj kun la tradukservo, bonvole rilatas al la detala dokumentaro en ĉiu modulo adresaro aŭ kontakto la evoluteamo.
