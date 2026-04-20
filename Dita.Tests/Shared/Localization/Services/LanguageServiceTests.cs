using Afrowave.SharedTools.Models.Localization;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dita.Tests.Shared.Localization.Services;

/// <summary>
/// Tests for LanguageService. Because the service uses static paths derived from AppDomain.CurrentDomain.BaseDirectory,
/// the helpers in this class pre-create the required files and directories in the expected locations before each test.
/// </summary>
public class LanguageServiceTests : IDisposable
{
   // Paths that LanguageService resolves internally (same computation as the service)
   private static string BaseDir => AppDomain.CurrentDomain.BaseDirectory
      [..AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin")];

   private static string JsonFilePath => Path.Combine(BaseDir, "Jsons", "languages.json");
   private static string LocalesPath => Path.Combine(BaseDir, "Locales");
   private static string OldTranslationPath => Path.Combine(Path.GetTempPath(), "old.json");

   private static readonly List<Language> SampleLanguages =
   [
      new Language { Code = "en", Name = "English", Native = "English", Rtl = false },
      new Language { Code = "ar", Name = "Arabic", Native = "العربية", Rtl = true },
      new Language { Code = "cs", Name = "Czech", Native = "Čeština", Rtl = false }
   ];

   private readonly List<string> _createdFiles = [];
   private readonly List<string> _createdDirs = [];

   // ─── Helpers ──────────────────────────────────────────────────────────────

   private static LanguageService CreateService()
   {
      ILogger<LanguageService> logger = Substitute.For<ILogger<LanguageService>>();
      IStringLocalizer<LanguageService> localizer = Substitute.For<IStringLocalizer<LanguageService>>();
      localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
      return new LanguageService(logger, localizer);
   }

   private void EnsureLanguagesJson(List<Language>? languages = null)
   {
      string dir = Path.GetDirectoryName(JsonFilePath)!;
      if(!Directory.Exists(dir))
      {
         Directory.CreateDirectory(dir);
         _createdDirs.Add(dir);
      }
      string json = JsonSerializer.Serialize(languages ?? SampleLanguages);
      File.WriteAllText(JsonFilePath, json);
      _createdFiles.Add(JsonFilePath);
   }

   private void EnsureLocalesDir(Dictionary<string, Dictionary<string, string>>? files = null)
   {
      if(!Directory.Exists(LocalesPath))
      {
         Directory.CreateDirectory(LocalesPath);
         _createdDirs.Add(LocalesPath);
      }
      if(files != null)
      {
         foreach(var (code, dict) in files)
         {
            string path = Path.Combine(LocalesPath, code + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(dict));
            _createdFiles.Add(path);
         }
      }
   }

   public void Dispose()
   {
      foreach(string f in _createdFiles)
         if(File.Exists(f)) File.Delete(f);
      if(File.Exists(OldTranslationPath)) File.Delete(OldTranslationPath);
      // Remove dirs only if empty to avoid collisions with other tests
      foreach(string d in _createdDirs)
         if(Directory.Exists(d) && !Directory.EnumerateFileSystemEntries(d).Any())
            Directory.Delete(d);
   }

   // ─── Constructor ──────────────────────────────────────────────────────────

   [Fact]
   public void WhenLanguagesJsonExistsThenLanguagesAreLoaded()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      Assert.Equal(3, svc.Languages.Count);
   }

   [Fact]
   public void WhenLanguagesJsonIsMissingThenLanguagesIsEmpty()
   {
      // Ensure no file exists
      if(File.Exists(JsonFilePath)) File.Delete(JsonFilePath);
      var svc = CreateService();
      Assert.Empty(svc.Languages);
   }

   [Fact]
   public void WhenLanguagesJsonIsCorruptedThenLanguagesIsEmpty()
   {
      string dir = Path.GetDirectoryName(JsonFilePath)!;
      if(!Directory.Exists(dir)) { Directory.CreateDirectory(dir); _createdDirs.Add(dir); }
      File.WriteAllText(JsonFilePath, "not valid json {{{");
      _createdFiles.Add(JsonFilePath);
      var svc = CreateService();
      Assert.Empty(svc.Languages);
   }

   // ─── GetLanguageNames ─────────────────────────────────────────────────────

   [Fact]
   public void WhenLanguagesLoadedThenGetLanguageNamesReturnsSortedNames()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      var names = svc.GetLanguageNames();
      Assert.Equal(["Arabic", "Czech", "English"], names);
   }

   [Fact]
   public void WhenNoLanguagesLoadedThenGetLanguageNamesReturnsEmpty()
   {
      if(File.Exists(JsonFilePath)) File.Delete(JsonFilePath);
      var svc = CreateService();
      Assert.Empty(svc.GetLanguageNames());
   }

   // ─── IsRtl ────────────────────────────────────────────────────────────────

   [Fact]
   public void WhenCodeIsRtlLanguageThenIsRtlReturnsTrue()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      Assert.True(svc.IsRtl("ar"));
   }

   [Fact]
   public void WhenCodeIsLtrLanguageThenIsRtlReturnsFalse()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      Assert.False(svc.IsRtl("en"));
   }

   [Fact]
   public void WhenCodeIsNullOrWhitespaceThenIsRtlReturnsFalse()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      Assert.False(svc.IsRtl(""));
      Assert.False(svc.IsRtl("   "));
   }

   [Fact]
   public void WhenCodeIsCaseInsensitiveThenIsRtlMatchesCorrectly()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      Assert.True(svc.IsRtl("AR"));
      Assert.False(svc.IsRtl("EN"));
   }

   [Fact]
   public void WhenCodeIsUnknownThenIsRtlReturnsFalse()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      Assert.False(svc.IsRtl("xx"));
   }

   // ─── GetLanguageByCode ────────────────────────────────────────────────────

   [Fact]
   public void WhenCodeExistsThenGetLanguageByCodeReturnsSuccess()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      var result = svc.GetLanguageByCode("en");
      Assert.NotNull(result);
      Assert.True(result!.Success);
      Assert.Equal("en", result.Data!.Code);
   }

   [Fact]
   public void WhenCodeDoesNotExistThenGetLanguageByCodeReturnsFail()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      var result = svc.GetLanguageByCode("xx");
      Assert.NotNull(result);
      Assert.False(result!.Success);
   }

   [Fact]
   public void WhenCodeIsCaseInsensitiveThenGetLanguageByCodeFindsLanguage()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      var result = svc.GetLanguageByCode("EN");
      Assert.NotNull(result);
      Assert.True(result!.Success);
   }

   // ─── GetRequiredLanguagesAsync ────────────────────────────────────────────

   [Fact]
   public void WhenLocalesDirMissingThenGetRequiredLanguagesAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      if(Directory.Exists(LocalesPath))
         foreach(var f in Directory.GetFiles(LocalesPath)) File.Delete(f);
      // Don't create the locales dir so it may be missing
      string moved = LocalesPath + "_backup_" + Guid.NewGuid();
      bool renamed = false;
      if(Directory.Exists(LocalesPath))
      {
         Directory.Move(LocalesPath, moved);
         renamed = true;
      }

      try
      {
         var svc = CreateService();
         var result = svc.GetRequiredLanguagesAsync();
         Assert.False(result.Success);
      }
      finally
      {
         if(renamed) Directory.Move(moved, LocalesPath);
      }
   }

   [Fact]
   public void WhenLocalesDirHasMatchingFilesThenGetRequiredLanguagesAsyncReturnsLanguages()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new() { ["hello"] = "Hello" },
         ["ar"] = new() { ["hello"] = "مرحبا" }
      });

      var svc = CreateService();
      var result = svc.GetRequiredLanguagesAsync();
      Assert.True(result.Success);
      Assert.Equal(2, result.Data!.Count);
   }

   [Fact]
   public void WhenLocalesDirHasNonLanguageFilesThenTheyAreIgnored()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir();
      string extra = Path.Combine(LocalesPath, "settings.json");
      File.WriteAllText(extra, "{}");
      _createdFiles.Add(extra);

      var svc = CreateService();
      var result = svc.GetRequiredLanguagesAsync();
      Assert.True(result.Success);
      Assert.DoesNotContain(result.Data!, l => l.Code == "settings");
   }

   [Fact]
   public void WhenLocalesContainDialectFilesThenGetRequiredLanguagesAsyncMapsThemToBaseLanguage()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en-US"] = new() { ["hello"] = "Hello" },
         ["cs-CZ"] = new() { ["hello"] = "Ahoj" }
      });

      var svc = CreateService();
      var result = svc.GetRequiredLanguagesAsync();

      Assert.True(result.Success);
      Assert.NotNull(result.Data);
      Assert.Contains(result.Data!, language => language.Code == "en");
      Assert.Contains(result.Data!, language => language.Code == "cs");
   }

   // ─── GetSelectedLanguagesInfo ─────────────────────────────────────────────

   [Fact]
   public void WhenCodeExistsThenGetSelectedLanguagesInfoReturnsCorrectLanguage()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      var result = svc.GetSelectedLanguagesInfo(["en", "cs"]);
      Assert.True(result.Success);
      Assert.Equal(2, result.Data!.Count);
      Assert.Contains(result.Data, l => l.Code == "en");
      Assert.Contains(result.Data, l => l.Code == "cs");
   }

   [Fact]
   public void WhenCodeDoesNotExistThenGetSelectedLanguagesInfoReturnsSyntheticLanguage()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      var result = svc.GetSelectedLanguagesInfo(["xx"]);
      Assert.Single(result.Data!);
      Assert.Equal("xx", result.Data![0].Code);
      Assert.Equal("xx", result.Data[0].Name);
   }

   [Fact]
   public void WhenEmptyListThenGetSelectedLanguagesInfoReturnsEmptyData()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      var result = svc.GetSelectedLanguagesInfo([]);
      Assert.NotNull(result.Data);
      Assert.Empty(result.Data!);
   }

   // ─── GetDictionaryAsync ───────────────────────────────────────────────────

   [Fact]
   public async Task WhenValidCodeAndFilePresentThenGetDictionaryAsyncReturnsTranslations()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new() { ["hello"] = "Hello", ["bye"] = "Goodbye" }
      });

      var svc = CreateService();
      var result = await svc.GetDictionaryAsync("en");
      Assert.True(result.Success);
      Assert.Equal(2, result.Data!.Count);
      Assert.Equal("Hello", result.Data["hello"]);
   }

   [Fact]
   public async Task WhenCodeIsTooShortThenGetDictionaryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      var result = await svc.GetDictionaryAsync("x");
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenCodeIsNullThenGetDictionaryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      var svc = CreateService();
      var result = await svc.GetDictionaryAsync(null!);
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenFileNotFoundThenGetDictionaryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(); // No files
      var svc = CreateService();
      var result = await svc.GetDictionaryAsync("de");
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenFileIsEmptyObjectThenGetDictionaryAsyncReturnsWarning()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new()
      });

      var svc = CreateService();
      var result = await svc.GetDictionaryAsync("en");
      // Empty dict is returned as SuccessWithWarning
      Assert.True(result.Success);
   }

   // ─── GetLastStored ────────────────────────────────────────────────────────

   [Fact]
   public async Task WhenOldTranslationFileMissingThenGetLastStoredReturnsFail()
   {
      EnsureLanguagesJson();
      if(File.Exists(OldTranslationPath)) File.Delete(OldTranslationPath);
      var svc = CreateService();
      var result = await svc.GetLastStored();
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenOldTranslationFileExistsThenGetLastStoredReturnsData()
   {
      EnsureLanguagesJson();
      var dict = new Dictionary<string, string> { ["key"] = "value" };
      await File.WriteAllTextAsync(OldTranslationPath, JsonSerializer.Serialize(dict));

      var svc = CreateService();
      var result = await svc.GetLastStored();
      Assert.NotNull(result.Data);
      Assert.Equal("value", result.Data!["key"]);
   }

   // ─── SaveOldTranslationAsync ──────────────────────────────────────────────

   [Fact]
   public async Task WhenDataProvidedThenSaveOldTranslationAsyncWritesFile()
   {
      EnsureLanguagesJson();
      if(File.Exists(OldTranslationPath)) File.Delete(OldTranslationPath);

      var svc = CreateService();
      var data = new Dictionary<string, string> { ["a"] = "1" };
      var result = await svc.SaveOldTranslationAsync(data);
      Assert.True(result.Success);
      Assert.True(File.Exists(OldTranslationPath));

      var stored = JsonSerializer.Deserialize<Dictionary<string, string>>(
         await File.ReadAllTextAsync(OldTranslationPath));
      Assert.Equal("1", stored!["a"]);
   }

   [Fact]
   public async Task WhenNullDictionaryThenSaveOldTranslationAsyncWritesEmptyJson()
   {
      EnsureLanguagesJson();
      if(File.Exists(OldTranslationPath)) File.Delete(OldTranslationPath);

      var svc = CreateService();
      var result = await svc.SaveOldTranslationAsync(null!);
      Assert.True(result.Success);
   }

   // ─── CreateMissingLanguageFilesAsync ──────────────────────────────────────

   [Fact]
   public async Task WhenFileNotExistsThenCreateMissingLanguageFilesAsyncCreatesIt()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir();
      string enPath = Path.Combine(LocalesPath, "en.json");
      if(File.Exists(enPath)) File.Delete(enPath);
      _createdFiles.Add(enPath);

      var svc = CreateService();
      var result = await svc.CreateMissingLanguageFilesAsync(["en"]);
      Assert.True(result["en"]);
      Assert.True(File.Exists(enPath));
   }

   [Fact]
   public async Task WhenFileAlreadyExistsThenCreateMissingLanguageFilesAsyncReturnsFalse()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new() { ["k"] = "v" }
      });

      var svc = CreateService();
      var result = await svc.CreateMissingLanguageFilesAsync(["en"]);
      Assert.False(result["en"]);
   }

   [Fact]
   public async Task WhenInvalidCodeThenCreateMissingLanguageFilesAsyncReturnsFalse()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir();

      var svc = CreateService();
      var result = await svc.CreateMissingLanguageFilesAsync(["x"]);
      Assert.False(result["x"]);
   }

   // ─── GetAllDictionariesAsync ───────────────────────────────────────────────

   [Fact]
   public async Task WhenLocaleFilesExistThenGetAllDictionariesAsyncReturnsAllTranslations()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new() { ["hi"] = "Hello" },
         ["cs"] = new() { ["hi"] = "Ahoj" }
      });

      var svc = CreateService();
      var result = await svc.GetAllDictionariesAsync();
      Assert.True(result.Success);
      Assert.Equal(2, result.Data!.Count);
   }

   [Fact]
   public async Task WhenNoLocaleFilesExistThenGetAllDictionariesAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      string moved = LocalesPath + "_empty_" + Guid.NewGuid();
      bool renamed = false;
      if(Directory.Exists(LocalesPath))
      {
         Directory.Move(LocalesPath, moved);
         renamed = true;
      }
      Directory.CreateDirectory(LocalesPath);

      try
      {
         var svc = CreateService();
         var result = await svc.GetAllDictionariesAsync();
         // Prázdný adresář = žádné překlady → Fail
         Assert.False(result.Success);
      }
      finally
      {
         if(Directory.Exists(LocalesPath))
            Directory.Delete(LocalesPath);
         if(renamed) Directory.Move(moved, LocalesPath);
      }
   }

   // ─── SaveTranslationsAsync ─────────────────────────────────────────────────

   [Fact]
   public async Task WhenMultipleTranslationsProvidedThenSaveTranslationsAsyncReturnsResultPerLanguage()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir();
      string enPath = Path.Combine(LocalesPath, "en.json");
      string csPath = Path.Combine(LocalesPath, "cs.json");
      _createdFiles.Add(enPath);
      _createdFiles.Add(csPath);

      var svc = CreateService();
      var tree = new List<SingleTranslation>
      {
         new() { Language = "en", Translations = new() { ["hi"] = "Hello" } },
         new() { Language = "cs", Translations = new() { ["hi"] = "Ahoj" } }
      };
      var result = await svc.SaveTranslationsAsync(tree);
      Assert.NotNull(result.Data);
      Assert.True(result.Data!.ContainsKey("en"));
      Assert.True(result.Data.ContainsKey("cs"));
   }

   // ─── AddTranslationEntryAsync ─────────────────────────────────────────────

   [Fact]
   public async Task WhenKeyDoesNotExistThenAddTranslationEntryAsyncAddsItAndSucceeds()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new() { ["hello"] = "Hello" }
      });

      var svc = CreateService();
      var result = await svc.AddTranslationEntryAsync("en", "bye", "Goodbye");

      Assert.True(result.Success);

      // Verify the value was persisted to disk
      string content = await File.ReadAllTextAsync(Path.Combine(LocalesPath, "en.json"));
      var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content)!;
      Assert.Equal("Goodbye", dict["bye"]);
      Assert.Equal("Hello", dict["hello"]);
   }

   [Fact]
   public async Task WhenKeyAlreadyExistsThenAddTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new() { ["hello"] = "Hello" }
      });

      var svc = CreateService();
      var result = await svc.AddTranslationEntryAsync("en", "hello", "Hi");

      Assert.False(result.Success);

      // Original value must be unchanged
      string content = await File.ReadAllTextAsync(Path.Combine(LocalesPath, "en.json"));
      var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content)!;
      Assert.Equal("Hello", dict["hello"]);
   }

   [Fact]
   public async Task WhenInvalidCodeThenAddTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir();
      var svc = CreateService();
      var result = await svc.AddTranslationEntryAsync("x", "key", "value");
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenNullKeyThenAddTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>> { ["en"] = new() });
      var svc = CreateService();
      var result = await svc.AddTranslationEntryAsync("en", null!, "value");
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenFileNotFoundThenAddTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(); // No files
      var svc = CreateService();
      var result = await svc.AddTranslationEntryAsync("de", "key", "value");
      Assert.False(result.Success);
   }

   // ─── RemoveTranslationEntryAsync ──────────────────────────────────────────

   [Fact]
   public async Task WhenKeyExistsThenRemoveTranslationEntryAsyncRemovesItAndSucceeds()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new() { ["hello"] = "Hello", ["bye"] = "Goodbye" }
      });

      var svc = CreateService();
      var result = await svc.RemoveTranslationEntryAsync("en", "hello");

      Assert.True(result.Success);

      string content = await File.ReadAllTextAsync(Path.Combine(LocalesPath, "en.json"));
      var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content)!;
      Assert.False(dict.ContainsKey("hello"));
      Assert.True(dict.ContainsKey("bye"));
   }

   [Fact]
   public async Task WhenKeyDoesNotExistThenRemoveTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new() { ["hello"] = "Hello" }
      });

      var svc = CreateService();
      var result = await svc.RemoveTranslationEntryAsync("en", "nonexistent");
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenInvalidCodeThenRemoveTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir();
      var svc = CreateService();
      var result = await svc.RemoveTranslationEntryAsync("x", "key");
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenNullKeyThenRemoveTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>> { ["en"] = new() });
      var svc = CreateService();
      var result = await svc.RemoveTranslationEntryAsync("en", null!);
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenFileNotFoundThenRemoveTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir();
      var svc = CreateService();
      var result = await svc.RemoveTranslationEntryAsync("de", "key");
      Assert.False(result.Success);
   }

   // ─── UpdateTranslationEntryAsync ──────────────────────────────────────────

   [Fact]
   public async Task WhenKeyExistsThenUpdateTranslationEntryAsyncOverwritesValue()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new() { ["hello"] = "Hello" }
      });

      var svc = CreateService();
      var result = await svc.UpdateTranslationEntryAsync("en", "hello", "Hi there");

      Assert.True(result.Success);

      string content = await File.ReadAllTextAsync(Path.Combine(LocalesPath, "en.json"));
      var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content)!;
      Assert.Equal("Hi there", dict["hello"]);
   }

   [Fact]
   public async Task WhenKeyDoesNotExistThenUpdateTranslationEntryAsyncCreatesIt()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new() { ["hello"] = "Hello" }
      });

      var svc = CreateService();
      var result = await svc.UpdateTranslationEntryAsync("en", "newkey", "NewValue");

      Assert.True(result.Success);

      string content = await File.ReadAllTextAsync(Path.Combine(LocalesPath, "en.json"));
      var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content)!;
      Assert.Equal("NewValue", dict["newkey"]);
      Assert.Equal("Hello", dict["hello"]);
   }

   [Fact]
   public async Task WhenInvalidCodeThenUpdateTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir();
      var svc = CreateService();
      var result = await svc.UpdateTranslationEntryAsync("x", "key", "value");
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenNullKeyThenUpdateTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>> { ["en"] = new() });
      var svc = CreateService();
      var result = await svc.UpdateTranslationEntryAsync("en", null!, "value");
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenFileNotFoundThenUpdateTranslationEntryAsyncReturnsFail()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir();
      var svc = CreateService();
      var result = await svc.UpdateTranslationEntryAsync("de", "key", "value");
      Assert.False(result.Success);
   }

   [Fact]
   public async Task WhenAddFollowedByUpdateThenUpdateOverwritesAddedValue()
   {
      EnsureLanguagesJson();
      EnsureLocalesDir(new Dictionary<string, Dictionary<string, string>>
      {
         ["en"] = new()
      });
      _createdFiles.Add(Path.Combine(LocalesPath, "en.json"));

      var svc = CreateService();

      // First add – must succeed
      var addResult = await svc.AddTranslationEntryAsync("en", "greeting", "Hello");
      Assert.True(addResult.Success);

      // Second add same key – must fail
      var addAgainResult = await svc.AddTranslationEntryAsync("en", "greeting", "Hi");
      Assert.False(addAgainResult.Success);

      // Update – must succeed and overwrite
      var updateResult = await svc.UpdateTranslationEntryAsync("en", "greeting", "Hey");
      Assert.True(updateResult.Success);

      string content = await File.ReadAllTextAsync(Path.Combine(LocalesPath, "en.json"));
      var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content)!;
      Assert.Equal("Hey", dict["greeting"]);
   }
}