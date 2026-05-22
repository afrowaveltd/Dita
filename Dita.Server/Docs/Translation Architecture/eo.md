# traduko de arkitekturo

Tiu dokumento priskribas la modulan arkitekturon de la aŭtomata traduko sistemo de Dita, enkondukita por plibonigi konserviblon, testeblon, kaj rezistecon.

## Dezajnoceloj

La rektoro traktis plurajn konzernojn kun la origina monolita dezajno:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Servodecomposition

### backendtranslation service (orchestrator)

**Responsibilities**:
- Pipeline vivociklo-administrado (komenco, kompletigo, eraro-manipulado)
- Semaphore-bazita konsentantkontrolo (preventoj imbrikitaj kuroj)
- Servilo validumado (latency, lingvohavebleco, konfiguracio)
- Delegado al sub-servoj

**Does NOT contain**:
- Traduko de logiko
- I/O por specifaj formatoj
- Reta logiko

### Landoj TranslationService

**Responsibilities**:
- Legu la dosierujon
- Sinkronigi landnomojn en la defaŭltan lokan vortaron
- Traduki mankantajn landnomojn per cellingvo
- Konservu ĉiun celvortaron tuj post traduko

**Key behaviors**:
- Se defaŭlta lingvo estas angla: landnomoj stokitaj kiel - estas
- Se defaŭlta lingvo estas alia: anglaj nomoj tradukitaj al defaŭlta lingvo unue
- Ĉiu lingvo estas prilaborita sendepende kun sia propra reenira buklo

### Lokalizo TranslationService

**Responsibilities**:
- Detect aldonis/removitajn ŝlosilojn komparante nunan defaŭltan vortaron kun antaŭa momentpafo
- Traduki aldonis ŝlosilojn en ĉiun cellingvon
- Forigo forigis ŝlosilojn de ĉiu cellingvo
- Savi momentfoton por venonta komparo

**Key behaviors**:
- Tradukoj ĉiam prenas prioritaton (neniam troskribita)
- Aldonitaj ŝlosiloj estas tradukitaj kaj ŝparitaj per-lingvo tuj
- Forigitaj ŝlosiloj estas forigitaj per-lingvo tuj
- Snapshot estas savita nur post kiam ĉiuj lingvoj kompletigas sukcese

### Dokumentoj TranslationService

**Responsibilities**:
- Piediro formis Markdown radikojn rekursive
- Detect ŝanĝis fontdosierojn uzantajn SHA-256 hashes
- Tra-bloka traduko statuso en
- Traduki bloko-post-bloko kun per-bloka retry
- Valida Markdown strukturo post traduko
- Konservu ĉiun cellingvodosieron sendepende

**Key behaviors**:
- Block-nivela grajneco: titoloj, paragrafoj, listeroj estas tradukitaj aparte
- Metadataj trakoj kiuj blokoj sukcesis/malsukcesa per lingvo
- Malsukcesaj blokoj estas retigitaj sur venonta kuro sen re-translating sukcesaj blokoj
- Strukturo validumado certigas titolojn, listojn, kodblokojn, ktp. matĉofonton

## Revizia strategio

La sistemo efektivigas retprovizojn sur tri niveloj:

### Nivelo 1 - HTTP (LibreTranslateService)

- Ĝis 5 provoj kun eksponenta dorso (1s, 2s, 3s, 4s, 5s)
- Handles-reto tempigas, 5xx erarojn, kaj pasemajn fiaskojn
- Konstruite en la HTTP klientkonfiguracion

### Nivelo 2 - Scenejo (Translation RetryService)

- Ĝis 3 provoj kun 30-dua prokrasto
- Re-rapidigas la tutan tradukon peto post HTTP-nivelaj retries estas elĉerpitaj
- Placeholder masking kaj restarigo estas uzitaj sur tiu nivelo

### Nivelo 3 - Bloko (Dokuments TranslationService)

- Individuaj Markdown blokoj kiuj malsukcesas estas markitaj en metadatenoj
- Retriigita aŭtomate sur la venonta dukto kuras
- Sukcesaj blokoj neniam estas re-tradukitaj

## Datenfluo

### JSON-vorta traduko

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

### Markdown traduko

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

### Landa nomo traduko

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

## Ŝtato persisto

### momentfotoj

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Haŝŝuo dosierojn

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Traduko de metadatenoj

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Fonto enhavo havas
- Per-lingva blokstatuso (radio de buleoj)
- La lasta ĝisdatigo
- **Purpose**: Enables partial re-translation of only failed blocks

### Situanta stokado

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## SignalR raportanta

### Fidante abstraktadon

ornamaj tradukservoj de SignalR-specifaj:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sequence garantias

- Mesaĝoj ene de ununura kuro estas monotonike sekvencitaj
- Sequence nombroj estas unikaj per-kontrolitaj per
- Klientoj povas detekti interspacojn aŭ restrukturitajn

### Hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Etendi punktojn

### Aldoni novan tradukon celo

1. Krei novan interfacon kun
2. Efektivigu la interfacon kun domajno-specifa logiko
3. Registro en DI-ujo
4. Injekto en konstrukciiston
5. Post ekzistantaj stadioj

### Kutima reenkonduka politiko

Superride-konstruanto parametroj:

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

### Kutima lokulo pritraktanta

Efektivigi lokulan sintakson aŭ stokadon:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfiguracio

### apps.json

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

### Runtime-agordado

la scenaro
|---------|---------|--------|
80
10
3
30

## Testanta strategio

### Unuo testas

Ĉiu sub-servo estas sendepende testebla:

- Mock por simuli sukceson/malsukceson
- Mock por konfirmi raportadon
- Uzu provizorajn adresarojn por dosiero mi / O
- Determini per-lingvan ŝparadkonduton

### Integriĝaj testoj

- Plena dukto kuras kun reala (loka) LibreTranslat kazo
- Verify SignalR mesaĝoj estas liveritaj al ligitaj klientoj
- Testo samtempa kuropreventado (semaphore)
- Valida Markdown strukturo post traduko

### Fin-al-finaj testoj

- Trigger traduko per API aŭ horaro
- Verify ĉiuj cellingvaj dosieroj estas kreitaj/ĝisdatigitaj
- Kontrolu metadatenoj dosierojn enhavas ĝustan blokstatuson
- Konfirmaj lokposedantoj estas konservitaj trans tradukoj

## Efikeckonsideroj

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migrado de monolita dezajno

La originalo enhavis ĉiun logikon en unu klaso. La migrado vojo:

1. Ekstraktado de la lando
2. Ekstraktado de JSON
3. Ekstraktado Markdown logiko
4. Ekstraktado SignalR-eldonado
5. Ekstraktado Retry logiko
6. Simpligi orkestron al delegacio-restriktita

Ĉiuj ekzistantaj interfacoj () restas senŝanĝaj. Konsumantoj de la dukto vidas neniujn rompoŝanĝojn.
