# përkthim i drejtpërdrejtë

The Live Translation Dashboard është një faqe admin që siguron shikim real në tubacionin automatik të përkthimit. Ajo lidhet me shpërndarësin e sinjalit dhe shfaq të gjitha ngjarjet e tubacionit ndërsa ato ndodhin.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Veçoritë

### Korrispondenti i eventit kur arrijnë mesazhe

Të gjitha ngjarjet Sinjal R nga tubacioni i përkthimit janë shfaqur në një tabelë të mbivendosur:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Ngjyra

Ngjyra
|-------|---------|
Blu ()
E gjelbër ()
e kuqe ()
E bardhë

### Gjendja e lidhjes

Një parullë statusi në shfaqjet kryesore:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

Lidhja përdor rilidhje automatike me mbështetje eksponenciale: 0s, 2s, 5s, 10s, 30s.

### Kontrollet

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## Qendër sinjali

Dyshemeja lidhet me:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Mesazhi

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

### Ndodhi

Tabela menazhon të gjitha vlerat:

Lloji
|------|---------|
Distinktivi blu
Distinktivi i gjelbër
Distinktivi i kuq
Distinktivi i gjelbër
Distinktivi i kuq
Informacione
Distinktivi i paralajmërimit

## Zbatimi teknik

### Mbrapa

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Frontend

- Pure HTML/JS me Bootstrap 5 styling
- Microsoft Sinjal JavaScript Library (nga CDN)
- Nuk është i nevojshëm përkthimi në server për mesazhin

### Struktura e faqes

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Përdorimi gjatë zhvillimit

1. Fillo me Ditën. Aplikativi i serverit
2. Navigate për
3. A
4. Shiko ngjarjet që shfaqen në kohë reale
5. Eksporto a për

## Përmirësime të ardhshme

Përmirësimet e planifikuara për tabelën:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Tavoleta

### Dashboard tregon "Gënje për t'u lidhur"

1. Kontrollo që serveri është në ekzekutim dhe është i arritshëm
2. Kontrollo konsolën e shfletuesit për gabimet e CORS apo të rrjetit
3. Konfirmo është i pranishëm
4. Siguria jo është

### Ngjarjet nuk janë shfaqur

1. Kontrolli URL server dhe klient ()
2. Kontrolli është në
3. Shiko në server për gabim
4. Kontrollo skedën e rrjetit të shfletuesit për mesazhet në Internet

### Mesazhet janë jashtë renditjes

Fusha garanton urdhërimin brenda një vrapimi të vetëm. Nëse mesazhet nuk janë në rregull, ai mund të tregojë:
- Tubacioni i shumtë kalon (nuk duhet të ndodhë për shkak të lockit sermafore)
- Shfletuesi
