# Zusammenfassung der Änderungen des Automatischen Übersetzungsdienstes

## Überblick

Dieses Dokument fasst alle Änderungen an dem automatischen Übersetzungsdienst von Dita zusammen, einschließlich der Architektur-Refactoring, neue Features, Beobachtungsverbesserungen und Lokalisierung Verbesserungen.

## Architekturveränderungen

### Refactored BackendTranslationService

Die Monolithik wurde in vier spezialisierte Dienstleistungen zerlegt, die von einem leichten Orchestermacher koordiniert wurden:

- **BackendTranslationService** — Pipeline-Orchester (Servervalidierung, Bühnendelegation, Fehlerbehandlung)
- **CountriesTranslationService** — Ländername Synchronisation (Englisch → Zielsprache)
- **LocalizationTranslationService** — JSON Wörterbuch-Synchronisation (beigefügte/entfernte Schlüssel)
- **DokumenteTranslationService** — Markdown-Dokumentationsübersetzung mit Block-Level-Tracking
- **SignalRPublisher** — Echtzeit-Fortschrittsberichte über SignalR
- **TranslationRetryService** — Bühnenretry mit Platzhalterbewahrung

### Leistungen

- **Separation of concerns**: Each service handles a single translation domain
- **Nachhaltigkeit**: Kleinere Klassen sind leichter zu verstehen und zu testen
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Neue Features

### Live-Übersetzung Monitor

** Location**:

Eine neue Admin-Seite, die Echtzeitsicht in die Übersetzungspipeline bietet:

- Zeigt alle Signale R Ereignisse wie sie auftreten
- Farbcodierte Nachrichtentypen (blue=started, green=completed, red=error)
- Verbindungsstatus-Banner mit Auto-Reconnect
- Nachrichtenzähler und Export nach JSON

### Name der Platzhalter

Das Lokalisierungssystem unterstützt nun benannte Platzhalter () für eine verbesserte Grammatik in verschiedenen Sprachen:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Eigenschaften:
- Platzhalterwerte, die zu Laufzeit bereitgestellt oder in
- Automatische Maskierung/Restaurierung während der Übersetzung, um Korruption zu verhindern
- Rückwärtskompatibel mit bestehenden Platzhaltern

### Inkrementelle Übersetzung

Markdown-Dateien werden inkrementell übersetzt:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-Level-Tracking**: Tracks Übersetzungsstatus pro Block
- **Selective Retry**: Nur gescheiterte Blöcke werden auf dem nächsten Lauf wiederversetzt
- **Metadata Persistenz**: Übersetzungsstaat überlebt Antragsneustarts

### verbesserte retry-logik

Drei Ebenen der Widerstandsfähigkeit:

1. **HTTP retry** (LibreTranslateService): 5 Versuche mit exponentiellem Backoff (1s–5s)
2. **State Retry** (TranslationRetryService): 3 weitere Versuche mit 30s Verzögerungen
3. **Block retry** (DokumentsTranslationService): Verfehlte Markdown-Blöcke auf dem nächsten Lauf

### signalgeber melden

Echtzeit-Fortschrittsberichte für alle Pipeline-Operationen:

- Jede Bühne veröffentlicht Veranstaltungen
- Per-Sprache Fortschritt veröffentlicht als Veranstaltungen
- Fehlerereignisse beinhalten detaillierten Kontext (Quelle, Fehlercode, Nachricht)
- Sequenznummern garantieren die Bestellung in jedem Lauf

## Konfigurationsänderungen

### werbungen.json

Keine Änderungen. Die bestehende Konfiguration funktioniert weiter:

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00"
  }
}
```

### Neue Dienste

Registriert in:

- /
- `TranslationRetryService`
- /
- /
- /
- /

Das Signal R-Hub wird für Client-Verbindungen abgebildet.

## Prüfung

### Prüfstand

- **243/244 tests passieren** (1 durch gleichzeitigen zugriff auf die datei in der testumgebung übersprungen)
- Neue Testabdeckung hinzugefügt für:
  - Platzhalter Service Funktionalität
  - Zurück zur Übersicht Service Orchester
  - JsonStringLocalizer Platzhalter Indexe

### Bekannte Einschränkungen

- der Test wird beim Parallellauf übersprungen, weil mehrere Testinstanzen dieselbe Datei teilen. Es passiert, wenn es isoliert läuft.

## Neue Dateistruktur

### Dienstleistungen in

- — Pfeifenorchester
- — Ländername Übersetzung
- — JSON Wörterbuchsynchronisation
- — Übersetzung von Markdown
- — Signal R-Nachrichtenveröffentlichung
- — Wiederhollogik mit Platzhaltermaske
- — verlegerschnittstelle
- — Schnittstelle zum Landservice
- — Schnittstelle zur Lokalisierung
- — Schnittstellen zum Dokumentendienst
- — Orchesterschnittstelle (aktualisiert)
- — Metadaten der per-Datei-Übersetzung

### Aktualisierte Dienste in

- — Hinzugefügt benannte Platzhalter Unterstützung
- — Aktualisiert für neue Parameter
- — Benannte Platzhalterverwaltung
- — Schnittstelle der Platzhalter

### Neue Admin-Seite in

- — Echtzeitüberwachungsseite
- — Seitenmodell

### Neue Dokumentation in

- — Aktualisierte Pipeline-Dokumentation
- — Leitfaden für den Platzhalter
- — Gebrauchsanleitung des Armaturenbretts
- — Übersicht über die technische Architektur

## Zurück zur Übersicht

Alle Änderungen sind additiv:

- Vorhandener Lokalisierungscode () funktioniert unverändert
- Positionsformatierung () funktioniert unverändert
- Das bestehende JSON Wörterbuchformat ist unverändert
- Vorhandene Markdown-Struktur ist unverändert
- Signal R-Nachrichten verwenden das gleiche Format

## Migrationspfad

Keine Migration erforderlich. Die Refactoring ist intern:

1. Alter wurde als Referenz erhalten und dann ersetzt
2. DI-Registrierungen wurden aktualisiert, um neue Schnittstellen zu verwenden
3. Alle bestehenden Verbraucher sehen keine Änderungen

## Leistungsverbesserungen

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- ** Bessere Sicht**: Echtzeit-Fortschritt hilft, langsame Phasen zu diagnostizieren

## Zukunftsverbesserungen

Geplante Verbesserungen:

1. **AI Feinabstimmung** — Übersetzungsrezension für Phrasen nach der Maschine > 5 Wörter
2. **Admin-Authentifizierung** — Administratorseiten für autorisierte Benutzer einschränken
3. **Dictionary Editor** — Web UI für die Verwaltung von Lokalisierungsschlüsseln
4. **Übersetzungsstatistik** — Diagramme mit Übersetzungszählungen und Fehlerquoten im Laufe der Zeit
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Kontakt

Für Fragen oder Probleme mit dem Übersetzungsdienst wenden Sie sich bitte an die detaillierte Dokumentation in jedem Modulverzeichnis oder wenden Sie sich an das Entwicklungsteam.
