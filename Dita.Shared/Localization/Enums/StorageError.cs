namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Storage and database-related error codes (range 2000-2999).
/// </summary>
public enum StorageError
{
   /// <summary>
   /// Database backup operation failed.
   /// </summary>
   BackupFailed = 2001,

   /// <summary>
   /// Database checkpoint operation failed.
   /// </summary>
   CheckpointFailed = 2002,

   /// <summary>
   /// Database connection failed.
   /// </summary>
   ConnectionFailed = 2003,

   /// <summary>
   /// Database connection pool exhausted.
   /// </summary>
   ConnectionPoolExhausted = 2004,

   /// <summary>
   /// Database connection timeout occurred.
   /// </summary>
   ConnectionTimeout = 2005,

   /// <summary>
   /// Constraint violation (foreign key, unique, check, etc.).
   /// </summary>
   ConstraintViolation = 2006,

   /// <summary>
   /// Database is corrupted or integrity check failed.
   /// </summary>
   DatabaseCorrupted = 2007,

   /// <summary>
   /// Database is locked and cannot be accessed.
   /// </summary>
   DatabaseLocked = 2008,

   /// <summary>
   /// Database does not exist.
   /// </summary>
   DatabaseNotFound = 2009,

   /// <summary>
   /// Deadlock detected during transaction.
   /// </summary>
   DeadlockDetected = 2010,

   /// <summary>
   /// Requested data or record was not found.
   /// </summary>
   DataNotFound = 2011,

   /// <summary>
   /// Duplicate key or unique constraint violation.
   /// </summary>
   DuplicateKey = 2012,

   /// <summary>
   /// Foreign key constraint violation.
   /// </summary>
   ForeignKeyViolation = 2013,

   /// <summary>
   /// Index is corrupted or invalid.
   /// </summary>
   IndexCorrupted = 2014,

   /// <summary>
   /// Insufficient storage space available.
   /// </summary>
   InsufficientSpace = 2015,

   /// <summary>
   /// Invalid query syntax or structure.
   /// </summary>
   InvalidQuery = 2016,

   /// <summary>
   /// Invalid storage configuration or settings.
   /// </summary>
   InvalidStorageConfiguration = 2017,

   /// <summary>
   /// Migration operation failed.
   /// </summary>
   MigrationFailed = 2018,

   /// <summary>
   /// No error occurred (success).
   /// </summary>
   None = 2000,

   /// <summary>
   /// Query execution timeout.
   /// </summary>
   QueryTimeout = 2019,

   /// <summary>
   /// Record already exists (duplicate entry).
   /// </summary>
   RecordAlreadyExists = 2020,

   /// <summary>
   /// Replication or synchronization failed.
   /// </summary>
   ReplicationFailed = 2021,

   /// <summary>
   /// Database restore operation failed.
   /// </summary>
   RestoreFailed = 2022,

   /// <summary>
   /// Schema mismatch or incompatible version.
   /// </summary>
   SchemaMismatch = 2023,

   /// <summary>
   /// Storage quota exceeded.
   /// </summary>
   StorageQuotaExceeded = 2024,

   /// <summary>
   /// Table does not exist.
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
   /// Transaction timeout occurred.
   /// </summary>
   TransactionTimeout = 2028,

   /// <summary>
   /// Unknown storage error occurred.
   /// </summary>
   UnknownStorageError = 2029,

   /// <summary>
   /// Storage write operation failed.
   /// </summary>
   WriteFailed = 2030
}
