# Live Vertaling Dashboard

Het Live Translation Dashboard is een admin pagina die real-time zichtbaarheid biedt in de automatische vertaalpijplijn. Het verbindt met de SignalR hub en toont alle pijpleiding gebeurtenissen als ze optreden.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Kenmerken

### Real-time activiteitsstream

Alle signalen R gebeurtenissen uit de vertaalpijplijn worden weergegeven in een live-updating tabel:

- **Sequence number**
- **Timestamp** Lokale tijd toen de gebeurtenis werd ontvangen
- **Preview ID**
- **Stage** "Twee" podiumbadge (CheckServers, TranslateCountries, etc.)
- **Type** Berichttype badge (StageStart, Voortgang, StadiumVoltooid, enz.)
- **Berichten**
- **Details**

### Kleurcodering

Kleur
|-------|---------|
Blauwe ()
Groen ()
Rood ()
Wit (standaard)

### Verbindingsstatus

Een status banner bovenaan toont:
- **Connecting**
- **Connected**
- **Reconnecting** Verbinding verloren, poging om opnieuw verbinding te maken
- **Verbinding verbroken**

De verbinding maakt gebruik van automatische herverbinding met exponentiële backoff: 0s, 2s, 5s, 10s, 30s.

### Controles

- **Clear Feed** Verwijdert alle weergegeven berichten en resetten de teller
- **Export JSON** Downloads alle ontvangen berichten als JSON-bestand voor analyse
- **Message teller**

## Signaal R-hub

Het dashboard verbindt met:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Berichtcontract

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

### Gebeurtenistypen

Het dashboard behandelt alle waarden:

Type
|------|---------|
Blauwe badge
Groene badge
Rode badge
Groene badge
Rode badge
Info-badge
Waarschuwingsbadge

## Technische uitvoering

### Backend

- **Locatie Hub** ()
- **ISignalRpublisher**
- **SignalRpublisher**

### Frontend

- Pure HTML/JS met Bootstrap 5 styling
- Gebruikt de Microsoft SignalR JavaScript-clientbibliotheek (geladen vanuit CDN)
- Geen server-side rendering vereist voor de eventfeed

### Paginastructuur

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Gebruik tijdens de ontwikkeling

1. Start de Dita. Servertoepassing
2. Navigeren naar
3. Trigger een vertaling uitvoeren (wacht op de planner of bel de API)
4. Gebeurtenissen in realtime bekijken
5. Gebruik de knop Exporteren om een volledig spoor voor debuggen vast te leggen

## Toekomstige verbeteringen

Geplande verbeteringen voor het dashboard:

- **Authenticatie**
- **Filtering**
- **Historische runs** Beeld voltooid draait vanuit een database of logbestand
- **Statistisch**
- **Handmatige triggers**
- **Configuratie**
- **Taalbeheer**
- **Dictionary preview**

## Problemen oplossen

### Dashboard toont "Failed to connect"

1. Controleren of de server actief en toegankelijk is
2. Controleer browserconsole voor CORS of netwerkfouten
3. Bevestig is aanwezig in
4. Zorg ervoor dat geen firewall WebSocket verbindingen blokkeert

### Gebeurtenissen verschijnen niet

1. Controleer of de SignalR-hub-URL overeenkomt met de server () en client ()
2. Controleer of de scheduler is ingeschakeld in
3. Kijk naar server logs voor vertaalpijplijn fouten
4. Browser controleren Netwerktabblad voor WebSocket-berichten

### Berichten zijn niet in orde

Het veld garandeert bestellen binnen een enkele run. Indien berichten buiten de orde verschijnen, kan dit aangeven:
- Meerdere pijpleiding loopt overlappend (moet niet gebeuren als gevolg van semafoor slot)
- Browser rendering problemen (Probeer de pagina te vernieuwen)
