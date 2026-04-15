using Afrowave.SharedTools.Models.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.Services;

public class LanguageService(ILogger<LanguageService> logger, IStringLocalizer<LanguageService> t)
{
   private readonly ILogger<LanguageService> _logger = logger;
   private readonly IStringLocalizer<LanguageService> _t = t;

   private static string JsonFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory
[..AppDomain.CurrentDomain.BaseDirectory
      .IndexOf("bin")], "Jsons", "languages.json");

   public List<Language> Languages
   {
      get
      {
         try
         {
            if(!File.Exists(JsonFilePath))
            {
               _logger.LogWarning("Languages JSON file not found at path: {JsonFilePath}", JsonFilePath);
               return [];
            }
            var json = File.ReadAllText(JsonFilePath);
            var languages = System.Text.Json.JsonSerializer.Deserialize<List<Language>>(json);
            return languages ?? [];
         }
         catch(Exception ex)
         {
            _logger.LogError(ex, "Error loading languages from JSON file at path: {JsonFilePath}", JsonFilePath);
            return [];
         }
      }
   }
}