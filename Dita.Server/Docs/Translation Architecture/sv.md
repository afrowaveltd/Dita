# Översättning Arkitektur

Detta dokument beskriver den modulära arkitekturen i Ditas automatiska översättningssystem, introducerat för att förbättra underhållsförmåga, testbarhet och motståndskraft.

## Designmål

Refactoring behandlade flera problem med den ursprungliga monolitiska designen:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Inkrementell uthållighet**: Filer sparas per språk omedelbart efter översättning, vilket minskar minnesanvändningen och ger tidigare resultat.
- **Resiliens**: Flera retrynivåer hanterar övergående misslyckanden utan att blockera hela rörledningen.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: Nya översättningsmål kan läggas till genom att implementera ett enda gränssnitt.

## Servicedekomposition

### BackendTranslationService (orkestrator)

** Ansvar**:
- Pipeline Lifecycle Management (start, slutförande, felhantering)
- Semafor-baserad valutakontroll (förhindrar överlappande körningar)
- Server validering (latens, språktillgänglighet, konfiguration)
- Delegation till undertjänster

** Innehåller inte**:
- Översättningslogik
- Fil I/O för specifika format
- Retry logic

### Länder TranslationService

** Ansvar**:
- Läs från katalogen
- Synkronisera landsnamn i standard lokal ordbok
- Översätta saknade landsnamn per målspråk
- Spara varje målordbok direkt efter översättning

**Key behaviors**:
- Om standardspråket är engelska: landsnamn lagrade som-är
- Om standardspråk är annat: engelska namn översatta till standardspråk först
- Varje språk behandlas oberoende med sin egen retry loop

### lokaliseringsöversättningsservice

** Ansvar**:
- Detektera extra / borttagna nycklar genom att jämföra nuvarande standardordbok med tidigare ögonblicksbild
- Översätta tillsatta nycklar till varje målspråk
- Ta bort raderade nycklar från varje målspråk
- Spara ögonblicksbild för nästa jämförelse

**Key behaviors**:
- Manuella översättningar prioriterar alltid (aldrig överskriven)
- Lägga till nycklar översätts och sparas per språk omedelbart
- Ta bort nycklar raderas per språk omedelbart
- Snapshot sparas först efter att alla språk är framgångsrika

### DokumentTranslationService

** Ansvar**:
- Walk konfigurerade Markdown rötter återkommande
- Detektera ändrade källfiler med SHA-256 hashes
- Spåra per block översättning status i
- Översätt block-by-block med per-block retry
- Validate Markdown struktur efter översättning
- Spara varje målspråksfil självständigt

**Key behaviors**:
- Blocknivågranularitet: rubriker, punkter, listobjekt översätts separat
- Metadata spår som blockerar lyckas / misslyckats per språk
- Misslyckade block hämtas på nästa körning utan att översätta framgångsrika block
- Struktur validering säkerställer rubrikräkningar, listor, kodblock, etc. matchkälla

## Retry strategi

Systemet genomför retries på tre nivåer:

### Nivå 1 – HTTP (LibreTranslateService)

- Upp till 5 försök med exponentiell backoff (1s, 2s, 3s, 4s, 5s)
- Hantera nätverks timeouts, 5xx-fel och övergående fel
- Byggd i HTTP-klientkonfigurationen

### Nivå 2 - Steg (TranslationRetryService)

- Upp till 3 försök med 30 sekunders förseningar
- Re-drives hela översättningsförfrågan efter HTTP-nivå retries är uttömda
- Placeholder maskering och restaurering tillämpas på denna nivå

### Nivå 3 – Block (DokumentTranslationService)

- Individuella Markdown block som misslyckas markeras i metadata
- Hämtas automatiskt på nästa pipeline körning
- Framgångsrika block översätts aldrig

## Dataflöde

### JSON ordbok översättning

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

### Markdown översättning

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

### Översättning av landsnamn

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

## State persistence

### Snapshots

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Möjliggör stegvis synkronisering genom att spåra vad som var närvarande i föregående lopp

### Hash filer

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detekterar källförändringar för att undvika onödig återöversättning

### Översättningsmetadata

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Källa innehåll hash
- Per-language block status (array of booleans)
- Sista uppdateringstidsstamp
- **Purpose**: Möjliggör partiell återöversättning av endast misslyckade block

### Placeholder Storage

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Ger standardvärden för namngivna platsägare i hela ansökan

## Signal R-rapportering

### Publicera abstraktion

decouples översättningstjänster från SignalR-specifikationer:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sekvensgarantier

- Meddelanden inom en enda körning är monotoniskt sekvenserade
- Sekvensnummer är unika per-run via
- Kunder kan upptäcka luckor eller reordering

### Hub mapping

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Förlängningspunkter

### Lägga till ett nytt översättningsmål

1. Skapa ett nytt gränssnitt med
2. Genomföra gränssnittet med domänspecifik logik
3. Registrera dig i DI container
4. Injicera till konstruktör
5. Ring från efter befintliga stadier

### Anpassad retry policy

Överskridande konstruktionsparametrar:

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

### Anpassad platshållarhantering

Implementera för att ändra placeholder syntax eller lagring:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfiguration

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

### Runtime Tuning

Inställning
|---------|---------|--------|
80
10
3
30

## Teststrategi

### Enhetstest

Varje undertjänst är oberoende testbar:

- Mock för att simulera framgång / misslyckande
- Mock för att verifiera rapportering
- Använd tillfälliga kataloger för fil I/O
- Verifiera per språk spara beteende

### Integrationstest

- Full pipeline körs med verkliga (lokala) LibreTranslate instans
- Verifiera Signal R-meddelanden levereras till anslutna kunder
- Test samtidig körförebyggande (semafor)
- Validate Markdown struktur efter översättning

### Slut-to-end tester

- Trigger översättning via API eller schemaläggare
- Kontrollera att alla språkfiler skapas/uppdateras
- Kontrollera metadatafiler innehåller korrekt blockstatus
- Bekräfta platshållare bevaras över översättningar

## Prestanda överväganden

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lätta meddelanden, ingen nyttolastkomprimering som behövs för typiska rapporter

## Migration från monolitisk design

Originalet innehöll all logik i en klass. Migrationsvägen:

1. Extrahera land logik
2. Extrahera JSON logik
3. Extrahera Markdown logik
4. Extrakt Signal R publicering
5. Extrahera retry logik
6. Förenkla orkestratorn för delegation endast

Alla befintliga gränssnitt () är oförändrade. Konsumenterna av rörledningen ser inga brytande förändringar.
