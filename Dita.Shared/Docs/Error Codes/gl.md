# Códigos de erro

Dita uses a **range-partitioned, unified error code architecture** that provides both domain-specific enums and a single catch-all `ErrorCode` type. Every error in the system — from network failures to disk I/O, from authentication to configuration — is represented by a member of this hierarchy.

## Arquitectura

### Distribución de rango

rango
|-------|----------|----------|
1000–1999
2000–2999
3000-3999
4000-4999
5000-5999
6000-6999
7.000-7999
8000-8999
9000-999

### Modelo Dual-enum

Each error domain is represented by **both** a focused sub-enum (e.g. `NetworkError`) and entries in the unified `ErrorCode` enum. The sub-enums use bare names; the unified enum prefixes names with the category:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

Isto permite que o código funcione con tipos específicos de dominio cando se coñece o contexto, mentres que tamén soporta o manexo xenérico de erros que funciona en todos os dominios.

### sentinel

Cada sub-enum define como o valor base do seu rango. O método recoñece e devolve.

## ErrorCode class

O enum consolida todos os valores sub-enum nun só tipo con intervalos enteiros **non solapados**. A clase estática compañeira proporciona unha humanización:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Lóxica humanización

seguindo un enfoque de convención sobre configuración:

1. Os nomes de PascalCase divídense en palabras por regex
2. Os acrónimos coñecidos son normalizados (Io → I/O, Api → API, Dns → DNS, Http → HTTP, Ssl → SSL, Mfa → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. Os tokens de todas as follas (por exemplo) consérvanse
4. Valores que acaban a cambio

## Enums específicos de dominio

### NetworkError (1000-1999)

Cubre DNS, SSL/TLS, proxies, pasarelas, erros de protocolo HTTP, conectividade e problemas de ciclo de vida de solicitude.

Notables deputados
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

### Almacenamento (2000–2999)

Cubra conexións de base de datos, transaccións (commit/rollback/timeout), integridade (constracións, deadlocks, claves estranxeiras), xestión de esquemas, copia de seguridade / restauración, replicación e cota.

Notables deputados
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

Cubra discos físicos de baixo nivel e erros de unidade: sectores malos, fallos SMART, degradación de RAID, táboas de partición, fallos de hardware, montaxe / montaxe, formato e operacións de proxecto.

Notables deputados
|---|---|
3.000
3001
3010
3012
3021
3027
3032

### FileSystemError (4000-4999)

Cubre erros de operación do sistema de ficheiros: acceso/permisión, bloqueo de ficheiros, compresión/descompresión/encriptación, problemas de camiños, ligazóns simbólicas, violacións compartidas e operacións xerais de I/O.

Notables deputados
|---|---|
4000
4001
4013
4011
4023
4024
4028

### Error de localización (5000-5999)

Cubre erros específicos do oleoduto de localización: dicionarios, codificación, validación local, formularios plurales, APIs de tradución externas (auth, dispoñibilidade, cola, timeout) e formato de cadea.

Notables deputados
|---|---|
5000
5001
5007
5014
5015
5016
5018

### Autenticación Error (6000-6999)

Cubre autenticación e autorización: credenciais, tokens (refresh/access), sesións, MFA/2FA, biometría, certificados, OAuth, SSO e estados de contas (disable, expirado, bloqueado).

Notables deputados
|---|---|
6000
6001
6004
6015
6024
6026

### Validación Error (7000-7999)

Cubre a validación de entradas: cheques de formato (correo, teléfono, URL, JSON, XML, datatime), restricións de rango/longas, fallos de conversión, campos requiridos, padrón/regex e complexidade do contrasinal.

Notables deputados
|---|---|
7000
7003
7016
7018

### Configuración (8000-8999)

Cubre configuración e configuración: acceso a ficheiros, análise, validación, segredos / bóveda de chave, cadeas de conexión, DI, bandeiras de características, variables de ambiente, e discordancias de esquema/versión.

Notables deputados
|---|---|
8000
8001
8016
8019

### GeneralError (9000-999)

Catch-all para erros de toda a aplicación: memoria, concurrencia, licenza, limitación de taxa, threading, xestión de recursos, soporte de recursos, e excepcións sen concesións.

Notables deputados
|---|---|
9000
9004
9007
9015
9014

## Pipeline enums

### proceso de fase

Define as fases do proceso de tradución automática:

Valor
|-------|------|-------------|
0
1
2
3
4
5

### LocalizaciónMessageType

Tipo de mensaxe en tempo real emitido polo pipeline:

Valor
|-------|------|---------|
0
1
2
3
4
5
6

### Tradución Obxectivo

Especifica o tipo de contido a traducir:

Valor
|-------|------|---------------|
0
1
2

### Frase Cambio

Tracks Cambio de estado para entradas do dicionario de localización:

Valor
|-------|------|
0
1
2
3

### Comparación

Operadores de comparación utilizados para avaliar / filtrar valores:

Valor
|-------|------|----------|
0
1
2
3
4
5
6

### Sexo

Xénero gramatical/social para a localización:

Valor
|-------|------|
0
1
2
3

## Códigos de erro

### Informes de pipeline

Os erros de tradución realízanse nos rexistros:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### Respostas da API

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Humanizar calquera código

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
