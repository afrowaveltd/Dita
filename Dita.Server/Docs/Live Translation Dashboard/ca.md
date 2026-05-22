# Tauler de traducció en directe

El tauler de traducció en directe és una pàgina d' administrador que proporciona visibilitat en temps real a la canonada de traducció automàtica. Connecta a l'eix de senyals i mostra tots els esdeveniments de canonada tal com ocorren.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Característiques

### Flux d' esdeveniments en temps real

Tots els esdeveniments senyalR de la canonada de traducció es mostren en una taula d' obertura en directe:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Strage ** Pipha Pipe Pipeline badge (Comproveu servidors, Tradueix comtats, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- ** Detalles ** Full JSON paga la càrrega de les dades de l' esdeveniment

### Codificació del color

Color
|-------|---------|
Blue ()
verd ()
Vermell ()
Blanc (per defecte)

### Estat de la connexió

Un indicador d' estat a la part superior mostra:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- S' ha perdut la connexió **Reconnexió **Comment
- **Disconnected** — Connection closed

La connexió usa la reconnexió automàtica amb l'operació exponencial: 0s, 2s, 5s, 10s, 30.

### Controls

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## Concentrador senyalR

El tauler es connecta a:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Contracte de missatge

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

### Tipus d' esdeveniment

El tauler gestiona tots els valors:

Tipus
|------|---------|
Placa blava
Placa verda
Placa vermella
Placa verda
Placa vermella
Placa d' informació
Placa d' avís

## Implementació tècnica

### Dorsal

- **LocalizationHub ** genionHyb () bELR que emet els missatges a tots els clients connectats
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Frontal

- Pur HTML/JS amb arrencada 5 stuling
- Usa la biblioteca de client JavaScript Microsoft SenyalR (carregat des de CDN)
- No es requereix cap representació del servidor per a la font d' esdeveniments

### Estructura de pàgina

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Ús durant el desenvolupament

1. Comença la Dita. Aplicació del servidor
2. Navega fins
3. Activa una execució de traducció (o espera el planificador o crida l' API)
4. Veure els esdeveniments apareixen en temps realName
5. Utilitzeu el botó Exporta per a capturar una traça completa per a la depuració

## Millores futures

Millores planificades per al tauler:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuració ** Edit directament des del tauler
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Solució de problemes

### El tauler mostra "Favat per connectar"

1. Verifica el servidor s' està executant i accessible
2. Comprova la consola del navegador pels errors de CORS o xarxa
3. Confirma l' entrada
4. Assegureu- vos que cap tallafocs està bloquejant les connexions WebSocket

### Els esdeveniments no apareixen

1. Comproveu que l' URL del senyalRG coincideix entre el servidor () i el client ()
2. Verifica el planificador està habilitat
3. Mireu els registres del servidor pels errors de canonada de traducció
4. Comprova la pestanya Xarxa del navegador pels missatges WebSocket

### Els missatges estan fora de l' ordre

El camp garanteix l'ordre d'una sola sortida. Si els missatges apareixen fora d' ordre, pot indicar:
- S' està sobreposant múltiples canonades (no hauria de passar degut al bloqueig del mapa)
- Problemes de representació del navegador (proventeix la pàgina)
