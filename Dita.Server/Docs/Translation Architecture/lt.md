# Vertimo raštu architektūra

Šiame dokumente aprašoma modulinė "Ditos" automatinio vertimo sistemos architektūra, kuri buvo įdiegta siekiant pagerinti priežiūros galimybes, stabilumą ir atsparumą.

## Konstrukcijos tikslai

Atsisakymas buvo skirtas kelioms problemoms, susijusioms su originaliu monolitiniu modeliu:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Eksploatavimo nutraukimas

### BackendTranslationService (orkestras)

**Responsibilities**:
- Vamzdynų gyvavimo ciklo valdymas (pradžia, užbaigimas, klaidų valdymas)
- Semaforinė concurrence kontrolė (užkertant kelią persidengiantiems važiavimams)
- Serverio patvirtinimas (vėlavimas, kalbos prieinamumas, konfigūracija)
- Delegavimas teikiant subpaslaugas

**Does NOT contain**:
- Vertimo logika
- Failas I / O, skirtas konkretiems formatams
- @ action: button

### ŠalysTranslationService

**Responsibilities**:
- Skaityti iš aplanko
- Sinchronizuoti šalių pavadinimus į numatytąjį locale žodyną
- Praleistų šalių pavadinimai pagal paskirties kalbą
- Išsaugoti kiekvieno tikslo žodyną iš karto po vertimo

**Key behaviors**:
- NAME OF TRANSLATORS
- NAME OF TRANSLATORS
- Kiekviena kalba yra tvarkomi savarankiškai su savo retry kilpa

### LokalizationTranslationService

**Responsibilities**:
- NAME OF TRANSLATORS
- Klubui buvo priskirtas žaidėjas dėl kiekvienos tikslinės kalbos
- Pašalinti ištrinti raktus iš kiekvienos tikslinės kalbos
- Įrašyti nuotrauką kitam palyginimui

**Key behaviors**:
- Rankinis vertimas visada prioritetas (niekada perrašyti)
- Pridėta raktai yra išverstos ir išsaugotos pe- kalba iš karto
- Šalinti raktai ištrinami iš karto
- Comment

### DokumentaiTranslationService

**Responsibilities**:
- @ info: whatsthis
- Detektuoti pakeistus pradinio kodo failus naudojant SHA-256 brūkšnelius
- Takelio per- block vertimo būsena
- Translate block- by- block su per- block retry
- @ info: tooltip
- @ info: tooltip

**Key behaviors**:
- Bloko lygio detalumas: antraštės, dalys, sąrašo straipsniai verčiami atskirai
- Name
- NAME OF TRANSLATORS
- Konstrukcijos patvirtinimas užtikrina pozicijų skaičių, sąrašus, kodų blokus ir t. t. rungtynių šaltinį

## Tęsti strategiją

Sistemoje atliekami bandymai trimis lygmenimis:

### 1 lygis - HTTP (LibreTranslateService)

- 5 bandymai su eksponentine atsarga (1, 2, 3, 4, 5)
- Name
- Integruoti į HTTP kliento konfigūraciją

### 2 lygis - etapas (TranslationRetryService)

- 3 bandymai su 30 sekundžių vėlavimu
- @ info: whatsthis
- Kambarių maskavimas ir atkūrimas taikomas šiame lygyje

### 3 lygis - blokas (DocumentTranslationService)

- Atskiri žymėjimo blokai, kurie neatitinka metaduomenų
- @ info: tooltip
- Sėkmingas blokai niekada iš naujo išversti

## Duomenų srautas

### JSON žodyno vertimas

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

### Žymeklio vertimas

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

### Šalies pavadinimo vertimas

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

## Valstybės atsparumas

### Comment

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Raktažodžiai

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Vertimo metaduomenys

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Šaltinio turinys
- Kalbų bloko statusas (banglenčių masyvas)
- Paskutinė atnaujinimo laiko žyma
- **Purpose**: Enables partial re-translation of only failed blocks

### Talpyklos laikymas

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## Signalas R pranešimas

### Leidėjo ėmimas

atsieti vertimo paslaugas nuo SionalR specifikos:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sekos garantijos

- @ info: whatsthis
- Sequence numeriai yra unikalus per- paleisti per
- Klientai gali aptikti spragas arba pakeisti užsakymą

### Šaknies kartografavimas

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Išplėtimo taškai

### Pridedamas naujas vertimo tikslas

1. Sukurti naują sąsają su
2. @ info: tooltip
3. Registras DI konteineryje
4. Sušvirkškite į konstruktorių
5. Skambutis iš po esamų etapų

### @ info: whatsthis

Konstruktoriaus parametrų nepaisymas:

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

### Korpuso tvarkymas

@ info: whatsthis

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfigūracija

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

### Skrydžio laiko reguliavimas

@ action: button
|---------|---------|--------|
80
10
3
30

## Testavimo strategija

### Vieneto bandymai

Kiekviena subpaslauga yra nepriklausomai tikrinama:

- Mock, siekiant imituoti sėkmę / nesėkmę
- Mock to patikrinti ataskaitų
- I / O failui naudoti laikinus katalogus
- Patikrinti per- kalbos taupymo elgesį

### Integracijos bandymai

- Full touch run with real (local) Librelaw instance
- Patikrinti signalą R pranešimai pristatomi prijungtiems klientams
- Testų atlikimas vienu metu (semaforas)
- @ info: tooltip

### galiniai bandymai

- Sigger vertimas per API arba Reguliatorius
- Patikrinti visus tikslinės kalbos failus yra sukurti / atnaujinti
- Patikrinkite metaduomenų failus, kuriuose yra teisinga bloko būsena
- Transliuojant išsaugomi patvirtinantys placebą turintys asmenys

## Našumas

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migracija iš monolitinio dizaino

Originale buvo visa logika vienoje klasėje. Migracijos kelias:

1. Ekstrahuoti šalies logiką →
2. Ekstraktas JSON loginis →
3. Ekstrakto žymėjimo logika →
4. Ekstrahavimo signalas R leidyba →
5. Ekstrahuoti pakartotinai logika →
6. Paprastesnis orchestratorius tik delegacijai

Visos esamos sąsajos () nekeičiamos. Vamzdyno vartotojai nemato jokių esminių pokyčių.
