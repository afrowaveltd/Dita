# páxina de tradución ao vivo

O Live Translation Dashboard é unha páxina de administración que proporciona visibilidade en tempo real ao proceso de tradución automática. Conecta co centro de SignalR e amosa todos os eventos de oleodutos a medida que ocorren.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Características

### Evento en tempo real stream

Todos os eventos SignalR do oleoduto de tradución móstranse nunha táboa de actualización en vivo:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** – GUID Acurtado para correlación
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- ** Mensaxe** - descrición lexible por humanos
- **Detalles**: Carga completa dos datos do evento

### Color codificación

Color
|-------|---------|
Azul)
Verde ()
Rojo ()
Branco (default)

### Estado de conexión

Unha bandeira de estado nos principais espectáculos:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

A conexión utiliza unha reconecta automática con backup exponencial: 0s, 2s, 5s, 10s, 30s.

### Control

- **Clear Feed** Elimina todas as mensaxes amosadas e restablece o contador
- **Exportar JSON** Descarga todas as mensaxes recibidas como un ficheiro JSON para a súa análise
- **Message counter** — Shows total number of events received in this session

## sinal hub

O panel conecta con:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Contrato de mensaxería

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

### Tipos de eventos

O taboleiro manexa todos os valores:

Tipo
|------|---------|
Azul insignia
Green insignia
Red badge
Green insignia
Red badge
Info insignia
Avisos distintivos

## Aplicación técnica

### Backend

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher**: Resumo sobre o hub para uso en servizos de tradución
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Frontend

- HTML/JS con Bootstrap 5 estilo
- Utiliza a biblioteca cliente de Microsoft SignalR (cargada de CDN)
- Non se require a representación do lado do servidor para o feed do evento

### Estrutura da páxina

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Uso durante o desenvolvemento

1. Inicio » Dita Aplicación do servidor
2. Navegar para
3. Corrixir unha tradución (xa sexa para o programador ou chamar a API)
4. Os eventos aparecen en tempo real
5. Use o botón Export para capturar unha traza completa para depurar

## Melloras futuras

Melloras previstas para o panel:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** - Filtrar eventos por etapa, tipo ou executar ID
- **Historical runs** – Ver completo execucións desde unha base de datos ou ficheiro de rexistro
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- ** Administración de idiomas** - Ver e editar idiomas soportados
- **Previsualización dixital** – Navegar e buscar dicionarios de localización

## Resolución de problemas

### Dashboard: 'Non conectado'

1. Comproba que o servidor está en funcionamento e
2. Comprobe a consola do navegador para CORS ou erros de rede
3. Confirmado está presente en
4. Ningún firewall está a bloquear as conexións de WebSocket

### Os acontecementos non aparecen

1. Comproba que a URL do hub de SignalR coincida entre o servidor () e o cliente ()
2. Comproba se o programa está activado
3. Vexa os rexistros do servidor para erros no pipeline de tradución
4. Consulte a pestana da Rede do navegador para as mensaxes de WebSocket

### As mensaxes están fóra de orde

O campo garante ordenar dentro dunha única carreira. Se aparecen mensaxes fóra de orde, pode indicar:
- O gasoduto multiplícase (non debería ocorrer debido ao bloqueo do semafóre)
- Problemas de representación do navegador (refrixerar a páxina)
