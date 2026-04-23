using Dita.Shared.Localization.Hubs;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.ScheduledTranslationService;

public class BackendTranslationService(
   ILanguageService languageService,
   IHubContext<LocalizationHub> hub,
   IConfiguration configuration,
   ILibreTranslateService translateService,
   ILogger<BackendTranslationService> logger) : IBackendTranslationService
{
   private readonly AutomaticTranslationSettings _settings = configuration.GetSection("AutomaticTranslationSettings").Get<AutomaticTranslationSettings>() ?? new AutomaticTranslationSettings();
   private readonly ILogger<BackendTranslationService> _logger = logger;
   private readonly ILanguageService _languageService = languageService;
   private readonly IHubContext<LocalizationHub> _hubContext = hub;
   private readonly ILibreTranslateService _translateService = translateService;
   private string DefaultLanguage => _settings.DefaultLanguage ?? "en";
   private List<string> IgnoredLanguages => _settings.IgnoredLanguages ?? [];
   public async Task RunAsync()
   {
      // Implementation for running the backend translation service
      await Task.CompletedTask;
   }
}