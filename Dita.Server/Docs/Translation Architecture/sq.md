# Arkitektura e përkthimit

Ky dokument përshkruan arkitekturën modulare të sistemit automatik të përkthimit të Ditës, e cila u fut në përmirësimin e qëndrueshmërisë, të provës dhe të elasticitetit.

## Përdor synime

Riprodhimi trajtoi disa shqetësime me projektin origjinal monolitik:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Shërbimi

### TranslationService (orchestrator)

**Responsibilities**:
- Menaxhimi i ciklit të jetës së tubacionit (fillimi, plotësimi, trajtimi i gabimit)
- Kontrolli i depozitimit me bazë Semafore (parandalimet mbivendosjen)
- Server
- Delegacioni

**Does NOT contain**:
- Logjika e përkthimit
- File I/O për formate specifikë
- Kërko logjikën

### Vendet ndërlidhëse

**Responsibilities**:
- Lexo nga directory
- Sinkronizo emrat e vendeve në fjalorin e prezgjedhur lokal
- Përkthe emrat e vendeve të humbura për gjuhën e synuara
- Ruaj çdo fjalor objektiv menjëherë mbas përkthimit

**Key behaviors**:
- Nëse gjuha e prezgjedhur është anglisht: emrat e vendeve të ruajtura si-is
- Nëse gjuha e prezgjedhur është tjetër: emrat anglezë të përkthyer në gjuhën e prezgjedhur së pari
- Çdo gjuhë përpunohet në mënyrë të pavarur me riprodhimin e vet

### TranslationService

**Responsibilities**:
- Detec shtohet/rihiq kyçet duke krahasuar fjalorin aktual të prezgjedhur me fotografi të mëparshme
- Përkthe në çdo gjuhë objektive kyçe
- Hiq kyçet e fshirë nga çdo gjuhë e synuar
- Ruaj për

**Key behaviors**:
- Përkthimet manuale gjithmonë kanë përparësi (nuk janë shkruar kurrë tepër)
- Çelësat e shtuar përkthehen dhe ruhen menjëherë
- Zhblloko menjëherë kyçet e fshirë
- Snackshot është ruajtur vetëm mbas të gjitha gjuhëve të plotësuara me sukses

### Dokumente TranslationService

**Responsibilities**:
- Ec
- Detec ndryshuar file burim duke përdorur crashes SHA-256
- Gjurmo gjendjen e përkthimit në bllok
- Përkthe bllokun në bllok me riprovo
- Rregullon strukturën e shënuar mbas përkthimit
- Ruaj

**Key behaviors**:
- Blloqe niveli
- Gjurmët e Metadatës që bllokojnë me sukses/shkelur për gjuhë
- Blloqet e kryera në vazhdim do të ripërkthehen pa ripërkthyer blloqet e suksesshme
- Struktura

## Strategji riprovo

Sistemi zbaton retries në tre nivele:

### Niveli 1 HTTP (Libre TranslatesService)

- Deri në 5 përpjekje me mbështetje eksponenciale (1s, 2s, 3s, 4s, 5s)
- Trajton kohën e skadimit të rrjetit, 5x gabimet dhe dështimet e përkohshme
- Ndërtuar në konfigurimin e klientit HTTP

### Faza e Nivelit 2

- Deri në 3 përpjekje me vonesa 30 sekondash
- Përsërit e gjithë kërkesa e përkthimit pasi retries e nivelit HTTP janë të rraskapitura
- Mashtruesi i vendeve dhe restaurimi aplikohet në këtë nivel

### Niveli 3 Blloku

- Blloqet individuale të shënuara që dështojnë janë shënuar në metadata
- Ripërsëritur automatikisht në rrjedhën e ardhshme të tubacionit
- Blloqet e suksesshme nuk janë ripërkthyer kurrë

## Të dhëna

### Përkthimi i fjalorit JSON

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

### Pikë

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

### Vendi emri

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

## Këmbëngulja shtetërore

### Snackshots

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### File Hash

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Përkthimi

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Përmbajtja burues
- Gjëndja e bllokimit të per-gjuhëve (ray of booleans)
- Fundi
- **Purpose**: Enables partial re-translation of only failed blocks

### Vendshënues

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## SinjalR raporton

### Abstraksioni

nga:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sekuenca garanton

- Mesazhet brenda një zbatimi të vetëm janë të sekuencuara monotonalisht
- Sekuenca është unike
- Klientët mund të zbulojnë boshllëqet ose riorganizimin

### Hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Prapashtesa

### Shto a i ri

1. Krijo një ndërfaqe të re me
2. A me
3. Regjistrohu në përmbajtës DI
4. Injektuar në ndërtimore
5. Thirrje mbas

### Politika e personalizuar e riprovo

Mbishkruaj

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

### E personalizuar

Zbatimi nga ndrysho:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfigurimi i Mail

### programe.json

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

### Klasifikimi i kohës së vrapimit

Rregullimi
|---------|---------|--------|
80
10
3
30

## Strategji prove

### Njësitë

Çdo nën-shërbim është i pavarur

- Të tallen për të simuluar suksesin/shkretërimin
- Të tallen për të verifikuar raportimin
- Përdor directories e përkohshme për file I/ O
- Verifikimi i sjelljes shpëtuese për çdo gjuhë

### Testet e integrimit

- Tubacioni i plotë shkon me instancat reale (locale) Libre Translate
- Janë dërguar mesazhe të verifikuara për klientët e lidhur
- Test
- Rregullon strukturën e shënuar mbas përkthimit

### Provat e fundit

- Nga
- Kontrollo të gjithë file e gjuhës së synuar janë krijuar/updded
- Kontrolli gjendja
- Konfermo vendshënuesit janë ruajtur nëpërmjet përkthimeve

## Pyetje për shfaqje

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migrimi nga dizajni monotik

Origjinali përmbante çdo logjikë në një klasë. Rruga e migrimit:

1. Nxirr logjikën e vendit →
2. Nxirr logjikën JSON →
3. Nxirr logjikën e shënimit →
4. Nxirr
5. Nxirr logjikën e riprovoj →
6. Simulo orkestruesin vetëm me delegacion

Të gjitha interfaqet ekzistuese () mbeten të pandryshuara. Konsumatorët e tubacionit nuk shohin ndryshime të thyera.
