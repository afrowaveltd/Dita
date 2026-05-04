# Live Translation Dashboard

Live Translation Dashboard är en admin sida som ger realtidssynlighet i den automatiska översättningsledningen. Den ansluter till SignalR-navet och visar alla pipelinehändelser när de inträffar.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Funktioner

### Real-time händelse stream

Alla signaler R-händelser från översättningsledningen visas i en live-updating-tabell:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** – Lokal tid då evenemanget mottogs
- **Run ID** — Shortened GUID for correlation
- ** Steg ** - Pipeline scenmärke (CheckServers, TranslateCountries, etc.)
- **Type ** - Meddelande typ märke (StageStarted, Progress, StageCompleted, etc.)
- **Meddelande** – Mänsklig läsbar beskrivning
- ** Detaljer** – Full JSON-belastning av händelsedata

### Färgkodning

Färgfärg
|-------|---------|
Blå ()
Grönt ()
Röd ()
Vit (standard)

### Anslutningsstatus

En statusbanner på toppen visar:
- **Connecting** - Etablering SignalR-anslutning
- **Connected** – Att ta emot händelser normalt
- ** Återanslutning** - Anslutning förlorad, försöker återansluta
- **Disconnected** – Anslutning stängd

Anslutningen använder automatisk återanslutning med exponentiell backoff: 0s, 2s, 5s, 10s, 30s.

### Kontroller

- **Clear Feed** - Ta bort alla visade meddelanden och återställer disken
- **Export JSON** – Ladda ner alla mottagna meddelanden som en JSON-fil för analys
- **Message counter** - Visar totalt antal händelser som mottagits under denna session

## Signal R Hub

Dashboard ansluter till:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Meddelandeavtal

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

### Eventtyper

Dashboarden hanterar alla värden:

Typ
|------|---------|
blå märke
Grönt märke
Red badge
Grönt märke
Red badge
Info Badge
Varning badge

## Tekniskt genomförande

### Backend

- **Lokalisering Hub ** () - SignalR-nav som sänder meddelanden till alla anslutna kunder
- **ISignalRPublisher** – Abstraktion över navet för användning i översättningstjänster
- **SignalRPublisher** - Standard implementering som inkrementerar en monoton sekvens och sändningar

### Frontend

- Ren HTML/JS med Bootstrap 5 styling
- Använder Microsoft SignalR JavaScript-klientbiblioteket (laddat från CDN)
- Ingen server-side rendering krävs för händelsen foder

### Sidstruktur

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Användning under utveckling

1. Börja dita. Server-program
2. Navigera till
3. Utlösa en översättning kör (antingen vänta på schemaläggaren eller ring API)
4. Titta på händelser visas i realtid
5. Använd Export-knappen för att fånga ett fullständigt spår för felsökning

## Framtida förbättringar

Planerade förbättringar för instrumentbrädan:

- **Autentisering** – Begränsa åtkomsten till användare med rollen
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistik** - Diagram som visar översättningsräkningar, felfrekvenser och latens över tiden
- **Manuella triggers** - Knappar för att manuellt starta specifika pipelinesteg
- **Konfiguration** - Redigera direkt från instrumentbrädan
- **Language management** – Visa och redigera stödda språk
- **Dictionary preview** — Browse and search localization dictionaries

## Felsökning

### Dashboard visar "Failed to connect"

1. Kontrollera att servern körs och är tillgänglig
2. Kontrollera webbläsarkonsol för CORS eller nätverksfel
3. Bekräftelse är närvarande i
4. Se till att ingen brandvägg blockerar WebSocket-anslutningar

### Händelser visas inte

1. Kontrollera att SignalR-nav URL matchar mellan server () och klient ()
2. Verifiera schemaläggaren är aktiverad i
3. Titta på serverloggar för översättningspipeline fel
4. Kontrollera webbläsare Nätverksflik för WebSocket-meddelanden

### Meddelanden är ur order

Fältet garanterar beställning inom en enda körning. Om meddelanden visas ur ordning kan det ange:
- Flera pipeline körs överlappande (ska inte hända på grund av semaphore lås)
- Webbläsare rendering problem (försök uppdatera sidan)
