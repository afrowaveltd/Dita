# Arquitectura de traducció

Aquest document descriu l' arquitectura modular del sistema de traducció automàtica de la Dita, introduït per millorar la mantébilitat, la prova i la resistència.

## Dissenya objectius

El refavorisme ha tractat diverses preocupacions amb el disseny monolètic original:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilitat **: Múltiples nivells reintentar errors transitoris sense bloquejar tota la canonada.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extenibilitat **: Es poden afegir nous objectius de traducció executant una única interfície.

## Descomposició del servei

### Dorsal detraducció delService (orchestrator)

**Responsibilities**:
- Gestió de cicles de vida de conducte (inici, compleció, gestió d' errors)
- Control d'injustència basat en el Smaphore (revisió sobrepassades)
- Validació del servidor (lateència, disponibilitat del llenguatge, configuració)
- Delegació als subserves

**Does NOT contain**:
- Lògica de traducció
- Fitxer E/ S pels formats específics
- Reintenta la lògica

### Servei de desenvolupament PaïsosAdvanced URLs: description or category

**Responsibilities**:
- Llegeix des del directori
- Sincronitza els noms dels països al diccionari local per omissió
- Tradueix els noms dels països que falten per llengua objectiu
- Desa immediatament cada diccionari objectiu després de la traducció

**Key behaviors**:
- Si l' idioma per omissió és anglès: els noms dels països desats com a- és
- Si l' idioma per omissió és un altre: els noms d' anglès traduïts a l' idioma per omissió primer
- Cada idioma es processa independentment amb el seu propi bucle de reintentar- ho

### Servei de localització

**Responsibilities**:
- Detecta tecles afegides/ recursades comparant el diccionari actual amb la instantània anterior
- Tradueix les claus afegides en cada idioma de destí
- Elimina les claus eliminades de cada idioma de destí
- Desa la instantània per a la comparació següent

**Key behaviors**:
- Les traduccions manuals sempre tenen prioritat (no sobreescrites)
- S' han afegit les tecles es tradueixen i s' han desat per llengua immediatament
- S' han eliminat les tecles per idioma immediatament
- La instantània es desa només després de que tots els idiomes tinguin èxit

### DocumentstrationService

**Responsibilities**:
- Camineu configurat arrels Markdown recursivament
- Detecta fitxers font canviats usant SHA- 256 hahes
- Estat de la traducció per blocada
- Tradueix bloc- a través del bloc amb per bloc torneu- ho a intentar
- Valida l' estructura Markdown després de la traducció
- Desa de forma independent cada fitxer d' idioma de destí

**Key behaviors**:
- Granularitat de bloc: capçaleres, paràgrafs, elements de llista es tradueixen per separat
- Les peces de metadades que els blocs han fallat/ ha fallat per llengua
- Els blocs erronis es retribueixen a la propera execució sense tornar a indexar els blocs d'èxit
- La validació de l' estructura assegura els comptadors de capçaleres, llistes, blocs de codi, etc. coincideix amb el codi font

## Reintenta l' estratègia

El sistema implementa reintents a tres nivells:

### Nivell 1 Manveen HTTP (LibrecharteService)

- Fins a 5 intents amb l'operació exponencial (1s, 2s, 3s, 4s, 5s)
- Gestiona els temps d' espera de la xarxa, 5xx errors, i errors transitoris
- S' ha encastat la configuració del client HTTP

### Nivell 2 Targe (traduccióRetitual)

- Tres intents amb 30 segons retards
- Torna a desar la petició de traducció sencera després que les reintents HTTP de nivell s' hagin acabat
- Màscara de reserva i restauració s' aplica a aquest nivell

### Bloc de nivell 3 (Documents AdvancedrService)

- Marca els blocs individuals que no estan marcats en metadades
- Reordenat automàticament a l' execució de canonada següent
- Els blocs amb èxit mai no es tornen a traduir

## Flux de dades

### Traducció al diccionari JSON

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

### Traducció enrere

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

### Traducció al nom del país

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

## Estat persisteix

### Instantànies

- ** JSON **: Emmagatzemat en un fitxer al costat del diccionari per omissió (nom varia pel proveïdor d' emmagatzematge)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Fitxers de resum

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Pourposa **: Detecta canvis de codi font per evitar una reducció innecessària

### Metadata de traducció

- **Marcat **:
- **Contents **:
  - Resum del contingut de la font
- Estat del bloqueig en llengua (desplegament de booleans)
- Última marca de temps d' actualització
- **Purpose**: Enables partial re-translation of only failed blocks

### Emmagatzematge de substitució

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## Informe de senyalR

### Abstracció de l' editor

decorades serveis de traducció dels senyals:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Aplicacions de seqüència

- Els missatges dins d' una única execució són corrotonament seqüenciats
- Els números de seqüència són únics perun pas via
- Els clients poden detectar buits o reordenar

### Mapatge Hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Punts d' extensió

### Afegir un nou objectiu de traducció

1. Crea una nova interfície amb
2. Implementa la interfície amb lògica específica de domini
3. Registra en un contenidor DI
4. Injecta al constructor
5. Crida des de després de les fases existents

### Política reintentar- ho personalitzada

Sobreescriu els paràmetres de construcció:

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

### Gestió de marcadors personalitzats

Implementa per a canviar la sintaxi de substitució o l' emmagatzematge:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Configuració

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

### Millora de l' execució

Configuració
|---------|---------|--------|
80
10
3
30

## Estratègia de proves

### Comprovacions d' unitat

Cada subserveble és independent:

- Falsa a simular èxit/faliure
- MEX per verificar l' informe
- Usa directoris temporals per al fitxer I/ O
- Verifica el comportament de desat per idioma

### Comprovacions d' integració

- Execució completa de canonada amb una instància real (local) Libretrape
- Verifica els missatges senyalR als clients connectats
- Prova la prevenció d' execució recurrent (semaphore)
- Valida l' estructura Markdown després de la traducció

### Comprovacions finals a final

- Activa la traducció mitjançant API o planificador
- Verifica tots els fitxers d' idioma destí es creen/ actualitzats
- Comprova els fitxers de metadades contenen un estat de bloc correcte
- Confirma la substitució es preserva a través de les traduccions

## Les consideracions de rendiment

- ** Memoriy **: En el desat en llengua evita mantenir tots els diccionaris en memòria
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migració del disseny monolithic

L'original conté tota lògica en una classe. La ruta de migració:

1. Extraieu la lògica del país
2. Extreu la lògica JSON eka
3. Extreu la lògica de markdown KFormula
4. Extreu la publicació de senyalsR bibliography
5. Extreu la lògica reintentar
6. Simplifiqueu orquestrator a només de delegació

Totes les interfícies existents () continuen sense canvis. Els consumidors de la canonada no veuen canvis trencar.
