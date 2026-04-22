using Dita.Shared.Localization.Hubs;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.ScheduledTranslationService;

public class BackendTranslationService(
   IConfiguration configuration,
   ILibreTranslateService translateService,
   ILogger<BackendTranslationService> logger,
   IHubContext<LocalizationHub> hub) : IBackendTranslationService
{
   private AutomaticTranslationSettings _settings => configuration
      .GetSection("ScheduledTranslationService:AutomaticTranslation")
      .Get<AutomaticTranslationSettings>() ?? new();

   private readonly ILogger<BackendTranslationService> _logger = logger;
   private readonly ILibreTranslateService _translateService = translateService;
   private readonly IHubContext<LocalizationHub> _hub = hub;

   public async Task RunAsync()
   {
      // Implementation for running the backend translation service
      await Task.CompletedTask;
   }
}