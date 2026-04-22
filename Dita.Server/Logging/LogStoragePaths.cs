namespace Dita.Server.Logging;

/// <summary>
/// Holds the resolved file-system paths used for log storage and the associated retention and cleanup configuration.
/// </summary>
/// <remarks>
/// Two independent retention periods are supported:
/// <list type="bullet">
///   <item><description><see cref="RetentionDaysInfo"/> – applied to the JSON text log directory (Information and below).</description></item>
///   <item><description><see cref="RetentionDaysWarning"/> – applied to the SQLite database directory (Warning and above).</description></item>
/// </list>
/// Both values are read from the <c>Logging</c> section of <c>appsettings.json</c> via <see cref="Create"/>.
/// </remarks>
internal sealed class LogStoragePaths
{
   /// <summary>The default number of days JSON text log files (Information and below) are retained.</summary>
   public const int DefaultRetentionDaysInfo = 7;

   /// <summary>The default number of days SQLite database log files (Warning and above) are retained.</summary>
   public const int DefaultRetentionDaysWarning = 30;

   /// <summary>The interval between consecutive cleanup passes run by <see cref="LogCleanupService"/>.</summary>
   public static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);

   /// <summary>
   /// Initializes a new instance of <see cref="LogStoragePaths"/> with explicitly supplied directory paths and retention periods.
   /// </summary>
   /// <param name="rootDirectory">The root directory that contains all log subdirectories.</param>
   /// <param name="textDirectory">The directory where JSON text log files are written.</param>
   /// <param name="databaseDirectory">The directory where SQLite log database files are written.</param>
   /// <param name="retentionDaysInfo">Days to retain JSON text log files (Information and below). Must be greater than zero.</param>
   /// <param name="retentionDaysWarning">Days to retain SQLite database files (Warning and above). Must be greater than zero.</param>
   public LogStoragePaths(
      string rootDirectory,
      string textDirectory,
      string databaseDirectory,
      int retentionDaysInfo,
      int retentionDaysWarning)
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

      if(retentionDaysInfo <= 0)
      {
         throw new ArgumentOutOfRangeException(nameof(retentionDaysInfo), retentionDaysInfo, "The retention period must be greater than zero.");
      }

      if(retentionDaysWarning <= 0)
      {
         throw new ArgumentOutOfRangeException(nameof(retentionDaysWarning), retentionDaysWarning, "The retention period must be greater than zero.");
      }

      RootDirectory = rootDirectory;
      TextDirectory = textDirectory;
      DatabaseDirectory = databaseDirectory;
      RetentionDaysInfo = retentionDaysInfo;
      RetentionDaysWarning = retentionDaysWarning;
   }

   /// <summary>Gets the root directory that contains all log subdirectories.</summary>
   public string RootDirectory { get; }

   /// <summary>Gets the directory where JSON text log files are written.</summary>
   public string TextDirectory { get; }

   /// <summary>Gets the directory where SQLite log database files are written.</summary>
   public string DatabaseDirectory { get; }

   /// <summary>
   /// Gets the number of days JSON text log files (Information and below) are retained before being deleted.
   /// Configured via <c>Logging:RetentionDaysInfo</c> in <c>appsettings.json</c>.
   /// </summary>
   public int RetentionDaysInfo { get; }

   /// <summary>
   /// Gets the number of days SQLite database log files (Warning and above) are retained before being deleted.
   /// Configured via <c>Logging:RetentionDaysWarning</c> in <c>appsettings.json</c>.
   /// </summary>
   public int RetentionDaysWarning { get; }

   /// <summary>
   /// Creates a <see cref="LogStoragePaths"/> instance by deriving subdirectory paths from the application's content root.
   /// </summary>
   /// <param name="contentRootPath">The application content root path; the log directories are created beneath it.</param>
   /// <param name="retentionDaysInfo">Days to keep JSON text logs. Defaults to <see cref="DefaultRetentionDaysInfo"/>.</param>
   /// <param name="retentionDaysWarning">Days to keep SQLite database logs. Defaults to <see cref="DefaultRetentionDaysWarning"/>.</param>
   /// <returns>A fully initialised <see cref="LogStoragePaths"/> instance.</returns>
   public static LogStoragePaths Create(
      string contentRootPath,
      int retentionDaysInfo = DefaultRetentionDaysInfo,
      int retentionDaysWarning = DefaultRetentionDaysWarning)
   {
      if(string.IsNullOrWhiteSpace(contentRootPath))
      {
         throw new ArgumentException("The value cannot be null or whitespace.", nameof(contentRootPath));
      }

      string rootDirectory = Path.Combine(contentRootPath, "Logs");
      string textDirectory = Path.Combine(rootDirectory, "Text");
      string databaseDirectory = Path.Combine(rootDirectory, "Database");

      return new LogStoragePaths(rootDirectory, textDirectory, databaseDirectory, retentionDaysInfo, retentionDaysWarning);
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