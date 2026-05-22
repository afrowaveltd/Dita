# Codis d' error

Dita uses a **range-partitioned, unified error code architecture** that provides both domain-specific enums and a single catch-all `ErrorCode` type. Every error in the system — from network failures to disk I/O, from authentication to configuration — is represented by a member of this hierarchy.

## Arquitectura

### Assignació d' abast

Interval
|-------|----------|----------|
100019991
2000999299
3000 2001- 1999
40099499
50005.000599
60099699
7007299
8000 2001- 200899
9000999

### Patró dual

Cada domini d' error és representat per ** Absions ** un subenum centrat (p. ex.) i entrades en l' enumeració unificat. Els subenums usen noms nus; els prefixos unificats amb la categoria:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

Això permet treballar en codi amb tipus específics de domini quan es coneix el context, mentre que també permet la gestió d' errors genèrics que funcionen a través de tots els dominis.

### sentinella

Cada subenum defineix com el valor base del seu abast (p. ex. ). El mètode reconeix això i retorna .

## Classe de codi d' error

The `ErrorCode` enum consolidates all sub-enum values into a single type with **non-overlapping** integer ranges. The companion `ErrorCodeText` static class provides humanization:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Lògica de humanitatització

segueix un enfocament de configuració de la convenció:

1. PascalCase noms es divideixen en paraules via regex
2. Els acrònims són normalitzats (Io eka I/O, Api  API, Dns eka DNS, Htp HTTP, Ssl eka SSL, Mfa Manveen MFA, OAuth eka OAuth, Sso SSO, Xml eka XML, Json JSON, URL  URL  URL de l' URL ANSI)
3. Totes les fitxes (p. ex.) es preservaen
4. Valors finals a canvi

## Enums específics de domini

### Error de xarxa (1999199)

Cobreix DNS, SSL/TLS, intermediaris, portadors, errors de protocol HTTP, connectivitat i sol· licitar problemes de cicle vital.

Membres noables
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

### Error d' emmagatzematge (20002299)

Les connexions de bases de dades, assentaments (comproteç/timeout), integritat (constracions, bloqueigs, claus externes), gestió d' esquemes, còpia de seguretat/ magatzem, replicació i quotes.

Membres noables
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

### Error de disc (30009399)

Cobreix errors físics de disc i unitat de baixa: sectors dolents, errors d' error de SMART, degradació ARID, taules de particions, fracassos de maquinari, muntatge/ desmunteu, format i operacions d' expulsió.

Membres noables
|---|---|
3000
3001
3010
3012
3021
3027
3032

### Error del sistema de fitxers (40099499)

Errors d' operació del sistema de fitxers: accés/missió, bloqueig de fitxers, compressió/ descompressió/ encrypt, problemes de camí, enllaços simbòlics, compartint violacions i operacions d' E/ S.

Membres noables
|---|---|
4.000
4001
4013
4011
4023
4024
4028

### Error de localització (5000; 1999)

Errors específics de la canonada localització: diccionaris, codificació, validació del locale, formularis en plural, API de traducció externes (auth, disponibilitat, cua, temps d' espera), i format de cadenes.

Membres noables
|---|---|
5000
5001
5007
5014
5015
5016
5018

### Error d' autenticació (60099699)

Descarrega autenticació i autorització: credencials, fitxes (refresh/ access), sessions, MFA/2FA, biomètrics, certificats, OAuth, SSO, i els estats del compte (deshabilitats, bloquejats).

Membres noables
|---|---|
600
6001
6004
6015
6024
6026

### Error de validació (7007299)

Separa la validació d' entrada: comprovar el format (correu, telèfon, URL, JSON, XML, data, interval/ longitud, restriccions de conversió, camps requerits, patró/regex, i complexitat de contrasenya.

Membres noables
|---|---|
7, 000
7003
7016
7018

### Error de configuració (8000; 1999)

Descarrega la configuració i l' arranjament: l' accés de fitxer, l' anàlisi, la validació, els secrets/ clau voltes, cadenes de connexió, els indicadors DI, les funcionalitats, les variables d' entorn i els desaparells.

Membres noables
|---|---|
8000
8001
8016
8019

### Error general (9000; 1999)

Agafa tots els errors de tot l' aplicació: memòria, probabilitat, llicència, taxa límit, fil, gestió de recursos, implementació de funcionalitats i excepcions sense gestionar.

Membres noables
|---|---|
9000
9004
9007
9015
9014

## Enums de conducte

### Processament de processosComment

Defineix les fases seqüencials de la canonada de traducció automàtica:

Valor
|-------|------|-------------|
0
1
2
3
4
5

### LocalitzacióMessageType

Un missatge de temps real emès per la canonada:

Valor
|-------|------|---------|
0
1
2
3
4
5
6

### Traducció Objectiu

Especifica quin tipus de contingut s' ha de traduir:

Valor
|-------|------|---------------|
0
1
2

### FraseChange

Estat del canvi de peces com ara elàstic de la localització del diccionari:

Valor
|-------|------|
0
1
2
3

### Comparació

Els operadors comparatius usats per a avaluar/ filtrar els valors:

Valor
|-------|------|----------|
0
1
2
3
4
5
6

### Gènere

El gènere atòmic/social per a la localització:

Valor
|-------|------|
0
1
2
3

## S' estan usant els codis d' error

### Informes de canonada

S' han produït errors de traducció en els registres:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### En respostes API

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Humanitzar qualsevol codi

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
