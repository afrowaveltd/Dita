namespace Dita.Shared.Localization.Enums;

/// <summary>
/// General and miscellaneous error codes (range 9000-9999).
/// </summary>
public enum GeneralError
{
   /// <summary>
   /// Application is in maintenance mode.
   /// </summary>
   ApplicationInMaintenance = 9001,

   /// <summary>
   /// Application initialization failed.
   /// </summary>
   ApplicationInitializationFailed = 9002,

   /// <summary>
   /// Argument is null or invalid.
   /// </summary>
   ArgumentInvalid = 9003,

   /// <summary>
   /// Concurrency conflict detected (optimistic locking failure).
   /// </summary>
   ConcurrencyConflict = 9004,

   /// <summary>
   /// Feature is deprecated and no longer supported.
   /// </summary>
   FeatureDeprecated = 9005,

   /// <summary>
   /// Feature is not implemented.
   /// </summary>
   FeatureNotImplemented = 9006,

   /// <summary>
   /// Internal server or application error.
   /// </summary>
   InternalError = 9007,

   /// <summary>
   /// Invalid operation for the current state.
   /// </summary>
   InvalidOperation = 9008,

   /// <summary>
   /// Invalid state detected.
   /// </summary>
   InvalidState = 9009,

   /// <summary>
   /// License is expired or invalid.
   /// </summary>
   LicenseInvalid = 9010,

   /// <summary>
   /// No error occurred (success).
   /// </summary>
   None = 9000,

   /// <summary>
   /// Null reference encountered.
   /// </summary>
   NullReference = 9011,

   /// <summary>
   /// Operation was cancelled by the user or system.
   /// </summary>
   OperationCancelled = 9012,

   /// <summary>
   /// Operation timeout occurred.
   /// </summary>
   OperationTimeout = 9013,

   /// <summary>
   /// Out of memory error.
   /// </summary>
   OutOfMemory = 9014,

   /// <summary>
   /// Rate limit exceeded (too many requests).
   /// </summary>
   RateLimitExceeded = 9015,

   /// <summary>
   /// Required dependency or service is unavailable.
   /// </summary>
   RequiredServiceUnavailable = 9016,

   /// <summary>
   /// Resource is busy and cannot be accessed.
   /// </summary>
   ResourceBusy = 9017,

   /// <summary>
   /// Resource not found.
   /// </summary>
   ResourceNotFound = 9018,

   /// <summary>
   /// Resource is temporarily unavailable.
   /// </summary>
   ResourceUnavailable = 9019,

   /// <summary>
   /// Stack overflow error.
   /// </summary>
   StackOverflow = 9020,

   /// <summary>
   /// Thread abort or termination error.
   /// </summary>
   ThreadAborted = 9021,

   /// <summary>
   /// Unhandled exception occurred.
   /// </summary>
   UnhandledException = 9022,

   /// <summary>
   /// Unknown error occurred.
   /// </summary>
   UnknownError = 9023,

   /// <summary>
   /// Unsupported feature or operation.
   /// </summary>
   UnsupportedFeature = 9024,

   /// <summary>
   /// Unsupported platform or environment.
   /// </summary>
   UnsupportedPlatform = 9025,

   /// <summary>
   /// Unsupported version detected.
   /// </summary>
   UnsupportedVersion = 9026,

   /// <summary>
   /// Version mismatch detected.
   /// </summary>
   VersionMismatch = 9027
}
