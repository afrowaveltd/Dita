# Oversettelse Arkitektur

Dette dokumentet beskriver den modulære arkitekturen til Ditas automatiske oversettelsessystem, introdusert for å forbedre vedlikeholdbarheten, testbarheten og motstandsdyktigheten.

## Designmål

Refabrikkeringen løste flere bekymringer med den opprinnelige monolitiske utforming:

- **Bevaring av bekymringer**: Hvert oversettelsesdomene (land, JSON ordbøker, Markdown) er isolert.
- **Inkrementell utholdenhet**: Filene lagres per språk umiddelbart etter oversettelse, reduserer minnebruken og gir tidligere resultater.
- **Resiliens**: Flere reprøvenivåer håndterer forbigående feil uten å blokkere hele rørledningen.
- **Observerbarhet**: Hver signifikant operasjon er rapportert via SignalR for sanntidsovervåkning.
- ** Omfattbarhet**: Nye oversettelsesmål kan legges til ved å implementere et enkelt grensesnitt.

## Tjenestedekomponering

### MotorTranslationService (orchestrator)

**Responsibilities**:
- Pipeline livssyklusstyring (start, ferdigstillelse, feilhåndtering)
- Semaphore-basert konvalutakontroll (forventer overlappende løp)
- Servervalidering (latens, språktilgjengelighet, konfigurasjon)
- Delegasjon til undertjenester

** Inneholder ikke**:
- Oversettelseslogikk
- Fil I/O for bestemte formater
- Prøv logikk igjen

### LandTranslationService

**Responsibilities**:
- Les fra mappe
- Synkroniser landnavn i standard ordbok
- Oversett manglende landnavn per målspråk
- Lagre hver målleksikon umiddelbart etter oversettelse

**Key atferd**:
- Hvis standardspråket er engelsk: landnavn lagret som-er
- Hvis standardspråk er annet: engelske navn oversatt til standardspråk først
- Hvert språk behandles uavhengig av sin egen reprøv loop

### Lokaliseringsoverføringstjeneste

**Responsibilities**:
- Oppdage lagt til/fjernede nøkler ved å sammenligne gjeldende standardordbok med tidligere øyeblikksbilde
- Oversett lagt til nøkler til hvert målspråk
- Fjern slettede nøkler fra hvert målspråk
- Lagre øyeblikksbilde for neste sammenligning

**Key atferd**:
- Manuelle oversettelser tar alltid prioritet (aldri overskrevet)
- Leggde nøkler oversettes og lagres umiddelbart per språk
- Fjernede nøkler slettes umiddelbart per språk
- Snapshot lagres bare etter at alle språk er fullførte

### DokumentoverføringService

**Responsibilities**:
- Gå konfigurert Markdown røtter rekursivt
- Oppdag endret kildefiler ved hjelp av SHA-256 hashes
- Spor per-blokk oversettelsesstatus i
- Oversett blokk-for-blokk med per-blokk reprøv
- Valider markørstruktur etter oversettelse
- Lagre hver målspråkfil uavhengig

**Key atferd**:
- Blocknivå granularitet: overskrifter, avsnitt, listeelementer oversettes separat
- Metadataspor som blokkerer vellykket/feilstilt per språk
- Mislykkes blokker blir forsøkt på neste løp uten å omsette vellykkede blokker
- Validering av struktur sikrer overskriftstall, lister, kodeblokker etc

## Prøv strategi på nytt

Systemet implementerer retries på tre nivåer:

### Nivå 1 — HTTP (LibreTranslate Service)

- Opp til 5 forsøk med eksponentiell backoff (1s, 2s, 3s, 4s, 5s)
- Håndterer nettverksavbrudd, 5xx feil og forbigående feil
- Bygget i HTTP-klientkonfigurasjonen

### Nivå 2 — Trinn (fleirtyService)

- Opptil 3 forsøk med 30 sekunders forsinkelser
- Re-driver hele oversettelsesforespørselen etter at HTTP-nivå retries er utmattet
- Stedholder maskering og restaurering påføres på dette nivået

### Nivå 3 — Blokk (DokumentsTranslationService)

- Individuelle markeringsblokker som feiler er merket i metadata
- Fortsatt automatisk på neste rørledningskjøring
- Suksessfulle blokker omsettes aldri

## Datastrøm

### JSON ordbok oversettelse

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

### Merkeoversettelse

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

### Landnavn oversettelse

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

## Statens utholdenhet

### øyeblikksbilder

- **JSON**: Lagret i en fil ved siden av standardordboka (navnet varierer fra lagerleverandør)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Hash-filer

- **Markdown**: ved siden av kildefilen
- **Fallback**: hvis primær plassering er skrivebeskyttet
- **Purpose**: Oppdager kildeendringer for å unngå unødvendig re-translasjon

### Oversettelsesmetadata

- **Markdown**:
- ** Innhold**:
  - Kildeinnhold hash
- Perspråklig blokkstatus (array of booles)
- Siste oppdatering tidsstempel
- **Purpose**: Aktiverer delvis re-translasjon av bare mislykkede blokker

### Oppbevaring av plassholdere

- **Fil**:
- **Content**: Ordbok over nøkler til plasserer navn-verdi par
- **Purpose**: Gir standardverdier for navngitte plassholdere på tvers av programmet

## Signal R rapportering

### Utgiver Abstraksjon

decouples oversettelsestjenester fra SignalR spesifikasjoner:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sekvensgarantier

- Meldinger i et enkelt løp er monotonisk sekvensert
- Sekvensnummer er unike per-run via
- Kunder kan oppdage hull eller ombestilling

### Hub kartlegging

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Utvidelsespunkter

### Legg til et nytt oversettelsesmål

1. Opprett et nytt grensesnitt med
2. Implementer grensesnittet med domenespesifikk logikk
3. Registrer deg i DI container
4. Injiser i konstruktøren
5. Ring fra etter eksisterende stadier

### Tilpasset gjenforsøkspolicy

Overstyr konstruktorparametre:

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

### Tilpasset håndtering av plassholder

Implementer for å endre stedsholders syntaks eller lagring:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfigurasjon

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

Innstilling
|---------|---------|--------|
80
10
3
30

## Teststrategi

### Enhetstester

Hver undertjeneste er uavhengig testbar:

- Mock å simulere suksess/feil
- Mock å verifisere rapportering
- Bruk midlertidige mapper for fil I/O
- Bekreft atferd på hvert språk

### Integrasjonsprøver

- Full rørledning kjører med ekte (lokal) LibreTranslate instans
- Bekreft signal R-meldinger leveres til tilkoblede kunder
- Test samtidig kjøreforebygging (semathore)
- Valider markørstruktur etter oversettelse

### Ende-til-ende-prøver

- Trigger oversettelse via API eller planlegger
- Bekreft alle målspråkfiler opprettes/oppdateres
- Sjekk metadatafiler inneholder riktig blokkstatus
- Bekrefte at plasshavere er bevart på tvers av oversettelser

## Ytelseshensyn

- **Minne**: Per-språklig lagring hindrer å holde alle ordbøker i minnet
- **Disk I/O**: Metadatafiler legger til lite overhead, men aktiverer gradvis arbeid
- **Nettverk**: Sequential behandling med trottling hindrer overveldende LibreTranslate
- **CPU**: SHA-256 hashing og regulær validering er rask i forhold til oversettelse latens
- **Signaler**: Lette meldinger, ingen nyttelastkompresjon som trengs for typiske rapporter

## Migrasjon fra monolitisk design

Den opprinnelige inneholder all logikk i én klasse. Migrasjonsstien:

1. Utdrag country logikk
2. Uttrekk JSON logikk
3. Utdrag Markdown logikk
4. Pakk ut signal R utgivelse
5. Utdrag reprøv logikk
6. Forenkle orkestror til kun delegasjon

Alle eksisterende grensesnitt () forblir uendret. Forbrukere i rørledningen ser ingen endringer.
