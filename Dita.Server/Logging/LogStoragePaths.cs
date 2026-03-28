namespace Dita.Server.Logging;

internal sealed class LogStoragePaths
{
   public const int DefaultRetentionDays = 30;
   public static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(12);

   public LogStoragePaths(string rootDirectory, string textDirectory, string databaseDirectory, int retentionDays)
   {
      if(string.IsNullOrWhiteSpace(rootDirectory))
      {
         throw new ArgumentException("The value cannot be null or whitespace.", nameof(rootDirectory));
      }

      if(string.IsNullOrWhiteSpace(textDirectory))
      {
         throw new ArgumentException("The value cannot be null or whitespace.", nameof(textDirectory));
      }

      if(string.IsNullOrWhiteSpace(databaseDirectory))
      {
         throw new ArgumentException("The value cannot be null or whitespace.", nameof(databaseDirectory));
      }

      if(retentionDays <= 0)
      {
         throw new ArgumentOutOfRangeException(nameof(retentionDays), retentionDays, "The retention period must be greater than zero.");
      }

      RootDirectory = rootDirectory;
      TextDirectory = textDirectory;
      DatabaseDirectory = databaseDirectory;
      RetentionDays = retentionDays;
   }

   public string RootDirectory { get; }

   public string TextDirectory { get; }

   public string DatabaseDirectory { get; }

   public int RetentionDays { get; }

   public static LogStoragePaths Create(string contentRootPath, int retentionDays = DefaultRetentionDays)
   {
      if(string.IsNullOrWhiteSpace(contentRootPath))
      {
         throw new ArgumentException("The value cannot be null or whitespace.", nameof(contentRootPath));
      }

      string rootDirectory = Path.Combine(contentRootPath, "Logs");
      string textDirectory = Path.Combine(rootDirectory, "Text");
      string databaseDirectory = Path.Combine(rootDirectory, "Database");

      return new LogStoragePaths(rootDirectory, textDirectory, databaseDirectory, retentionDays);
   }

   public void EnsureDirectories()
   {
      Directory.CreateDirectory(RootDirectory);
      Directory.CreateDirectory(TextDirectory);
      Directory.CreateDirectory(DatabaseDirectory);
   }

   public string GetDatabasePath(DateTimeOffset timestamp)
   {
      DateTime localTimestamp = timestamp.ToLocalTime().Date;
      return Path.Combine(DatabaseDirectory, $"warnings-{localTimestamp:yyyyMMdd}.db");
   }
}
