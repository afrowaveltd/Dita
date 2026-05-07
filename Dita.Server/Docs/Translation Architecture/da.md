# Oversættelse Arkitektur

Dette dokument beskriver den modulære arkitektur af Ditas automatiske oversættelsessystem, indført for at forbedre vedligeholdeligheden, testbarheden og modstandsdygtigheden.

## Konstruktionsmål

Refactoring behandlet flere bekymringer med det oprindelige monolitiske design:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Service dekomponering

### BackendTranslationService (orkester)

**Responsibilities**:
- Pipeline livscyklusstyring (start, afslutning, fejlhåndtering)
- Semaphorebaseret koncurrency kontrol (forhindrer overlappende kørsler)
- Servervalidering (latency, sprog tilgængelighed, konfiguration)
- Delegation til undertjenestegrene

**Does NOT contain**:
- Oversættelseslogik
- Fil I / O for specifikke formater
- Prøv igen

### landestratislationstjeneste

**Responsibilities**:
- Læs fra mappen
- Synkronisér landenavne i standardordbogen
- Oversæt manglende landenavne pr. målsprog
- Gem hvert mål ordbog umiddelbart efter oversættelse

**Key behaviors**:
- Hvis standardsprog er engelsk: landenavne gemt as- is
- Hvis standardsprog er andet: Engelsk navn oversat til standardsprog først
- Hvert sprog behandles uafhængigt med sin egen prøve loop

### LokalizationTranslationService

**Responsibilities**:
- Detektér tilføjede / fjernede nøgler ved at sammenligne nuværende standard ordbog med tidligere øjebliksbillede
- Oversæt tilføjede nøgler til hvert målsprog
- Fjern slettede nøgler fra hvert målsprog
- Gem øjebliksbillede til næste sammenligning

**Key behaviors**:
- Manuelle oversættelser altid tage prioritet (aldrig overskrevet)
- Tilføjet nøgler er oversat og gemt per- sprog straks
- Fjernede nøgler slettes pr. sprog med det samme
- Snapshot gemmes kun efter alle sprog fuldført med succes

### Dokumentoverførselstjeneste

**Responsibilities**:
- Gå konfigureret Markdown rødder rekursivt
- Detektér ændrede kildefiler ved hjælp af SHA- 256 hashs
- Track per- block oversættelse status i
- Oversæt block- by- block med per- block returforsøg
- Validér Marknedstruktur efter oversættelse
- Gem hver målsprogfil uafhængigt

**Key behaviors**:
- Block- niveau granularitet: overskrifter, afsnit, liste poster er oversat separat
- Metadata spor som blokke lykkedes / mislykkedes per sprog
- Mislykkede blokke genprøves på næste kørsel uden at genoversætte succesfulde blokke
- Strukturen validering sikrer overskrifter, lister, kodeblokke osv

## Prøv igen strategi

Systemet gennemfører forsøg på tre niveauer:

### Niveau 1 - HTTP (LibreTranslateService)

- Op til 5 forsøg med eksponentiel backoff (1s, 2s, 3s, 4s, 5s)
- Håndterer netværkstimer, 5xx fejl og forbigående svigt
- Bygget ind i HTTP- klientkonfigurationen

### Niveau 2 - Fase (TranslationRetryService)

- Op til 3 forsøg med 30 sekunders forsinkelser
- Returnerer hele oversættelsesanmodningen efter HTTP- niveau forsøg er udtømt
- Placeholder maskering og restaurering anvendes på dette niveau

### Niveau 3 - Blok (DocumentsTranslationService)

- Individuelle Markdown blokke, der mislykkes er markeret i metadata
- Genprøvet automatisk ved næste rørledning
- Succesfulde blokke bliver aldrig omoversat

## Datastrøm

### JSON ordbog oversættelse

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

### Markering oversættelse

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

### Oversættelse af landenavn

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

## Statslig persistens

### Snapshots

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Hash-filer

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Oversættelsesmetadata

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Kildeindhold hash
- Per- sprog blok status (række af booster)
- Senest opdateret tidsstempel
- **Purpose**: Enables partial re-translation of only failed blocks

### Opbevaring

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## Signal R-rapportering

### Udgiver abstraktion

afkobling af oversættelsestjenester fra SignorR-specialer:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sekvensgarantier

- Meddelelser inden for et enkelt løb er monotonisk sekvenserede
- Sekvensnumre er unikke per- run via
- Klienter kan opdage huller eller ombestilling

### nav mapping

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Udvidelsespunkter

### Tilføjelse af et nyt oversættelsesmål

1. Opret en ny grænseflade med
2. Implementere grænsefladen med domæne-specifik logik
3. Registrer dig i DI-container
4. Injicér i konstruktør
5. Opkald fra eksisterende faser

### Brugerdefineret retriepolitik

Parametre for overridning af konstruktionsapparat:

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

### Brugerdefineret pladshåndtering

Implementere at ændre pladsholder syntaks eller lagring:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Indstilling

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

### Runtime tuning

Indstilling
|---------|---------|--------|
80
10
3
30

## Teststrategi

### Enhedstest

Hver deltjeneste kan afprøves uafhængigt:

- Mock at simulere succes / fiasko
- Mock til at verificere rapportering
- Brug midlertidige mapper til fil I / O
- Verificér per- sprog besparende adfærd

### Integrationstest

- Fuld rørledning med reel (lokal) LibreTranslate instans
- Verificér signal R-beskeder leveres til tilsluttede kunder
- Test samtidig kørsel forebyggelse (semaphore)
- Validér Marknedstruktur efter oversættelse

### Ende- to- end test

- Trigger oversættelse via API eller Scheduler
- Verificér alle målsprogfiler er oprettet / opdateret
- Tjek metadatafiler indeholder korrekt blokstatus
- Bekræft pladsholdere bevares på tværs af oversættelser

## Præstationsbetragtninger

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migration fra monolitisk design

Originalen indeholdt al logik i én klasse. Indvandringsvejen:

1. Uddrag land logik →
2. Uddrag JSON logik →
3. Uddrag Markdown logik →
4. Uddrag signal R offentliggørelse →
5. Uddrag gentry logik →
6. Forenkling af orkester til delegationer- kun

Alle eksisterende grænseflader () forbliver uændrede. Forbrugerne af rørledningen ser ingen ændringer.
