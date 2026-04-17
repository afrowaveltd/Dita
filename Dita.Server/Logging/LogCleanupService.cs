using Serilog.Debugging;

namespace Dita.Server.Logging;

/// <summary>
/// Background service that periodically removes log files older than the configured retention period.
/// </summary>
/// <remarks>
/// The cleanup runs once on startup and then on the interval defined by <see cref="LogStoragePaths.CleanupInterval"/>.
/// Errors during cleanup are written to the Serilog self-log so they never suppress normal application logging.
/// </remarks>
/// <param name="paths">Storage path configuration that determines where log files reside and how long they are kept.</param>
internal sealed class LogCleanupService(LogStoragePaths paths) : BackgroundService
{
   /// <summary>
   /// Executes the cleanup loop. Runs one immediate cleanup on start, then waits for each timer tick.
   /// </summary>
   /// <param name="stoppingToken">Token signalled when the host is shutting down.</param>
   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      RunCleanup();

      using PeriodicTimer timer = new(LogStoragePaths.CleanupInterval);

      try
      {
         while(await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
         {
            RunCleanup();
         }
      }
      catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
      {
      }
   }

   private void RunCleanup()
   {
      try
      {
         LogStorageMaintenance.CleanupExpiredFiles(paths);
      }
      catch(IOException exception)
      {
         SelfLog.WriteLine("Failed to clean up log files: {0}", exception);
      }
      catch(UnauthorizedAccessException exception)
      {
         SelfLog.WriteLine("Failed to clean up log files: {0}", exception);
      }
   }
}