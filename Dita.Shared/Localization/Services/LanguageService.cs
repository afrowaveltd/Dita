using Afrowave.SharedTools.Models.Localization;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dita.Shared.Localization.Services;

public class LanguageService : ILanguageService
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
   public async Task<Response<bool>> SaveDictionaryAsync(SingleTranslation data)
   {

      if(data.Language == null)
      {
         return Response<bool>.Fail(_t["Code can't be null"].Value);
      }
      if(data.Language.Length < 2)
      {
         return Response<bool>.Fail(_t["Invalid code"].Value);
      }

      string json = JsonSerializer.Serialize(data.Translations ?? []);
      var path = Path.Combine(LocalesPath, data.Language + ".json");

      if(File.Exists(path))
      {
         try
         {
            string backupPath = Path.Combine(Path.GetTempPath(), "dita", data.Language + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak");
            File.Move(path, backupPath);
         }
         catch(Exception ex)
         {
            _logger.LogError(ex, "Backup file of the translation {language} could not be made", data.Language);
            return Response<bool>.Fail($"Error creating backup: {ex.Message}");
         }
      }
      try
      {
         await File.WriteAllTextAsync(json, path);
         return Response<bool>.Ok(true, _t["Successfully stored"].Value);
      }
      catch(Exception ex)
      {
         return Response<bool>.Fail($"Error storing data: {ex.Message}");
      }

   }

   public async Task<Response<bool>> SaveOldTranslationAsync(Dictionary<string, string> data)
   {
      try
      {
         string json = JsonSerializer.Serialize(data ?? []);
         await File.WriteAllTextAsync(OldTranslationPath, json);
         return Response<bool>.Ok(true);
      }
      catch(Exception ex)
      {
         {
            _logger.LogError(ex, "Couldn't save an old translation");
            return Response<bool>.Fail("Couldn't save an old translation");
         }
      }
   }

   public async Task<Response<Dictionary<string, bool>>> SaveTranslationsAsync(List<SingleTranslation> tree)
   {
      var response = new Response<Dictionary<string, bool>>
      {
         Data = []
      };
      foreach(SingleTranslation item in tree)
      {
         string language = item.Language;
         Dictionary<string, string> dictionary = item.Translations;
         var result = await SaveDictionaryAsync(item);
         response.Data[language] = result.Success;
      }
      return response;
   }

   public async Task<Dictionary<string, bool>> CreateMissingLanguageFilesAsync(List<string> languages)
   {
      var result = new Dictionary<string, bool>();
      foreach(string language in languages)
      {
         var res = await CreateEmptyLanguageFile(language);
         result[language] = res;
      }
      return result;
   }

   private async Task<bool> CreateEmptyLanguageFile(string code)
   {
      if(string.IsNullOrWhiteSpace(code) || code.Length < 2)
      {
         return false;
      }
      var path = Path.Combine(LocalesPath, code + ".json");
      if(File.Exists(path))
      {
         return false;
      }
      try
      {
         await File.WriteAllTextAsync(path, "{}");
         return true;
      }
      catch
      {
         return false;
      }
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