using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Hubs;
using Dita.Shared.Localization.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dita.Shared.Localization.Services;

public class LibreTranslateService(AutomaticTranslationSettings settings, ILibreTranslateHttpClientFactory httpClientFactory, IHubContext<LocalizationHub> hub, ILogger<LibreTranslateService> logger)
{
   private readonly AutomaticTranslationSettings _settings = settings;
   private readonly HttpClient libreClient = httpClientFactory.LibreClient;
   private readonly IHubContext<LocalizationHub> _hub = hub;
   private readonly ILogger<LibreTranslateService> _logger = logger;

   private readonly JsonSerializerOptions _options = new()
   {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DefaultIgnoreCondition = JsonIgnoreCondition.Never,
      ReferenceHandler = ReferenceHandler.IgnoreCycles
   };

   private Response<int> ServerLatency()
   {
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();
      var response = libreClient.GetAsync("/").Result;
      stopwatch.Stop();
      if(response.IsSuccessStatusCode)
      {
         return new Response<int>
         {
            Success = true,
            Data = (int)stopwatch.ElapsedMilliseconds,
            Message = "Server latency measured successfully."
         };
      }
      else
      {
         _logger.LogError("Failed to measure server latency. Status code: {StatusCode}", response.StatusCode);
         return new Response<int>
         {
            Success = false,
            Data = 0,
            Message = "Failed to measure server latency."
         };
      }
   }
}