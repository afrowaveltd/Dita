# dashboard tłumaczenie na żywo

Live Translation Dashboard to strona administracyjna, która zapewnia real- time widoczność do automatycznego rurociągu tłumaczeniowego. Łączy się z węzłem SignalR i wyświetla wszystkie zdarzenia związane z rurociągiem.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Cechy

### Strumień zdarzeń w czasie rzeczywistym

Wszystkie zdarzenia SignalR z rurociągu tłumaczeniowego są wyświetlane w tabeli aktualizacji żywej:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Kodowanie kolorów

Kolor
|-------|---------|
Niebieski ()
Zielony ()
Czerwony ()
Biały (domyślnie)

### Status połączenia

Baner stanu na górze pokazuje:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

Połączenie wykorzystuje automatyczne ponowne połączenie z backupem wykładniczym: 0s, 2s, 5s, 10s, 30s.

### Kontrole

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## Głowica sygnalizacyjna

Deska rozdzielcza łączy się z:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Umowa na przesłanie

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

### Typy zdarzeń

Deska rozdzielcza obsługuje wszystkie wartości:

Typ
|------|---------|
Niebieska odznaka
Zielona odznaka
Czerwona odznaka
Zielona odznaka
Czerwona odznaka
Oznaczenie informacyjne
Odznaka ostrzegawcza

## Realizacja techniczna

### Przycisk

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Frontend

- Czysty HTML / JS z Bootstrap 5 styling
- Korzysta z biblioteki klienta Microsoft SignalR JavaScript (załadowanej z CDN)
- Nie wymaga się renderowania po stronie serwera dla kanału zdarzeń

### Struktura strony

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Wykorzystanie podczas rozwoju

1. Uruchom Ditę. Aplikacja serwera
2. Przejdź do
3. Uruchomić tłumaczenie (albo czekać na terminarza lub zadzwonić API)
4. Oglądaj wydarzenia pojawiają się w czasie rzeczywistym
5. Użyj przycisku Eksportuj, aby uchwycić pełny ślad dla debugowania

## Przyszłe udoskonalenia

Planowane ulepszenia deski rozdzielczej:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Rozwiązywanie problemów

### Deska rozdzielcza pokazuje "Nie powiodło się połączenie"

1. Weryfikacja serwera jest uruchomiona i dostępna
2. Sprawdź konsolę przeglądarki w poszukiwaniu błędów CORS lub sieci
3. Potwierdź obecność w
4. Upewnij się, że żadna zapora nie blokuje połączeń WebSocket

### Zdarzenia nie pojawiają się

1. Sprawdź, czy adres URL węzła SignalR pasuje do serwera () i klienta ()
2. Weryfikacja terminarza jest włączona w
3. Spójrz na logi serwerów dla błędów rurociągu tłumaczenia
4. Sprawdź zakładkę Sieć przeglądarki wiadomości WebSocket

### Wiadomości nie są w porządku

Pole gwarantuje zamawianie w jednym biegu. Jeżeli wiadomości nie są w porządku, mogą one wskazywać:
- Wiele rurociągów działa nakładanie (nie powinno się zdarzyć z powodu blokady semafora)
- Problemy z renderowaniem przeglądarki (spróbuj odświeżyć stronę)
