# Live Translation Dashboard

The Live Translation Dashboard is an admin page that provides real-time visibility into the automatic translation pipeline. It connects to the SignalR hub and displays all pipeline events as they occur.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Features

### Real-time event stream

All SignalR events from the translation pipeline are displayed in a live-updating table:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Color coding

| Color | Meaning |
|-------|---------|
| Blue (`table-primary`) | Stage started |
| Green (`table-success`) | Stage completed or pipeline completed |
| Red (`table-danger`) | Error condition |
| White (default) | Informational progress |

### Connection status

A status banner at the top shows:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

The connection uses automatic reconnect with exponential backoff: 0s, 2s, 5s, 10s, 30s.

### Controls

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## SignalR hub

The dashboard connects to:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Message contract

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

### Event types

The dashboard handles all `LocalizationMessageType` values:

| Type | Display |
|------|---------|
| `StageStarted` | Blue badge |
| `StageCompleted` | Green badge |
| `StageFailed` | Red badge |
| `PipelineCompleted` | Green badge |
| `PipelineFailed` | Red badge |
| `Progress` | Info badge |
| `Warning` | Warning badge |

## Technical implementation

### Backend

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Frontend

- Pure HTML/JS with Bootstrap 5 styling
- Uses the Microsoft SignalR JavaScript client library (loaded from CDN)
- No server-side rendering required for the event feed

### Page structure

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Usage during development

1. Start the Dita.Server application
2. Navigate to `/Admin/LiveTranslation`
3. Trigger a translation run (either wait for the scheduler or call the API)
4. Watch events appear in real time
5. Use the Export button to capture a full trace for debugging

## Future enhancements

Planned improvements for the dashboard:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Troubleshooting

### Dashboard shows "Failed to connect"

1. Verify the server is running and accessible
2. Check browser console for CORS or network errors
3. Confirm `app.MapHub<LocalizationHub>("/hubs/localization")` is present in `Program.cs`
4. Ensure no firewall is blocking WebSocket connections

### Events are not appearing

1. Check that the SignalR hub URL matches between server (`Program.cs`) and client (`LiveTranslation.cshtml`)
2. Verify the scheduler is enabled in `appsettings.json`
3. Look at server logs for translation pipeline errors
4. Check browser Network tab for WebSocket messages

### Messages are out of order

The `Sequence` field guarantees ordering within a single run. If messages appear out of order, it may indicate:
- Multiple pipeline runs overlapping (should not happen due to semaphore lock)
- Browser rendering issues (try refreshing the page)
