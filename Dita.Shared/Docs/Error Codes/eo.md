# Eraroj

Dita uses a **range-partitioned, unified error code architecture** that provides both domain-specific enums and a single catch-all `ErrorCode` type. Every error in the system — from network failures to disk I/O, from authentication to configuration — is represented by a member of this hierarchy.

## Arkitekturo

### Areo asigno

Montaro
|-------|----------|----------|
1000-99
2000-2999
3000-3999
4000-4999
5000-5999
6000-6999
7000-7999
8000-8999
9000-9999

### Dual-enum-padrono

Ĉiu erardomajno estas reprezentita per  ** kaj ** fokusita sub-enum (ekz.) kaj kontribuoj en la unuigita enum. La sub-enum'oj utiligas nudajn nomojn; la unuigita enum prefiksoj nomas kun la kategorio:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

Tio permesas al kodo labori kun domajno-specifaj tipoj kiam la kunteksto estas konata, dum ankaŭ apogante senmarkan eraron pritraktantan tion laboras trans ĉiuj domajnoj.

### gardo

Ĉiu sub-enum difinas kiel la bazvaloron de ĝia intervalo (ekz.). La metodo rekonas tion kaj revenas.

## EraroCode klaso

The `ErrorCode` enum consolidates all sub-enum values into a single type with **non-overlapping** integer ranges. The companion `ErrorCodeText` static class provides humanization:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Humaniga logiko

sekvas kongres-super-konfiguradan aliron:

1. PaskaloCase-nomoj estas dividitaj en vortojn per regex
2. Konataj akronimoj estas normaligitaj (Io → I/O, Api → API, Dns → DNS, Http → HTTP, Ssl → SSL, Mfa → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. Tute-kaptaj ĵetonoj (ekz.) estas konservitaj
4. Valoroj finiĝantaj en rendimento

## Dom-specifaj enum'oj

### NetworkError (1000-1999)

Kovras DNS, SSL/TLS, anstataŭantojn, enirejojn, HTTP protokolerarojn, konekteblecon, kaj petas vivocikloproblemojn.

Famaj membroj
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
101010
1019
1020
1021

### StorageError (2000-2999)

Kovras databaseligojn, transakciojn (komisiono/reludigo/tempo), integrecon (konsistroj, blokiĝoj, eksterlandaj ŝlosiloj), skemadministradon, rezervon/restore, reproduktadon, kaj kvoton.

Famaj membroj
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

### DiskError (3000-3999)

Kovras malalt-nivelan fizikan diskon kaj movas erarojn: malbonaj sektoroj, SMART fiaskoj, RAID-degenero, sekciotabloj, hardvarfiaskoj, monto/unmount, formato, kaj ejektaj operacioj.

Famaj membroj
|---|---|
3000
3001
3010
3012
3021
3027
3032

### FilestemError (4000-4999)

Kovras dosiersistem operacioerarojn: aliro/permisio, dosierŝlosado, kunpremado/decompression/encryption, padtemoj, simbolaj ligiloj, dividante malobservojn, kaj generalon I/Operaciojn.

Famaj membroj
|---|---|
4000
4001
4013
4011
4023
4024
4028

### Lokalizo (5000-5999)

Kovras erarojn specifajn al la lokalizo dukto: vortaroj, kodigado, loka validumado, pluralo formoj, ekstera traduko APIs (aŭto, havebleco, atendovico, tempigo), kaj kordformato.

Famaj membroj
|---|---|
5000
5001
5007
5014
5015
5016
5018

### AuthenticationError (6000-6999)

Kovras konfirmon kaj aprobon: akreditaĵoj, ĵetonoj (refresh/aliro), sesioj, MFA/2FA, biometrikoj, atestiloj, OAuth, SSO, kaj raporto deklaras (disable, eksvalidiĝis, ŝlosis).

Famaj membroj
|---|---|
6000
6001
6004
6015
6024
6026

### ValidationError (7000-7999)

Kovriloj enigas validumadon: formatkontroloj ( retpoŝto, telefono, URL, JSON, XML, dattempo), intervalo/longaj limoj, konvertiĝofiaskoj, postulataj kampoj, padrono/regex, kaj pasvorton komplekseco.

Famaj membroj
|---|---|
7000
7003
7016
7018

### Error (8000-8999)

Kovras konfiguracion kaj valorojn: dosieraliro, analizado, validumado, sekretoj/esenca trezorejo, ligokordoj, DI, havas flagojn, mediovariablojn, kaj skemo-/inversio misaglojn.

Famaj membroj
|---|---|
8000
8001
8016
8019

### GeneralError (9000-9999)

Catch-all por aplikiĝ-kovrantaj eraroj: memoro, konsento, licencado, indico limiganta, surfadenigadon, rimedadministradon, trajtosubtenon, kaj netraktitajn esceptojn.

Famaj membroj
|---|---|
9000
9004
9007
9015
9014

## dukto enums

### Procedoj

Defioj la sinsekvaj stadioj de la aŭtomata traduko dukto:

Valora Valoro
|-------|------|-------------|
0
1
2
3
4
5

### Situo de lokalizo

Kind of realtempa mesaĝo elsendita per la dukto:

Valora Valoro
|-------|------|---------|
0
1
2
3
4
5
6

### Traduko de tradukado Cela Celo

Specifu kiu enhavo tajpas traduki:

Valora Valoro
|-------|------|---------------|
0
1
2

### frazo

Tracks CRUD-simila ŝanĝŝtato por lokalizo-vortaj kontribuoj:

Valora Valoro
|-------|------|
0
1
2
3

### Komparo

Komparofunkciigistoj uzitaj por analizado/filtrilvaloroj:

Valora Valoro
|-------|------|----------|
0
1
2
3
4
5
6

### Sekso

Grammatical/socia sekso por lokalizo:

Valora Valoro
|-------|------|
0
1
2
3

## Uzante erarkodojn

### En dukto raportas

Traduko eraroj estas portitaj en diskoj:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### En API respondoj

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Humanizing ajna kodo

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
