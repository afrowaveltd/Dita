# Fehlercodes

Dita verwendet eine **range-partitionierte, einheitliche Fehlercode-Architektur**, die sowohl Domänen-spezifische Enums als auch einen einzigen Fangtyp bietet. Jeder Fehler im System - von Netzausfallen auf die Festplatte I/O, von der Authentifizierung bis zur Konfiguration - wird durch ein Mitglied dieser Hierarchie dargestellt.

## Architektur

### Zuweisung der Range

Reichweite
|-------|----------|----------|
1000-1999
2000–2999
3000–3999
4000–4999
5000–5999
6000–6999
7000–7999
8000–8999
9000-9999

### Dual-Enum Muster

Jede Fehlerdomäne wird durch **both** ein fokussiertes Sub-Enum (z.B. ) und Einträge im einheitlichen enum repräsentiert. Die Sub-Enums verwenden bloße Namen; die vereinheitlichten Enum präfixiert Namen mit der Kategorie:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

Damit kann der Code bei Bekanntwerden des Kontexts mit Domänen-spezifischen Typen arbeiten und gleichzeitig eine generische Fehlerbehandlung unterstützen, die über alle Domänen hinweg funktioniert.

### insekt

Jedes Sub-Enum definiert als Basiswert seines Bereichs (z.B. ). Das Verfahren erkennt dies und gibt zurück.

## Fehlercode Klasse

Das enum konsolidiert alle Sub-Enum-Werte in einen einzigen Typ mit **non-overlapping** ganze Bereiche. Die gegnerische statische Klasse ermöglicht die Humanisierung:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Humanisierungslogik

folgt ein herangehensweise der konvention-over-konfiguration:

1. PascalCase-Namen werden über Regex in Wörter aufgeteilt
2. Bekannte Akronyme werden normalisiert (Io → I/O, Api → API, Dns → DNS, Htp → HTTP, Ssl → SSL, Mfa → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. All-Cap-Token (z.B. ) erhalten
4. Werte, die im Gegenzug enden

## Domain-spezifische Enums

### NetworkError (1000–1999)

Covers DNS, SSL/TLS, Proxies, Gateways, HTTP-Protokollfehler, Konnektivität und Anforderung von Lifecycle-Problemen.

Notwendige Mitglieder
|---|---|
ANHANG
1001
1002
1003
1004
100 %
1006
1007
1008
1009
1010
1019
1020
1021

### lagerhalter (2000–2999)

Bezieht Datenbank-Verbindungen, Transaktionen (Vermittlung/Rollback/Timeout), Integrität (Beschränkungen, Schließungen, Fremdschlüssel), Schema-Management, Backup/Restore, Replikation und Quote.

Notwendige Mitglieder
|---|---|
2000
2003
2004
2007
2010
2012
2013
2018
2023
2029

### DiskError (3000–3999)

Bedeckt physikalische Festplatten- und Laufwerksfehler auf niedrigem Niveau: schlechte Sektoren, SMART-Ausfälle, RAID-Degradation, Partitionstabellen, Hardware-Ausfälle, Mount/unmount, Format und Eject-Betriebe.

Notwendige Mitglieder
|---|---|
ANHANG
1
ANHANG
3012
3021
3027
ANHANG

### dateisystemerror (4000–4999)

Bezieht Dateisystem-Betriebsfehler: Zugriff/Berechtigung, Dateiverriegelung, Kompression/Dekompression/Verschlüsselung, Pfadprobleme, symbolische Links, Freigabe von Verstößen und allgemeine I/O-Betriebe.

Notwendige Mitglieder
|---|---|
4
L 347 vom 20.12.2013, S. 1)
Artikel 2
KAPITEL 11
4023
4024
4028

### LokalisierungError (5000–5999)

Bezieht Fehler, die für die Lokalisierungspipeline spezifisch sind: Wörterbücher, Kodierung, lokale Validierung, Pluralformen, externe Übersetzungs-APIs (Aud, Verfügbarkeit, Warteschlange, Timeout) und String-Formatierung.

Notwendige Mitglieder
|---|---|
5000
500
5007
5014
5015
5016
5018

### authentifizierungsgerät (6000–6999)

Covers Authentifizierung und Autorisierung: Anmeldeinformationen, Token (refresh/access), Sitzungen, MFA/2FA, Biometrie, Zertifikate, OAuth, SSO und Kontozustände (deaktiviert, abgelaufen, gesperrt).

Notwendige Mitglieder
|---|---|
6000
600
6004
6015
6024
6026

### ValidierungFehler (7000–7999)

Covers Eingabevalidierung: Format-Checks (E-Mail, Telefon, URL, JSON, XML, Datumszeit), Reichweite/Länge Einschränkungen, Konvertierungsausfälle, benötigte Felder, Muster/Regex und Passwort-Komplexität.

Notwendige Mitglieder
|---|---|
7
7003
7016
7018

### KonfigurationFehler (8000–8999)

Covers Konfiguration und Einstellungen: Dateizugriff, Parsing, Validierung, Geheimnisse/Schlüsselgewölbe, Verbindungsstrings, DI, Feature-Flags, Umgebungsvariablen und Schema/Version-Mixaten.

Notwendige Mitglieder
|---|---|
8000
800
8016
8019

### GeneralError (9000–9999)

Catch-all für anwendungsweite Fehler: Speicher, Konkurrenz, Lizenzierung, Rate Limiting, Threading, Ressourcenmanagement, Feature-Unterstützung, und unhandled Ausnahmen.

Notwendige Mitglieder
|---|---|
9000
9004
9007
9015
9014

## Rohrumfänge

### Verfahren

Definiert die sequentiellen Stufen der automatischen Übersetzungspipeline:

Wert
|-------|------|-------------|
0)
1
2
3
ANHANG
5

### LokalisierungMessageTyp

Art der Echtzeit-Nachricht aus der Pipeline:

Wert
|-------|------|---------|
0)
1
2
3
ANHANG
5
6

### Übersetzung Ziel

Gibt an, welcher Inhaltstyp übersetzt werden soll:

Wert
|-------|------|---------------|
0)
1
2

### phrasen

Tracks CRUD-ähnlicher Änderungszustand für Lokalisierungswörtereinträge:

Wert
|-------|------|
0)
1
2
3

### Vergleich

Vergleichsoperatoren zur Auswertung/Filterung von Werten:

Wert
|-------|------|----------|
0)
1
2
3
ANHANG
5
6

### Geschlecht

Grammatikalische/soziale Geschlechter für die Lokalisierung:

Wert
|-------|------|
0)
1
2
3

## Fehlercodes verwenden

### In Pipeline-Berichten

Übersetzungsfehler werden in Aufzeichnungen durchgeführt:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### In API Antworten

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Erkennen eines beliebigen Codes

```csharp
// From enum value
string text = ErrorCodeText.ErrorText(ErrorCode.StorageDeadlockDetected);
// → "Storage deadlock detected"

// From raw integer (validates against defined values)
string text2 = ErrorCodeText.ErrorText(2010);
// → "Storage deadlock detected"

// Undefined code
string text3 = ErrorCodeText.ErrorText(99999);
// → "Unknown error (99999)"
```
