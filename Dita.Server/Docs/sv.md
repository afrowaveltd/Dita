# Sammanfattning av ändringar av den automatiska översättningstjänsten

## Översikt

Detta dokument sammanfattar alla ändringar som gjorts till Dita automatisk översättningstjänst, inklusive arkitektur refactoring, nya funktioner, observerbarhet förbättringar och lokaliseringsförbättringar.

## Arkitekturändringar

### Refactored BackendTranslationService

Monoliten har sammansatts i fyra specialiserade tjänster som samordnas av en lättviktsorkestrator:

- **BackendTranslationService** — Pipeline orchestrator (server validering, scendelegation, felhantering)
- **CountriesTranslationService** - Landsnamn synkronisering (engelska → målspråk)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DokumentTranslationService** - Markdown dokumentation översättning med block-nivå spårning
- **SignalRPublisher** – Realtidsrapportering via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Fördelar

- **Separation of concerns**: Each service handles a single translation domain
- ** Hållbarhet**: Mindre klasser är lättare att förstå och testa
- **Extensibility**: Nya översättningsmål kan läggas till via implementering av gränssnitt
- **Reliability**: Independent services provide better fault isolation

## Nya funktioner

### Live Translation Monitor

** Plats**:

En ny administratörssida som ger realtidssynlighet i översättningsledningen:

- Visar alla signaler R händelser när de inträffar
- Färgkodade meddelandetyper (blue=started, green=completed, red=error)
- Anslutningsstatus banner med auto-reconnect
- Message counter och export till JSON

### Namngivna platshållare

Lokaliseringssystemet stöder nu namngivna platser () för förbättrad grammatikalitet på olika språk:

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
- Placeholder värden som tillhandahålls vid drifttid eller lagras i
- Automatisk maskering/restaurering under översättning för att förhindra korruption
- Bakåtkompatibel med befintliga positionsägare

### Incremental översättning

Markdown filer översätts stegvis:

- **Per-language spar**: Varje målspråk sparas omedelbart efter översättning, vilket minskar minnestrycket
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- ** Selektiv retry**: Endast misslyckade block översätts på nästa körning
- **Metadata persistence**: Translation state survives application restarts

### Förbättrad Retry Logic

Tre nivåer av motståndskraft:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DokumentTranslationService): Misslyckade Markdown block hämtas på nästa körning

### SignalR-rapportering

Realtidsrapportering för alla pipelineverksamheter:

- Varje steg publicerar evenemang
- Per-language framsteg publiceras som händelser
- Felhändelser inkluderar detaljerad kontext (källa, felkod, meddelande)
- Sekvensnummer garanterar beställning inom varje körning

## Konfigurationsförändringar

### appsettings.json

Inga bryta förändringar. Befintlig konfiguration fortsätter att fungera:

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

### Nya tjänster

Registrerad i:

- ///
- `TranslationRetryService`
- ///
- ///
- ///
- ///

Signalen R hub är kartlagd för klientanslutningar.

## Testning

### Teststatus

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Ny testtäckning tillsatt för:
  - Placeholder Servicefunktionalitet
  - BackendTranslation Service orchestration
  - JsonStringLocalizer placeholder indexers

### Kända begränsningar

- testet hoppas parallellt eftersom flera testinstanser delar samma fil. Det passerar när man kör i isolering.

## Ny filstruktur

### Tjänster i

- Pipeline orchestrator
- översättning av landsnamn
- JSON ordbok synkronisering
- Markdown översättning
- Signal R-meddelande publicering
- Retry logik med placeholder masking
- Publicera gränssnitt
- Land service Interface
- Lokalisering service gränssnitt
- Dokumentservice gränssnitt
- Orchestrator gränssnitt (uppdaterat)
- Per-fil översättning metadata

### Uppdaterade tjänster i

- Tillsatt namngiven placeholder support
- —— Uppdaterad för ny parameter
- Namngivna placeholder management
- Placeholder interface

### Ny Admin Page i

- Realtidsövervakningssida
- - Sidmodell

### Ny dokumentation i

- —— Uppdaterad pipelinedokumentation
- Placeholder systemguide
- Dashboard användning guide
- Teknisk arkitekturöversikt

## Bakåtkompatibilitet

Alla ändringar är additiva:

- Befintlig lokaliseringskod () fungerar oförändrad
- Positionell formatering () fungerar oförändrad
- Befintligt JSON-ordboksformat är oförändrat
- Befintlig Markdown struktur är oförändrad
- Signal R-meddelanden använder samma format

## Migrationsväg

Ingen migration krävs. Refaktoreringen är intern:

1. Gamla bevarades som referens och ersattes sedan
2. DI-registreringar uppdaterades för att använda nya gränssnitt
3. Alla befintliga konsumenter ser inga förändringar

## Prestandaförbättringar

- **Reducerad minnesanvändning**: Filer sparade per språk omedelbart istället för att hålla allt i minnet
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Framtida förbättringar

Planerade förbättringar:

1. **AI finjustering** – Översättning efter maskin för fraser > 5 ord
2. **Admin autentisering** – Begränsa administreringssidor till auktoriserade användare
3. **Dictionary editor** -- Web UI för hantering av lokaliseringsnycklar
4. **Translationsstatistik** – Diagram som visar översättningsräkningar och felfrekvenser över tiden
5. **Custom placeholder syntax** – Stöd för alternativa platshållarformat

## Kontakta Kontakt

För frågor eller problem med översättningstjänsten, se detaljerad dokumentation i varje moduls katalog eller kontakta utvecklingsteamet.
