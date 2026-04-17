using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Globalization;
using System.Text;

namespace Dita.Server.Logging;

/// <summary>
/// A Serilog sink that appends log events as objects inside a JSON array file, creating one file per calendar day.
/// </summary>
/// <remarks>
/// Each file is named <c>server-yyyyMMdd.json</c> and stored in the text log directory supplied via
/// <see cref="LogStoragePaths"/>. The sink is thread-safe and performs daily cleanup of files that exceed the
/// configured retention period. Legacy NDJSON files are automatically migrated to the JSON-array format on first
/// write.
/// </remarks>
/// <param name="paths">Storage path configuration that determines where log files are written and how long they are kept.</param>
internal sealed class JsonArrayFileSink(LogStoragePaths paths) : ILogEventSink
{
   private readonly Lock syncRoot = new();
   private readonly CompactJsonFormatter formatter = new();
   private DateOnly lastCleanupDate = DateOnly.MinValue;

   /// <summary>Formats and persists a single log event to the daily JSON array file.</summary>
   /// <param name="logEvent">The log event to persist. Must not be <see langword="null"/>.</param>
   public void Emit(LogEvent logEvent)
   {
      ArgumentNullException.ThrowIfNull(logEvent);

      try
      {
         string filePath = GetFilePath(logEvent.Timestamp);
         string eventJson = FormatEvent(logEvent);

         lock(syncRoot)
         {
            CleanupIfNeeded();
            paths.EnsureDirectories();
            EnsureJsonArrayFile(filePath);
            AppendToJsonArray(filePath, eventJson);
         }
      }
      catch(IOException exception)
      {
         SelfLog.WriteLine("Failed to persist a log event to JSON file: {0}", exception);
      }
      catch(UnauthorizedAccessException exception)
      {
         SelfLog.WriteLine("Failed to persist a log event to JSON file: {0}", exception);
      }
      catch(InvalidOperationException exception)
      {
         SelfLog.WriteLine("Failed to persist a log event to JSON file: {0}", exception);
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

   private string FormatEvent(LogEvent logEvent)
   {
      using StringWriter writer = new(CultureInfo.InvariantCulture);
      formatter.Format(logEvent, writer);
      return writer.ToString().TrimEnd('\r', '\n');
   }

   private string GetFilePath(DateTimeOffset timestamp)
   {
      DateTime localDate = timestamp.ToLocalTime().Date;
      return Path.Combine(paths.TextDirectory, $"server-{localDate:yyyyMMdd}.json");
   }

   private static void EnsureJsonArrayFile(string filePath)
   {
      if(!File.Exists(filePath))
      {
         File.WriteAllText(filePath, "[]", new UTF8Encoding(false));
         return;
      }

      string content = File.ReadAllText(filePath);
      string trimmed = content.Trim();

      if(trimmed.Length == 0)
      {
         File.WriteAllText(filePath, "[]", new UTF8Encoding(false));
         return;
      }

      if(trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal))
      {
         return;
      }

      string[] lineEvents = content
         .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

      string migratedContent = lineEvents.Length == 0
         ? "[]"
         : $"[\n{string.Join(",\n", lineEvents)}\n]";

      File.WriteAllText(filePath, migratedContent, new UTF8Encoding(false));
   }

   private static void AppendToJsonArray(string filePath, string eventJson)
   {
      using FileStream stream = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

      if(stream.Length == 0)
      {
         WriteText(stream, "[]");
      }

      long closingBracketPosition = FindClosingBracketPosition(stream);
      bool hasItems = HasItems(stream, closingBracketPosition);

      stream.SetLength(closingBracketPosition);
      stream.Seek(closingBracketPosition, SeekOrigin.Begin);

      string appendText = hasItems
         ? $",\n{eventJson}\n]"
         : $"\n{eventJson}\n]";

      WriteText(stream, appendText);
   }

   private static long FindClosingBracketPosition(FileStream stream)
   {
      for(long position = stream.Length - 1; position >= 0; position--)
      {
         stream.Seek(position, SeekOrigin.Begin);
         int value = stream.ReadByte();
         if(value < 0)
         {
            continue;
         }

         char character = (char)value;
         if(char.IsWhiteSpace(character))
         {
            continue;
         }

         if(character == ']')
         {
            return position;
         }

         throw new InvalidOperationException("The log file is not a JSON array.");
      }

      throw new InvalidOperationException("The log file is empty or invalid.");
   }

   private static bool HasItems(FileStream stream, long closingBracketPosition)
   {
      for(long position = closingBracketPosition - 1; position >= 0; position--)
      {
         stream.Seek(position, SeekOrigin.Begin);
         int value = stream.ReadByte();
         if(value < 0)
         {
            continue;
         }

         char character = (char)value;
         if(char.IsWhiteSpace(character))
         {
            continue;
         }

         if(character == '[')
         {
            return false;
         }

         return true;
      }

      return false;
   }

   private static void WriteText(FileStream stream, string content)
   {
      using StreamWriter writer = new(stream, new UTF8Encoding(false), leaveOpen: true);
      writer.Write(content);
      writer.Flush();
   }
}