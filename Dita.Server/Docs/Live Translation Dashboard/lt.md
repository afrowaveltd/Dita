# Name

Live Translation Dashboard yra admin puslapis, kuris suteikia realiu laiku matomumą į automatinio vertimo vamzdyną. Jungia prie SionalR stebulės ir rodo visus vamzdynų įvykius, kai jie įvyksta.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Savybės

### @ info: whatsthis

Signalas R vertimų biuro renginiai pateikiami atnaujinamoje lentelėje:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Spalvų kodavimas

Spalva
|-------|---------|
Mėlyna ()
Žalia ()
Raudona ()
Balta (numatytoji)

### Prisijungimo būsena

Statuso vėliava viršuje rodo:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

Jungtis naudoja automatinį reconnect su eksponentine nugaros: 0S, 2S, 5S, 10S, 30S.

### Kontrolė

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## Signalas R stebulė

Prietaisų skydelio jungtis:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Laiško sutartis

```typescript
interface LocalizationHubMessage {
    runId: string;        // Guid
    sequence: number;     // long
    type: LocalizationMessageType;
    stage: ProcessStage;
    timestampUtc: string; // ISO 8601
    isError: boolean;
    message: string;
    data: object | null;
}
```

### Įvykių rūšys

Prietaisų skydelį sudaro visos reikšmės:

Tipas
|------|---------|
Mėlynasis ženklelis
Žalias ženklelis
Raudonasis ženklelis
Žalias ženklelis
Raudonasis ženklelis
Informacinis ženklelis
Įspėjimo ženklas

## Techninis įgyvendinimas

### Comment

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Viršūnė

- Grynas HTML / JS su Bootstrap 5 stiliaus
- Naudoja Microsoft SignalR JavaScript klientų biblioteką (įkelta iš CDN)
- Nr server- side atvaizdavimo reikia renginio pašarų

### Puslapio struktūra

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Naudojimas kūrimo metu

1. Pradėk Ditą. Serverio programa
2. Pereiti prie
3. Sigger vertimo paleisti (arba laukti reguliatoriaus arba skambinti API)
4. Stebėti įvykius realiu laiku
5. @ info: whatsthis

## Tolesni veiksmai

Planuojami prietaisų skydelio patobulinimai:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Trikčių šalinimas

### Name

1. Patikrinkite, ar serveris veikia ir ar yra prieinamas
2. Patikrinkite naršyklės konsolę CORS arba tinklo klaidoms
3. Patvirtinu, kad
4. NAME OF TRANSLATORS

### @ info: whatsthis

1. Patikrinkite, ar SionalR mazgo URL atitinka serverio () ir kliento ()
2. @ info: whatsthis
3. Žvilgsnis į serverio žurnalus vertimo vamzdyno klaidoms
4. Patikrinti naršyklę Comment

### Laiškai neveikia

Laukas garantuoja užsakymas per vieną kartą. (neprivaloma)
- Kelių vamzdynų tiesimas iš dalies sutampantis (neturėtų įvykti dėl semaforo šliuzo)
- Naršyklės atvaizdavimo problemos (pabandykite atnaujinti puslapį)
