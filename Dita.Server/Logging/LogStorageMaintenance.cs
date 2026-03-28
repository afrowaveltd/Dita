namespace Dita.Server.Logging;

internal static class LogStorageMaintenance
{
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
