# Errore-kodeak

Dita uses a **range-partitioned, unified error code architecture** that provides both domain-specific enums and a single catch-all `ErrorCode` type. Every error in the system — from network failures to disk I/O, from authentication to configuration — is represented by a member of this hierarchy.

## Arkitektura

### Barrutiko esleipena

Barrutia
|-------|----------|----------|
1000-1999
2000-2999
3000-3999
4000-4999
5000-5999
6000-6999
7000-7999
8000-8999
9000-9999

### Eredu bikoitza

Errore-domeinu bakoitza **biak** azpi-enum zentratua (adib.) da eta sarrera bateratuak. Azpienumek izen hutsak erabiltzen dituzte; enum bateratuak kategoriaren izenak ditu:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

Honek domeinu-mota jakinekin lan egiteko aukera ematen du testuingurua ezagutzen denean, eta, aldi berean, domeinu guztietan funtzionatzen duen errore generikoen kudeaketa onartzen du.

### sentinel

Azpi-enum bakoitzak bere barrutiaren oinarrizko balio gisa definitzen du (adibidez, ). Metodoak hau ezagutu eta itzultzen du.

## ErrorCode klasea

Enum-ak azpi-enum balio guztiak mota bakar batean finkatzen ditu, **gainjarri gabe** osoko barrutiekin. Klase estatiko lagunak humanizazioa eskaintzen du:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Humanizazio logikoa

konbentzio-konfigurazioari jarraitzen dio:

1. PascalCase izenak hitzen bidez zatitzen dira regex bidez
2. Akronimo ezagunak normalizatuak dira (Io → I/O, Api → API, Dns → DNS, Htp → HTTP, Ssl → SSL, Mfa → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. Kapturatu guztien tokenak (adib.) mantentzen dira
4. Trukean amaitzen diren balioak

## Domeinu espezifikoak

### NetworkError (1000-1999)

DNS, SSL/TLS, proxy-ak, atebideak, HTTP protokoloaren erroreak, konektibitatea eta bizi-zikloaren arazoak azaltzen ditu.

Kide nabarmenak
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

Datu-basearen konexioak, eragiketak (konmit/rollback/timeout), osotasuna (konstraintak, desblokeoak, kanpoko gakoak), eskema-kudeaketa, babeskopia/biltegia, erreplikazioa eta kuota.

Kide nabarmenak
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

### Disko-Errorea (3000-3999)

Maila txikiko disko fisikoa eta disko-erroreak estaltzen ditu: sektore txarrak, SMART hutsegiteak, RAID degradazioa, partizio-taulak, hardware-hutsegiteak, muntatzea/desmuntatzea, formatua eta egoztea.

Kide nabarmenak
|---|---|
3000
3001
3010
3012
3021
3027
3032

### FileSystemError (4000-4999)

Fitxategi-sistemaren eragiketaren akatsak estaltzen ditu: sarbide/baimena, fitxategi-blokeaketa, konpresio/deskonpresioa/enkriptazioa, bide-gaiak, esteka sinbolikoak, bortxaketak eta I/O eragiketa orokorrak.

Kide nabarmenak
|---|---|
4000
4001
4013
4011
4023
4024
4028

### Lokalizazio-Errorea (5000-5999)

Lokalizazio-hodiaren berariazko akatsak estaltzen ditu: hiztegiak, kodeketa, balioztapen lokala, forma anitzak, kanpoko itzulpen- APIak (auth, erabilgarritasuna, ilara, denbora-muga), eta kate-formatua.

Kide nabarmenak
|---|---|
5000
5001
5007
5014
5015
5016
5018

### Autentifikazio-Errorea (6000-6999)

Autentifikatzea eta baimena estaltzen ditu: kredentzialak, tokenak (freskoa/sarbidea), saioak, MFA/2FA, biometriak, ziurtagiriak, OAuth, SSO, eta kontu-egoerak (desgaituta, iraungita, blokeatuta).

Kide nabarmenak
|---|---|
6000
6001
6004
6015
6024
6026

### ValidationError (7000-7999)

Sarrerako balidazioa estaltzen du: formatu-kontrolak (posta elektronikoa, telefonoa, URLa, JSON, XML, data-ordua), barruti/luzera-mugak, bihurketa-hutsegiteak, beharrezko eremuak, eredua/regex eta pasahitzaren konplexutasuna.

Kide nabarmenak
|---|---|
7000
7003
7016
7018

### Konfigurazio-Errorea (8000-8999)

Konfigurazioa eta ezarpenak estaltzen ditu: fitxategi-sarbidea, analisia, balidazioa, sekretu/gako ganga, konexio-kateak, DI, eginbide-banderak, ingurune-aldagaiak eta eskema/bertsioa ez datoz bat.

Kide nabarmenak
|---|---|
8000
8001
8016
8019

### GeneralError (9000-9999)

Denak aplikazio-mailako erroreetarako: memoria, konkurrentzia, lizentziak, tasa mugatzea, haria, baliabideen kudeaketa, eginbideen euskarria eta kudeatu gabeko salbuespenak.

Kide nabarmenak
|---|---|
9000
9004
9007
9015
9014

## kanalizazio enums

### Prozesamendua

Kanalizazio automatikoaren urrats sekuentzialak definitzen ditu:

Balioa
|-------|------|-------------|
0
1
2
3
4
5

### LocalizationMesssageType

Kanalizazioak igorritako denbora errealeko mezua:

Balioa
|-------|------|---------|
0
1
2
3
4
5
6

### Itzulpena Helburua

Zein eduki mota itzultzeko zehazten du:

Balioa
|-------|------|---------------|
0
1
2

### PhraseChange

Pistak CRUD-en moduko aldaketa-egoera hiztegi-sarreren lokalizaziorako:

Balioa
|-------|------|
0
1
2
3

### Konparazioa

Balioak ebaluatzeko eta iragazteko erabiltzen diren konparazio-operadoreak:

Balioa
|-------|------|----------|
0
1
2
3
4
5
6

### Generoa

Genero gramatikala/soziala lokalizatzeko:

Balioa
|-------|------|
0
1
2
3

## Errore-kodeak erabiltzea

### Kanalizazioko txostenetan

Itzulpen-erroreak erregistroetan egiten dira:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### API erantzunetan

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Edozein kode humanizatzen

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
