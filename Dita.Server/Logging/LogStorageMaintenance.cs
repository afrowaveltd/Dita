namespace Dita.Server.Logging;

/// <summary>
/// Provides static helpers for removing expired log files from the configured storage directories.
/// </summary>
/// <remarks>
/// The text directory (JSON, Information and below) uses <see cref="LogStoragePaths.RetentionDaysInfo"/>.
/// The database directory (SQLite, Warning and above) uses <see cref="LogStoragePaths.RetentionDaysWarning"/>.
/// </remarks>
internal static class LogStorageMaintenance
{
   /// <summary>
   /// Deletes log files whose last-write timestamp is older than the configured retention period.
   /// The text and database directories use independent retention windows.
   /// </summary>
   /// <param name="paths">Storage path configuration containing the directories to clean and the retention periods.</param>
   /// <param name="timeProvider">Optional time provider used to determine the current UTC time; defaults to <see cref="TimeProvider.System"/>.</param>
   public static void CleanupExpiredFiles(LogStoragePaths paths, TimeProvider? timeProvider = null)
   {
      ArgumentNullException.ThrowIfNull(paths);

      paths.EnsureDirectories();

      TimeProvider provider = timeProvider ?? TimeProvider.System;
      DateTimeOffset now = provider.GetUtcNow();

      DateTime infoCutoffUtc = now.AddDays(-paths.RetentionDaysInfo).UtcDateTime;
      DateTime warningCutoffUtc = now.AddDays(-paths.RetentionDaysWarning).UtcDateTime;

      CleanupDirectory(paths.TextDirectory, infoCutoffUtc);
      CleanupDirectory(paths.DatabaseDirectory, warningCutoffUtc);
   }

   private static void CleanupDirectory(string directory, DateTime cutoffUtc)
   {
      foreach(string filePath in Directory.EnumerateFiles(directory))
      {
         FileInfo fileInfo = new(filePath);
         if(fileInfo.LastWriteTimeUtc < cutoffUtc)
         {
            fileInfo.Delete();
         }
      }
   }
}