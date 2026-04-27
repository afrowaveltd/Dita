using Dita.Shared.Localization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Hosted background service that periodically triggers the <see cref="IBackendTranslationService"/>
/// to process pending translations. Scheduling behaviour is driven by the
/// <c>AutomaticTranslationSettings</c> section in application configuration.
/// </summary>
/// <param name="logger">Logger used by this service.</param>
/// <param name="settings">Loaded automatic translation settings.</param>
/// <param name="configuration">Application configuration used for robust schedule parsing.</param>
/// <param name="service">Root service provider used to create per-run DI scopes.</param>
public class Scheduller(
   ILogger<Scheduller> logger,
   AutomaticTranslationSettings settings,
   IConfiguration configuration,
   IServiceProvider service) : IHostedService, IDisposable
{
   private readonly ILogger<Scheduller> _logger = logger;
   private readonly IServiceProvider _service = service;
   private readonly AutomaticTranslationSettings _settings = settings;

   /// <summary>Delay before the first translation run after startup.</summary>
   private readonly TimeSpan _waitingTime = ResolveWaitingTime(configuration, settings);

   /// <summary>Delay between subsequent translation runs.</summary>
   private readonly TimeSpan _checkingPeriod = ResolveCheckingPeriod(settings);

   /// <summary>Whether the periodic timer should be started automatically.</summary>
   private readonly bool _automaticRun = settings.AutomaticRun;

   /// <summary>Guards against overlapping runs triggered by the timer.</summary>
   private int _isProcessing;

   private Timer? _timer;

   /// <summary>
   /// Starts the background service. Schedules the periodic timer when <c>AutomaticRun</c> is enabled.
   /// </summary>
   /// <param name="cancellationToken">Token that signals host shutdown.</param>
   public Task StartAsync(CancellationToken cancellationToken)
   {
      _logger.LogInformation(
         "Scheduled Translation Service is starting. AutomaticRun={AutomaticRun}, WaitingTime={WaitingTime}, CheckingPeriod={CheckingPeriod}.",
         _automaticRun,
         _waitingTime,
         _checkingPeriod);

      if(_automaticRun)
      {
         _timer = new Timer(
            async _ => await DoWorkAsync(),
            state: null,
            dueTime: _waitingTime,
            period: _checkingPeriod);

         _logger.LogDebug(
            "Translation timer scheduled: first run in {WaitingTime}, then every {CheckingPeriod}.",
            _waitingTime,
            _checkingPeriod);
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
      if(Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
      {
         _logger.LogWarning("Scheduled Translation Service is already processing – skipping this run.");
         return;
      }

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
         Interlocked.Exchange(ref _isProcessing, 0);
      }
   }

   private static TimeSpan ResolveWaitingTime(IConfiguration configuration, AutomaticTranslationSettings settings)
   {
      string? rawWaitingTime = configuration["AutomaticTranslationSettings:WaitingTime"];
      if(string.IsNullOrWhiteSpace(rawWaitingTime))
      {
         return TimeSpan.Zero;
      }

      if(int.TryParse(rawWaitingTime, NumberStyles.Integer, CultureInfo.InvariantCulture, out int waitingMinutes))
      {
         return waitingMinutes <= 0 ? TimeSpan.Zero : TimeSpan.FromMinutes(waitingMinutes);
      }

      return settings.WaitingTime < TimeSpan.Zero ? TimeSpan.Zero : settings.WaitingTime;
   }

   private static TimeSpan ResolveCheckingPeriod(AutomaticTranslationSettings settings)
   {
      int periodMinutes = settings.CheckingPeriod;
      if(periodMinutes <= 0)
      {
         return TimeSpan.FromMinutes(30);
      }

      return TimeSpan.FromMinutes(periodMinutes);
   }
}
