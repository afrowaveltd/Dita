namespace Dita.Shared.Localization.Enums;

/// <summary>
/// File system and I/O-related error codes (range 4000-4999).
/// </summary>
public enum FileSystemError
{
   /// <summary>
   /// Access to the file or directory is denied.
   /// </summary>
   AccessDenied = 4001,

   /// <summary>
   /// File or directory already exists.
   /// </summary>
   AlreadyExists = 4002,

   /// <summary>
   /// File copy operation failed.
   /// </summary>
   CopyFailed = 4003,

   /// <summary>
   /// Directory is not empty (cannot be deleted).
   /// </summary>
   DirectoryNotEmpty = 4004,

   /// <summary>
   /// Directory does not exist.
   /// </summary>
   DirectoryNotFound = 4005,

   /// <summary>
   /// End of file reached unexpectedly.
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
   /// File is in use by another process.
   /// </summary>
   FileInUse = 4010,

   /// <summary>
   /// File is locked and cannot be accessed.
   /// </summary>
   FileLocked = 4011,

   /// <summary>
   /// File move operation failed.
   /// </summary>
   FileMoveFailed = 4012,

   /// <summary>
   /// File does not exist.
   /// </summary>
   FileNotFound = 4013,

   /// <summary>
   /// File size exceeds maximum allowed limit.
   /// </summary>
   FileSizeExceeded = 4014,

   /// <summary>
   /// File system is corrupted or damaged.
   /// </summary>
   FileSystemCorrupted = 4015,

   /// <summary>
   /// File system is read-only.
   /// </summary>
   FileSystemReadOnly = 4016,

   /// <summary>
   /// Unsupported or unknown file system type.
   /// </summary>
   FileSystemTypeUnsupported = 4017,

   /// <summary>
   /// File name or path contains invalid characters.
   /// </summary>
   InvalidFileName = 4018,

   /// <summary>
   /// Invalid file format or structure.
   /// </summary>
   InvalidFileFormat = 4019,

   /// <summary>
   /// Invalid file handle or descriptor.
   /// </summary>
   InvalidHandle = 4020,

   /// <summary>
   /// Invalid or malformed path.
   /// </summary>
   InvalidPath = 4021,

   /// <summary>
   /// I/O operation failed.
   /// </summary>
   IoOperationFailed = 4022,

   /// <summary>
   /// No error occurred (success).
   /// </summary>
   None = 4000,

   /// <summary>
   /// Path is too long (exceeds system limit).
   /// </summary>
   PathTooLong = 4023,

   /// <summary>
   /// Insufficient permissions for the operation.
   /// </summary>
   PermissionDenied = 4024,

   /// <summary>
   /// Read operation failed.
   /// </summary>
   ReadFailed = 4025,

   /// <summary>
   /// File or directory rename operation failed.
   /// </summary>
   RenameFailed = 4026,

   /// <summary>
   /// File seek operation failed.
   /// </summary>
   SeekFailed = 4027,

   /// <summary>
   /// Sharing violation (file opened with incompatible sharing mode).
   /// </summary>
   SharingViolation = 4028,

   /// <summary>
   /// Symbolic link is invalid or points to non-existent target.
   /// </summary>
   SymbolicLinkInvalid = 4029,

   /// <summary>
   /// Too many open files or handles.
   /// </summary>
   TooManyOpenFiles = 4030,

   /// <summary>
   /// Unknown file system error occurred.
   /// </summary>
   UnknownFileSystemError = 4031,

   /// <summary>
   /// Unsupported file operation.
   /// </summary>
   UnsupportedOperation = 4032,

   /// <summary>
   /// Write operation failed.
   /// </summary>
   WriteFailed = 4033
}
