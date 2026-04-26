# Echtzeit-Übersetzungen

Dieses Dokument existiert als Live-Test-Eingabe für die automatische Übersetzungspipeline.

## Was der Service tut

Der Dienst läuft in einem Zeitplan und validiert den Übersetzungsserver, die Konfiguration und die verfügbaren Sprachen, bevor eine Übersetzung beginnt.

Nach dem Validierungsschritt synchronisiert es Ländernamen aus dem Nur-Länder-Katalog in die Standard-Location JSON-Wörterbücher. Wenn die Standardsprache der Anwendung Englisch ist, wird der Ländereintrag als Schlüsselwert gespeichert. Wenn die Standardsprache anders ist, wird der englische Ländername zunächst in die Standardsprache übersetzt und erst dann als Schlüsselwert im Standardwörterbuch gespeichert.

Als nächstes vergleicht der Dienst das aktuelle Standardlokalisierungswörterbuch mit dem abgespeicherten Snapshot aus dem vorherigen Lauf. Neu hinzugefügte Einträge werden nur dann in Zielsprachen übersetzt, wenn der Schlüssel nicht bereits vorhanden ist, so dass manuelle Übersetzungen Priorität haben. Entfernte Einträge werden von allen Zielwörtern gelöscht, um den ganzen Satz konsistent zu halten.

Schließlich scannt der Service konfigurierte Dokumentationswurzeln für Markdown-Bäume. Jeder Themenordner soll eine Quelldatei enthalten, die nach der Standardsprache benannt wird, wie z.B. en.md. Der Dienst hashes that source file, erkennt Änderungen, übersetzt fehlende oder veraltete Ziel-Markdown-Dateien und speichert den aktuellen Hash neben der Quelldatei. Wenn das Schreiben des Hashs neben der Quelldatei nicht möglich ist, fällt es auf den temporären Speicher zurück.

## Wie funktioniert der Service

Das Backend sendet allgemeine SignalR-Nachrichten über einen Nachrichtenumschlag durch den Lokalisierungs-Hub aus. Jede Nachricht trägt einen Nachrichtentyp, die aktuelle Prozessstufe, einen UTC-Zeitstempel, eine Textübersicht und optionale stufenspezifische Nutzlast.

Die aktuellen Stufen sind:

- CheckServer
- Übersetzte Länder
- ÜbersetzenJsonFiles
- ÜbersetzenMarkdownFiles
- Antworten auf

Typischer Nachrichtenfluss wird Stufe gestartet, Stufe abgeschlossen und Pipeline abgeschlossen. Wenn eine Stufe ausfällt, wird die Nachricht als Fehler markiert und enthält strukturierte Fehlerinformationen mit einheitlichen Fehlercodes.

## Gestaltungsprinzipien

Übersetzungen werden sequentiell verarbeitet, um eine Überlastung des LibreTranslate-Servers zu vermeiden.

Lokalisierung JSON Wörterbücher werden immer mit alphabetisch sortierten Schlüsseln und formatiert JSON für einfachere Wartung gespeichert.

Der vorherige Standard-Wörterbuch-Snapshot wird persistent gespeichert, so dass ein Neustart der Anwendung keine Änderungsverfolgung verliert.

**Viele Übersetzungen haben immer Vorrang vor automatischen Ergänzungen.**
