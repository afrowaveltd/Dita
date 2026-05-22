# Fejlkoder

Dita uses a **range-partitioned, unified error code architecture** that provides both domain-specific enums and a single catch-all `ErrorCode` type. Every error in the system — from network failures to disk I/O, from authentication to configuration — is represented by a member of this hierarchy.

## Arkitektur

### Områdefordeling

Område
|-------|----------|----------|
1000- 1999
2000-2999
3000- 3999
4-4999
5000- 5999
6000- 6999
7000- 7999
8000- 8999
9000- 9999

### Dual- enum- mønster

Each error domain is represented by **both** a focused sub-enum (e.g. `NetworkError`) and entries in the unified `ErrorCode` enum. The sub-enums use bare names; the unified enum prefixes names with the category:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

Dette giver kode til at arbejde med domæne-specifikke typer, når sammenhængen er kendt, mens også støtte generiske fejl håndtering, der virker på tværs af alle domæner.

### sentinel

Hvert sub- enum definerer som grundværdien af sit område (f.eks.). Metoden anerkender dette og vender tilbage.

## ErrorCode-klasse

The `ErrorCode` enum consolidates all sub-enum values into a single type with **non-overlapping** integer ranges. The companion `ErrorCodeText` static class provides humanization:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Humaniseringslogik

følger en metode med konventionel overkonfiguration:

1. PascalCase navne er opdelt i ord via regex
2. Kendte acronymer normaliseres (Io → I / O, Api → API, Dns → DNS, Http → HTTP, Ssl → SSL, Mfa → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. All- caps tokens (fx) er bevaret
4. Værdier, der slutter til gengæld

## Domæne-specifikke enums

### NetworkError (1000- 1999)

Dækker DNS, SSL / TLS, proxyer, gateways, HTTP protokol fejl, forbindelse, og anmode om livscyklusproblemer.

Bemærkelsesværdige medlemmer
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

### StorageError (2000- 2999)

Dækker databaseforbindelser, transaktioner (commit / rollback / timeout), integritet (begrænsninger, dødvande, udenlandske nøgler), schema management, backup / retablering, replikation og kvote.

Bemærkelsesværdige medlemmer
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

### DiskFejl (3000- 3999)

Dækker lav-niveau fysiske disk og drev fejl: dårlige sektorer, SMART fejl, RAID nedbrydning, partition tabeller, hardware fejl, mount / afmontere, format, og skubbe operationer.

Bemærkelsesværdige medlemmer
|---|---|
3000
3001
3010
3012
3021
3027
3032

### FileSystemFejl (ECF- 4999)

Dækker filsystemfejl: adgang / tilladelse, fillåsning, kompression / dekompression / kryptering, problemer med sti, symbolske links, deling af overtrædelser og generelle I / O-operationer.

Bemærkelsesværdige medlemmer
|---|---|
4000
4001
4013
4011
4023
4024
4028

### LokaliseringsFejl (5000- 5999)

Dækker fejl specifikke for lokalisering rørledning: ordbøger, kodning, lokalisering validering, flertal former, ekstern oversættelse API 'er (auth, tilgængelighed, kø, timeout), og streng formatering.

Bemærkelsesværdige medlemmer
|---|---|
5000
5001
5007
5014
5015
5016
5018

### AuthenticationError (6000- 6999)

Omfatter godkendelse og godkendelse: legitimation, tokens (refresh / access), sessioner, MFA / 2FA, biometrics, certifikater, OAuth, SSO, og konto stater (deaktiveret, udløbet, låst).

Bemærkelsesværdige medlemmer
|---|---|
6000
6001
6004
6015
6024
6026

### ValiderError (7000- 7999)

Omfatter inputvalidering: formatkontrol (e-mail, telefon, URL, JSON, XML, datetime), interval- / længdebegrænsninger, konverteringsfejl, krævede felter, mønster / regex og kodeordskompleks.

Bemærkelsesværdige medlemmer
|---|---|
7000
7003
7016
7018

### KonfigurationFejl (8000- 8999)

Omfatter konfiguration og indstillinger: filadgang, fortolkning, validering, hemmeligheder / nøgle hvælving, forbindelsesstrenge, DI, funktion flag, miljøvariabler, og skema / version mismatch.

Bemærkelsesværdige medlemmer
|---|---|
8000
8001
8016
8019

### GeneralError (9000- 9999)

Catch- all for application- wide fejl: hukommelse, concurrency, licenser, sats begrænsning, threading, ressource management, feature support, og uhåndterede undtagelser.

Bemærkelsesværdige medlemmer
|---|---|
9000
9004
9007
9015
9014

## rørledninger

### Processtrin

Definerer sekventielle stadier af den automatiske oversættelsesrørledning:

Værdi
|-------|------|-------------|
0
1
2
3
4
5

### LokalizationMessageType

Type realtidsmeddelelse fra rørledningen:

Værdi
|-------|------|---------|
0
1
2
3
4
5
6

### Oversættelse Mål

Angiver hvilken indholdstype der skal oversættes:

Værdi
|-------|------|---------------|
0
1
2

### frassechan

Spor CRUD- lignende ændring tilstand for lokalisering ordbog indgange:

Værdi
|-------|------|
0
1
2
3

### Sammenligning

Sammenligningsoperatører, der anvendes til vurdering / filtrering af værdier:

Værdi
|-------|------|----------|
0
1
2
3
4
5
6

### Køn

Grammatisk / socialt køn for lokalisering:

Værdi
|-------|------|
0
1
2
3

## Brug af fejlkoder

### I rørledningsrapporter

Oversættelsesfejl udføres i records:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### I API svar

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Humanisering af kode

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
