namespace Dita.Server.Logging;

/// <summary>
/// Holds the resolved file-system paths used for log storage and the associated retention and cleanup configuration.
/// </summary>
internal sealed class LogStoragePaths
{
   /// <summary>The default number of days log files are retained before being deleted.</summary>
   public const int DefaultRetentionDays = 30;
   /// <summary>The interval between consecutive cleanup passes run by <see cref="LogCleanupService"/>.</summary>
   public static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(12);

   /// <summary>
   /// Initializes a new instance of <see cref="LogStoragePaths"/> with explicitly supplied directory paths and retention period.
   /// </summary>
   /// <param name="rootDirectory">The root directory that contains all log subdirectories.</param>
   /// <param name="textDirectory">The directory where JSON text log files are written.</param>
   /// <param name="databaseDirectory">The directory where SQLite log database files are written.</param>
   /// <param name="retentionDays">The number of days to retain log files before deleting them. Must be greater than zero.</param>
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

   /// <summary>Gets the root directory that contains all log subdirectories.</summary>
   public string RootDirectory { get; }

   /// <summary>Gets the directory where JSON text log files are written.</summary>
   public string TextDirectory { get; }

   /// <summary>Gets the directory where SQLite log database files are written.</summary>
   public string DatabaseDirectory { get; }

   /// <summary>Gets the number of days log files are retained before being deleted.</summary>
   public int RetentionDays { get; }

   /// <summary>
   /// Creates a <see cref="LogStoragePaths"/> instance by deriving subdirectory paths from the application's content root.
   /// </summary>
   /// <param name="contentRootPath">The application content root path; the log directories are created beneath it.</param>
   /// <param name="retentionDays">The number of days to retain log files. Defaults to <see cref="DefaultRetentionDays"/>.</param>
   /// <returns>A fully initialised <see cref="LogStoragePaths"/> instance.</returns>
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

   /// <summary>Creates the root, text, and database log directories if they do not already exist.</summary>
   public void EnsureDirectories()
   {
      Directory.CreateDirectory(RootDirectory);
      Directory.CreateDirectory(TextDirectory);
      Directory.CreateDirectory(DatabaseDirectory);
   }

   /// <summary>
   /// Returns the full path to the SQLite database file that should receive a log event with the given timestamp.
   /// </summary>
   /// <param name="timestamp">The timestamp of the log event; used to derive the date-stamped file name.</param>
   /// <returns>The absolute path to the daily SQLite database file.</returns>
   public string GetDatabasePath(DateTimeOffset timestamp)
   {
      DateTime localTimestamp = timestamp.ToLocalTime().Date;
      return Path.Combine(DatabaseDirectory, $"warnings-{localTimestamp:yyyyMMdd}.db");
   }
}