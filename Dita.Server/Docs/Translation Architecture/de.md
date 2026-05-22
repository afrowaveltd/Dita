# Übersetzung Architektur

Dieses Dokument beschreibt die modulare Architektur des automatischen Übersetzungssystems von Dita, das zur Verbesserung der Standfestigkeit, Testbarkeit und Widerstandsfähigkeit eingeführt wird.

## Designziele

Die Refactoring befasste sich mit dem ursprünglichen monolithischen Design:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilienz**: Mehrere Retry Levels behandeln transiente Fehler, ohne die gesamte Pipeline zu blockieren.
- **Verbrauchbarkeit**: Jeder signifikante Betrieb wird über SignalR zur Echtzeitüberwachung gemeldet.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Service Zersetzung

### zurückendtranslationservice (orchestrator)

**Responsibilities**:
- Pipeline-Lebenszyklusmanagement (Start, Fertigstellung, Fehlerbehandlung)
- Semaphorebasierte Konkurrenzkontrolle (verhindert Überlappungsabläufe)
- Servervalidierung (Lattung, Sprachverfügbarkeit, Konfiguration)
- Delegation bei Unteraufträgen

**Does NOT contain**:
- Übersetzungslogik
- Datei I/O für bestimmte Formate
- Retry Logik

### LänderTranslationService

**Responsibilities**:
- Lesen Sie das Verzeichnis
- Ländernamen in das Standard-Sortenwörterbuch synchronisieren
- Übersetzen fehlende Ländernamen pro Zielsprache
- Jedes Zielwörterbuch sofort nach der Übersetzung speichern

**Key behaviors**:
- Wenn Standardsprache Englisch ist: Ländernamen gespeichert als-is
- Wenn Standardsprache andere ist: Englische Namen übersetzt in Standardsprache zuerst
- Jede Sprache wird unabhängig mit einer eigenen Retryschleife verarbeitet

### LokalisierungTranslationService

**Responsibilities**:
- Erkennen Sie hinzugefügte / gelöschte Tasten, indem Sie das aktuelle Standardwörterbuch mit vorheriger Snapshot vergleichen
- Hinzufügen von Schlüsseln in jede Zielsprache übersetzen
- Entfernen Sie gelöschte Tasten aus jeder Zielsprache
- Snapshot für den nächsten Vergleich speichern

**Key behaviors**:
- Manuelle Übersetzungen nehmen immer Priorität (nicht überschrieben)
- Hinzugefügte Tasten werden übersetzt und gespeichert per-Sprache sofort
- Entfernte Schlüssel werden per-Sprache sofort gelöscht
- Snapshot wird nur gespeichert, nachdem alle Sprachen erfolgreich abgeschlossen sind

### DokumenteTranslationService

**Responsibilities**:
- Walk konfigurierte Markdown Wurzeln rekursiv
- Erkennen Sie geänderte Quelldateien mit SHA-256 hashes
- Track per-Block Übersetzungsstatus in
- Block-by-Block mit per-Block-Retry
- Gültige Markdown-Struktur nach Übersetzung
- Jede Zielsprachedatei unabhängig voneinander speichern

**Key behaviors**:
- Block-Level-Granulat: Überschriften, Absätze, Listenpositionen werden getrennt übersetzt
- Metadaten-Tracks, die pro Sprache erfolgreich/verfehlt blockieren
- Versäumte Blöcke werden auf dem nächsten Lauf zurückgerufen, ohne erfolgreiche Blöcke neu zu übersetzen
- Strukturvalidierung gewährleistet Überschriftenzählungen, Listen, Codeblöcke, etc

## Retry Strategie

Das System implementiert Retries auf drei Ebenen:

### Ebene 1 — HTTP (LibreTranslateService)

- Bis zu 5 Versuche mit exponentiellem Backoff (1s, 2s, 3s, 4s, 5s)
- Unterstützt Netzwerk-Timeouts, 5xx Fehler und transiente Fehler
- In der HTTP-Client-Konfiguration integriert

### Stufe 2 — Stufe (TranslationRetryService)

- Bis zu 3 Versuche mit 30 Sekunden Verzögerungen
- Re-drives die gesamte Übersetzungsanfrage, nachdem HTTP-Level-Retries erschöpft sind
- Platzhalter Maskierung und Restaurierung wird auf dieser Ebene angewendet

### Ebene 3 — Block (DokumenteTranslationService)

- Einzelne Markdown-Blöcke, die scheitern, sind in Metadaten markiert
- Auf dem nächsten Pipelinelauf automatisch abrufen
- Erfolgreiche Blöcke werden nie wieder übersetzt

## Datenfluss

### JSON Wörterbuch Übersetzung

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### Markdown Übersetzung

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### Landname Übersetzung

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## Staatliche Beharrlichkeit

### Fingern

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Ermöglicht Inkrementalsync durch Tracking, was im vorherigen Lauf vorhanden war

### Hash Dateien

- **Markdown**: neben der Quelldatei
- **Fallback**: wenn der primäre Standort nur gelesen wird
- **Purpose**: Erkennt die Änderungen der Quellen, um unnötige Änderungen zu vermeiden

### Übersetzung von Metadaten

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Quelle Inhalt hash
- Per-Sprache Block Status (Array von Booleans)
- Letzte Aktualisierung Zeitstempel
- **Purpose**: Ermöglicht eine teilweise Umschaltung nur fehlgeschlagener Blöcke

### Lagerplatz

- **File**:
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Liefert Standardwerte für benannte Platzhalter über die Anwendung

## Meldung von Signalen

### Verlagsabstraktion

entkoppelt Übersetzungsdienste von SignalR-Spezifikationen:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Folgegarantien

- Meldungen innerhalb eines einzelnen Durchlaufs werden monoton sequenziert
- Sequenznummern sind einzigartig per-run via
- Clients können Lücken erkennen oder Nachbestellung

### Hub Mapping

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Erweiterungspunkte

### Hinzufügen eines neuen Übersetzungsziels

1. Erstellen Sie eine neue Schnittstelle mit
2. Implementierung der Schnittstelle mit domainspezifischer Logik
3. Registrieren in DI-Container
4. Injizieren in Konstrukteur
5. Anruf von nach bestehenden Phasen

### Zollpolitik

Override constructor Parameter:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### Kundenbetreuung

Ergänzung zur Änderung der Platzhalter-Syntax oder Speicher:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfiguration

### werbungen.json

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### laufzeitabstimmung

Einstellung
|---------|---------|--------|
80
10
3
30

## Teststrategie

### Einzeltests

Jeder Subservice ist unabhängig testbar:

- Mock zu simulieren Erfolg/Verleugnung
- Mock zur Überprüfung der Berichterstattung
- Verwenden Sie temporäre Verzeichnisse für die Datei I/ O
- Verifizieren Sie per-Sprache Sparverhalten

### Integrationstests

- Vollständige Pipeline mit realer (lokaler) LibreTranslate-Instanz
- Verify SignalR-Nachrichten werden an verbundene Clients geliefert
- Test gleichzeitiger Laufverhütung (Semaphore)
- Gültige Markdown-Struktur nach Übersetzung

### End-to-End-Tests

- Triggerübersetzung über API oder Scheduler
- Überprüfen Sie alle Zielsprachendateien erstellt/aktualisiert
- Metadaten-Dateien überprüfen korrekten Blockstatus
- Feste Platzhalter werden über Übersetzungen erhalten

## Leistungsbeurteilungen

- **Memory**: Per-language Speicher verhindert das Halten aller Wörterbücher im Speicher
- **Disk I/O**: Metadaten-Dateien addieren kleine Overhead, ermöglichen aber inkrementelle Arbeit
- **Netzwerk**: Sequentielle Verarbeitung mit Drosselung verhindert überwältigend LibreTranslate
- **CPU**: SHA-256 Hashing und Regex Validierung sind schnell relativ zur Translation Latenz
- **SignalR**: Leichte Nachrichten, keine Nutzlastkompression für typische Berichte erforderlich

## Migration von monolithischem Design

Das Original enthielt alle Logik in einer Klasse. Der Migrationspfad:

1. Länderlogik extrahieren →
2. JSON Logik extrahieren →
3. Markdown-Logik →
4. Auszug SignalR Publishing →
5. Wiederherstellungslogik →
6. Vereinfachen Sie den Orchestrator nur delegieren

Alle vorhandenen Schnittstellen () bleiben unverändert. Verbraucher der Pipeline sehen keine Bruchänderungen.
