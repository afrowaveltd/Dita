using Afrowave.SharedTools.Models.Localization;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dita.Shared.Localization.Services;

public class LanguageService
{
   private readonly ILogger<LanguageService> _logger;
   private readonly IStringLocalizer<LanguageService> _t;

   public List<Language> Languages { get; private set; }
   public LanguageService(ILogger<LanguageService> logger, IStringLocalizer<LanguageService> t)
   {
      _logger = logger;
      _t = t;
      try
      {
         if(!File.Exists(JsonFilePath))
         {
            _logger.LogWarning("Languages JSON file not found at path: {JsonFilePath}", JsonFilePath);
            Languages = [];
         }
         var json = File.ReadAllText(JsonFilePath);
         var languages = System.Text.Json.JsonSerializer.Deserialize<List<Language>>(json);
         Languages = languages ?? [];
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error loading languages from JSON file at path: {JsonFilePath}", JsonFilePath);
         Languages = [];
      }
   }

   public List<string> GetLanguageNames() => [.. Languages.Select(l => l.Name).OrderBy(l => l)];
   public bool IsRtl(string code)
   {
      if(string.IsNullOrWhiteSpace(code)) return false;

      var language = Languages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
      return language?.Rtl ?? false;
   }

   public Response<Language>? GetLanguageByCode(string code)
   {
      Response<Language> result = new();
      if(string.IsNullOrWhiteSpace(code))
      {
         Response<Language>.Fail(_t["Language code cannot be null or empty"].Value);
      }
      var language = Languages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
      if(language == null)
      {
         return Response<Language>.Fail($"{_t["Language code"]} {code} {_t["not found"].Value}");
      }
      return Response<Language>.Ok(language, _t["Language found successfully"].Value);
   }

   public Response<List<Language>> GetRequiredLanguagesAsync()
   {
      try
      {
         if(!Directory.Exists(LocalesPath))
         {
            _logger.LogWarning("Locales directory not found {directory}", LocalesPath);
            return Response<List<Language>>.Fail(_t["Locales directory not found"].Value);
         }

         var files = Directory.GetFiles(LocalesPath, "*.json");
         List<Language> requiredLanguages = [];

         foreach(var file in files)
         {
            var languageCode = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();

            if(languageCode.Length == 2 && languageCode.All(char.IsLetter))
            {
               var language = Languages.FirstOrDefault(l =>
                    l.Code.Equals(languageCode, StringComparison.OrdinalIgnoreCase));

               if(language != null)
               {
                  requiredLanguages.Add(language);
               }
            }
         }

         return Response<List<Language>>.Ok(requiredLanguages, $"Successfully retrieved {requiredLanguages.Count} languages");

      }
      catch(Exception ex)
      {
         _logger.LogWarning(ex, "Error retreaving language list");
         return Response<List<Language>>.Fail(ex);
      }
   }

   public Response<List<Language>> GetSelectedLanguagesInfo(List<string> languages)
   {
      Response<List<Language>> result = new()
      {
         Data = []
      };
      foreach(var language in languages)
      {
         try
         {
            if(Languages.Where(s => s.Code == language).First() == null)
            {
               result.Data.Add(new Language { Code = language, Name = language, Native = language, Rtl = false });
            }
            else
            {
               result.Data.Add(Languages.Where(s => s.Code == language).First());
            }
         }
         catch(Exception ex)
         {
            _logger.LogWarning(ex, "Error getting language info");
            result.Data.Add(new Language { Code = language, Name = language, Native = language, Rtl = false });
         }
      }
      return result;
   }

   public async Task<Response<Dictionary<string, string>>> GetDictionaryAsync(string code)
   {
      if(code == null || code.Length < 2)
      {
         _logger.LogDebug("Invalid code {language}", code);
         return Response<Dictionary<string, string>>.Fail(_t["Invalid code"].Value);
      }
      var filePath = Path.Combine(LocalesPath, code.ToLowerInvariant() + ".json");

      if(!File.Exists(filePath))
      {
         _logger.LogDebug("File not found for dictionary {dictionary}", code);
         return Response<Dictionary<string, string>>.Fail(_t["Dictionary file not found"].Value);
      }
      try
      {
         var fileContext = await File.ReadAllTextAsync(filePath);
         var data = new Dictionary<string, string>();
         try
         {
            data = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonDocument.Parse(fileContext));
            if(data == null)
            {
               return Response<Dictionary<string, string>>.SuccessWithWarning(data!, _t["No data in the file"].Value);
            }
            if(data?.Count == 0)
            {
               return Response<Dictionary<string, string>>.SuccessWithWarning(data!, _t["The list is empty"].Value);
            }
            return Response<Dictionary<string, string>>.Ok(data!, code);
         }
         catch(Exception ex)
         {
            return Response<Dictionary<string, string>>.Fail(ex);
         }
      }
      catch(Exception ex)
      {
         return Response<Dictionary<string, string>>.Fail(ex);
      }
   }

   public async Task<Response<Dictionary<string, string>>> GetLastStored()
   {
      if(!File.Exists(OldTranslationPath))
      {
         _logger.LogDebug("Previous version of the default language file was not found");
         return Response<Dictionary<string, string>>.Fail(_t["not found"].Value);
      }
      try
      {
         string oldTranslationJson = await File.ReadAllTextAsync(OldTranslationPath);
         if(oldTranslationJson == null)
            return Response<Dictionary<string, string>>.Fail(_t["old Translation File is empty"].Value);
         if(oldTranslationJson.Length == 0)
            return Response<Dictionary<string, string>>.Fail(_t["old Translation File is empty"].Value);
         return new Response<Dictionary<string, string>>() { Data = JsonSerializer.Deserialize<Dictionary<string, string>>(oldTranslationJson) ?? [] };
      }
      catch(Exception ex)
      {
         return Response<Dictionary<string, string>>.Fail(ex);
      }
   }
   public async Task<Response<List<SingleTranslation>>> GetAllDictionariesAsync()
   {
      var languagesNeeded = TranslationsPresented();
      if(languagesNeeded != null)
      {
         var final = new List<SingleTranslation>();
         foreach(var language in languagesNeeded)
         {
            Response<Dictionary<string, string>> response = await GetDictionaryAsync(language);
            final.Add(new SingleTranslation
            {
               Language = language,
               Translations = response.Data ?? []
            });
         }
         return new Response<List<SingleTranslation>>() { Success = true, Data = final };
      }
      return Response<List<SingleTranslation>>.Fail(_t["No files in the folder"].Value);
   }

   private string[] TranslationsPresented()
   {
      List<string> result = [];
      var languageFiles = Directory.GetFiles(LocalesPath, "*.json");
      foreach(var languageFile in languageFiles)
      {
         result.Add(Path.GetFileNameWithoutExtension(languageFile).ToLowerInvariant());
      }
      return [.. result];
   }

   private static string JsonFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory
[..AppDomain.CurrentDomain.BaseDirectory
      .IndexOf("bin")], "Jsons", "languages.json");
   private static string LocalesPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory
[..AppDomain.CurrentDomain.BaseDirectory
      .IndexOf("bin")], "Locales");
   private static string OldTranslationPath => Path.Combine(Path.GetTempPath(), "old.json");



}