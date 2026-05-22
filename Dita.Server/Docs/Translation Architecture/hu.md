# Fordítás Építészet

Ez a dokumentum a Dita automatikus fordítási rendszerének moduláris felépítését írja le, amelyet azért vezetünk be, hogy javítsuk a fenntarthatóságot, a stabilitást és az ellenálló képességet.

## Tervezési célok

A bírálat az eredeti monolitikus kialakítással kapcsolatban számos aggályt vetett fel:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Szolgáltatási bomlás

### BackendTranslationService (zenekari)

**Responsibilities**:
- Pipeline életciklus menedzsment (start, complete, hibakezelés)
- Semaphore- alapú konvaluta ellenőrzés (megakadályozza az átfedő futásokat)
- Szerver validálása (láthatóság, nyelvi rendelkezésre állás, konfiguráció)
- Alszolgáltatások átruházása

**Does NOT contain**:
- Fordítási logika
- A különleges formátumok I / O fájlja
- ismételt logika

### countriestranslamationservice

**Responsibilities**:
- Olvass a könyvtárból
- Az országnevek szinkronizálása az alapértelmezett szótárba
- Hiányzó országnevek fordítása célnyelvenként
- Minden célszótár mentése azonnal fordítás után

**Key behaviors**:
- Ha az alapértelmezett nyelv angol: országnevek tárolt as-is
- Ha az alapértelmezett nyelv más: angol nevek lefordított alapértelmezett nyelv első
- Minden nyelvet önállóan dolgozunk fel a saját retry hurokkal

### lokalizationtranslationservice

**Responsibilities**:
- A hozzáadott / eltávolított billentyűk meghatározása az aktuális alapértelmezett szótár és a korábbi pillanatfelvétel összehasonlításával
- Hozzáadott kulcsok fordítása az egyes célnyelvekre
- Törölt kulcsok eltávolítása az egyes célnyelvekről
- A pillanatfelvétel mentése a következő összehasonlításhoz

**Key behaviors**:
- Kézi fordítás mindig elsőbbséget élvez (soha nem felülírva)
- Hozzáadott kulcsok fordítása és mentett per- nyelv azonnal
- Eltávolított billentyűk törölve per- nyelv azonnal
- A Premier csak az összes nyelv sikeres befejezése után menthető fel

### Dokumentumfordítási szolgáltatás

**Responsibilities**:
- Séta konfigurált Marklown gyökerek rekurzívan
- A forrásfájlok meghatározása SHA- 256 hashesszel
- Pálya per- block fordítás állapota
- A blokk- by-block lefordítása perblock relével
- Jelölési struktúra jóváhagyása fordítás után
- Minden célnyelvi fájl mentése függetlenül

**Key behaviors**:
- Blokkos szintű granularitás: a címek, bekezdések, listatételek külön fordításra kerülnek
- Olyan metaadatok, amelyek nyelvenként sikeresek / sikertelenek
- A sikertelen blokkok újratervezése a következő körben sikertelen blokkok átfordítása nélkül
- Szerkezetérvényesítés biztosítja az irányszámok, listák, kódblokkok stb. egyező forrás

## Visszaállítási stratégia

A rendszer három szinten hajt végre ismétléseket:

### Szint - HTTP (LibreTranslateService)

- Maximum 5 kísérlet exponenciális háttérrel (1, 2, 3, 4, 5)
- Hálózati időszámítás, 5xx hibák és átmeneti hibák kezelése
- Beépített HTTP kliens konfigurációba

### Szint - Stage (TranslationRetryService)

- Legfeljebb 3 kísérlet 30 másodperces késéssel
- A HTTP- szint remisszió kimerítése után a teljes fordítási kérés újraindítása
- A helytartó elfedése és helyreállítása ezen a szinten történik

### Szint - Block (Dokumentumfordítási szolgáltatás)

- A nem megfelelő egyedi jelölőtömbök metaadatokkal vannak jelölve
- Automatikus visszaállítás a következő csővezetéken
- A sikeres blokkokat soha nem fordítják újra

## Adatáramlás

### JSON szótár fordítás

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

### Jelölési fordítás

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

### Ország neve fordítás

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

## Állam perzisztencia

### pillanatképek

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Hash fájlok

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Fordítási metaadatok

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Forrástartalom hash
- Per- language block status (boolates tömb)
- Utolsó frissítési időbélyegző
- **Purpose**: Enables partial re-translation of only failed blocks

### A helytartó tárolása

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## A jeladó jelentése

### Kiadó absztrakció

elválasztja a fordítási szolgáltatásokat a SignalR sajátosságaitól:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sorozatgaranciák

- Az üzenetek egy rajzon belül monotonikusan szekvenálódnak
- A szekvencia számok egyedi per- run via
- Az ügyfelek felismerik a hiányosságokat vagy újratervezik

### Hub feltérképezése

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Meghosszabbítási pontok

### Új fordítási cél hozzáadása

1. Új felület létrehozása
2. Az interfész végrehajtása domainspecifikus logikával
3. Nyilvántartás az DI konténerben
4. Fecskendezze be a konstruktort
5. Hívás a meglévő szakaszok után

### Egyéni helyreállítási politika

A konstruktor felülbírálási paraméterei:

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

### A placeHolder egyéni kezelése

A helymeghatározó szintaxisának vagy tárolásának módosítása:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Beállítás

### apsettings.json

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

### A futásidő beállítása

Beállítás
|---------|---------|--------|
80
10
3
30

## Vizsgálati stratégia

### Egységvizsgálatok

Minden alszolgáltatás egymástól függetlenül tesztelhető:

- Gúnyolódik a siker / kudarc szimulálására
- A jelentéstétel ellenőrzése
- Ideiglenes könyvtárak használata az I / fájlhoz O
- Per- nyelvi megtakarítási viselkedés ellenőrzése

### Integrációs vizsgálatok

- Teljes csővezeték üzemeltetése valódi (helyi) LibreTranslate példával
- A SignalR üzenetek ellenőrzése az ügyfeleknek
- Egyidejűleg végzett vizsgálat prevenció (szemafore)
- Jelölési struktúra jóváhagyása fordítás után

### Végső vizsgálatok

- Az API-n vagy időbeosztáson keresztüli fordítás
- Minden célnyelvi fájl létrehozása / frissítése
- A metaadatok fájljainak ellenőrzése megfelelő blokkállapotot tartalmaz
- A helyfoglalók a fordítások során megőrződnek

## Teljesítménymegfontolások

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migráció monolitikus tervezésből

Az eredeti minden logikát tartalmazott egy osztályban. A migrációs út:

1. Kivonat ország logika →
2. Kivonat JSON logika →
3. Extract Marklown logika →
4. Kivonat SignalR kiadás →
5. Extrahálási logika →
6. Egyszerűsíteni kell a csak delegálásra szolgáló zenekart

Minden meglévő interfész () változatlan marad. A csővezeték fogyasztói nem látnak megszakító változásokat.
