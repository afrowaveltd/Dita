# Veakoodid

Dita kasutab **range-partitioned, ühtse veakoodi arhitektuuri**, mis pakub nii domeenipõhiseid enums kui ka ühte catch-all tüüpi. Iga viga süsteemis - alates võrgu tõrgetest kuni ketta I/O, alates autentimisest kuni konfiguratsioonini - esindab selle hierarhia liige.

## Arhitektuur

### Vahemiku jaotamine

ulatus
|-------|----------|----------|
1000–1999
2000–2999
3000–3999
4000–4999
5000–5999
6000–6999
7000–7999
8000–8999
9000–9999

### Kahekordne aastakäik

Each error domain is represented by **both** a focused sub-enum (e.g. `NetworkError`) and entries in the unified `ErrorCode` enum. The sub-enums use bare names; the unified enum prefixes names with the category:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

See võimaldab koodil töötada domeenispetsiifiliste tüüpidega, kui kontekst on teada, toetades samal ajal ka üldist veakäsitlust, mis töötab kõigis domeenides.

### sentinel

Iga alam-enum määratleb oma vahemiku (nt ) baasväärtusena. Meetod tunneb selle ära ja tagastab .

## Vigakoodi klass

Enum koondab kõik alamühiku väärtused ühte tüüpi ** mittekattuvate ** täisarvuvahemikega. Kaaslase staatiline klass pakub humaniseerimist:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Humaniseerimise loogika

järgib konfigureerimisel tavapõhist lähenemisviisi:

1. PascalCase'i nimed jagatakse regexi kaudu sõnadeks
2. Tuntud akronüümid on normaliseeritud (Io → I/O, Api → API, Dns → DNS, Http → HTTP, Ssl → SSL, Mfa → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. Säilivad kõik mütsid ( nt )
4. Vastutasuks lõppevad väärtused

## Domeenipõhised enums

### võrguvead (1000–1999)

Hõlmab DNS-i, SSL/TLS-i, proksi, lüüsi, HTTP-protokolli vigu, ühenduvust ja päringu elutsükli probleeme.

Märkimisväärsed liikmed
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

### hoidla (2000–2999)

Hõlmab andmebaasiühendusi, tehinguid (commit/rollback/timeout), terviklikkust (piirangud, ummikseisud, võõrvõtmed), skeemi haldamist, varundamist/taastamist, replikatsiooni ja kvooti.

Märkimisväärsed liikmed
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

Hõlmab madala taseme füüsilisi ketta- ja draivivigu: halvad sektorid, SMART-rikked, RAID-i lagunemine, partitsioonitabelid, riistvararikked, ühendamine / lahutamine, formaat ja eject operatsioonid.

Märkimisväärsed liikmed
|---|---|
3000
3001
3010
3012
3021
3027
3032

### FailSystemError (4000–4999)

Hõlmab failisüsteemi operatsioonivigu: juurdepääs/luba, failide lukustamine, tihendamine/dekompressioon/krüptimine, asukohaprobleemid, sümboolsed lingid, rikkumised ja üldised I/O operatsioonid.

Märkimisväärsed liikmed
|---|---|
4000
4001
4013
4011
4023
4024
4028

### LokaliseerimineError (5000–5999)

Hõlmab lokaliseerimistorule omaseid vigu: sõnaraamatuid, kodeeringut, lokaadi valideerimist, mitmuse vorme, välise tõlke rakendusliideseid (aut, kättesaadavus, järjekord, aegumine) ja stringivormindust.

Märkimisväärsed liikmed
|---|---|
5000
5001
5007
5014
5015
5016
5018

### AutentimineError (6000–6999)

Hõlmab autentimist ja autoriseerimist: volikirjad, märgid (värskendus/juurdepääs), seansid, MFA/2FA, biomeetria, sertifikaadid, OAuth, SSO ja konto olekud (keelatud, aegunud, lukustatud).

Märkimisväärsed liikmed
|---|---|
6000
6001
6004
6015
6024
6026

### valideerija (7000–7999)

Hõlmab sisendi valideerimist: vormingu kontroll (e-post, telefon, URL, JSON, XML, kuupäevaaeg), vahemiku/pikkuse piirangud, teisendamise vead, nõutavad väljad, muster/regex ja parooli keerukus.

Märkimisväärsed liikmed
|---|---|
7000
7003
7016
7018

### SeadistamineError (8000–8999)

Hõlmab konfiguratsiooni ja seadistusi: failijuurdepääs, parsimine, valideerimine, saladused/võtmevõti, ühendusestringid, DI, funktsioonilipud, keskkonnamuutujad ja skeemi/ versiooni mittevastavused.

Märkimisväärsed liikmed
|---|---|
8000
8001
8016
8019

### GeneralError (9000–9999)

Püüa kõik rakendusesiseste vigade puhul: mälu, vastavus, litsentseerimine, määra piiramine, lõimestamine, ressursihaldus, funktsioonitoetus ja käsitlemata erandid.

Märkimisväärsed liikmed
|---|---|
9000
9004
9007
9015
9014

## Torujuhtme anumad

### protsessietapp

Määrab automaatse tõlketorustiku järjestikused etapid:

Väärtus
|-------|------|-------------|
0
1
2
3
4
5

### LokaliseerimineMessageType

Torujuhtme kaudu edastatava reaalajas sõnumi tüüp:

Väärtus
|-------|------|---------|
0
1
2
3
4
5
6

### Tõlkimine Sihtmärk

Määrab, millist sisutüüpi tõlkida:

Väärtus
|-------|------|---------------|
0
1
2

### fraasivahetus

Lokaliseerimissõnastiku kirjete rajad CRUD- laadne muutus:

Väärtus
|-------|------|
0
1
2
3

### Võrdlus

Väärtuste hindamiseks/filtreerimiseks kasutatud võrdlusoperaatorid:

Väärtus
|-------|------|----------|
0
1
2
3
4
5
6

### Soolise

Grammaatiline/sotsiaalne sugu lokaliseerimiseks:

Väärtus
|-------|------|
0
1
2
3

## Veakoodide kasutamine

### Torujuhtmete aruanded

Tõlkevead kantakse kirjetesse:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### API vastustes

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Mis tahes koodi humaniseerimine

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
