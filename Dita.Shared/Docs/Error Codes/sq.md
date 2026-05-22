# Kodi

Dita uses a **range-partitioned, unified error code architecture** that provides both domain-specific enums and a single catch-all `ErrorCode` type. Every error in the system — from network failures to disk I/O, from authentication to configuration — is represented by a member of this hierarchy.

## Arkitektura

### Intervali

Interval
|-------|----------|----------|
1000 udhërrëfyes
20002999
30003999
4000499
5000599
699
70.000799
8000899
9999

### Modeli Dual-enum

Each error domain is represented by **both** a focused sub-enum (e.g. `NetworkError`) and entries in the unified `ErrorCode` enum. The sub-enums use bare names; the unified enum prefixes names with the category:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

Kjo lejon që kodi të funksionojë me tipe domain-të veçanta kur konteksti të njihet, ndërsa mbështet gjithashtu gabimin e përgjithshëm që funksionon në të gjitha fushat.

### ruaj

Çdo nën-enum përcakton si vlerën bazë të gamës së tij (p.sh. .g. ). Metoda e njeh këtë dhe kthehet.

## Gabim

The `ErrorCode` enum consolidates all sub-enum values into a single type with **non-overlapping** integer ranges. The companion `ErrorCodeText` static class provides humanization:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Logjika e humanizimit

pas një qasjeje të mbi-konfigurimit:

1. Emrat PaskalCase ndahen në fjalë nëpërmjet regex
2. Akronimët e njohur janë normalizuar (Io → I/O, Api →API, Dns → DNS, Htp → http, Ssl → SSL, Mfa → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. Gjithçka përmban
4. Vlera në shkëmbim

## Domain-figurues

### NetworkError (1000)

Mbulimet DNS, SSL/TLS, Prokura, porta, gabime protokolli HTTP, lidhje dhe kërkojnë probleme me ciklin e jetës.

Anëtarë të notueshëm
|---|---|
1000
1001
1002
1003
1004
1005
1006
1007
1008
1009
1010
1019
1020
1021

### Magazinim

Kopertinat e lidhjeve në bazë të të dhënave, transaksionet (komitet/rollack/time/out), integriteti (kontraints, bllokime, çelësa të huaj), menazhim i skemave, backup/restor, replikim dhe kuotë.

Anëtarë të notueshëm
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

### DiskErrar (3000399)

Kopertinat në nivelin e ulët të diskut fizik dhe gabimet me makinë: sektorë të keq, dështime të SMART, degradim të NGA-së, tryeza ndarëse, dështime të hardware-ve, rritje/unive, format dhe operacione të nxjerrjes.

Anëtarë të notueshëm
|---|---|
3000
3001
3010
3012
3021
3027
3032

### File SystemError (4000)

Mbulon gabimet e operacionit të skedarëve: aksesi/permisioni, bllokimi i skedarëve, ngjeshja/dekompresimi, çështjet e shtegut, lidhjet simbolike, ndarja e shkeljeve dhe operacionet e përgjithshme I/O.

Anëtarë të notueshëm
|---|---|
4000
4001
4013
4011
4023
4024
4028

### Lokale

Mbulimet gabuar specifikisht në tubacionin e lokalizimit: fjalorët, kodifikimi, vlefshmëria lokale, format e shumës, përkthimi i jashtëm API (auth, disponibilitet, rradhë, pushim) dhe formatim string.

Anëtarë të notueshëm
|---|---|
5000
5001
5007
5014
5015
5016
5018

### AuthenticationEror (6000)

Autentifikim dhe autorizime: kredencialet, shenjat (e freskëta/açesi), seancat MFA/2FA, biometrike, çertifikatat, OAuth, SSO, dhe vendet e llogarisë (të paafrueshme, të bllokuara).

Anëtarë të notueshëm
|---|---|
6000
6001
6004
6015
6024
6026

### VlefErra (000799)

Kopertinat e vlefshme: kontrollet e formatit (email, telefon, URL, JSON, XML, datën), kufizimet e gjatë, dështimet në kthim, fushat e kërkuara, modeli/rex dhe kompleksiteti i fjalëkalimit.

Anëtarë të notueshëm
|---|---|
7000
7003
7016
7018

### Konfigurimi

Konfigurimet dhe rregullimet: hyrjet në skedarë, parsifikimi, vlefshmëria, sekretet/kerma e kyçit, lidhjet, DI, funksioni i flamujve, ndryshot e mjedisit dhe mospërputhjet në skemë/version.

Anëtarë të notueshëm
|---|---|
8000
8001
8016
8019

### Gjenerali Errar (9000999)

Kapja e të gjitha gabimeve të përgjithshme të aplikativit: kujtesa, pajtimi, licensimi, kufizimi i normës, kufizimi i shpejtësisë, administrimi i burimeve, mbështetja karakteristike dhe përjashtimet e patrajtuara.

Anëtarë të notueshëm
|---|---|
9000
9004
9007
9015
9014

## Enumët e tubacionit

### Proçesi

Përcakton fazat sekuente të tubacionit automatik të përkthimit:

Vlera
|-------|------|-------------|
0
1
2
3
4
5

### Lloji i mesazheve të lokalizimit

Lloji nga:

Vlera
|-------|------|---------|
0
1
2
3
4
5
6

### Përkthimi Objektivi

Specifikon llojin e përmbajtjes që duhet përkthyer:

Vlera
|-------|------|---------------|
0
1
2

### Ndryshimi

Gjurmët e ndryshimit të tipit CRUD për zërat e fjalorit:

Vlera
|-------|------|
0
1
2
3

### Krahasim

Operatorët e krahasimeve të përdorura për vlerësimin/ filtrimin e vlerave:

Vlera
|-------|------|----------|
0
1
2
3
4
5
6

### Gjinorë

Emri i gjinisë gramatikore/sociale për lokalet:

Vlera
|-------|------|
0
1
2
3

## Duke përdorur kodet e gabimeve

### Në raportet e tubacionit

Gabimet e përkthimit gjenden në të dhënat:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### Në përgjigje API

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Duke humanizuar çdo kod

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
