# Live-Übersetzung Dashboard

Das Live Translation Dashboard ist eine Admin-Seite, die Echtzeit-Übersicht in die automatische Übersetzungspipeline bietet. Es verbindet sich mit der SignalR-Hub und zeigt alle Pipeline-Ereignisse, wie sie auftreten.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Eigenschaften

### Echtzeit-Ereignisstrom

Alle SignalR-Ereignisse aus der Übersetzungspipeline werden in einer Live-Updating-Tabelle angezeigt:

- **Sequenznummer** — Monotonzähler innerhalb jeder Pipeline
- **Timestamp** — Ortszeit, zu dem die Veranstaltung empfangen wurde
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline Stage Badge (CheckServer, TranslateCountries, etc.)
- **Typ** — Nachrichtentypabzeichen (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human lesbare Beschreibung
- **Details** — Full JSON payload of the event data

### Farbcodierung

Farbe
|-------|---------|
Blau ()
Grün ()
Rot ()
Weiß (Standard)

### Verbindungsstatus

Ein Statusbanner an der Spitze zeigt:
- **Connecting** — Aufbau von SignalR-Verbindung
- **Connected** — Veranstaltungen in der Regel
- **Reconnecting** — Verbindung verloren, versucht, wieder zu verbinden
- **Disconnected** — Verbindung geschlossen

Die Verbindung verwendet eine automatische Wiederverbindung mit exponentiellem Backoff: 0s, 2s, 5s, 10s, 30s.

### Kontrolle

- **Clear Feed** — Entfernt alle angezeigten Nachrichten und setzt den Zähler zurück
- **Export JSON** — Downloads aller empfangenen Nachrichten als JSON-Datei zur Analyse
- **Messagezähler** — Zeigt die Gesamtzahl der in dieser Sitzung empfangenen Veranstaltungen

## SignalR Nabe

Das Armaturenbrett verbindet:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Nachrichtenübermittlung

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

### Art der Veranstaltung

Das Dashboard behandelt alle Werte:

Art
|------|---------|
Blaue Abzeichen
Grüne Abzeichen
Rote Abzeichen
Grüne Abzeichen
Rote Abzeichen
Info Badge
Warnzeichen

## Technische Umsetzung

### zurück

- **LocalizationHub** () — SignalR-Hub, der Nachrichten an alle angeschlossenen Kunden sendet
- **ISignalRPublisher** — Zusammenfassung über den Hub für den Einsatz in Übersetzungsdiensten
- **SignalRPublisher** — Standard-Implementierung, die eine monotone Sequenz inkrementiert und sendet

### vorderteil

- Pure HTML/JS mit Bootstrap 5 Styling
- Verwenden Sie die Microsoft SignalR JavaScript Client-Bibliothek (aus CDN geladen)
- Kein serverseitiges Rendern für den Event-Feed erforderlich

### Struktur

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Nutzung während der Entwicklung

1. Starten Sie die Dita. Serveranwendung
2. Navigieren
3. Auslösen Sie einen Übersetzungslauf (entweder warten Sie auf den Scheduler oder rufen Sie die API an)
4. Sehen Sie Ereignisse in Echtzeit
5. Verwenden Sie die Schaltfläche Export, um eine vollständige Spur für Debugging zu erfassen

## Zukunftsverbesserungen

Geplante Verbesserungen für das Dashboard:

- **Authentication** — Beschränkung des Zugangs zu Benutzern mit der Rolle
- **Filterung** — Filtern von Ereignissen nach Stufe, Typ oder ID
- **Historical runs** — View completed runs from a database or log file
- **Statistik** — Diagramme mit Übersetzungszählungen, Fehlerquoten und Latenz im Laufe der Zeit
- **Manuelle Trigger** — Buttons zum manuellen Start bestimmter Pipeline-Stufen
- **Konfiguration** — Direkt aus dem Dashboard bearbeiten
- **Language management** — Unterstützte Sprachen ansehen und bearbeiten
- **Dictionary Vorschau** — Lokalisierungswörter durchsuchen und suchen

## Fehlerbehebung

### Dashboard zeigt "Failed to connect"

1. Überprüfen Sie, ob der Server läuft und zugänglich ist
2. Überprüfen Sie die Browser-Konsole für CORS oder Netzwerkfehler
3. Confirm ist in
4. Stellen Sie sicher, dass keine Firewall WebSocket-Verbindungen blockiert

### Veranstaltungen erscheinen nicht

1. Überprüfen Sie, ob die SignalR-Hub-URL zwischen Server () und Client () übereinstimmt
2. Überprüfen Sie den Scheduler aktivieren
3. Schauen Sie sich Server-Logs für Translation Pipeline-Fehler an
4. Browser-Netzwerk-Tab für WebSocket-Nachrichten überprüfen

### Nachrichten sind aus dem Auftrag

Das Feld garantiert die Bestellung innerhalb eines einzigen Laufs. Wenn Nachrichten aus der Bestellung erscheinen, kann es Folgendes angeben:
- Mehrere Pipeline läuft überlappend (sollte nicht durch Semaphore Schloss geschehen)
- Browser-Rendering-Probleme (die Seite aktualisieren)
