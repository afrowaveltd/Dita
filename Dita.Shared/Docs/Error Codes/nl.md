# Foutcodes

Dita maakt gebruik van een **range-partitioned, unified error code architectuur** die zowel domeinspecifieke enums als een enkel catch-all type biedt. Elke fout in het systeem van netwerkstoringen tot schijf I/O, van authenticatie tot configuratie wordt vertegenwoordigd door een lid van deze hiërarchie.

## Bouwkunde

### Range allocatie

Bereik
|-------|----------|----------|
1000
2000
3000
4000
5000
6000
7000
8000
[99]99

### Dual-enum patroon

Elk foutdomein wordt vertegenwoordigd door **beide** een gefocust sub-enum (bijv. .) en vermeldingen in het unified enum. De sub-enums gebruiken kale namen; de unified enum voorvoegt namen met de categorie:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

Dit staat code toe om te werken met domeinspecifieke types wanneer de context bekend is, terwijl ook ondersteuning generieke foutafhandeling die werkt in alle domeinen.

### sentinel

Elk sub-enum definieert als de basiswaarde van zijn bereik (bv. ). De methode herkent dit en keert terug.

## Foutcodeklasse

De enum consolideert alle sub-enum waarden in één type met **non-overlapping** integer bereik. De metgezel statische klasse zorgt voor humanisering:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Humaniseringslogica

een conventie-overconfiguratiebenadering volgt:

1. PascalCase namen worden opgesplitst in woorden via regex
2. Bekende acroniemen zijn genormaliseerd (Io → I/O, Api → API, Dns → DNS, Http → HTTP, Ssl → SSL, MFA → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. All-caps tokens (bijv.) worden bewaard
4. Waarden eindigend in ruil

## Domeinspecifieke enums

### Netwerkfout (1000

Omvat DNS, SSL/TLS, proxies, gateways, HTTP protocol fouten, connectiviteit, en verzoeken om levenscyclusproblemen.

Opmerkelijke leden
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

### OpslagFout (2000

Omvat databaseverbindingen, transacties (commit/rollback/timeout), integriteit (beperkingen, impasses, buitenlandse sleutels), schemabeheer, back-up/herstel, replicatie en quota.

Opmerkelijke leden
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

### Schijffout (3000

Beschikt over fysieke schijf- en schijffouten op laag niveau: slechte sectoren, SMART-storingen, RAID-degradatie, partitietabellen, hardwarestoringen, mount/unmount, formaat en uitwerpen.

Opmerkelijke leden
|---|---|
3000
3001
3010
3012
3021
3027
3032

### FileSystemError (4000

Omvat foutmeldingen van het bestandssysteem: toegang/toestemming, bestandsvergrendeling, compressie/decompressie/encryptie, padproblemen, symbolische links, het delen van schendingen en algemene I/O-operaties.

Opmerkelijke leden
|---|---|
4000
4001
4013
4011
4023
4024
4028

### Lokalisatiefout (5000

Omvat fouten die specifiek zijn voor de lokalisatiepijplijn: woordenboeken, codering, lokale validatie, meervoudsformulieren, externe vertaling API's (auth, beschikbaarheid, wachtrij, timeout) en tekenreeksopmaak.

Opmerkelijke leden
|---|---|
5000
5001
5007
5014
5015
5016
5018

### Authenticatiefout (6000

Omvat authenticatie en autorisatie: referenties, tokens (vernieuwen/toegang), sessies, MFA/2FA, biometrische gegevens, certificaten, OAuth, SSO, en account staten (uitgeschakeld, verlopen, vergrendeld).

Opmerkelijke leden
|---|---|
6000
6001
6004
6015
6024
6026

### Valideringsfout (7000

Omvat invoervalidatie: formaatcontroles (e-mail, telefoon, URL, JSON, XML, datumtijd), bereik/lengte beperkingen, conversie storingen, vereiste velden, patroon/redex, en wachtwoord complexiteit.

Opmerkelijke leden
|---|---|
7000
7003
7016
7018

### Configuratiefout

Omvat configuratie en instellingen: bestandstoegang, ontleden, validatie, geheimen/sleutelkluis, verbindingsstrings, DI, feature flags, omgevingsvariabelen, en schema/versie mismatches.

Opmerkelijke leden
|---|---|
8000
8001
8016
8019

### AlgemeenError

Catch-all voor applicatie-brede fouten: geheugen, concurrency, licentie, tarief beperking, threading, resource management, feature support, en unhandled uitzonderingen.

Opmerkelijke leden
|---|---|
9000
9004
9007
9015
9014

## Pijpleidingen

### Procesfase

Bepaalt de opeenvolgende fasen van de automatische vertaalpijpleiding:

Waarde
|-------|------|-------------|
0
1
2
3
4
5

### Lokalisatieberichtentype

Soort real-time bericht uitgezonden door de pijpleiding:

Waarde
|-------|------|---------|
0
1
2
3
4
5
6

### Vertaling Doel

Geeft aan welk type inhoud moet worden vertaald:

Waarde
|-------|------|---------------|
0
1
2

### Woorden wijzigen

Tracks CRUD-achtige verandering staat voor lokalisatie woordenboek items:

Waarde
|-------|------|
0
1
2
3

### Vergelijking

Vergelijkingsoperators die worden gebruikt voor het evalueren/filteren van waarden:

Waarde
|-------|------|----------|
0
1
2
3
4
5
6

### Geslacht

Grammaticaal/sociaal geslacht voor lokalisatie:

Waarde
|-------|------|
0
1
2
3

## Foutcodes gebruiken

### In pijplijnrapporten

Vertaalfouten worden geregistreerd:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### In API-antwoorden

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Humaniseren van elke code

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
