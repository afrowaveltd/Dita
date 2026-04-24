namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Configuration and settings-related error codes (range 8000-8999).
/// </summary>
public enum ConfigurationError
{
   /// <summary>
   /// Configuration file not found.
   /// </summary>
   ConfigurationFileNotFound = 8001,

   /// <summary>
   /// Configuration file is locked and cannot be accessed.
   /// </summary>
   ConfigurationFileLocked = 8002,

   /// <summary>
   /// Configuration value is invalid or out of range.
   /// </summary>
   ConfigurationValueInvalid = 8003,

   /// <summary>
   /// Configuration section or key not found.
   /// </summary>
   ConfigurationKeyNotFound = 8004,

   /// <summary>
   /// Configuration parsing failed.
   /// </summary>
   ConfigurationParsingFailed = 8005,

   /// <summary>
   /// Configuration reload failed.
   /// </summary>
   ConfigurationReloadFailed = 8006,

   /// <summary>
   /// Configuration validation failed.
   /// </summary>
   ConfigurationValidationFailed = 8007,

   /// <summary>
   /// Configuration write or save operation failed.
   /// </summary>
   ConfigurationWriteFailed = 8008,

   /// <summary>
   /// Circular dependency detected in configuration.
   /// </summary>
   CircularDependency = 8009,

   /// <summary>
   /// Connection string is invalid or malformed.
   /// </summary>
   ConnectionStringInvalid = 8010,

   /// <summary>
   /// Dependency injection or service registration failed.
   /// </summary>
   DependencyInjectionFailed = 8011,

   /// <summary>
   /// Environment variable not found.
   /// </summary>
   EnvironmentVariableNotFound = 8012,

   /// <summary>
   /// Feature flag or toggle configuration error.
   /// </summary>
   FeatureFlagError = 8013,

   /// <summary>
   /// Invalid configuration format (JSON, XML, YAML, etc.).
   /// </summary>
   InvalidConfigurationFormat = 8014,

   /// <summary>
   /// Invalid or unsupported configuration provider.
   /// </summary>
   InvalidConfigurationProvider = 8015,

   /// <summary>
   /// Missing required configuration setting.
   /// </summary>
   MissingRequiredConfiguration = 8016,

   /// <summary>
   /// No error occurred (success).
   /// </summary>
   None = 8000,

   /// <summary>
   /// Configuration schema mismatch or incompatibility.
   /// </summary>
   SchemaMismatch = 8017,

   /// <summary>
   /// Configuration settings conflict with each other.
   /// </summary>
   SettingsConflict = 8018,

   /// <summary>
   /// Configuration secret decryption failed.
   /// </summary>
   SecretDecryptionFailed = 8019,

   /// <summary>
   /// Configuration secret not found (key vault, secret manager, etc.).
   /// </summary>
   SecretNotFound = 8020,

   /// <summary>
   /// Unknown configuration error occurred.
   /// </summary>
   UnknownConfigurationError = 8021,

   /// <summary>
   /// Unsupported configuration version.
   /// </summary>
   UnsupportedConfigurationVersion = 8022
}
