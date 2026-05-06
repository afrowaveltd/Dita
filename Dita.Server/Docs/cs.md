# Shrnutí změn Automatické překladatelské služby

## Přehled

Tento dokument shrnuje všechny změny provedené na Dita automatické překladatelské služby, včetně architektura refaktoring, nové funkce, zlepšení pozorovatelnosti a lokalizace vylepšení.

## Změny architektury

### Refaktorovaný BackendTranslationService

Monolitický se rozkládá na čtyři specializované služby koordinované lehkým orchestrátorem:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Dávky

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Nové funkce

### Name

**Location**: `/Admin/LiveTranslation`

Nová admin stránka, která poskytuje skutečnou viditelnost do překladatelského potrubí:

- Zobrazí veškerý signál R nežádoucí účinky, které se vyskytly
- Typ barevně kódované zprávy (modrá = spuštěna, zelená = dokončena, červená = chyba)
- Spojení status banner s auto- reconnect
- Počitadlo zpráv a export do JSON

### Pojmenované paměťové nosiče

Systém lokalizace nyní podporuje pojmenované nosiče () pro lepší gramatiku v různých jazycích:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Vlastnosti:
- Hodnoty zásobníku poskytované v runtime nebo uložené v
- Automatické maskování / restaurování během překladu, aby se zabránilo korupci
- Backward kompatibilní se stávajícími pozičními stojany

### Doplňkový překlad

Soubory Markdownu se překládají postupně:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Vylepšená logika retry

Tři úrovně odolnosti:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### SignalR hlášení

Pokrok v reálném čase pro všechny operace potrubí:

- Každá etapa publikuje události
- Per- jazyk pokrok zveřejněn jako události
- Chybové události zahrnují podrobný kontext (zdroj, chybový kód, zpráva)
- Pořadové číslo záruky objednávky v rámci každého běhu

## Změny konfigurace

### appsettings.json

Žádné změny. Stávající konfigurace stále funguje:

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

### Nové služby

Zaevidováno v:

- /
- `TranslationRetryService`
- /
- /
- /
- /

Signál Rhub je zmapován pro připojení klientů.

## Zkouška

### Stav zkoušky

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Nové zkušební pokrytí přidáno pro:
  - Placeholder Funkce služby
  - BackendTranslation Organizace služeb
  - Nosiče JsonStringLocalizer

### Známá omezení

- test se přeskočí, když běží paralelně, protože více zkušebních případů sdílí stejný soubor. Projde, když běží v izolaci.

## Nová struktura souborů

### Služby v

- - Pipeline orchestrátor
- - Překlady názvu země
- - Synchronizace slovníku JSON
- - Markdown překlad
- - Signál R publikování zpráv
- - Zopakujte logiku pomocí maskáče
- - Publisher interface
- - Rozhraní služeb země
- - Lokalizační servisní rozhraní
- - Rozhraní služby dokumentů
- - Orchestrační rozhraní (aktualizováno)
- - Per- file translation metadata

### Aktualizované služby v

- - Přidána jmenovaná podpora na místo
- - Aktualizováno pro nový parametr
- - Pojmenovaná správa místa
- - Rozhraní Placeholder

### Nová admin stránka in

- - Stránka pro sledování reálného času
- - Model stránky

### Nová dokumentace v

- - Aktualizovaná dokumentace potrubí
- - Průvodce systémem Placeholder
- - Přístrojová příručka
- - Přehled technické architektury

## Zpětná kompatibilita

Všechny změny jsou aditivní:

- Stávající lokalizační kód () funguje beze změny
- Poziční formátování () funguje beze změny
- Stávající formát slovníku JSON je nezměněn
- Stávající struktura Markdown se nezměnila
- Signál R zprávy používají stejný formát

## Migrační cesta

Žádná migrace není nutná. Refaktoring je vnitřní:

1. Starý byl zachován jako odkaz a pak nahrazen
2. Registrace DI byly aktualizovány, aby využívaly nová rozhraní
3. Všichni stávající spotřebitelé nevidí žádné změny

## Zlepšení výkonnosti

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Budoucí zlepšení

Plánované zlepšení:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Kontakt

Pro dotazy nebo otázky s překladatelskou službou se prosím podívejte do podrobné dokumentace v adresáři každého modulu nebo kontaktujte vývojový tým.
