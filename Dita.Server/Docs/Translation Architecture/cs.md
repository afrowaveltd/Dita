# Architektura překladu

Tento dokument popisuje modulární architekturu automatického překladového systému Dita, který byl zaveden za účelem zlepšení proveditelnosti, ověřitelnosti a odolnosti.

## Designové cíle

Refaktoring se zabýval několika obavami s původním monolitickým designem:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Rozklad služby

### BackendTranslationService (orchestrátor)

**Responsibilities**:
- Správa životního cyklu potrubí (start, dokončení, manipulace s chybami)
- Semafore-based concurrency conconcurrency concontrol (zabraňuje překrývání řádků)
- Ověření správnosti serveru (latence, jazyková dostupnost, konfigurace)
- Delegace subútvarů

**Does NOT contain**:
- Logika překladu
- Soubor I / O pro konkrétní formáty
- Opakovat logiku

### radnice

**Responsibilities**:
- Čtěte z adresáře
- Synchronizovat názvy zemí do výchozího lokálního slovníku
- Přeložit chybějící názvy zemí podle cílového jazyka
- Uložit každý cílový slovník ihned po překladu

**Key behaviors**:
- Je-li výchozí jazyk anglický: názvy zemí uložené as- je
- Pokud je výchozí jazyk jiný: Anglická jména přeložena do výchozího jazyka první
- Každý jazyk je zpracováván nezávisle na své vlastní retry smyčce

### LokalizationTranslationService

**Responsibilities**:
- Detekovat přidané / odstraněné klávesy porovnáním aktuálního výchozího slovníku s předchozím snímkem
- Přeložit přidány klíče do každého cílového jazyka
- Odstranit smazány klíče z každého cílového jazyka
- Uložit snímek pro další srovnání

**Key behaviors**:
- Ruční překlady mají vždy přednost (nikdy přepsaný)
- Přidány klíče jsou přeloženy a uloženy per- jazyk okamžitě
- Odstraněné klíče se okamžitě vymažou z jazyka
- Snímek je uložen pouze po úspěšném dokončení všech jazyků

### dokumentstranslationservice

**Responsibilities**:
- Procházka nakonfigurována Markdown kořeny rekurzivně
- Detekovat změněné zdrojové soubory pomocí SHA-256 hashes
- Track per- block překlad stavu v
- Translate block- by- block with per- block retry
- Potvrdit Markdown strukturu po překladu
- Uložit každý soubor cílového jazyka nezávisle

**Key behaviors**:
- Granularita na blokové úrovni: položky, odstavce, položky seznamu se překládají odděleně
- Sledování metadat, které bloky uspěly / selhaly na jazyk
- Neúspěšné bloky jsou znovu vyzkoušeny na další běh bez překládání úspěšných bloků
- Ověření struktury zajišťuje počet bodů, seznamy, kódové bloky atd. shodný zdroj

## Opakovat strategii

Systém retestuje na třech úrovních:

### Úroveň 1 - HTTP (LibreTranslateService)

- Až 5 pokusů o exponenciální backoff (1s, 2s, 3s, 4s, 5s)
- Manipuluje s timeouty sítě, chybami 5xx a přechodnými poruchami
- Zabudovaný do konfigurace HTTP klienta

### Úroveň 2 - etapa (TranslationRetryService)

- Až 3 pokusy s 30sekundovým zpožděním
- Re- řídí celou žádost o překlad poté, co HTTP- úroveň opakování jsou vyčerpány
- Na této úrovni se používá maskování a obnova zásobníku

### Úroveň 3 - blok (DocumentsTranslationService)

- Jednotlivé bloky Markdownu, které selžou, jsou označeny v metadatech
- Automaticky retrailed on the next ropovod run
- Úspěšné bloky nejsou nikdy překládány

## Průtok dat

### Slovník JSON překlad

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

### Překlad Markdownu

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

### Název země překlad

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

## Vytrvalost státu

### Snímky

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Hašovací soubory

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Metadata překladu

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Zdrojový obsah hash
- Per- language block status (pole booleys)
- Datum poslední aktualizace
- **Purpose**: Enables partial re-translation of only failed blocks

### Skladování zásobníků

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## SignalR hlášení

### Vydavatel abstrakce

odděluje překladatelské služby od specifik SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Pořadové záruky

- Zprávy v rámci jednoho běhu jsou monotónně sekvenovány
- Pořadová čísla jsou unikátní per- run prostřednictvím
- Klienti mohou detekovat mezery nebo přeobjednávání

### Mapování hrdla

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Rozšíření bodů

### Přidání nového překladového cíle

1. Vytvořit nové rozhraní s
2. Provést rozhraní s logikou specifickou pro domácí prostředí
3. Zaregistrujte se v kontejneru DI
4. Injikujte do konstruktoru
5. Hovor z po existujících fázích

### Vlastní retry politika

Override konstrukční parametry:

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

### Nakládání s vlastním prostorem

Provedení změny syntaxe nebo skladování vlastníka:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Nastavení

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

### Nastavení runtime

Nastavení
|---------|---------|--------|
80
10
3
30

## Zkušební strategie

### Jednotkové zkoušky

Každá subslužba je nezávisle ověřitelná:

- Name
- Mock pro ověření hlášení
- Použít dočasné adresáře pro soubor I / O
- Ověřit chování pro ukládání jazyků

### Zkoušky integrace

- Plný plynovod se skutečnou (místní) instance LibreTranslate
- Ověřit SignalR zprávy jsou dodávány připojeným klientům
- Zkouška souběžné prevence (semafore)
- Potvrdit Markdown strukturu po překladu

### Konečné zkoušky

- Spouštěcí překlad přes API nebo plánovač
- Ověřit všechny soubory cílového jazyka jsou vytvořeny / aktualizovány
- Zkontrolovat soubory metadat obsahují správný stav bloku
- Potvrzení umístění jsou zachovány v překladech

## Zohlednění výkonnosti

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migrace z monolitického designu

Originál obsahoval všechny logiky v jedné třídě. Migrační cesta:

1. Logika extrakce země →
2. Extrakt JSON logika →
3. Výtažek Markdown logika →
4. extrakt signalr publishing →
5. Extrakt retry logika →
6. Zjednodušit orchestrátora pouze pro delegování

Všechna stávající rozhraní () zůstávají nezměněna. Spotřebitelé potrubí nevidí žádné změny.
