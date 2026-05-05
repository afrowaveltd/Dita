# Oversigt over ændringer i den automatiske oversættelsestjeneste

## Oversigt

Dette dokument opsummerer alle ændringer foretaget til Dita automatisk oversættelse service, herunder arkitektur refactoring, nye funktioner, observerbarhed forbedringer, og lokalisering forbedringer.

## Arkitektændringer

### Refaktoreret backendTranslationService

Den monolitiske er blevet opdelt i fire specialiserede tjenester koordineret af en letvægts orkester:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Ydelser

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Nye funktioner

### Live oversættelsesskærm

**Location**: `/Admin/LiveTranslation`

En ny admin side, der giver real- time synlighed i oversættelsesledningen:

- Viser alle SignalR begivenheder som de forekommer
- Farvekodede meddelelsestyper (blå = startet, grøn = afsluttet, rød = fejl)
- Forbindelsesstatusbanner med automatisk genforbindelse
- Meddelelsestæller og eksport til JSON

### Opkaldt placeholdere

Lokaliseringssystemet understøtter nu navngivne pladsholdere () for forbedret grammatik på forskellige sprog:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Funktioner:
- Placeholderværdier ved driftstid eller opbevaret i
- Automatisk maskering / restaurering under oversættelse for at forhindre korruption
- Baglæns kompatibel med eksisterende positioneringspladsholdere

### Incremental oversættelse

Markdown filer er oversat trinvist:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Forstærket Retry Logic

Tre niveauer af modstandsdygtighed:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### SignalR-rapportering

Realtidsrapportering for alle rørledningsoperationer:

- Hver fase offentliggør begivenheder
- Per- sprog fremskridt offentliggjort som begivenheder
- Fejlbegivenheder omfatter detaljeret kontekst (kilde, fejlkode, meddelelse)
- Sekvensnumre garanti bestilling inden for hvert løb

## Indstillingsændringer

### appsettings.json

Ingen ændringer. Eksisterende konfiguration virker fortsat:

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

### Nye tjenester

Registreret i:

- /
- `TranslationRetryService`
- /
- /
- /
- /

SignalR-hubben er kortlagt for kundeforbindelser.

## Test

### Prøvningsstatus

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Ny testdækning tilføjet for:
  - Funktionen PlaceholderService
  - BackendTranslationService-orkester
  - JsonStringLocalizer pladsholder indekserer

### Kendte begrænsninger

- test er sprunget over, når du kører parallelt, fordi flere testinstanser deler den samme fil. Den passerer, når den løber i isolation.

## Ny filstruktur

### Tjenesteydelser i

- - Pipeline orkester
- - Landenavn oversættelse
- - JSON ordbog synkronisering
- - Markering oversættelse
- - SignalR meddelelse udgivelse
- - Prøv igen logik med pladskortlægning
- - Publisher interface
- - Land service interface
- - Lokalisering service interface
- - Document Service interface
- - Orkestrator interface (opdateret)
- - Per- fil oversættelse metadata

### Opdateret service i

- - Tilføjet navngivet pladsholder support
- - Opdateret for ny parameter
- - Opkaldt pladsholder management
- - Placeholder interface

### Ny Admin- side i

- - Real- time overvågning side
- - Side model

### Ny dokumentation i

- - Updated pipeline documentation
- - Systemguide til stedholdere
- - brugsvejledning for instrumentbrættet
- - Teknisk arkitektur oversigt

## Kompatibilitet baglæns

Alle ændringer er additive:

- Eksisterende lokaliseringskode () virker uændret
- Positional formatering () virker uændret
- Eksisterende JSON ordbog format er uændret
- Eksisterende markeringsstruktur er uændret
- SignalR-meddelelser bruger samme format

## Migrationsvej

Ingen migration påkrævet. Refactoring er intern:

1. Gammel blev bevaret som reference og derefter erstattet
2. DI registreringer blev opdateret til at bruge nye grænseflader
3. Alle eksisterende forbrugere ser ingen ændringer

## Præstationsforbedringer

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Fremtidige forbedringer

Planlagte forbedringer:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Kontakt

For spørgsmål eller spørgsmål med oversættelsestjenesten henvises til den detaljerede dokumentation i hvert moduls mappe eller kontakt udviklingsteamet.
