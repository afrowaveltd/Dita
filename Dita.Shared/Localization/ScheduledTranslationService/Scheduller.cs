using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Hosted background service that periodically triggers the <see cref="IBackendTranslationService"/>
/// to process pending translations. Scheduling behaviour is driven by the
/// <c>ScheduledTranslationService</c> section in application configuration.
/// </summary>
/// <param name="logger">Logger used by this service.</param>
/// <param name="configuration">Application configuration; reads <c>WaitingTime</c>, <c>CheckingPeriod</c> and <c>AutomaticRun</c>.</param>
/// <param name="service">Root service provider used to create per-run DI scopes.</param>
public class Scheduller(ILogger<Scheduller> logger, IConfiguration configuration, IServiceProvider service) : IHostedService, IDisposable
{
   private readonly ILogger<Scheduller> _logger = logger;
   private readonly IServiceProvider _service = service;

   /// <summary>Minutes to wait before the first translation run after startup.</summary>
   private readonly int _waitingTime = configuration.GetValue<int>("ScheduledTranslationService:WaitingTime");

   /// <summary>Minutes between subsequent translation runs.</summary>
   private readonly int _checkingPeriod = configuration.GetValue<int>("ScheduledTranslationService:CheckingPeriod");

   /// <summary>Whether the periodic timer should be started automatically.</summary>
   private readonly bool _automaticRun = configuration.GetValue<bool>("ScheduledTranslationService:AutomaticRun");

   /// <summary>Guards against overlapping runs triggered by the timer.</summary>
   private volatile bool _isProcessing;

   private Timer? _timer;

   /// <summary>
   /// Starts the background service. Schedules the periodic timer when <c>AutomaticRun</c> is enabled.
   /// </summary>
   /// <param name="cancellationToken">Token that signals host shutdown.</param>
   public Task StartAsync(CancellationToken cancellationToken)
   {
      _logger.LogInformation(
         "Scheduled Translation Service is starting. AutomaticRun={AutomaticRun}, WaitingTime={WaitingTime} min, CheckingPeriod={CheckingPeriod} min.",
         _automaticRun, _waitingTime, _checkingPeriod);

      if(_automaticRun)
      {
         _timer = new Timer(
            async _ => await DoWorkAsync(),
            state: null,
            dueTime: TimeSpan.FromMinutes(_waitingTime),
            period: TimeSpan.FromMinutes(_checkingPeriod));

         _logger.LogDebug(
            "Translation timer scheduled: first run in {WaitingTime} min, then every {CheckingPeriod} min.",
            _waitingTime, _checkingPeriod);
      }
      else
      {
         _logger.LogInformation("AutomaticRun is disabled – translation timer will not be started.");
      }

      return Task.CompletedTask;
   }

   /// <summary>
   /// Stops the background service by disabling the timer. Does not cancel an in-progress run.
   /// </summary>
   /// <param name="cancellationToken">Token that signals host shutdown.</param>
   public Task StopAsync(CancellationToken cancellationToken)
   {
      _logger.LogInformation("Scheduled Translation Service is stopping.");
      _timer?.Change(Timeout.Infinite, 0);
      return Task.CompletedTask;
   }

   /// <summary>Releases the timer resource.</summary>
   public void Dispose()
   {
      _timer?.Dispose();
      GC.SuppressFinalize(this);
   }

   /// <summary>
   /// Executes a single translation run inside a dedicated DI scope.
   /// Skips the run if a previous one is still in progress.
   /// </summary>
   private async Task DoWorkAsync()
   {
      if(_isProcessing)
      {
         _logger.LogWarning("Scheduled Translation Service is already processing – skipping this run.");
         return;
      }

      _isProcessing = true;
      _logger.LogDebug("Scheduled translation run started.");

      try
      {
         using var scope = _service.CreateScope();
         var translationService = scope.ServiceProvider.GetRequiredService<IBackendTranslationService>();
         await translationService.RunAsync();
         _logger.LogInformation("Scheduled translation run completed successfully.");
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "An error occurred during the scheduled translation run.");
      }
      finally
      {
         _isProcessing = false;
      }
   }
}
