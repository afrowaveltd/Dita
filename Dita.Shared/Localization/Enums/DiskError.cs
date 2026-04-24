namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Physical disk and drive-related error codes (range 3000-3999).
/// </summary>
public enum DiskError
{
   /// <summary>
   /// Bad sector detected on disk.
   /// </summary>
   BadSector = 3001,

   /// <summary>
   /// Disk benchmark or test failed.
   /// </summary>
   BenchmarkFailed = 3002,

   /// <summary>
   /// Disk boot sector is corrupted.
   /// </summary>
   BootSectorCorrupted = 3003,

   /// <summary>
   /// Device or drive is busy and cannot be accessed.
   /// </summary>
   DeviceBusy = 3004,

   /// <summary>
   /// Device or drive not found.
   /// </summary>
   DeviceNotFound = 3005,

   /// <summary>
   /// Device or drive is not ready.
   /// </summary>
   DeviceNotReady = 3006,

   /// <summary>
   /// Disk controller error occurred.
   /// </summary>
   DiskControllerError = 3007,

   /// <summary>
   /// Disk defragmentation failed.
   /// </summary>
   DiskDefragmentationFailed = 3008,

   /// <summary>
   /// Disk eject operation failed.
   /// </summary>
   DiskEjectFailed = 3009,

   /// <summary>
   /// Disk is full (no space available).
   /// </summary>
   DiskFull = 3010,

   /// <summary>
   /// Disk format operation failed.
   /// </summary>
   DiskFormatFailed = 3011,

   /// <summary>
   /// Disk mount operation failed.
   /// </summary>
   DiskMountFailed = 3012,

   /// <summary>
   /// Disk is not formatted or has invalid format.
   /// </summary>
   DiskNotFormatted = 3013,

   /// <summary>
   /// Disk is not initialized.
   /// </summary>
   DiskNotInitialized = 3014,

   /// <summary>
   /// Disk partition error occurred.
   /// </summary>
   DiskPartitionError = 3015,

   /// <summary>
   /// Disk quota exceeded.
   /// </summary>
   DiskQuotaExceeded = 3016,

   /// <summary>
   /// Disk unmount operation failed.
   /// </summary>
   DiskUnmountFailed = 3017,

   /// <summary>
   /// Disk verification failed (checksum, integrity check, etc.).
   /// </summary>
   DiskVerificationFailed = 3018,

   /// <summary>
   /// Disk is write-protected.
   /// </summary>
   DiskWriteProtected = 3019,

   /// <summary>
   /// Drive letter is not available or already in use.
   /// </summary>
   DriveLetterUnavailable = 3020,

   /// <summary>
   /// Hardware failure detected.
   /// </summary>
   HardwareFailure = 3021,

   /// <summary>
   /// Input/output error occurred.
   /// </summary>
   IoError = 3022,

   /// <summary>
   /// Media is not present in the drive.
   /// </summary>
   MediaNotPresent = 3023,

   /// <summary>
   /// No error occurred (success).
   /// </summary>
   None = 3000,

   /// <summary>
   /// Partition table is corrupted or invalid.
   /// </summary>
   PartitionTableCorrupted = 3024,

   /// <summary>
   /// RAID array degraded or failed.
   /// </summary>
   RaidDegraded = 3025,

   /// <summary>
   /// Read error occurred on disk.
   /// </summary>
   ReadError = 3026,

   /// <summary>
   /// SMART (Self-Monitoring, Analysis and Reporting Technology) failure detected.
   /// </summary>
   SmartFailure = 3027,

   /// <summary>
   /// Unsupported disk or drive type.
   /// </summary>
   UnsupportedDiskType = 3028,

   /// <summary>
   /// Unknown disk error occurred.
   /// </summary>
   UnknownDiskError = 3029,

   /// <summary>
   /// Volume label operation failed.
   /// </summary>
   VolumeLabelError = 3030,

   /// <summary>
   /// Volume does not exist.
   /// </summary>
   VolumeNotFound = 3031,

   /// <summary>
   /// Write error occurred on disk.
   /// </summary>
   WriteError = 3032
}
