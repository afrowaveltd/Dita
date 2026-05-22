# Chybové kódy

Dita uses a **range-partitioned, unified error code architecture** that provides both domain-specific enums and a single catch-all `ErrorCode` type. Every error in the system — from network failures to disk I/O, from authentication to configuration — is represented by a member of this hierarchy.

## Architektura

### Rozdělení rozsahu

Rozsah
|-------|----------|----------|
1000- 1999
2000- 2999
3000- 3999
4000- 4999
550- 5999
6000- 6999
7000- 7999
8000- 8999
9000- 9999

### Dual- enum vzor

Each error domain is represented by **both** a focused sub-enum (e.g. `NetworkError`) and entries in the unified `ErrorCode` enum. The sub-enums use bare names; the unified enum prefixes names with the category:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

To umožňuje, aby kód pracoval s domain- specifickými typy, když je znám kontext, a zároveň podporuje obecné zpracování chyb, které funguje ve všech oblastech.

### pentinel

Každé subenum definuje jako základní hodnotu svého rozsahu (např.). Metoda to rozpozná a vrátí.

## Třída ErrorCode

The `ErrorCode` enum consolidates all sub-enum values into a single type with **non-overlapping** integer ranges. The companion `ErrorCodeText` static class provides humanization:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Logika humanizace

sleduje přístup konvence - nadkonfigurace:

1. Jména PascalCase jsou rozdělena do slov pomocí regexu
2. Známé zkratky jsou normalizovány (Io → I / O, Api → API, Dns → DNS, Http → HTTP, Ssl → SSL, Mfa → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. All- Caps žetony (např.) jsou zachovány
4. Hodnoty končící na oplátku

## Domain- specific enums

### NetworkChyba (1000- 1999)

Zahrnuje DNS, SSL / TLS, proxies, brány, HTTP protokoly chyby, konektivita, a požádat o celoživotní problémy.

Významní členové
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

### StorageError (2000-2999)

Zahrnuje připojení k databázi, transakce (revize / rollback / timeout), integritu (omezení, zámky, cizí klíče), správu schémat, zálohování / obnovení, replikaci a kvótu.

Významní členové
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

### Chyba disku (3000-3999)

Zahrnuje chyby na fyzickém disku a pohonu na nízké úrovni: špatné sektory, selhání SMART, degradaci RAID, tabulky oddílů, selhání hardwaru, montáž / odstavení, formátování a vysunutí.

Významní členové
|---|---|
3000
3001
3010
3012
3021
3027
3032

### FileSystemError (4000- 4999)

Zahrnuje chyby v provozu souborového systému: přístup / oprávnění, blokování souborů, komprese / dekomprese / šifrování, problémy s cestami, symbolické odkazy, porušení sdílení a obecné operace I / O.

Významní členové
|---|---|
4000
4001
4013
4011
4023
4024
4028

### LocalizationError (5000- 5999)

Zahrnuje chyby specifické pro lokalizační potrubí: slovníky, enkódování, validace locale, množné formuláře, externí překlady API (auth, dostupnost, fronta, timeout), a formátování řetězců.

Významní členové
|---|---|
5000
5001
5007
5014
5015
5016
5018

### Autentizace Chyba (6000- 6999)

Zahrnuje autentizaci a autorizaci: pověřovací listiny, žetony (obnova / přístup), relace, MFA / 2FA, biometrika, certifikáty, OAuth, SSO a stav účtu (vypnuto, uzamčeno).

Významní členové
|---|---|
6000
6001
6004
6015
6024
6026

### Chyba validace (7000- 7999)

Zahrnuje validaci vstupu: kontroly formátu (e-mail, telefon, URL, JSON, XML, datetime), omezení rozsahu / délky, selhání konverze, požadovaná pole, vzor / regex a složitost hesla.

Významní členové
|---|---|
7000
7003
7016
7018

### Chyba konfigurace (8000- 8999)

Zahrnuje konfiguraci a nastavení: přístup k souboru, parsing, validace, tajemství / klíč trezor, řetězce připojení, DI, funkce vlajky, proměnné prostředí, a schéma / verze neshody.

Významní členové
|---|---|
8000
8001
8016
8019

### GeneralError (9000-9999)

Catch- all for application- wide blues: memory, concurrency, licencing, rate limiting, threading, resource management, feature support, and unhanded exceptions.

Významní členové
|---|---|
9000
9004
9007
9015
9014

## Zásuvky potrubí

### fáze zpracování

Definuje postupné fáze automatického překladového potrubí:

Hodnota
|-------|------|-------------|
0
1
2
3
4
5

### LokalizationMessageType

Druh zprávy v reálném čase vydávané potrubím:

Hodnota
|-------|------|---------|
0
1
2
3
4
5
6

### Překlad Cíl

Určuje, jaký typ obsahu přeložit:

Hodnota
|-------|------|---------------|
0
1
2

### frazisechange

Stopy CRUD- jako změna stavu pro lokalizaci slovníků položky:

Hodnota
|-------|------|
0
1
2
3

### Srovnání

Srovnání operátorů používaných pro hodnocení / filtrování hodnot:

Hodnota
|-------|------|----------|
0
1
2
3
4
5
6

### Pohlaví

Gramatické / společenské pohlaví pro lokalizaci:

Hodnota
|-------|------|
0
1
2
3

## Použití chybových kódů

### Zprávy o potrubí

Chyby překladu jsou v záznamech:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### V odpovědi API

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Humanizace jakéhokoliv kódu

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
