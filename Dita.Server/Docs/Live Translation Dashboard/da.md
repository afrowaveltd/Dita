# Live oversættelse Dashboard

Live Oversættelse Dashboard er en admin side, der giver realtid synlighed i den automatiske oversættelse rørledning. Det forbinder til SignalR hub og viser alle rørledninger begivenheder, som de opstår.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Funktioner

### Realtidshændelsesstrøm

Alle signalR begivenheder fra oversættelsesledningen vises i en live- updating tabel:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Farvekode

Farve
|-------|---------|
Blå ()
Grøn ()
Rød ()
Hvid (standard)

### Tilslutningsstatus

Et statusbanner øverst viser:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

Forbindelsen bruger automatisk genforbindelse med eksponentiel backoff: halvfems, 2s, 5s, 10s, 30s.

### Kontrol

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## SignalR-hub

Instrumentbrættet forbinder til:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Meddelelseskontrakt

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

### Begivenhedstyper

Dashboardet håndterer alle værdier:

Type
|------|---------|
Blå skilt
Grønt skilt
Rødt skilt
Grønt skilt
Rødt skilt
Info-skilt
Advarselsskilt

## Teknisk gennemførelse

### motor

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Frontend

- Pure HTML / JS med Bootstrap 5 styling
- Bruger Microsoft SignalR JavaScript- klientbiblioteket (indlæst fra CDN)
- Ingen server- side rendering kræves for begivenheden foder

### Sidestruktur

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Anvendelse under udvikling

1. Start Ditaen. Serverprogram
2. Naviger til
3. Trigger en oversættelse køre (enten vente på scheduler eller ring til API)
4. Se begivenheder vises i realtid
5. Brug knappen Eksportér for at fange et fuldt spor for fejlsøgning

## Fremtidige forbedringer

Planlagte forbedringer til instrumentbrættet:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Fejlfinding

### Dashboard viser "Mislykkedes at forbinde"

1. Verificér at serveren kører og er tilgængelig
2. Tjek browserkonsol for CORS eller netværkssvigt
3. Bekræft er til stede i
4. Sørg for ingen firewall blokerer WebSocket forbindelser

### Begivenheder vises ikke

1. Kontroller at signalR hubs URL matcher mellem server () og klient ()
2. Verificér scheduler er aktiveret i
3. Kig på serverlogfiler for oversættelsesrørledningsfejl
4. Tjek browsernetværkets faneblad for WebSocket-beskeder

### Meddelelser er ude af drift

Feltet garanterer bestilling inden for et enkelt løb. Hvis meddelelser forekommer i uorden, kan de angive:
- Flere rørledninger kører overlappende (bør ikke ske på grund af semafore lås)
- Browser rendering problemer (prøv forfriskende siden)
