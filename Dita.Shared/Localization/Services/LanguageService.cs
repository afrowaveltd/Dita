using Afrowave.SharedTools.Models.Localization;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dita.Shared.Localization.Services;
/// <summary>
/// A service responsible for managing languages and their translations. It loads language information from a JSON file and provides methods to retrieve language details, check if a language is right-to-left, and manage translation dictionaries. The service also handles saving and retrieving translations, as well as creating missing language files when necessary.
/// </summary>
public class LanguageService : ILanguageService
{
   private readonly ILogger<LanguageService> _logger;
   private readonly IStringLocalizer<LanguageService> _t;
/// <summary>
/// A list of available languages loaded from the JSON file. Each language includes its code, name, native name, and whether it is a right-to-left language. This list is used throughout the service to provide language information and manage translations.
/// </summary>
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
            Languages = new List<Language>();
         }
         var json = File.ReadAllText(JsonFilePath);
         var languages = System.Text.Json.JsonSerializer.Deserialize<List<Language>>(json);
         Languages = languages ?? new List<Language>();
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error loading languages from JSON file at path: {JsonFilePath}", JsonFilePath);
         Languages = [];
      }
   }

/// <summary>
/// Retrieves a list of language names available in the service. The names are sorted alphabetically for easier access. This method is useful for displaying language options to users or for any functionality that requires a list of language names without needing the full language details.
/// </summary>
/// <returns>A list of language names sorted alphabetically.</returns>
   public List<string> GetLanguageNames() => [.. Languages.Select(l => l.Name).OrderBy(l => l)];

   /// <summary>
   /// Checks if a given language code corresponds to a right-to-left (RTL) language. This is determined by looking up the language in the list of available languages and checking its RTL property. This method is important for ensuring that the user interface can adapt appropriately for languages that are read from right to left, such as Arabic or Hebrew.
   /// </summary>
   /// <param name="code">The language code to check.</param>
   /// <returns>True if the language is right-to-left; otherwise, false.</returns>
   public bool IsRtl(string code)
   {
      if(string.IsNullOrWhiteSpace(code)) return false;

      var language = Languages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
      return language?.Rtl ?? false;
   }
/// <summary>
/// Retrieves a language object based on its code. The method checks if the provided code is valid and then looks up the language in the list of available languages. If the language is found, it returns a successful response with the language data; if not, it returns a failure response indicating that the language code was not found. This method is essential for any functionality that requires detailed information about a specific language based on its code.
/// </summary>
/// <param name="code">The language code to look up.</param>
/// <returns>A response containing the language object if found, or an error message if not.</returns>
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
/// <summary>
/// Retrieves a list of required languages based on the JSON files present in the "Locales" directory. The method checks for the existence of the directory and then looks for JSON files that correspond to language codes. It validates the language codes and matches them against the available languages in the service. The result is a list of languages that are required based on the existing translation files, which can be used to ensure that all necessary translations are available for the application. If any issues arise during this process, such as missing directories or files, appropriate error messages are logged and returned in the response.
/// </summary>
/// <returns>A response containing the list of required languages if successful, or an error message if not.</returns>
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
/// <summary>
/// Retrieves information about a list of selected languages based on their codes. The method iterates through the provided list of language codes, checks if each code corresponds to a valid language in the service, and collects the language information. If a language code is not found in the available languages, it creates a new language object with the code as its name and native name, and assumes it is not a right-to-left language. The result is a list of language objects corresponding to the provided codes, which can be used for various purposes such as displaying selected languages or managing translations. Any errors encountered during this process are logged, and the method ensures that all provided codes are processed even if some are invalid.
/// </summary>
/// <param name="languages">A list of language codes to retrieve information for.</param>
/// <returns>A response containing the list of language objects corresponding to the provided codes.  </returns>
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
/// <summary>
/// Retrieves a dictionary of translations for a specific language code. The method checks if the provided language code is valid and then looks for a corresponding JSON file in the "Locales" directory. If the file exists, it reads the content and deserializes it into a dictionary of key-value pairs representing translation keys and their corresponding translated values. If the file does not exist or if any errors occur during this process, appropriate error messages are logged and returned in the response. This method is essential for loading translations for a specific language, which can then be used to display localized content in the application.
/// </summary>
/// <param name="code">The language code for which to retrieve the dictionary.</param>
/// <returns>A response containing the dictionary of translations for the specified language code.</returns>
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
/// <summary>
/// Retrieves the last stored translation dictionary from a temporary file. This method is used to access a backup of the previous version of the default language file, which can be useful for restoring translations or comparing changes. The method checks if the backup file exists and then reads its content, deserializing it into a dictionary of translations. If the file does not exist, is empty, or if any errors occur during this process, appropriate error messages are logged and returned in the response. This functionality is crucial for maintaining translation data integrity and providing a fallback option in case of issues with the current translation files.
/// </summary>
/// <returns>A response containing the dictionary of translations from the last stored backup file.</returns>
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
/// <summary>
/// Retrieves all available translation dictionaries. This method iterates through the list of languages for which translations are needed, retrieves each dictionary, and compiles them into a list of `SingleTranslation` objects. Each `SingleTranslation` object contains the language code and its corresponding translations. If no languages are found, an appropriate error message is returned.
/// </summary>
/// <returns>A response containing a list of all translation dictionaries.</returns>
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
   /// <summary>
   /// Saves a translation dictionary for a specific language. This method validates the language code and then serializes the translation data into a JSON file. If a file already exists for the language, a backup is created before overwriting it. Any errors encountered during this process are logged and returned in the response.
   /// </summary>
   /// <param name="data">The translation data to be saved.</param>
   /// <returns>A response indicating the success or failure of the save operation.</returns>
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
/// <summary>
/// Saves the previous version of the default language file as a backup before it gets overwritten. This method takes a dictionary of translations and saves it to a temporary file. This backup can be used to restore the previous translations if needed. The method handles any errors that may occur during the file writing process and logs them accordingly. It returns a response indicating whether the save operation was successful or if it failed due to an error.
/// </summary>
/// <param name="data">The dictionary of translations to be saved as a backup.</param>
/// <returns>A response indicating the success or failure of the backup operation.</returns>
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
/// <summary>
/// Saves multiple translation dictionaries at once. This method takes a list of `SingleTranslation` objects, each containing a language code and its corresponding translations, and saves each dictionary using the `SaveDictionaryAsync` method. The result is a response containing a dictionary that indicates the success or failure of saving each language's translations. This method is useful for batch operations where multiple translations need to be updated or added at the same time. Any errors encountered during the saving process are logged and included in the response.
/// </summary>
/// <param name="tree">A list of `SingleTranslation` objects representing the translations to be saved.</param>
/// <returns>A response containing a dictionary that maps each language code to a boolean indicating the success or failure of the save operation.</returns>
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
/// <summary>
/// Creates missing language files based on a list of language codes. This method checks if a JSON file exists for each language code in the "Locales" directory, and if not, it creates an empty JSON file for that language. The result is a dictionary that indicates whether a new file was created for each language code. This functionality is important for ensuring that all necessary language files are present in the system, especially when new languages are added or when setting up the application for the first time. Any errors encountered during the file creation process are logged and included in the response.
/// </summary>
/// <param name="languages">A list of language codes for which to create missing files.</param>
/// <returns>A dictionary mapping each language code to a boolean indicating whether a new file was created.</returns>
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