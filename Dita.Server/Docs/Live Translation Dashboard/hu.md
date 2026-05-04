# Élő fordítás műszerfal

A Live Translation Dashboard egy admin oldal, amely valós idejű láthatóságot biztosít az automatikus fordítási csővezetékben. Kapcsolódik a SignalR csomóponthoz, és megjeleníti az összes csővezetékes eseményt, ahogy azok bekövetkeznek.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Jellemzők

### Real-time eseménysorozat

Minden jel A fordítóvezetékből származó R események egy élő-frissítő táblázatban jelennek meg:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Színkód

Szín
|-------|---------|
Kék ()
Zöld ()
Piros ()
Fehér (alapértelmezett)

### Csatlakozási állapot

A status banner a tetején mutatja:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

A kapcsolat automatikus visszacsatolást használ exponenciális visszacsatolással: 80, 2, 5, 10, 30.

### Ellenőrzések

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## Jelzés R csomópont

A műszerfal a következőkhöz kapcsolódik:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Üzenetszerződés

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

### Eseménytípusok

A műszerfal a következő értékeket kezeli:

Típus
|------|---------|
Kék jelvény
Zöld jelvény
Vörös jelvény
Zöld jelvény
Vörös jelvény
Info jelvény
Figyelmeztető jelvény

## Technikai végrehajtás

### Háttér

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Frontend

- Tiszta HTML / JS Bootstrap 5 stílusban
- A Microsoft SignalR JavaScript kliens könyvtárát használja (a CDN-ből betöltve)
- Az eseményhez nem szükséges szerveroldali renderelés

### Oldalszerkezet

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Felhasználás fejlesztés közben

1. Indítsd a Dita-t. Szerver alkalmazás
2. Naiv
3. A fordítási folyamat kiírása (vagy várni a menetrend, vagy hívja az API)
4. Az események valós időben jelennek meg
5. Használja az Export gombot, hogy rögzítse a teljes nyom hibakeresés

## Jövőbeli fejlesztések

Tervezett javítások a műszerfalon:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Problémamegoldás

### A műszerfal mutatja, hogy "nem sikerült csatlakozni"

1. Ellenőrizze, hogy a szerver fut és elérhető-e
2. A böngésző konzol ellenőrzése CORS vagy hálózati hibák esetén
3. Megerősítem, hogy
4. Győződjön meg róla, hogy nincs tűzfal blokkolja WebSocket kapcsolatok

### Az események nem jelennek meg

1. Ellenőrizze, hogy a SignalR hub URL illeszkedik-e a szerver () és az ügyfél () között
2. Ellenőrizze, hogy az ütemező be van-e kapcsolva
3. Nézze meg a szervernaplók fordítási csővezeték hibák
4. A böngésző ellenőrzése Hálózati lap WebSocket üzenetekhez

### Az üzenetek nem működnek

A mező garantálja a rendelést egyetlen futáson belül. Ha az üzenetek nem megfelelően jelennek meg, jelezheti:
- Többszörös csővezeték egymást átfedő (nem történhet meg a szemaforos zár miatt)
- Browser rendering problémák (próbálja frissíteni az oldalt)
