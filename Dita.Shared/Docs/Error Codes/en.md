# Error Codes

Dita uses a **range-partitioned, unified error code architecture** that provides both domain-specific enums and a single catch-all `ErrorCode` type. Every error in the system — from network failures to disk I/O, from authentication to configuration — is represented by a member of this hierarchy.

## Architecture

### Range allocation

| Range | Category | Sub-enum |
|-------|----------|----------|
| 1000–1999 | Network | `NetworkError` |
| 2000–2999 | Storage / Database | `StorageError` |
| 3000–3999 | Disk / Physical drive | `DiskError` |
| 4000–4999 | File system / I/O | `FileSystemError` |
| 5000–5999 | Localization / Translation | `LocalizationError` |
| 6000–6999 | Authentication / Authorization | `AuthenticationError` |
| 7000–7999 | Validation | `ValidationError` |
| 8000–8999 | Configuration | `ConfigurationError` |
| 9000–9999 | General / Miscellaneous | `GeneralError` |

### Dual-enum pattern

Each error domain is represented by **both** a focused sub-enum (e.g. `NetworkError`) and entries in the unified `ErrorCode` enum. The sub-enums use bare names; the unified enum prefixes names with the category:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

This allows code to work with domain-specific types when the context is known, while also supporting generic error handling that works across all domains.

### `None` sentinel

Every sub-enum defines `None` as the base value of its range (e.g. `NetworkError.None = 1000`). The `ErrorCodeText.ErrorText()` method recognizes this and returns `"No error"`.

## ErrorCode class

The `ErrorCode` enum consolidates all sub-enum values into a single type with **non-overlapping** integer ranges. The companion `ErrorCodeText` static class provides humanization:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Humanization logic

`ErrorCodeText` follows a convention-over-configuration approach:

1. PascalCase names are split into words via regex
2. Known acronyms are normalized (Io → I/O, Api → API, Dns → DNS, Http → HTTP, Ssl → SSL, Mfa → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. All-caps tokens (e.g. `API`) are preserved
4. Values ending in `None` return `"No error"`

## Domain-specific enums

### NetworkError (1000–1999)

Covers DNS, SSL/TLS, proxies, gateways, HTTP protocol errors, connectivity, and request lifecycle problems.

| Notable members | Value |
|---|---|
| `None` | 1000 |
| `BadGateway` | 1001 |
| `CertificateValidationFailed` | 1002 |
| `ConnectionRefused` | 1003 |
| `ConnectionReset` | 1004 |
| `ConnectionTimeout` | 1005 |
| `DnsResolutionFailed` | 1006 |
| `GatewayTimeout` | 1007 |
| `HostUnreachable` | 1008 |
| `HttpProtocolError` | 1009 |
| `InvalidUrl` | 1010 |
| `SslHandshakeFailed` | 1019 |
| `TooManyRedirects` | 1020 |
| `UnknownNetworkError` | 1021 |

### StorageError (2000–2999)

Covers database connections, transactions (commit/rollback/timeout), integrity (constraints, deadlocks, foreign keys), schema management, backup/restore, replication, and quota.

| Notable members | Value |
|---|---|
| `None` | 2000 |
| `ConnectionFailed` | 2003 |
| `ConnectionPoolExhausted` | 2004 |
| `DatabaseCorrupted` | 2007 |
| `DeadlockDetected` | 2010 |
| `DuplicateKey` | 2012 |
| `ForeignKeyViolation` | 2013 |
| `MigrationFailed` | 2018 |
| `SchemaMismatch` | 2023 |
| `UnknownStorageError` | 2029 |

### DiskError (3000–3999)

Covers low-level physical disk and drive errors: bad sectors, SMART failures, RAID degradation, partition tables, hardware failures, mount/unmount, format, and eject operations.

| Notable members | Value |
|---|---|
| `None` | 3000 |
| `BadSector` | 3001 |
| `DiskFull` | 3010 |
| `DiskMountFailed` | 3012 |
| `HardwareFailure` | 3021 |
| `SmartFailure` | 3027 |
| `WriteError` | 3032 |

### FileSystemError (4000–4999)

Covers file system operation errors: access/permission, file locking, compression/decompression/encryption, path issues, symbolic links, sharing violations, and general I/O operations.

| Notable members | Value |
|---|---|
| `None` | 4000 |
| `AccessDenied` | 4001 |
| `FileNotFound` | 4013 |
| `FileLocked` | 4011 |
| `PathTooLong` | 4023 |
| `PermissionDenied` | 4024 |
| `SharingViolation` | 4028 |

### LocalizationError (5000–5999)

Covers errors specific to the localization pipeline: dictionaries, encoding, locale validation, plural forms, external translation APIs (auth, availability, queue, timeout), and string formatting.

| Notable members | Value |
|---|---|
| `None` | 5000 |
| `DictionaryCorrupted` | 5001 |
| `LanguageNotSupported` | 5007 |
| `TranslationApiUnavailable` | 5014 |
| `TranslationFailed` | 5015 |
| `TranslationQueueFull` | 5016 |
| `TranslationTimeout` | 5018 |

### AuthenticationError (6000–6999)

Covers authentication and authorization: credentials, tokens (refresh/access), sessions, MFA/2FA, biometrics, certificates, OAuth, SSO, and account states (disabled, expired, locked).

| Notable members | Value |
|---|---|
| `None` | 6000 |
| `AccessForbidden` | 6001 |
| `AccountLocked` | 6004 |
| `MfaRequired` | 6015 |
| `TokenExpired` | 6024 |
| `Unauthenticated` | 6026 |

### ValidationError (7000–7999)

Covers input validation: format checks (email, phone, URL, JSON, XML, datetime), range/length constraints, conversion failures, required fields, pattern/regex, and password complexity.

| Notable members | Value |
|---|---|
| `None` | 7000 |
| `InvalidEmailFormat` | 7003 |
| `MissingRequiredField` | 7016 |
| `PasswordComplexityNotMet` | 7018 |

### ConfigurationError (8000–8999)

Covers configuration and settings: file access, parsing, validation, secrets/key vault, connection strings, DI, feature flags, environment variables, and schema/version mismatches.

| Notable members | Value |
|---|---|
| `None` | 8000 |
| `ConfigurationFileNotFound` | 8001 |
| `MissingRequiredConfiguration` | 8016 |
| `SecretDecryptionFailed` | 8019 |

### GeneralError (9000–9999)

Catch-all for application-wide errors: memory, concurrency, licensing, rate limiting, threading, resource management, feature support, and unhandled exceptions.

| Notable members | Value |
|---|---|
| `None` | 9000 |
| `ConcurrencyConflict` | 9004 |
| `InternalError` | 9007 |
| `RateLimitExceeded` | 9015 |
| `OutOfMemory` | 9014 |

## Pipeline enums

### ProcessStage

Defines the sequential stages of the automatic translation pipeline:

| Value | Name | Description |
|-------|------|-------------|
| 0 | `Iddle` | No active processing |
| 1 | `CheckServers` | Environment and translation server validation |
| 2 | `TranslateCountries` | Country name synchronisation |
| 3 | `TranslateJsonFiles` | JSON localization dictionary synchronisation |
| 4 | `TranslateMarkdownFiles` | Markdown documentation translation |
| 5 | `StoringResults` | Final result aggregation and persistence |

### LocalizationMessageType

Kind of real-time message emitted by the pipeline:

| Value | Name | Meaning |
|-------|------|---------|
| 0 | `StageStarted` | A pipeline stage began execution |
| 1 | `StageCompleted` | A pipeline stage finished successfully |
| 2 | `StageFailed` | A pipeline stage encountered a fatal error |
| 3 | `PipelineCompleted` | All stages completed |
| 4 | `PipelineFailed` | Unrecoverable pipeline error |
| 5 | `Progress` | Informational update |
| 6 | `Warning` | Non-fatal warning |

### TranslationTarget

Specifies what content type to translate:

| Value | Name | Maps to stage |
|-------|------|---------------|
| 0 | `Languages` | `TranslateCountries` |
| 1 | `JsonFiles` | `TranslateJsonFiles` |
| 2 | `MDFiles` | `TranslateMarkdownFiles` |

### PhraseChange

Tracks CRUD-like change state for localization dictionary entries:

| Value | Name |
|-------|------|
| 0 | `NoChange` |
| 1 | `Added` |
| 2 | `Updated` |
| 3 | `Removed` |

### Comparison

Comparison operators used for evaluating/filtering values:

| Value | Name | Operands |
|-------|------|----------|
| 0 | `Equal` | single |
| 1 | `Greater` | single |
| 2 | `GreaterOrEqual` | single |
| 3 | `Less` | single |
| 4 | `LessOrEqual` | single |
| 5 | `Between` | two (lower, upper) |
| 6 | `Any` | no restriction |

### Gender

Grammatical/social gender for localization:

| Value | Name |
|-------|------|
| 0 | `Male` |
| 1 | `Female` |
| 2 | `Neutral` |
| 3 | `Other` |

## Using error codes

### In pipeline reports

Translation errors are carried in `TranslationError` records:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### In API responses

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Humanizing any code

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