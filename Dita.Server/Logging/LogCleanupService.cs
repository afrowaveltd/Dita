using Serilog.Debugging;

namespace Dita.Server.Logging;

internal sealed class LogCleanupService(LogStoragePaths paths) : BackgroundService
{
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
