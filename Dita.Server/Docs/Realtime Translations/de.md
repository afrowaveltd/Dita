# Echtzeit-Übersetzungen

Dieses Dokument existiert als Live-Test-Eingabe für die automatische Übersetzungspipeline. Jede Änderung dieser Datei löst eine erneute Übersetzung aller Zielsprachendateien auf dem nächsten geplanten Lauf aus.

## Architekturübersicht

Die Übersetzungspipeline wurde in eine modulare Architektur umstrukturiert, mit vier spezialisierten Subservices, die von einem leichten Orchester koordiniert werden:

- **BackendTranslationService** — Orchestriert die gesamte Pipeline, verwaltet die Servervalidierung und Delegierte arbeiten an Sub-Services.
- **CountriesTranslationService** — Synchronisiert Ländernamen aus per-language Wörterbüchern.
- **LocalizationTranslationService** — Erkennt hinzugefügte/entfernte Schlüssel im Standard-JSON-Wörterbuch und übersetzt sie in Zielsprachen.
- **DocumentsTranslationService** — Übersetzt Markdown-Dokumentationsdateien mit Per-Block-Tracking und Metadaten.

Jeder Subservice arbeitet unabhängig und meldet Fortschritt über SignalR in Echtzeit.

## Was der Service tut

Der Dienst läuft in einem Zeitplan und führt eine fünfstufige Pipeline aus: Servervalidierung, Ländersynchronisation, JSON Wörterbuch Synchronisation, Markdown-Dateiübersetzung und die Ergebnisse bleiben. Jede Stufe sendet strukturierte Echtzeit-Fortschrittsereignisse über SignalR aus, so dass verbundene Kunden bei einem Arbeitsablauf mitkommen können.

## Rohrleitungsstufen

### Stufe 1 — Prüfserver

Vor Beginn einer Übersetzungsarbeit überprüft der Service, dass alle Voraussetzungen erfüllt sind:

- Der Konfigurationsabschnitt muss vorhanden und gültig sein.
- Der LibreTranslate Server muss innerhalb einer akzeptablen Latenz reagieren.
- Die auf dem Übersetzungsserver verfügbare Liste der Sprachen wird abgeholt.
- Die konfigurierte Standardsprache muss in dieser Liste vorhanden sein.
- Fehlen von lokalen JSON-Dateien für jede unterstützte Sprache werden automatisch erstellt.

Wenn eine Überprüfung ausfällt, stoppt die Pipeline sofort und eine Nachricht wird ausgesandt.

### Stage 2 — ÜbersetzenCountries

Ländernamen werden aus einem nur lesbaren Katalog () in die Lokalisierungs-JSON-Wörterbücher synchronisiert gehalten.

- Wenn die Standardsprache der Anwendung Englisch ist, wird jeder Ländername wie ohne Übersetzung gespeichert.
- Wenn die Standardsprache eine andere Sprache ist, wird der Name des englischen Landes zuerst in diese Sprache übersetzt und das Ergebnis wird der Eintrag im Standardwörterbuch.
- Nach dem Update des Standardwörterbuchs wird jeder fehlende Ländereintrag in jedem Zielsprachewörterbuch übersetzt und gespeichert ** Sofort pro Sprache**.
- Bereits übersetzte Einträge werden ohne Modifikation erhalten.
- Wenn eine Übersetzung ausfällt, rettet der Dienst bis zu 3 mal mit 30 Sekunden Verzögerungen, bevor er in die nächste Sprache wechselt.

### stufe 3 — translatejsonfiles

Der Dienst vergleicht das aktuelle Standard-Sortisations-Wörterbuch mit einem Snapshot, der vom vorherigen Lauf gespeichert ist:

- **Added keys** — Einträge, die im aktuellen Standard, aber nicht aus dem Snapshot stammen, werden in jede Zielsprache übersetzt, die keinen manuellen Eintrag für diesen Schlüssel hat.
- **Entfernte Schlüssel** — Einträge im Snapshot, aber abwesend aus dem aktuellen Standard — werden aus jedem Zielsprachewörterbuch gelöscht.
- Manuelle Übersetzungen nehmen immer Priorität. Wenn ein Zielwörterbuch bereits einen Wert für einen Schlüssel enthält, bleibt dieser Eintrag unabhängig davon, was die Quelle sagt, unverändert.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Fällt eine Übersetzung für eine bestimmte Sprache aus, so rettet der Dienst automatisch. Nur hartnäckige Fehler (z.B. nicht unterstützte Sprache) bewirken, dass die Sprache übersprungen wird.
- Nach dem Start wird das aktuelle Standardwörterbuch als neuer Snapshot für den nächsten Vergleich gespeichert.

Alle Wörterbücher werden immer mit alphabetisch sortierten Schlüsseln gespeichert und JSON für die menschliche Lesbarkeit identifiziert.

### stufe 4 — übersetzenmarkdownfiles

Der Dienst führt die konfigurierten Dokumentationswurzeln (Standard: ) und verarbeitet jede Quelldatei wiederkehrend:

1. Der Quelldateiinhalt wird gelesen und ein SHA-256 Hash berechnet.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. Der gespeicherte Hash aus dem vorherigen Lauf (in einer Datei neben der Quelldatei oder in einem temporären Fallback-Standort) wird mit dem aktuellen Hash verglichen.
4. Für jede Zielsprache wird die entsprechende Datei auch auf strukturelle Integrität überprüft.
5. Jede Zieldatei, die fehlt, hat einen veralteten Hash, versagt Strukturvalidierung oder enthält untranslatierte Blöcke wird für die Re-Translation abgefragt.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Erfolgreich übersetzte Dateien werden für Strukturparität mit der Quelle validiert (gleiche Überschriftenzählungen, Listenpositionen, Codeblöcke, Blockquoten, Links, fett/italienische Marker und HTML-Tags), bevor sie auf Festplatte geschrieben werden.
8. Wenn alle Zieldateien für eine Quelle erfolgreich sind, wird der neue Hash neben der Quelle gespeichert. Wenn das Schreiben neben der Quelle ausfällt (z.B. in Nur-Einstellungen), fällt der Hash zurück in das temporäre Verzeichnis.
9. Wenn eine Zielübersetzung nicht validiert wird, markiert die Metadaten diese Blöcke als untranslatiert, so dass sie auf dem nächsten Lauf abgerufen werden.

### Stufe 5 — StoringErgebnisse

Ein konsolidierter Konzern wird zusammengestellt und veröffentlicht. Es umfasst:

- UTC laufen Start- und Fertigzeitstempel.
- Anzahl der gespeicherten lokalen JSON-Dateien, gespeicherte Markdown-Dateien, gespeicherte Hash-Dateien, und Fallback Hash schreibt.
- Alle während des Laufs gesammelten Speicherfehler.
- Per-Sprache Übersetzungsstatistiken (übersetzte Anzahl, übersprungene Anzahl, Fehlerzahl).

## SignalR Nachrichtenübermittlung

Jedes Fortschrittsereignis wird wie a mit folgenden Feldern ausgeliefert:

Feld
|-------|------|-------------|
Korrelationskennung für den aktuellen Pipelinelauf
Monotonzähler innerhalb eines Laufs, beginnend bei 1
Semantische Art der Nachricht
Pipeline-Stufe die Nachricht gehört
UTC-Zeit, als die Nachricht ausgesandt wurde
Ob die Nachricht eine Fehlerbedingung darstellt
Human lesbare Zusammenfassung
Stufenspezifische Nutzlast (Reportobjekt oder Null)

### Nachrichten

Wert
|-------|------|---------|
0)
1
2
3
ANHANG
5
6

### Rohrleitungsstufen

Wert
|-------|------|-------------|
0)
1
2
3
ANHANG
5

### Typischer Nachrichtenfluss

```text
StageStarted  / CheckServers
Progress / CheckServers — Server latency: 42ms
StageCompleted / CheckServers
StageStarted  / TranslateCountries
Progress / TranslateCountries — Found 195 country names
Progress / TranslateCountries — Starting translations for 'cs'...
Progress / TranslateCountries — Saved dictionary for 'cs' (198 entries)
StageCompleted / TranslateCountries
StageStarted  / TranslateJsonFiles
Progress / TranslateJsonFiles — Detected 3 added and 0 removed keys
Progress / TranslateJsonFiles — Starting JSON translations for 'cs'...
Progress / TranslateJsonFiles — Saved dictionary for 'cs' (201 entries)
StageCompleted / TranslateJsonFiles
StageStarted  / TranslateMarkdownFiles
Progress / TranslateMarkdownFiles — Scanning 2 source files in '/Docs'
Progress / TranslateMarkdownFiles — File 'en.md' has 12 translatable blocks
Progress / TranslateMarkdownFiles — Translating 'en.md' to 'cs'...
Progress / TranslateMarkdownFiles — Saved 'cs' translation for 'en.md' (12/12 blocks)
StageCompleted / TranslateMarkdownFiles
StageCompleted / StoringResults
PipelineCompleted / StoringResults
```

Wenn eine Stufe ausfällt, werden die restlichen Stufen übersprungen, eine Nachricht ausgesandt, und schließlich schließt eine Nachricht den Ablauf.

## Übersetzung retry logic

Die Pipeline implementiert zwei Niveaus der Widerstandsfähigkeit:

### Bühnenretry (TranslationRetryService)

- Versäumt eine Übersetzungsanfrage nach LibreTranslates internen Retries, führt die bis zu 3 zusätzliche Stufen-Level-Retries mit 30-Sekunden Verzögerungen durch.
- Platzhalter-Masking: Namete Platzhalter () im Text werden vorübergehend durch sichere Token () vor der Übersetzung ersetzt und nachher wiederhergestellt, um eine korrekte Grammatik in Zielsprachen zu gewährleisten.

### Sprachvalidierung

- Vor dem Übersetzen auf eine Zielsprache wird der Dienst die Sprache vom Übersetzungsserver unterstützt.
- Ununterstützte Sprachen werden mit einer Warnung übersprungen und wiederholte gescheiterte Versuche verhindern.

### Markdown-Block-Level-Retry

- Markdown-Übersetzungen werden Block-by-Block (Positionen, Absätze, Listenpositionen) durchgeführt.
- Wenn ein einzelner Block die Übersetzung ausfällt, wird er in der Metadatendatei als untranslatiert markiert und auf dem nächsten Pipeline-Laufwerk abruft.
- Der Service Tracks per-Sprache, per-Block-Status in Dateien neben jeder Quelle Markdown-Datei.

## Fehlercodes

Fehler werden mit einem einheitlichen enum gruppiert in Bereiche gemeldet:

Reichweite
|-------|----------|
1000-1999
2000–2999
3000–3999
4000–4999
5000–5999

Jeder Fehler in einem Bericht trägt die Quellkennung (Sprachcode, Dateipfad oder Phasenname), den Fehlercode und eine human lesbare Nachricht.

## Live-Übersetzung Dashboard

Das Server-Projekt beinhaltet eine Admin-Seite, die sich mit dem SignalR-Hub verbindet und alle Pipeline-Ereignisse in Echtzeit zeigt.

- Zeigt Verbindungsstatus, Nachrichtenzählung und eine Live-updating-Tabelle aller Ereignisse an.
- Farbcodierte Zeilen: blau für Bühnenstart, grün für Fertigstellung, rot für Fehler.
- Unterstützt das Clearing des Feeds und Export aller Nachrichten an JSON.
- Auto-reconnects mit exponentiellem Backoff, wenn die Verbindung sinkt.

## Gestaltungsprinzipien

- **Modularität**: Jedes Übersetzungsproblem ist in seinem eigenen Dienst für die Aufrechterhaltung und Testbarkeit isoliert.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Elastizität**: Mehrere Retry Levels (HTTP, Stage, Block) sorgen dafür, dass transiente Fehler die Pipeline nicht blockieren.
- **State-Tracking**: Per-Datei-Metadaten () und Hash-Dateien ermöglichen eine präzise Inkrementalarbeit auf nachfolgenden Laufwerken.
- ** Echtzeitsicht**: Jede signifikante Operation wird über SignalR zur Überwachung und Debugging gemeldet.
- **Viele Übersetzungen haben immer Vorrang vor automatischen Ergänzungen.**
