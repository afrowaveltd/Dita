using Afrowave.SharedTools.Models.Localization;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Manages available languages and their translation dictionaries.
/// Loads language metadata from a JSON file and provides methods to look up languages,
/// read/write locale files, and manipulate individual translation entries.
/// All file I/O is serialised through a <see cref="SemaphoreSlim"/> so the service
/// is safe to use from a background translation service running concurrently.
/// </summary>
public class LanguageService : ILanguageService
{
   private readonly ILogger<LanguageService> _logger;
   private readonly IStringLocalizer<LanguageService> _t;

   // Ensures that only one file operation runs at a time.
   private readonly SemaphoreSlim _fileLock = new(1, 1);

   // Path to the languages metadata file (languages.json), resolved outside the bin directory.
   private static string JsonFilePath
   {
      get
      {
         string baseDir = AppDomain.CurrentDomain.BaseDirectory;
         int binIndex = baseDir.IndexOf("bin", StringComparison.OrdinalIgnoreCase);
         string root = binIndex >= 0 ? baseDir[..binIndex] : baseDir;
         return Path.Combine(root, "Jsons", "languages.json");
      }
   }

   // Directory that holds per-language locale files (*.json).
   private static string LocalesPath
   {
      get
      {
         string baseDir = AppDomain.CurrentDomain.BaseDirectory;
         int binIndex = baseDir.IndexOf("bin", StringComparison.OrdinalIgnoreCase);
         string root = binIndex >= 0 ? baseDir[..binIndex] : baseDir;
         return Path.Combine(root, "Locales");
      }
   }

   // Temporary file used to keep a backup of the previous default translation.
   private static string OldTranslationPath => Path.Combine(Path.GetTempPath(), "old.json");

   /// <summary>
   /// List of available languages loaded from the JSON metadata file.
   /// </summary>
   public List<Language> Languages { get; private set; }

   /// <summary>
   /// Initialises the service and loads the language list from the JSON file.
   /// If the file is missing or corrupted, <see cref="Languages"/> is initialised as an empty list
   /// so the service can still operate without crashing.
   /// </summary>
   public LanguageService(ILogger<LanguageService> logger, IStringLocalizer<LanguageService> t)
   {
      _logger = logger;
      _t = t;
      Languages = LoadLanguages();
   }

   private List<Language> LoadLanguages()
   {
      if(!File.Exists(JsonFilePath))
      {
         _logger.LogWarning("Language metadata file not found: {JsonFilePath}", JsonFilePath);
         return [];
      }
      try
      {
         string json = File.ReadAllText(JsonFilePath);
         List<Language>? languages = JsonSerializer.Deserialize<List<Language>>(json);
         _logger.LogDebug("Loaded {Count} languages from {JsonFilePath}", languages?.Count ?? 0, JsonFilePath);
         return languages ?? [];
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error loading languages from: {JsonFilePath}", JsonFilePath);
         return [];
      }
   }

   /// <summary>
   /// Returns an alphabetically sorted list of language display names.
   /// </summary>
   public List<string> GetLanguageNames()
      => [.. Languages.Select(l => l.Name).OrderBy(l => l, StringComparer.CurrentCulture)];

   /// <summary>
   /// Determines whether the specified language code represents a right-to-left language.
   /// </summary>
   /// <param name="code">The ISO language code (e.g. "ar", "he").</param>
   /// <returns><c>true</c> if the language is RTL; otherwise <c>false</c>.</returns>
   public bool IsRtl(string code)
   {
      if(string.IsNullOrWhiteSpace(code)) return false;
      Language? language = Languages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
      return language?.Rtl ?? false;
   }

   /// <summary>
   /// Looks up a language by its code.
   /// </summary>
   /// <param name="code">The language code to search for.</param>
   /// <returns>A successful response containing the <see cref="Language"/>, or a failure response.</returns>
   public Response<Language>? GetLanguageByCode(string code)
   {
      if(string.IsNullOrWhiteSpace(code))
      {
         _logger.LogDebug("GetLanguageByCode: code is null or whitespace");
         return Response<Language>.Fail(_t["Language code cannot be null or empty"].Value);
      }

      Language? language = Languages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
      if(language is null)
      {
         _logger.LogDebug("Language with code '{Code}' was not found", code);
         return Response<Language>.Fail($"{_t["Language code"]} {code} {_t["not found"].Value}");
      }

      return Response<Language>.Ok(language, _t["Language found successfully"].Value);
   }

   /// <summary>
   /// Returns the set of languages for which a locale file exists in the Locales directory.
   /// Only 2-letter ISO-named files are considered.
   /// </summary>
   public Response<List<Language>> GetRequiredLanguagesAsync()
   {
      if(!Directory.Exists(LocalesPath))
      {
         _logger.LogWarning("Locales directory not found: {LocalesPath}", LocalesPath);
         return Response<List<Language>>.Fail(_t["Locales directory not found"].Value);
      }

      try
      {
         string[] files = Directory.GetFiles(LocalesPath, "*.json");
         List<Language> requiredLanguages = [];

         foreach(string file in files)
         {
            string languageCode = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            if(languageCode.Length != 2 || !languageCode.All(char.IsLetter)) continue;

            Language? language = Languages.FirstOrDefault(l =>
               l.Code.Equals(languageCode, StringComparison.OrdinalIgnoreCase));

            if(language is not null)
               requiredLanguages.Add(language);
         }

         _logger.LogDebug("GetRequiredLanguagesAsync: found {Count} languages", requiredLanguages.Count);
         return Response<List<Language>>.Ok(requiredLanguages,
            $"Loaded {requiredLanguages.Count} languages");
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error reading required language list");
         return Response<List<Language>>.Fail(ex);
      }
   }

   /// <summary>
   /// Returns detailed information for the given list of language codes.
   /// If a code is not found in the known languages list, a synthetic <see cref="Language"/>
   /// whose <c>Code</c>, <c>Name</c>, and <c>Native</c> are all set to the code is returned.
   /// </summary>
   /// <param name="languages">List of language codes to look up.</param>
   public Response<List<Language>> GetSelectedLanguagesInfo(List<string> languages)
   {
      List<Language> data = [];

      foreach(string code in languages)
      {
         Language? found = Languages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
         if(found is not null)
         {
            data.Add(found);
         }
         else
         {
            _logger.LogDebug("Language '{Code}' not found; using synthetic record", code);
            data.Add(new Language { Code = code, Name = code, Native = code, Rtl = false });
         }
      }

      return Response<List<Language>>.Ok(data);
   }

   /// <summary>
   /// Loads the translation dictionary for the specified language code from the Locales directory.
   /// </summary>
   /// <param name="code">Language code (minimum 2 characters).</param>
   public async Task<Response<Dictionary<string, string>>> GetDictionaryAsync(string code)
   {
      if(string.IsNullOrWhiteSpace(code) || code.Length < 2)
      {
         _logger.LogDebug("GetDictionaryAsync: invalid code '{Code}'", code);
         return Response<Dictionary<string, string>>.Fail(_t["Invalid code"].Value);
      }

      string filePath = Path.Combine(LocalesPath, code.ToLowerInvariant() + ".json");

      if(!File.Exists(filePath))
      {
         _logger.LogDebug("GetDictionaryAsync: locale file for '{Code}' not found at {FilePath}", code, filePath);
         return Response<Dictionary<string, string>>.Fail(_t["Dictionary file not found"].Value);
      }

      await _fileLock.WaitAsync().ConfigureAwait(false);
      try
      {
         Dictionary<string, string>? data = await ReadLocaleFileInternalAsync(filePath).ConfigureAwait(false);

         if(data is null)
            return Response<Dictionary<string, string>>.SuccessWithWarning([], _t["No data in the file"].Value);

         if(data.Count == 0)
         {
            _logger.LogDebug("Locale file for '{Code}' is empty", code);
            return Response<Dictionary<string, string>>.SuccessWithWarning(data, _t["The list is empty"].Value);
         }

         _logger.LogDebug("Loaded {Count} entries for language '{Code}'", data.Count, code);
         return Response<Dictionary<string, string>>.Ok(data, code);
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error reading locale file for '{Code}'", code);
         return Response<Dictionary<string, string>>.Fail(ex);
      }
      finally
      {
         _fileLock.Release();
      }
   }

   /// <summary>
   /// Loads the last stored backup of the default translation from the temp file.
   /// </summary>
   public async Task<Response<Dictionary<string, string>>> GetLastStored()
   {
      if(!File.Exists(OldTranslationPath))
      {
         _logger.LogDebug("Backup translation file not found: {OldTranslationPath}", OldTranslationPath);
         return Response<Dictionary<string, string>>.Fail(_t["not found"].Value);
      }

      try
      {
         string json = await File.ReadAllTextAsync(OldTranslationPath).ConfigureAwait(false);
         if(json.Length == 0)
         {
            _logger.LogWarning("Backup translation file is empty: {OldTranslationPath}", OldTranslationPath);
            return Response<Dictionary<string, string>>.Fail(_t["old Translation File is empty"].Value);
         }

         Dictionary<string, string> data = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
         return Response<Dictionary<string, string>>.Ok(data);
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error reading backup translation file");
         return Response<Dictionary<string, string>>.Fail(ex);
      }
   }

   /// <summary>
   /// Loads all available translation dictionaries from the Locales directory.
   /// Returns a failure response when no locale files are present.
   /// </summary>
   public async Task<Response<List<SingleTranslation>>> GetAllDictionariesAsync()
   {
      string[] languages = TranslationsPresented();

      if(languages.Length == 0)
      {
         _logger.LogWarning("GetAllDictionariesAsync: no locale files found in {LocalesPath}", LocalesPath);
         return Response<List<SingleTranslation>>.Fail(_t["No files in the folder"].Value);
      }

      List<SingleTranslation> result = [];
      foreach(string language in languages)
      {
         Response<Dictionary<string, string>> response = await GetDictionaryAsync(language).ConfigureAwait(false);
         result.Add(new SingleTranslation
         {
            Language = language,
            Translations = response.Data ?? []
         });
      }

      _logger.LogDebug("GetAllDictionariesAsync: loaded {Count} locale files", result.Count);
      return Response<List<SingleTranslation>>.Ok(result);
   }

   /// <summary>
   /// Saves the full translation dictionary for a language.
   /// If a file already exists it is moved to a timestamped backup before being overwritten.
   /// </summary>
   /// <param name="data">Translation data to save.</param>
   public async Task<Response<bool>> SaveDictionaryAsync(SingleTranslation data)
   {
      if(string.IsNullOrWhiteSpace(data.Language))
      {
         _logger.LogDebug("SaveDictionaryAsync: language code is null or whitespace");
         return Response<bool>.Fail(_t["Code can't be null"].Value);
      }

      if(data.Language.Length < 2)
      {
         _logger.LogDebug("SaveDictionaryAsync: language code '{Language}' is too short", data.Language);
         return Response<bool>.Fail(_t["Invalid code"].Value);
      }

      string path = Path.Combine(LocalesPath, data.Language + ".json");
      string json = JsonSerializer.Serialize(data.Translations ?? []);

      await _fileLock.WaitAsync().ConfigureAwait(false);
      try
      {
         if(File.Exists(path))
         {
            string backupDir = Path.Combine(Path.GetTempPath(), "dita");
            Directory.CreateDirectory(backupDir);
            string backupPath = Path.Combine(backupDir,
               $"{data.Language}_{DateTime.Now:yyyyMMddHHmmss}.bak");
            File.Move(path, backupPath, overwrite: true);
            _logger.LogDebug("Backup of '{Language}' saved to: {BackupPath}", data.Language, backupPath);
         }

         await File.WriteAllTextAsync(path, json).ConfigureAwait(false);
         _logger.LogInformation("Translation for '{Language}' saved: {Path}", data.Language, path);
         return Response<bool>.Ok(true, _t["Successfully stored"].Value);
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error saving translation for '{Language}'", data.Language);
         return Response<bool>.Fail($"Error storing data: {ex.Message}");
      }
      finally
      {
         _fileLock.Release();
      }
   }

   /// <summary>
   /// Saves a translation dictionary to the backup temp file before the default language is overwritten.
   /// </summary>
   /// <param name="data">Dictionary to back up.</param>
   public async Task<Response<bool>> SaveOldTranslationAsync(Dictionary<string, string> data)
   {
      try
      {
         string json = JsonSerializer.Serialize(data ?? []);
         await File.WriteAllTextAsync(OldTranslationPath, json).ConfigureAwait(false);
         _logger.LogDebug("Previous translation backup saved to: {OldTranslationPath}", OldTranslationPath);
         return Response<bool>.Ok(true);
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error saving translation backup to: {OldTranslationPath}", OldTranslationPath);
         return Response<bool>.Fail("Couldn't save an old translation");
      }
   }

   /// <summary>
   /// Saves multiple translation dictionaries in a single call.
   /// Returns a per-language success/failure map.
   /// </summary>
   /// <param name="tree">List of translations to save.</param>
   public async Task<Response<Dictionary<string, bool>>> SaveTranslationsAsync(List<SingleTranslation> tree)
   {
      Dictionary<string, bool> results = [];

      foreach(SingleTranslation item in tree)
      {
         Response<bool> result = await SaveDictionaryAsync(item).ConfigureAwait(false);
         results[item.Language] = result.Success;
      }

      _logger.LogDebug("SaveTranslationsAsync: saved {Count} language files", results.Count);
      return Response<Dictionary<string, bool>>.Ok(results);
   }

   /// <summary>
   /// Creates empty JSON locale files for every language code that does not already have a file.
   /// Returns <c>false</c> for codes whose file already exists.
   /// </summary>
   /// <param name="languages">Language codes to process.</param>
   public async Task<Dictionary<string, bool>> CreateMissingLanguageFilesAsync(List<string> languages)
   {
      Dictionary<string, bool> result = [];
      foreach(string language in languages)
         result[language] = await CreateEmptyLanguageFile(language).ConfigureAwait(false);
      return result;
   }

   /// <summary>
   /// Adds a new key/value entry to the specified language's locale file.
   /// The operation is atomic: the file is locked for the entire read-modify-write cycle.
   /// Returns a failure response (without modifying the file) if the key already exists.
   /// </summary>
   /// <param name="code">Language code (minimum 2 characters).</param>
   /// <param name="key">Translation key to add.</param>
   /// <param name="value">Translation value for the key.</param>
   public async Task<Response<bool>> AddTranslationEntryAsync(string code, string key, string value)
   {
      if(string.IsNullOrWhiteSpace(code) || code.Length < 2)
      {
         _logger.LogDebug("AddTranslationEntryAsync: invalid language code '{Code}'", code);
         return Response<bool>.Fail(_t["Invalid code"].Value);
      }

      if(string.IsNullOrWhiteSpace(key))
      {
         _logger.LogDebug("AddTranslationEntryAsync: key is null or whitespace");
         return Response<bool>.Fail(_t["Key cannot be null or empty"].Value);
      }

      string filePath = Path.Combine(LocalesPath, code.ToLowerInvariant() + ".json");
      if(!File.Exists(filePath))
      {
         _logger.LogDebug("AddTranslationEntryAsync: locale file for '{Code}' not found", code);
         return Response<bool>.Fail(_t["Dictionary file not found"].Value);
      }

      await _fileLock.WaitAsync().ConfigureAwait(false);
      try
      {
         Dictionary<string, string> dict = await ReadLocaleFileInternalAsync(filePath).ConfigureAwait(false) ?? [];

         if(dict.ContainsKey(key))
         {
            _logger.LogDebug(
               "AddTranslationEntryAsync: key '{Key}' already exists in '{Code}' – not overwriting",
               key, code);
            return Response<bool>.Fail(
               $"{_t["Key already exists"].Value}: '{key}'");
         }

         dict[key] = value;
         await WriteLocaleFileInternalAsync(filePath, dict).ConfigureAwait(false);
         _logger.LogInformation("Added entry '{Key}' to '{Code}'", key, code);
         return Response<bool>.Ok(true, _t["Successfully stored"].Value);
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error adding entry '{Key}' to '{Code}'", key, code);
         return Response<bool>.Fail(ex);
      }
      finally
      {
         _fileLock.Release();
      }
   }

   /// <summary>
   /// Removes the entry with the specified key from the language's locale file.
   /// The operation is atomic: the file is locked for the entire read-modify-write cycle.
   /// Returns a failure response if the key does not exist.
   /// </summary>
   /// <param name="code">Language code (minimum 2 characters).</param>
   /// <param name="key">Translation key to remove.</param>
   public async Task<Response<bool>> RemoveTranslationEntryAsync(string code, string key)
   {
      if(string.IsNullOrWhiteSpace(code) || code.Length < 2)
      {
         _logger.LogDebug("RemoveTranslationEntryAsync: invalid language code '{Code}'", code);
         return Response<bool>.Fail(_t["Invalid code"].Value);
      }

      if(string.IsNullOrWhiteSpace(key))
      {
         _logger.LogDebug("RemoveTranslationEntryAsync: key is null or whitespace");
         return Response<bool>.Fail(_t["Key cannot be null or empty"].Value);
      }

      string filePath = Path.Combine(LocalesPath, code.ToLowerInvariant() + ".json");
      if(!File.Exists(filePath))
      {
         _logger.LogDebug("RemoveTranslationEntryAsync: locale file for '{Code}' not found", code);
         return Response<bool>.Fail(_t["Dictionary file not found"].Value);
      }

      await _fileLock.WaitAsync().ConfigureAwait(false);
      try
      {
         Dictionary<string, string> dict = await ReadLocaleFileInternalAsync(filePath).ConfigureAwait(false) ?? [];

         if(!dict.Remove(key))
         {
            _logger.LogDebug("RemoveTranslationEntryAsync: key '{Key}' not found in '{Code}'", key, code);
            return Response<bool>.Fail($"{_t["Key not found"].Value}: '{key}'");
         }

         await WriteLocaleFileInternalAsync(filePath, dict).ConfigureAwait(false);
         _logger.LogInformation("Removed entry '{Key}' from '{Code}'", key, code);
         return Response<bool>.Ok(true, _t["Successfully stored"].Value);
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error removing entry '{Key}' from '{Code}'", key, code);
         return Response<bool>.Fail(ex);
      }
      finally
      {
         _fileLock.Release();
      }
   }

   /// <summary>
   /// Creates or overwrites the entry with the specified key in the language's locale file (upsert).
   /// The operation is atomic: the file is locked for the entire read-modify-write cycle.
   /// Unlike <see cref="AddTranslationEntryAsync"/>, this method always writes the value even if
   /// the key already exists.
   /// </summary>
   /// <param name="code">Language code (minimum 2 characters).</param>
   /// <param name="key">Translation key to create or overwrite.</param>
   /// <param name="value">New translation value.</param>
   public async Task<Response<bool>> UpdateTranslationEntryAsync(string code, string key, string value)
   {
      if(string.IsNullOrWhiteSpace(code) || code.Length < 2)
      {
         _logger.LogDebug("UpdateTranslationEntryAsync: invalid language code '{Code}'", code);
         return Response<bool>.Fail(_t["Invalid code"].Value);
      }

      if(string.IsNullOrWhiteSpace(key))
      {
         _logger.LogDebug("UpdateTranslationEntryAsync: key is null or whitespace");
         return Response<bool>.Fail(_t["Key cannot be null or empty"].Value);
      }

      string filePath = Path.Combine(LocalesPath, code.ToLowerInvariant() + ".json");
      if(!File.Exists(filePath))
      {
         _logger.LogDebug("UpdateTranslationEntryAsync: locale file for '{Code}' not found", code);
         return Response<bool>.Fail(_t["Dictionary file not found"].Value);
      }

      await _fileLock.WaitAsync().ConfigureAwait(false);
      try
      {
         Dictionary<string, string> dict = await ReadLocaleFileInternalAsync(filePath).ConfigureAwait(false) ?? [];
         bool isUpdate = dict.ContainsKey(key);
         dict[key] = value;
         await WriteLocaleFileInternalAsync(filePath, dict).ConfigureAwait(false);

         _logger.LogInformation(
            isUpdate
               ? "Updated entry '{Key}' in '{Code}'"
               : "Created entry '{Key}' in '{Code}'",
            key, code);

         return Response<bool>.Ok(true, _t["Successfully stored"].Value);
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error updating entry '{Key}' in '{Code}'", key, code);
         return Response<bool>.Fail(ex);
      }
      finally
      {
         _fileLock.Release();
      }
   }

   // ── Private helpers ──────────────────────────────────────────────────────

   // Creates an empty locale JSON file if it does not already exist.
   private async Task<bool> CreateEmptyLanguageFile(string code)
   {
      if(string.IsNullOrWhiteSpace(code) || code.Length < 2)
      {
         _logger.LogDebug("CreateEmptyLanguageFile: invalid code '{Code}'", code);
         return false;
      }

      string path = Path.Combine(LocalesPath, code + ".json");
      if(File.Exists(path))
      {
         _logger.LogDebug("Locale file for '{Code}' already exists: {Path}", code, path);
         return false;
      }

      await _fileLock.WaitAsync().ConfigureAwait(false);
      try
      {
         // Re-check after acquiring lock (another thread may have created the file).
         if(File.Exists(path)) return false;
         await File.WriteAllTextAsync(path, "{}").ConfigureAwait(false);
         _logger.LogInformation("Created empty locale file for '{Code}': {Path}", code, path);
         return true;
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Error creating locale file for '{Code}'", code);
         return false;
      }
      finally
      {
         _fileLock.Release();
      }
   }

   // Returns the language codes for which locale files exist in the Locales directory.
   private string[] TranslationsPresented()
   {
      if(!Directory.Exists(LocalesPath))
      {
         _logger.LogWarning("TranslationsPresented: directory {LocalesPath} does not exist", LocalesPath);
         return [];
      }

      return [.. Directory.GetFiles(LocalesPath, "*.json")
         .Select(f => Path.GetFileNameWithoutExtension(f).ToLowerInvariant())];
   }

   // Deserialises a locale JSON file. Must be called while holding _fileLock.
   private static async Task<Dictionary<string, string>?> ReadLocaleFileInternalAsync(string filePath)
   {
      string content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
      return JsonSerializer.Deserialize<Dictionary<string, string>>(content);
   }

   // Serialises a dictionary and writes it to a locale JSON file. Must be called while holding _fileLock.
   private static async Task WriteLocaleFileInternalAsync(string filePath, Dictionary<string, string> dict)
   {
      string json = JsonSerializer.Serialize(dict);
      await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
   }
}