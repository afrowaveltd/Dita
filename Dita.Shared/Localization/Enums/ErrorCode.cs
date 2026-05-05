using System.Text.RegularExpressions;

namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Unified error codes across all categories.
/// </summary>
public enum ErrorCode
{
    /// Network (1000-1999)

    /// <summary>
    /// No network error.
    /// </summary>
    NetworkNone = 1000,
    /// <summary>
    /// Bad gateway error.
    /// </summary>
    BadGateway = 1001,
    /// <summary>
    /// Certificate validation failed.
    /// </summary>
    CertificateValidationFailed = 1002,
    /// <summary>
    /// Connection refused.
    /// </summary>
    ConnectionRefused = 1003,
    /// <summary>
    /// Connection reset.
    /// </summary> 
    ConnectionReset = 1004,
    /// <summary>
    /// Network connection timeout.
    /// </summary>
    NetworkConnectionTimeout = 1005,
    /// <summary>
    /// DNS resolution failed.
    /// </summary>
    DnsResolutionFailed = 1006,
    /// <summary>
    /// Gateway timeout.
    /// </summary>
    GatewayTimeout = 1007,
    /// <summary>
    /// Host unreachable.
    /// </summary>
    HostUnreachable = 1008,
    /// <summary>
    /// HTTP protocol error.
    /// </summary>
    HttpProtocolError = 1009,
    /// <summary>
    /// Invalid URL.
    /// </summary>
    InvalidUrl = 1010,
    /// <summary>
    /// Network interface unavailable.
    /// </summary>
    NetworkInterfaceUnavailable = 1011,
    /// <summary>
    /// Network unreachable.
    /// </summary>
    NetworkUnreachable = 1012,
    /// <summary>
    /// Proxy authentication failed.
    /// </summary>
    ProxyAuthenticationFailed = 1013,
    /// <summary>
    /// Proxy connection error.
    /// </summary> 
    ProxyConnectionError = 1014,
    /// <summary>
    /// Request was cancelled.
    /// </summary>
    RequestCancelled = 1015,
    /// <summary>
    /// Request entity too large.
    /// </summary>
    RequestEntityTooLarge = 1016,
    /// <summary>
    /// Request timeout.
    /// </summary>
    RequestTimeout = 1017,
    /// <summary>
    /// Service unavailable.
    /// </summary>
    ServiceUnavailable = 1018,
    /// <summary>
    /// SSL handshake failed.
    /// </summary>
    SslHandshakeFailed = 1019,
    /// <summary>
    /// Too many redirects.
    /// </summary>
    TooManyRedirects = 1020,
    /// <summary>
    /// Unknown network error.
    /// </summary>
    UnknownNetworkError = 1021,

    // Storage (2000-2999)
    /// <summary>
    /// No storage error.
    /// </summary>
    StorageNone = 2000,
    /// <summary>
    /// Backup operation failed.
    /// </summary>
    BackupFailed = 2001,
    /// <summary>
    /// Checkpoint operation failed.
    /// </summary>
    CheckpointFailed = 2002,
    /// <summary>
    /// Connection to storage failed.
    /// </summary>
    ConnectionFailed = 2003,
    /// <summary>
    /// Connection pool exhausted.
    /// </summary>
    ConnectionPoolExhausted = 2004,
    /// <summary>
    /// Storage connection timeout.
    /// </summary>
    StorageConnectionTimeout = 2005,
    /// <summary>
    /// Constraint violation.
    /// </summary>
    ConstraintViolation = 2006,
    /// <summary>
    /// Database corrupted.
    /// </summary>
    DatabaseCorrupted = 2007,
    /// <summary>
    /// Database locked.
    /// </summary>
    DatabaseLocked = 2008,
    /// <summary>
    /// Database not found.
    /// </summary>
    DatabaseNotFound = 2009,
    /// <summary>
    /// Deadlock detected.
    /// </summary>
    DeadlockDetected = 2010,
    /// <summary>
    /// Data not found.
    /// </summary>
    DataNotFound = 2011,
    /// <summary>
    /// Duplicate key.
    /// </summary>
    DuplicateKey = 2012,
    /// <summary>
    /// Foreign key violation.
    /// </summary>
    ForeignKeyViolation = 2013,
    /// <summary>
    /// Index corrupted.
    /// </summary>
    IndexCorrupted = 2014,
    /// <summary>
    /// Insufficient space.
    /// </summary>
    InsufficientSpace = 2015,
    /// <summary>
    /// Invalid query.
    /// </summary>
    InvalidQuery = 2016,
    /// <summary>
    /// Invalid storage configuration.
    /// </summary>
    InvalidStorageConfiguration = 2017,
    /// <summary>
    /// Migration failed.
    /// </summary>
    MigrationFailed = 2018,
    /// <summary>
    /// Query timeout.
    /// </summary>
    QueryTimeout = 2019,
    /// <summary>
    /// Record already exists.
    /// </summary>
    RecordAlreadyExists = 2020,
    /// <summary>
    /// Replication failed.
    /// </summary>
    ReplicationFailed = 2021,
    /// <summary>
    /// Restore failed.
    /// </summary>
    RestoreFailed = 2022,
    /// <summary>
    /// Storage schema mismatch.
    /// </summary>
    StorageSchemaMismatch = 2023,
    /// <summary>
    /// Storage quota exceeded.
    /// </summary>
    StorageQuotaExceeded = 2024,
    /// <summary>
    /// Table not found.
    /// </summary>
    TableNotFound = 2025,
    /// <summary>
    /// Transaction commit failed.
    /// </summary>
    TransactionCommitFailed = 2026,
    /// <summary>
    /// Transaction rollback failed.
    /// </summary>
    TransactionRollbackFailed = 2027,
    /// <summary>
    /// Transaction timeout.
    /// </summary>
    TransactionTimeout = 2028,
     /// <summary>
    /// Unknown storage error.
    /// </summary>
    UnknownStorageError = 2029,
    /// <summary>
    /// Storage write failed.
    /// </summary>
    StorageWriteFailed = 2030,

    // Disk (3000-3999)
    /// <summary>
    /// No disk error.
    /// </summary>
    DiskNone = 3000,
    /// <summary>
    /// Bad sector found on disk.
    /// </summary>
    BadSector = 3001,
    /// <summary>
    /// Benchmark operation failed due to disk issues.
    /// </summary> 
    BenchmarkFailed = 3002,
    /// <summary>
    /// Boot sector corrupted.
    /// </summary>
    BootSectorCorrupted = 3003,
    /// <summary>
    /// Device is busy.
    /// </summary>
    DeviceBusy = 3004,
    /// <summary>
    /// Device not found.
    /// </summary>
    DeviceNotFound = 3005,
    /// <summary>
    /// Device not ready.
    /// </summary>
    DeviceNotReady = 3006,
    /// <summary>
    /// Disk controller error.
    /// </summary>
    DiskControllerError = 3007,
    /// <summary>
    /// Disk defragmentation failed.
    /// </summary>
    DiskDefragmentationFailed = 3008,
    /// <summary>
    /// Disk eject failed.
    /// </summary>
    DiskEjectFailed = 3009,
    /// <summary>
    /// Disk is full.
    /// </summary>
    DiskFull = 3010,
    /// <summary>
    /// Disk format failed.
    /// </summary>
    DiskFormatFailed = 3011,
    /// <summary>
    /// Disk mount failed.
    /// </summary>
    DiskMountFailed = 3012,
    /// <summary>
    /// Disk is not formatted.
    /// </summary>
    DiskNotFormatted = 3013,
    /// <summary>
    /// Disk is not initialized.
    /// </summary>
    DiskNotInitialized = 3014,
    /// <summary>
    /// Disk partition error.
    /// </summary>
    DiskPartitionError = 3015,
    /// <summary>
    /// Disk quota exceeded.
    /// </summary>
    DiskQuotaExceeded = 3016,
    /// <summary>
    /// Disk unmount failed.
    /// </summary>
    DiskUnmountFailed = 3017,
    /// <summary>
    /// Disk verification failed.
    /// </summary>
    DiskVerificationFailed = 3018,
    /// <summary>
    /// Disk is write-protected.
    /// </summary>
    DiskWriteProtected = 3019,
    /// <summary>
    /// Drive letter is unavailable.
    /// </summary>
    DriveLetterUnavailable = 3020,
    /// <summary>
    /// Hardware failure detected on disk.
    /// </summary>
    HardwareFailure = 3021,
    /// <summary>
    /// I/O error occurred while accessing the disk.
    /// </summary>
    IoError = 3022,
    /// <summary>
    /// Media not present in the drive.
    /// </summary>
    MediaNotPresent = 3023,
    /// <summary>
    /// Partition table is corrupted.
    /// </summary>
    PartitionTableCorrupted = 3024,
    /// <summary>
    /// RAID array is degraded.
    /// </summary>
    RaidDegraded = 3025,
    /// <summary>
    /// Read error occurred while accessing the disk.
    /// </summary>
    ReadError = 3026,
    /// <summary>
    /// S.M.A.R.T. failure predicted on disk.
    /// </summary>
    SmartFailure = 3027,
    /// <summary>
    /// Unsupported disk type detected.
    /// </summary>
    UnsupportedDiskType = 3028,
    /// <summary>
    /// Unknown disk error occurred.
    /// </summary>
    UnknownDiskError = 3029,
    /// <summary>
    /// Volume label is invalid or too long.
    /// </summary>
    VolumeLabelError = 3030,
    /// <summary>
    /// Volume not found or inaccessible.
    /// </summary>
    VolumeNotFound = 3031,
    /// <summary>
    /// Write error occurred while accessing the disk.
    /// </summary>
    WriteError = 3032,
    /// <summary>
    /// Disk is read-only.
    /// </summary>
    ReadOnlyDisk = 3033,
    // File system (4000-4999)
    /// <summary>
    /// No file system error.
    /// </summary>
    FileSystemNone = 4000,
    /// <summary>
    /// Access to the file or directory is denied.
    /// </summary>
    AccessDenied = 4001,
    /// <summary>
    /// The file or directory already exists.
    /// </summary>
    AlreadyExists = 4002,
    /// <summary>
    /// Copy operation failed.
    /// </summary>
    CopyFailed = 4003,
    /// <summary>
    /// Directory is not empty.
    /// </summary>
    DirectoryNotEmpty = 4004,
    /// <summary>
    /// Directory not found.
    /// </summary>
    DirectoryNotFound = 4005,
    /// <summary>
    /// End of file reached.
    /// </summary>
    EndOfFile = 4006,
    /// <summary>
    /// File compression failed.
    /// </summary>
    FileCompressionFailed = 4007,
    /// <summary>
    /// File decompression failed.
    /// </summary>
    FileDecompressionFailed = 4008,
    /// <summary>
    /// File encryption failed.
    /// </summary>
    FileEncryptionFailed = 4009,
    /// <summary>
    /// File is in use.
    /// </summary>
    FileInUse = 4010,
    /// <summary>
    /// File is locked.
    /// </summary>
    FileLocked = 4011,
    /// <summary>
    /// File move operation failed.
    /// </summary>
    FileMoveFailed = 4012,
    /// <summary>
    /// File not found.
    /// </summary>
    FileNotFound = 4013,
    /// <summary>
    /// File size exceeded.
    /// </summary>
    FileSizeExceeded = 4014,
    /// <summary>
    /// File system is corrupted.
    /// </summary>
    FileSystemCorrupted = 4015,
    /// <summary>
    /// File system is read-only.
    /// </summary>
    FileSystemReadOnly = 4016,
    /// <summary>
    /// File system type is unsupported.
    /// </summary>
    FileSystemTypeUnsupported = 4017,
    /// <summary>
    /// Invalid file name.
    /// </summary>
    InvalidFileName = 4018,
    /// <summary>
    /// Invalid file format.
    /// </summary>
    InvalidFileFormat = 4019,
    /// <summary>
    /// Invalid handle.
    /// </summary>
    InvalidHandle = 4020,
    /// <summary>
    /// Invalid path.
    /// </summary>
    InvalidPath = 4021,
    /// <summary>
    /// I/O operation failed.
    /// </summary>
    IoOperationFailed = 4022,
    /// <summary>
    /// Path is too long.
    /// </summary>
    PathTooLong = 4023,
    /// <summary>
    /// File system permission denied.
    /// </summary>
    FileSystemPermissionDenied = 4024,
    /// <summary>
    /// Read operation failed.
    /// </summary>
    ReadFailed = 4025,
    /// <summary>
    /// Rename operation failed.
    /// </summary>
    RenameFailed = 4026,
    /// <summary>
    /// Seek operation failed.
    /// </summary>
    SeekFailed = 4027,
    /// <summary>
    /// Sharing violation occurred.
    /// </summary>
    SharingViolation = 4028,
    /// <summary>
    /// Symbolic link is invalid.
    /// </summary>
    SymbolicLinkInvalid = 4029,
    /// <summary>
    /// Too many open files.
    /// </summary>
    TooManyOpenFiles = 4030,
    /// <summary>
    /// Unknown file system error occurred.
    /// </summary>
    UnknownFileSystemError = 4031,
    /// <summary>
    /// Unsupported operation.
    /// </summary>
    UnsupportedOperation = 4032,
    /// <summary>
    /// File system write operation failed.
    /// </summary>
    FileSystemWriteFailed = 4033,
    /// <summary>
    /// Directory creation failed.
    /// </summary>
    DirectoryCreationFailed = 4034,
    // Localization (5000-5999)
    /// <summary>
    /// No localization error.
    /// </summary>
    LocalizationNone = 5000,
    /// <summary>
    /// Localization dictionary is corrupted or unreadable.
    /// </summary>
    DictionaryCorrupted = 5001,
    /// <summary>
    /// Localization dictionary not found for the specified locale.
    /// </summary>
    DictionaryNotFound = 5002,
    /// <summary>
    /// Encoding conversion failed during localization processing.
    /// </summary>
    EncodingConversionFailed = 5003,
    /// <summary>
    /// The specified locale is invalid or not recognized.
    /// </summary>
    InvalidLocale = 5004,
    /// <summary>
    /// The translation format is invalid or cannot be processed.
    /// </summary>
    InvalidTranslationFormat = 5005,
    /// <summary>
    /// Language detection failed due to insufficient or ambiguous input data.
    /// </summary>
    LanguageDetectionFailed = 5006,
    /// <summary>
    /// The specified language is not supported by the localization system.
    /// </summary>
    LanguageNotSupported = 5007,
    /// <summary>
    /// Parsing of localized content failed due to syntax errors or unsupported constructs.
    /// </summary>
    LocaleParsingFailed = 5008,
    /// <summary>
    /// A required translation is missing for the specified key and locale.
    /// </summary>
    MissingTranslation = 5009,
    /// <summary>
    /// Resolution of plural forms failed due to missing rules or unsupported locale.
    /// </summary>
    PluralFormResolutionFailed = 5010,
    /// <summary>
    /// Failed to load the resource bundle for the specified locale.
    /// </summary>
    ResourceBundleLoadFailed = 5011,
    /// <summary>
    /// String formatting failed during localization processing, such as when placeholders in a translation cannot be replaced with the provided arguments or when the format string is invalid.
    /// </summary>
    StringFormattingFailed = 5012,
    /// <summary>
    /// Authentication with the translation API failed due to invalid credentials or insufficient permissions.
    /// </summary>
    TranslationApiAuthenticationFailed = 5013,
    /// <summary>
    /// The translation API is currently unavailable due to maintenance, network issues, or service disruptions.
    /// </summary>
    TranslationApiUnavailable = 5014,
    /// <summary>
    /// An error occurred during the translation process, such as an unexpected response from the translation API or an internal processing error.
    /// </summary>
    TranslationFailed = 5015,
    /// <summary>
    /// The translation queue is full and cannot accept new translation requests at this time. This may occur during periods of high demand or when the translation service is experiencing performance issues.
    /// </summary>
    TranslationQueueFull = 5016,
    /// <summary>
    /// An unspecified error occurred within the translation service.
    /// </summary>
    TranslationServiceError = 5017,
    /// <summary>
    /// The translation request timed out due to network issues or service delays.
    /// </summary>
    TranslationTimeout = 5018,
    /// <summary>
    /// The specified language is unknown or not supported.
    /// </summary>
    UnknownLanguage = 5019,
    /// <summary>
    /// An unknown error occurred within the localization system.
    /// </summary>
    UnknownLocalizationError = 5020,
    /// <summary>
    /// The specified encoding is not supported.
    /// </summary>
    UnsupportedEncoding = 5021,

    // Authentication (6000-6999)
    /// <summary>
    /// No authentication error.
    /// </summary>
    AuthenticationNone = 6000,
    /// <summary>
    /// Access is forbidden due to insufficient permissions or other restrictions.
    /// </summary>
    AccessForbidden = 6001,
    /// <summary>
    /// The account is disabled and cannot be used for authentication.
    /// </summary>
    AccountDisabled = 6002,
    /// <summary>
    /// The account has expired and is no longer valid.
    /// </summary>
    AccountExpired = 6003,
    /// <summary>
    /// The account is locked due to multiple failed login attempts or security policies.
    /// </summary>
    AccountLocked = 6004,
    /// <summary>
    /// The account was not found.
    /// </summary>
    AccountNotFound = 6005,
    /// <summary>
    /// The API key is invalid.
    /// </summary>
    ApiKeyInvalid = 6006,
    /// <summary>
    /// Authentication failed due to invalid credentials or other reasons.
    /// </summary>
    AuthenticationFailed = 6007,
    /// <summary>
    /// Biometric authentication failed.
    /// </summary>
    BiometricAuthenticationFailed = 6008,
    /// <summary>
    /// Certificate-based authentication failed.
    /// </summary>
    CertificateAuthenticationFailed = 6009,
    /// <summary>
    /// Email verification is required for authentication.
    /// </summary>
    EmailVerificationRequired = 6010,
    /// <summary>
    /// The provided password is invalid.
    /// </summary>
    InvalidPassword = 6011,
    /// <summary>
    /// The provided token is invalid.
    /// </summary>
    InvalidToken = 6012,
    /// <summary>
    /// The provided username is invalid.
    /// </summary>
    InvalidUsername = 6013,
    /// <summary>
    /// Multi-factor authentication (MFA) failed.
    /// </summary>
    MfaFailed = 6014,
    /// <summary>
    /// Multi-factor authentication (MFA) is required.
    /// </summary>
    MfaRequired = 6015,
    /// <summary>
    /// OAuth authentication failed.
    /// </summary>
    OAuthFailed = 6016,
    /// <summary>
    /// The password has expired.
    /// </summary>
    PasswordExpired = 6017,
    /// <summary>
    /// Password reset is required.
    /// </summary>
    PasswordResetRequired = 6018,
    /// <summary>
    /// Authentication permission is denied.
    /// </summary>
    AuthenticationPermissionDenied = 6019,
    /// <summary>
    /// The refresh token is invalid.
    /// </summary>
    RefreshTokenInvalid = 6020,
    /// <summary>
    /// The session has expired.
    /// </summary>
    SessionExpired = 6021,
    /// <summary>
    /// The session is invalid.
    /// </summary>
    SessionInvalid = 6022,
    /// <summary>
    /// Single sign-on (SSO) authentication failed.
    /// </summary>
    SsoFailed = 6023,
    /// <summary>
    /// The token has expired.
    /// </summary>
    TokenExpired = 6024,
    /// <summary>
    /// The two-factor authentication code is invalid.
    /// </summary>
    TwoFactorCodeInvalid = 6025,
    /// <summary>
    /// The user is unauthenticated.
    /// </summary>
    Unauthenticated = 6026,
    /// <summary>
    /// The user is unauthorized.
    /// </summary>
    Unauthorized = 6027,
    /// <summary>
    /// An unknown authentication error occurred.
    /// </summary>
    UnknownAuthenticationError = 6028,


    // Validation (7000-7999)

    /// <summary>
    /// No validation error.
    /// </summary>
    ValidationNone = 7000,
    /// <summary>
    /// Conversion failed.
    /// </summary>
    ConversionFailed = 7001,
    /// <summary>
    /// Duplicate value found.
    /// </summary>
    DuplicateValue = 7002,
    /// <summary>
    /// Invalid email format.
    /// </summary>
    InvalidEmailFormat = 7003,
    /// <summary>
    /// Invalid date-time format.
    /// </summary>
    InvalidDateTimeFormat = 7004,
    /// <summary>
    /// Invalid format.
    /// </summary>
    InvalidFormat = 7005,
    /// <summary>
    /// Invalid JSON or AJIS format.
    /// </summary>
    InvalidJsonFormat = 7006,
    /// <summary>
    /// Invalid numeric format.
    /// </summary>
    InvalidNumericFormat = 7007,
    /// <summary>
    /// Invalid phone format.
    /// </summary>
    InvalidPhoneFormat = 7008,
    /// <summary>
    /// Invalid pattern.
    /// </summary>
    InvalidPattern = 7009,
    /// <summary>
    /// Invalid URL format.
    /// </summary>
    InvalidUrlFormat = 7010,
    /// <summary>
    /// Invalid XML format.
    /// </summary>
    InvalidXmlFormat = 7011,
    /// <summary>
    /// Maximum length exceeded.
    /// </summary>
    MaxLengthExceeded = 7012,
    /// <summary>
    /// Maximum value exceeded.
    /// </summary>
    MaxValueExceeded = 7013,
    /// <summary>
    /// Minimum length not met.
    /// </summary>
    MinLengthNotMet = 7014,
    /// <summary>
    /// Minimum value not met.
    /// </summary>
    MinValueNotMet = 7015,
    /// <summary>
    /// Missing required field.
    /// </summary>
    MissingRequiredField = 7016,
    /// <summary>
    /// Value out of range.
    /// </summary>
    OutOfRange = 7017,
    /// <summary>
    /// Password complexity requirements not met.
    /// </summary>
    PasswordComplexityNotMet = 7018,
    /// <summary>
    /// An unknown validation error occurred.
    /// </summary>
    UnknownValidationError = 7019,
    /// <summary>
    /// Unsupported value type.
    /// </summary>
    UnsupportedValueType = 7020,
    /// <summary>
    ///     Validation failed due to a conflict with the current state of the resource.
    /// </summary>

    // Configuration (8000-8999)
    /// <summary>
    /// No configuration error.
    /// </summary>
    ConfigurationNone = 8000,
    /// <summary>
    /// Configuration file not found.
    /// </summary>
    ConfigurationFileNotFound = 8001,
    /// <summary>
    /// Configuration file is locked.
    /// </summary>
    ConfigurationFileLocked = 8002,
    /// <summary>
    /// Configuration value is invalid.
    /// </summary>
    ConfigurationValueInvalid = 8003,
    /// <summary>
    /// Configuration key not found.
    /// </summary>
    ConfigurationKeyNotFound = 8004,

    /// <summary>
    /// Configuration parsing failed.
    /// </summary>
    ConfigurationParsingFailed = 8005,
        /// <summary>
        /// Configuration reload failed due to errors in the new configuration or issues applying the changes.
        /// </summary>
    ConfigurationReloadFailed = 8006,
    /// <summary>
    /// Configuration validation failed.
    /// </summary>
    ConfigurationValidationFailed = 8007,
    /// <summary>
    /// Configuration write failed.
    /// </summary>
    ConfigurationWriteFailed = 8008,
    /// <summary>
    /// Circular dependency detected in configuration.
    /// </summary>
    CircularDependency = 8009,
    /// <summary>
    /// Connection string is invalid.
    /// </summary>
    ConnectionStringInvalid = 8010,
    /// <summary>
    /// Dependency injection failed.
    /// </summary>
    DependencyInjectionFailed = 8011,
    /// <summary>
    /// Environment variable not found.
    /// </summary>
    EnvironmentVariableNotFound = 8012,
    /// <summary>
    /// Feature flag error.
    /// </summary>
    FeatureFlagError = 8013,
    /// <summary>
    /// Invalid configuration format.
    /// </summary>
    InvalidConfigurationFormat = 8014,
    /// <summary>
    /// Invalid configuration provider.
    /// </summary>
    InvalidConfigurationProvider = 8015,
    /// <summary>
    /// Missing required configuration.
    /// </summary>
    MissingRequiredConfiguration = 8016,
    /// <summary>
    /// Configuration schema mismatch.
    /// </summary>
    ConfigurationSchemaMismatch = 8017,
    /// <summary>
    /// Settings conflict.
    /// </summary>
    SettingsConflict = 8018,
    /// <summary>
    /// Secret decryption failed.
    /// </summary>
    SecretDecryptionFailed = 8019,
    /// <summary>
    /// Secret not found.
    /// </summary>
    SecretNotFound = 8020,
    /// <summary>
    /// Unknown configuration error.
    /// </summary>
    UnknownConfigurationError = 8021,
    /// <summary>
    /// Unsupported configuration version detected. This error occurs when the application encounters a configuration file or schema version that it does not recognize or support, indicating that the configuration may need to be updated or migrated to a compatible version.
    /// </summary>
    UnsupportedConfigurationVersion = 8022,

    // General (9000-9999)
    /// <summary>
    /// No general error.
    /// </summary>
    GeneralNone = 9000,
    /// <summary>
    /// Application is in maintenance mode.
    /// </summary>
    ApplicationInMaintenance = 9001,
    /// <summary>
    /// Application initialization failed.
    /// </summary>
    ApplicationInitializationFailed = 9002,
    /// <summary>
    /// Invalid argument.
    /// </summary>
    ArgumentInvalid = 9003,
    /// <summary>
    /// Concurrency conflict.
    /// </summary>
    ConcurrencyConflict = 9004,
    /// <summary>
    /// Feature is deprecated and will be removed in a future release.
    /// </summary>
    FeatureDeprecated = 9005,
    /// <summary>
    /// Feature is not implemented.
    /// </summary>
    FeatureNotImplemented = 9006,
    /// <summary>
    /// Internal error.
    /// </summary>
    InternalError = 9007,
    /// <summary>
    /// Invalid operation.
    /// </summary>
    InvalidOperation = 9008,
    /// <summary>
    /// Invalid state.
    /// </summary>
    InvalidState = 9009,
    /// <summary>
    /// Invalid license.
    /// </summary>
    LicenseInvalid = 9010,
    /// <summary>
    /// Null reference.
    /// </summary>
    NullReference = 9011,
    /// <summary>
    /// Operation cancelled.
    /// </summary>
    OperationCancelled = 9012,
    /// <summary>
    /// Operation timeout.
    /// </summary>
    OperationTimeout = 9013,
    /// <summary>
    /// Out of memory.
    /// </summary>
    OutOfMemory = 9014,
    /// <summary>
    /// Rate limit exceeded.
    /// </summary>
    RateLimitExceeded = 9015,
    /// <summary>
    /// Required service is unavailable.
    /// </summary>
    RequiredServiceUnavailable = 9016,
    /// <summary>
    /// Resource is busy.
    /// </summary>
    ResourceBusy = 9017,
    /// <summary>
    /// Resource not found.
    /// </summary>
    ResourceNotFound = 9018,
    /// <summary>
    /// Resource unavailable.
    /// </summary>
    ResourceUnavailable = 9019,
    /// <summary>
    /// Stack overflow.
    /// </summary>
    StackOverflow = 9020,
    /// <summary>
    /// Thread aborted.
    /// </summary>
    ThreadAborted = 9021,
    /// <summary>
    /// Unhandled exception.
    /// </summary>
    UnhandledException = 9022,
    /// <summary>
    /// Unknown error.
    /// </summary>
    UnknownError = 9023,
    /// <summary>
    /// Unsupported feature.
    /// </summary>
    UnsupportedFeature = 9024,
    /// <summary>
    /// Unsupported platform.
    /// </summary>
    UnsupportedPlatform = 9025,
    /// <summary>
    /// Unsupported version.
    /// </summary>
    UnsupportedVersion = 9026,
    /// <summary>
    /// Version mismatch.
    /// </summary>
    VersionMismatch = 9027
}

/// <summary>
/// Provides human-readable error messages for the defined error codes. This class contains methods to convert error codes into user-friendly text descriptions, making it easier for developers and users to understand the nature of the errors that occur within the application.
/// </summary>
public static partial class ErrorCodeText
{
    /// <summary>
    /// Returns a human-readable error message corresponding to the given <see cref="ErrorCode"/>. This method maps each error code to a descriptive string that can be displayed to users or logged for debugging purposes. If the error code is not recognized, it returns a generic "Unknown error" message along with the numeric code.
    /// </summary>
    /// <param name="errorCode">The error code for which to retrieve the message.</param>
    /// <returns>A human-readable error message.</returns>
    public static string ErrorText(ErrorCode errorCode)
    {
        if (errorCode == ErrorCode.BadSector)
        {
            return "Bad sector found";
        }

        var name = errorCode.ToString();
        if (name.EndsWith("None", StringComparison.Ordinal))
        {
            return "No error";
        }

        return HumanizeErrorCodeName(name);
    }
    /// <summary>
    /// Returns a human-readable error message corresponding to the given integer error code. This method first checks if the provided integer value corresponds to a defined <see cref="ErrorCode"/>. If it does, it converts it to the appropriate enum value and returns the corresponding error message. If the integer does not match any defined error code, it returns a generic "Unknown error" message along with the numeric code.    
    /// </summary>
    /// <param name="errorCode">The integer error code for which to retrieve the message.</param>
    /// <returns>A human-readable error message.</returns>
    public static string ErrorText(int errorCode)
    {
        if (!Enum.IsDefined(typeof(ErrorCode), errorCode))
        {
            return $"Unknown error ({errorCode})";
        }

        return ErrorText((ErrorCode)errorCode);
    }

    private static string HumanizeErrorCodeName(string name)
    {
        var parts = Regex.Matches(name, @"[A-Z]+(?![a-z])|[A-Z][a-z0-9]*")
           .Select(m => NormalizeToken(m.Value))
           .ToArray();

        if (parts.Length == 0)
        {
            return "Unknown error";
        }

        var first = parts[0];
        if (!IsAllCaps(first))
        {
            first = char.ToUpperInvariant(first[0]) + first[1..].ToLowerInvariant();
        }

        for (var i = 1; i < parts.Length; i++)
        {
            if (!IsAllCaps(parts[i]))
            {
                parts[i] = parts[i].ToLowerInvariant();
            }
        }

        parts[0] = first;
        return string.Join(' ', parts);
    }

    private static string NormalizeToken(string token) => token switch
    {
        "Io" => "I/O",
        "Api" => "API",
        "Dns" => "DNS",
        "Http" => "HTTP",
        "Ssl" => "SSL",
        "Tls" => "TLS",
        "Mfa" => "MFA",
        "OAuth" => "OAuth",
        "Sso" => "SSO",
        "Xml" => "XML",
        "Json" => "JSON",
        "Url" => "URL",
        _ => token
    };

    private static bool IsAllCaps(string token)
    {
        foreach (var c in token)
        {
            if (char.IsLetter(c) && !char.IsUpper(c))
            {
                return false;
            }
        }

        return true;
    }
}
