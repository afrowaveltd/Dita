# Traduko de Dashboard

La Live Translation Dashboard estas admin paĝo kiu disponigas realtempan videblecon en la aŭtomatan tradukon dukto. Ĝi ligas al la SignalR-nabo kaj montras ĉiujn duktokazaĵojn kiam ili okazas.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Plendoj

### Realtempa okazaĵrivereto

Ĉiuj SignalR-okazaĵoj de la traduko dukto estas elmontritaj en vigla tablo:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Koloro

Koloro
|-------|---------|
Blua ()
Verda ()
Ruĝa ()
Blanka (defaŭlto)

### Ligostatuso

Statusstandardo ĉe la pintekspozicioj:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

La ligo uzas aŭtomatan religon kun eksponenta dorsflanko: 0s, 2s, 5s, 10s, 30'oj.

### Kontroloj

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## SignalR

La dashboard ligas al:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Mesaĝo kontrakto

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

### Okazaĵo tipoj

La dashboard pritraktas ĉiujn valorojn:

Tipo
|------|---------|
Blua insigno
Verda insigno
Ruĝa insigno
Verda insigno
Ruĝa insigno
Info insigno
Averto insigno

## Teknika efektivigo

### Malantaŭa

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Fronto

- Pura HTML/JS kun Bootstrap 5 titolado
- Uzu la Microsoft SignalR JavaScript kliento biblioteko (ŝarĝita de CDN)
- Neniu servil-flanka interpreto necesa por la okazaĵo furaĝo

### Paĝostrukturo

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Uzo dum evoluo

1. Komencu la Dita. Servilo
2. Navigi al
3. Trigger traduko kuras (aŭ atendas la horaron aŭ vokas la API)
4. La okazaĵoj aperas en reala tempo
5. Uzu la Eksportbutonon por kapti plenan spuron por malkonstruado

## Estontaj pliboniĝoj

Planitaj plibonigoj por la dashboard:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Problemoj

### Dashboard montras "Failed ligi"

1. La servilo kuras kaj alirebla
2. Kontrolu retumilo konzolo por CORS aŭ reto eraroj
3. Konfirmo ĉeestas en
4. Certigi neniun fajromuron blokas WebSocket-ligojn

### La okazaĵoj ne aperas

1. Kontrolu ke la SignalR nabo URL matĉoj inter servilo () kaj kliento ()
2. Verify la plandisto estas ebligita en
3. Vidu servilregistrojn por traduko dukto eraroj
4. Kontrolu retumilo Network-klapeto por WebSocket mesaĝoj

### Mesaĝoj estas el ordo

La kampo garantias ordigi ene de ununura kuro. Se mesaĝoj aperas el ordo, ĝi povas indiki:
- Multobla dukto kuras imbrikita (ne devus okazi pro semaforokluzo)
- Browser iganta temojn (try refreŝigante la paĝon)
