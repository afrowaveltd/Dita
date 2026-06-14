# Live Oversettelse Dashboard

Live Translation Dashboard er en admin-side som gir sanntid synlighet i den automatiske oversettelsesrørledningen. Den kobler til SignalR-hub og viser alle rørledningshendelser som de oppstår.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Funksjoner

### Real-time event stream

Alle SignalR hendelser fra oversettelsesrørledningen vises i en live-updatering tabell:

- **Sekvensnummer** — Monotonisk teller innenfor hvert rørledningskjøring
- **Timestamp** — Lokal tid da hendelsen ble mottatt
- **Rør ID** — Forkortet GUID for korrelasjon
- **Stage** — Pipeline scenemerke (SjekkServere, Oversettere, etc.)
- ** Type** — Meldingstypemerket (Stagestartet, Fremskritt, StageCompleted, etc.)
- ** Melding** — Menneskelesbar beskrivelse
- ** Detaljer** — Full JSON nyttelast av hendelsesdata

### Fargekoding

Farge
|-------|---------|
Blå ()
Grønn ()
Rød ()
Hvit (standard)

### Tilkoblingsstatus

Et status banner øverst viser:
- **Connecting** — Etablering av signalR-tilkobling
- **Connected** — Mottak av hendelser normalt
- **Tilkobling** — Forbindelse tapt, forsøker å koble til på nytt
- **Disconnected** — Connection closed

Tilkoblingen bruker automatisk reconnect with exponential backoff: 0s, 2s, 5s, 10s, 30s.

### Kontroller

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Eksporter JSON** — Nedlastinger alle mottatte meldinger som en JSON-fil for analyse
- ** Melding teller** — Viser totalt antall hendelser mottatt i denne sesjonen

## signaler hub

Dashboard kobler til:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Meldingskontrakt

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

### hendelsestyper

Dashboard håndterer alle verdier:

Type
|------|---------|
Blått merket
Grønn merke
Rødt merket
Grønn merke
Rødt merket
Infomerke
Advarselsmerke

## Teknisk implementering

### Motor

- **LocalizationHub** () — SignalR nav som sender meldinger til alle tilkoblede klienter
- **ISignalRPublicer** — Abstraksjon over navet for bruk i oversettelsestjenester
- **SignalRPublisher** — Standard implementering som øker en monoton sekvens og sendinger

### Frontend

- Ren HTML/JS med Bootstrap 5 styling
- Bruker Microsoft SignalR JavaScript-klientbiblioteket (lastet fra CDN)
- Ingen serversiden gjengivelse nødvendig for hendelsesfeed

### Sidestruktur

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Bruk under utvikling

1. Start Dita. Serverprogram
2. Gå til
3. Utløse en oversettelseskjøring (enten vente på planleggeren eller ring API)
4. Se hendelser vises i sanntid
5. Bruk Eksporter-knappen til å fange opp et fullstendig spor for feilsøking

## Fremtidige forbedringer

Planlagte forbedringer for dashboard:

- **Autentisering** — Begrense tilgangen til brukere med rollen
- ** Filtrering** — Filtrer hendelser etter trinn, type eller kjøre ID
- **Historiske kjører** — Vis fullførte kjører fra en database eller loggfil
- **Statistics** — Kart som viser antall oversettelser, feilpriser og latens over tid
- **Manuelt utløser** — Knapper for å manuelt starte bestemte rørledningsstadier
- ** Konfigurasjon** — Rediger direkte fra instrumentbordet
- ** Språkhåndtering** — Vis og rediger støttede språk
- **Dokumentær forhåndsvisning** — Bla gjennom og søk i lokale ordbøker

## Feilsøking

### Dashboard-visninger "Failed to connect"

1. Bekreft at serveren kjører og er tilgjengelig
2. Sjekk nettleserkonsollen for CORS eller nettverksfeil
3. Bekreftelse er tilstede i
4. Sørg for at ingen brannmur blokkerer WebSocket-tilkoblinger

### Hendelser vises ikke

1. Sjekk at URL-adressen til signalR samsvarer mellom server () og klient ()
2. Kontroller at planleggeren er aktivert i
3. Se på serverlogger for oversettelsesrørledningsfeil
4. Sjekk nettlesernettverksfanen for WebSocket-meldinger

### Meldingene er ute av orden

Feltet garanterer bestilling i et enkelt løp. Hvis meldinger vises ut av orden, kan det indikere:
- Flere rørledninger kjører overlappende (bør ikke skje på grunn av semaforlås)
- Nettlesergjengivelse problemer (prøv forfriskende siden)
