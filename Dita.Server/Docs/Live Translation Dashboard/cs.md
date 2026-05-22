# Živý překlad Přístrojová deska

Přístrojová deska Live Translation je admin stránka, která zajišťuje viditelnost v reálném čase do automatického překladu potrubí. Připojí se k náboji SignalR a zobrazí všechny události plynovodu, jak k nim dochází.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Vlastnosti

### Real- time event stream

Všechny události SignalR z překladatelského potrubí jsou zobrazeny v tabulce s živou aktualizací:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Kódování barev

Barva
|-------|---------|
Modrá ()
Zelená ()
Červená ()
Bílá (výchozí)

### Stav připojení

Status banner na vrcholu ukazuje:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

Spojení využívá automatické spojení s exponenciálním zpětným pásmem: 0s, 2s, 5s, 10s, 30s.

### Kontroly

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## Uzel SignalR

Přístrojová deska se připojí k:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Smlouva o zprávě

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

### Typy událostí

Přístrojová deska zpracovává všechny hodnoty:

Typ
|------|---------|
Modrý odznak
Zelený odznak
Červený odznak
Zelený odznak
Červený odznak
Info odznak
Výstražný odznak

## Technické provádění

### Backend

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Hranice

- Čisté HTML / JS s Bootstrap 5 styling
- Používá knihovnu klienta Microsoft SignalR JavaScript (načteno z CDN)
- Žádné serverboční vykreslování nutné pro událost krmiva

### Struktura stránky

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Použití během vývoje

1. Začněte s Ditou. Aplikace serveru
2. Přejít na
3. Spustit překlad běh (buď čekat na plánovač nebo volat API)
4. Sledovat události se objeví v reálném čase
5. Pomocí tlačítka Export můžete zachytit celou stopu pro ladění

## Budoucí zlepšení

Plánované zlepšení palubní desky:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Řešení problémů

### Přístrojová deska ukazuje "Selhalo připojení"

1. Ověřte, zda server běží a je přístupný
2. Zkontrolujte konzoli prohlížeče pro chyby CORS nebo sítě
3. Potvrzení je přítomen v
4. Zajistěte, aby firewall neblokoval připojení WebSocket

### Události se neobjevují

1. Zkontrolujte, zda URL URL rozhraní SignalR odpovídá serveru () a klientovi ()
2. Ověřte, zda je zapnutý plánovač v
3. Podívejte se na protokoly serverů pro chyby v překladu potrubí
4. Zkontrolujte kartu Síť prohlížeče pro zprávy WebSocket

### Zprávy jsou mimo provoz

Pole zaručuje pořadí v rámci jednoho běhu. Pokud se zprávy objeví mimo provoz, může být uvedeno:
- Vícenásobné překrývání potrubí (nemělo by k němu dojít kvůli semaforu)
- Prohlížeč renderování problémy (zkuste osvěžení stránky)
