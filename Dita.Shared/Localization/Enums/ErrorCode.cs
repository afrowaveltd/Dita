using System.Text.RegularExpressions;

namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Unified error codes across all categories.
/// </summary>
public enum ErrorCode
{
    // Network (1000-1999)
    NetworkNone = 1000,
    BadGateway = 1001,
    CertificateValidationFailed = 1002,
    ConnectionRefused = 1003,
    ConnectionReset = 1004,
    NetworkConnectionTimeout = 1005,
    DnsResolutionFailed = 1006,
    GatewayTimeout = 1007,
    HostUnreachable = 1008,
    HttpProtocolError = 1009,
    InvalidUrl = 1010,
    NetworkInterfaceUnavailable = 1011,
    NetworkUnreachable = 1012,
    ProxyAuthenticationFailed = 1013,
    ProxyConnectionError = 1014,
    RequestCancelled = 1015,
    RequestEntityTooLarge = 1016,
    RequestTimeout = 1017,
    ServiceUnavailable = 1018,
    SslHandshakeFailed = 1019,
    TooManyRedirects = 1020,
    UnknownNetworkError = 1021,

    // Storage (2000-2999)
    StorageNone = 2000,
    BackupFailed = 2001,
    CheckpointFailed = 2002,
    ConnectionFailed = 2003,
    ConnectionPoolExhausted = 2004,
    StorageConnectionTimeout = 2005,
    ConstraintViolation = 2006,
    DatabaseCorrupted = 2007,
    DatabaseLocked = 2008,
    DatabaseNotFound = 2009,
    DeadlockDetected = 2010,
    DataNotFound = 2011,
    DuplicateKey = 2012,
    ForeignKeyViolation = 2013,
    IndexCorrupted = 2014,
    InsufficientSpace = 2015,
    InvalidQuery = 2016,
    InvalidStorageConfiguration = 2017,
    MigrationFailed = 2018,
    QueryTimeout = 2019,
    RecordAlreadyExists = 2020,
    ReplicationFailed = 2021,
    RestoreFailed = 2022,
    StorageSchemaMismatch = 2023,
    StorageQuotaExceeded = 2024,
    TableNotFound = 2025,
    TransactionCommitFailed = 2026,
    TransactionRollbackFailed = 2027,
    TransactionTimeout = 2028,
    UnknownStorageError = 2029,
    StorageWriteFailed = 2030,

    // Disk (3000-3999)
    DiskNone = 3000,
    BadSector = 3001,
    BenchmarkFailed = 3002,
    BootSectorCorrupted = 3003,
    DeviceBusy = 3004,
    DeviceNotFound = 3005,
    DeviceNotReady = 3006,
    DiskControllerError = 3007,
    DiskDefragmentationFailed = 3008,
    DiskEjectFailed = 3009,
    DiskFull = 3010,
    DiskFormatFailed = 3011,
    DiskMountFailed = 3012,
    DiskNotFormatted = 3013,
    DiskNotInitialized = 3014,
    DiskPartitionError = 3015,
    DiskQuotaExceeded = 3016,
    DiskUnmountFailed = 3017,
    DiskVerificationFailed = 3018,
    DiskWriteProtected = 3019,
    DriveLetterUnavailable = 3020,
    HardwareFailure = 3021,
    IoError = 3022,
    MediaNotPresent = 3023,
    PartitionTableCorrupted = 3024,
    RaidDegraded = 3025,
    ReadError = 3026,
    SmartFailure = 3027,
    UnsupportedDiskType = 3028,
    UnknownDiskError = 3029,
    VolumeLabelError = 3030,
    VolumeNotFound = 3031,
    WriteError = 3032,

    // File system (4000-4999)
    FileSystemNone = 4000,
    AccessDenied = 4001,
    AlreadyExists = 4002,
    CopyFailed = 4003,
    DirectoryNotEmpty = 4004,
    DirectoryNotFound = 4005,
    EndOfFile = 4006,
    FileCompressionFailed = 4007,
    FileDecompressionFailed = 4008,
    FileEncryptionFailed = 4009,
    FileInUse = 4010,
    FileLocked = 4011,
    FileMoveFailed = 4012,
    FileNotFound = 4013,
    FileSizeExceeded = 4014,
    FileSystemCorrupted = 4015,
    FileSystemReadOnly = 4016,
    FileSystemTypeUnsupported = 4017,
    InvalidFileName = 4018,
    InvalidFileFormat = 4019,
    InvalidHandle = 4020,
    InvalidPath = 4021,
    IoOperationFailed = 4022,
    PathTooLong = 4023,
    FileSystemPermissionDenied = 4024,
    ReadFailed = 4025,
    RenameFailed = 4026,
    SeekFailed = 4027,
    SharingViolation = 4028,
    SymbolicLinkInvalid = 4029,
    TooManyOpenFiles = 4030,
    UnknownFileSystemError = 4031,
    UnsupportedOperation = 4032,
    FileSystemWriteFailed = 4033,

    // Localization (5000-5999)
    LocalizationNone = 5000,
    DictionaryCorrupted = 5001,
    DictionaryNotFound = 5002,
    EncodingConversionFailed = 5003,
    InvalidLocale = 5004,
    InvalidTranslationFormat = 5005,
    LanguageDetectionFailed = 5006,
    LanguageNotSupported = 5007,
    LocaleParsingFailed = 5008,
    MissingTranslation = 5009,
    PluralFormResolutionFailed = 5010,
    ResourceBundleLoadFailed = 5011,
    StringFormattingFailed = 5012,
    TranslationApiAuthenticationFailed = 5013,
    TranslationApiUnavailable = 5014,
    TranslationFailed = 5015,
    TranslationQueueFull = 5016,
    TranslationServiceError = 5017,
    TranslationTimeout = 5018,
    UnknownLanguage = 5019,
    UnknownLocalizationError = 5020,
    UnsupportedEncoding = 5021,

    // Authentication (6000-6999)
    AuthenticationNone = 6000,
    AccessForbidden = 6001,
    AccountDisabled = 6002,
    AccountExpired = 6003,
    AccountLocked = 6004,
    AccountNotFound = 6005,
    ApiKeyInvalid = 6006,
    AuthenticationFailed = 6007,
    BiometricAuthenticationFailed = 6008,
    CertificateAuthenticationFailed = 6009,
    EmailVerificationRequired = 6010,
    InvalidPassword = 6011,
    InvalidToken = 6012,
    InvalidUsername = 6013,
    MfaFailed = 6014,
    MfaRequired = 6015,
    OAuthFailed = 6016,
    PasswordExpired = 6017,
    PasswordResetRequired = 6018,
    AuthenticationPermissionDenied = 6019,
    RefreshTokenInvalid = 6020,
    SessionExpired = 6021,
    SessionInvalid = 6022,
    SsoFailed = 6023,
    TokenExpired = 6024,
    TwoFactorCodeInvalid = 6025,
    Unauthenticated = 6026,
    Unauthorized = 6027,
    UnknownAuthenticationError = 6028,

    // Validation (7000-7999)
    ValidationNone = 7000,
    ConversionFailed = 7001,
    DuplicateValue = 7002,
    InvalidEmailFormat = 7003,
    InvalidDateTimeFormat = 7004,
    InvalidFormat = 7005,
    InvalidJsonFormat = 7006,
    InvalidNumericFormat = 7007,
    InvalidPhoneFormat = 7008,
    InvalidPattern = 7009,
    InvalidUrlFormat = 7010,
    InvalidXmlFormat = 7011,
    MaxLengthExceeded = 7012,
    MaxValueExceeded = 7013,
    MinLengthNotMet = 7014,
    MinValueNotMet = 7015,
    MissingRequiredField = 7016,
    OutOfRange = 7017,
    PasswordComplexityNotMet = 7018,
    UnknownValidationError = 7019,
    UnsupportedValueType = 7020,

    // Configuration (8000-8999)
    ConfigurationNone = 8000,
    ConfigurationFileNotFound = 8001,
    ConfigurationFileLocked = 8002,
    ConfigurationValueInvalid = 8003,
    ConfigurationKeyNotFound = 8004,
    ConfigurationParsingFailed = 8005,
    ConfigurationReloadFailed = 8006,
    ConfigurationValidationFailed = 8007,
    ConfigurationWriteFailed = 8008,
    CircularDependency = 8009,
    ConnectionStringInvalid = 8010,
    DependencyInjectionFailed = 8011,
    EnvironmentVariableNotFound = 8012,
    FeatureFlagError = 8013,
    InvalidConfigurationFormat = 8014,
    InvalidConfigurationProvider = 8015,
    MissingRequiredConfiguration = 8016,
    ConfigurationSchemaMismatch = 8017,
    SettingsConflict = 8018,
    SecretDecryptionFailed = 8019,
    SecretNotFound = 8020,
    UnknownConfigurationError = 8021,
    UnsupportedConfigurationVersion = 8022,

    // General (9000-9999)
    GeneralNone = 9000,
    ApplicationInMaintenance = 9001,
    ApplicationInitializationFailed = 9002,
    ArgumentInvalid = 9003,
    ConcurrencyConflict = 9004,
    FeatureDeprecated = 9005,
    FeatureNotImplemented = 9006,
    InternalError = 9007,
    InvalidOperation = 9008,
    InvalidState = 9009,
    LicenseInvalid = 9010,
    NullReference = 9011,
    OperationCancelled = 9012,
    OperationTimeout = 9013,
    OutOfMemory = 9014,
    RateLimitExceeded = 9015,
    RequiredServiceUnavailable = 9016,
    ResourceBusy = 9017,
    ResourceNotFound = 9018,
    ResourceUnavailable = 9019,
    StackOverflow = 9020,
    ThreadAborted = 9021,
    UnhandledException = 9022,
    UnknownError = 9023,
    UnsupportedFeature = 9024,
    UnsupportedPlatform = 9025,
    UnsupportedVersion = 9026,
    VersionMismatch = 9027
}

public static partial class ErrorCodeText
{
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
