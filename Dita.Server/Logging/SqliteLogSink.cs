using Microsoft.Data.Sqlite;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Globalization;

namespace Dita.Server.Logging;

/// <summary>
/// A Serilog sink that persists log events to a per-day SQLite database file.
/// </summary>
/// <remarks>
/// Each database is named <c>warnings-yyyyMMdd.db</c> and stored in the database log directory supplied via
/// <see cref="LogStoragePaths"/>. The sink is thread-safe and performs daily cleanup of databases that exceed the
/// configured retention period. The schema is created automatically on first use.
/// </remarks>
/// <param name="paths">Storage path configuration that determines where database files are written and how long they are kept.</param>
internal sealed class SqliteLogSink(LogStoragePaths paths) : ILogEventSink
{
   private readonly Lock syncRoot = new();
   private readonly CompactJsonFormatter formatter = new();
   private readonly HashSet<string> initializedDatabases = new(StringComparer.OrdinalIgnoreCase);
   private DateOnly lastCleanupDate = DateOnly.MinValue;

   /// <summary>Formats and persists a single log event to the daily SQLite database file.</summary>
   /// <param name="logEvent">The log event to persist. Must not be <see langword="null"/>.</param>
   public void Emit(LogEvent logEvent)
   {
      ArgumentNullException.ThrowIfNull(logEvent);

      try
      {
         string databasePath = paths.GetDatabasePath(logEvent.Timestamp);
         string eventJson = FormatEvent(logEvent);

         lock(syncRoot)
         {
            CleanupIfNeeded();
            paths.EnsureDirectories();
            using SqliteConnection connection = CreateConnection(databasePath);
            connection.Open();

            if(initializedDatabases.Add(databasePath))
            {
               InitializeDatabase(connection);
            }

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Logs (
                    TimestampUtc,
                    Level,
                    MessageTemplate,
                    RenderedMessage,
                    Exception,
                    EventJson,
                    TraceId,
                    RequestId)
                VALUES (
                    $timestampUtc,
                    $level,
                    $messageTemplate,
                    $renderedMessage,
                    $exception,
                    $eventJson,
                    $traceId,
                    $requestId);
                """;

            command.Parameters.AddWithValue("$timestampUtc", logEvent.Timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("$level", logEvent.Level.ToString());
            command.Parameters.AddWithValue("$messageTemplate", logEvent.MessageTemplate.Text);
            command.Parameters.AddWithValue("$renderedMessage", logEvent.RenderMessage(CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("$exception", (object?)logEvent.Exception?.ToString() ?? DBNull.Value);
            command.Parameters.AddWithValue("$eventJson", eventJson);
            command.Parameters.AddWithValue("$traceId", (object?)GetPropertyValue(logEvent, "TraceId") ?? DBNull.Value);
            command.Parameters.AddWithValue("$requestId", (object?)GetPropertyValue(logEvent, "RequestId") ?? DBNull.Value);

            command.ExecuteNonQuery();
         }
      }
      catch(SqliteException exception)
      {
         SelfLog.WriteLine("Failed to persist a log event to SQLite: {0}", exception);
      }
      catch(IOException exception)
      {
         SelfLog.WriteLine("Failed to persist a log event to SQLite: {0}", exception);
      }
      catch(UnauthorizedAccessException exception)
      {
         SelfLog.WriteLine("Failed to persist a log event to SQLite: {0}", exception);
      }
      catch(InvalidOperationException exception)
      {
         SelfLog.WriteLine("Failed to persist a log event to SQLite: {0}", exception);
      }
   }

   private void CleanupIfNeeded()
   {
      DateOnly currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
      if(currentDate <= lastCleanupDate)
      {
         return;
      }

      LogStorageMaintenance.CleanupExpiredFiles(paths);
      lastCleanupDate = currentDate;
   }

   private static SqliteConnection CreateConnection(string databasePath)
   {
      SqliteConnectionStringBuilder connectionStringBuilder = new()
      {
         DataSource = databasePath,
         Cache = SqliteCacheMode.Shared,
         Mode = SqliteOpenMode.ReadWriteCreate,
         Pooling = true
      };

      return new SqliteConnection(connectionStringBuilder.ToString());
   }

   private static void InitializeDatabase(SqliteConnection connection)
   {
      using SqliteCommand command = connection.CreateCommand();
      command.CommandText = """
          PRAGMA journal_mode = WAL;
          PRAGMA synchronous = NORMAL;
          CREATE TABLE IF NOT EXISTS Logs (
              Id INTEGER PRIMARY KEY AUTOINCREMENT,
              TimestampUtc TEXT NOT NULL,
              Level TEXT NOT NULL,
              MessageTemplate TEXT NOT NULL,
              RenderedMessage TEXT NOT NULL,
              Exception TEXT NULL,
              EventJson TEXT NOT NULL,
              TraceId TEXT NULL,
              RequestId TEXT NULL
          );
          CREATE INDEX IF NOT EXISTS IX_Logs_TimestampUtc ON Logs (TimestampUtc);
          CREATE INDEX IF NOT EXISTS IX_Logs_Level ON Logs (Level);
          """;
      command.ExecuteNonQuery();
   }

   private string FormatEvent(LogEvent logEvent)
   {
      using StringWriter writer = new(CultureInfo.InvariantCulture);
      formatter.Format(logEvent, writer);
      return writer.ToString();
   }

   private static string? GetPropertyValue(LogEvent logEvent, string propertyName)
   {
      if(!logEvent.Properties.TryGetValue(propertyName, out LogEventPropertyValue? propertyValue))
      {
         return null;
      }

      return propertyValue switch
      {
         ScalarValue { Value: null } => null,
         ScalarValue scalarValue => Convert.ToString(scalarValue.Value, CultureInfo.InvariantCulture),
         _ => propertyValue.ToString().Trim('"')
      };
   }
}