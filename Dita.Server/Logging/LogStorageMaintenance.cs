namespace Dita.Server.Logging;

/// <summary>
/// Provides static helpers for removing expired log files from the configured storage directories.
/// </summary>
internal static class LogStorageMaintenance
{
   /// <summary>
   /// Deletes log files in the text and database directories whose last-write timestamp is older than
   /// the retention period specified in <paramref name="paths"/>.
   /// </summary>
   /// <param name="paths">Storage path configuration containing the directories to clean and the retention period.</param>
   /// <param name="timeProvider">Optional time provider used to determine the current UTC time; defaults to <see cref="TimeProvider.System"/>.</param>
   public static void CleanupExpiredFiles(LogStoragePaths paths, TimeProvider? timeProvider = null)
   {
      ArgumentNullException.ThrowIfNull(paths);

      paths.EnsureDirectories();

      TimeProvider provider = timeProvider ?? TimeProvider.System;
      DateTime cutoffUtc = provider.GetUtcNow().AddDays(-paths.RetentionDays).UtcDateTime;

      CleanupDirectory(paths.TextDirectory, cutoffUtc);
      CleanupDirectory(paths.DatabaseDirectory, cutoffUtc);
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