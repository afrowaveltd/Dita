using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Dita.Tests.Shared.Localization.Services;

[Collection("JsonStringLocalizerTests")]
public sealed class JsonStringLocalizerTests : IDisposable
{
   private readonly string _localesDirectory;
   private readonly Dictionary<string, string?> _backups = new(StringComparer.OrdinalIgnoreCase);
   private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
   private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

   public JsonStringLocalizerTests()
   {
      _localesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory[..AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin", StringComparison.Ordinal)], "Locales");
      Directory.CreateDirectory(_localesDirectory);

      BackupFile("en.json");
      BackupFile("cs.json");
      BackupFile("de.json");
   }

   [Fact]
   public void WhenValueExistsInCacheThenCachedValueIsReturned()
   {
      SetCulture("cs-CZ");
      WriteLocale("en.json", "{}");

      var cache = new InMemoryDistributedCache();
      cache.SetString("locale_cs-CZ_Hello", "Nazdar");
      var translationService = Substitute.For<ILibreTranslateService>();
      var logger = Substitute.For<ILogger<JsonStringLocalizer>>();
      var placeholderService = Substitute.For<IPlaceholderService>();
      var localizer = new JsonStringLocalizer(cache, translationService, new AutomaticTranslationSettings(), placeholderService, logger);

      LocalizedString value = localizer["Hello"];

      Assert.Equal("Nazdar", value.Value);
      Assert.False(value.ResourceNotFound);
      translationService.DidNotReceiveWithAnyArgs().TranslateTextAsync(default!, default!, default!);
   }

   [Fact]
   public void WhenValueExistsInJsonFileThenValueIsReturnedAndCached()
   {
      SetCulture("cs-CZ");
      WriteLocale("cs.json", "{" + "\"Hello\":\"Ahoj\"" + "}");
      WriteLocale("en.json", "{" + "\"Hello\":\"Hello\"" + "}");

      var cache = new InMemoryDistributedCache();
      var translationService = Substitute.For<ILibreTranslateService>();
      var logger = Substitute.For<ILogger<JsonStringLocalizer>>();
      var placeholderService = Substitute.For<IPlaceholderService>();
      var localizer = new JsonStringLocalizer(cache, translationService, new AutomaticTranslationSettings(), placeholderService, logger);

      LocalizedString value = localizer["Hello"];

      Assert.Equal("Ahoj", value.Value);
      Assert.Equal("Ahoj", cache.GetString("locale_cs-CZ_Hello"));
   }

   [Fact]
   public void WhenNamedPlaceholderArgumentProvidedThenItFormatsByPlaceholderOrder()
   {
      SetCulture("en");
      WriteLocale("en.json", "{" + "\"User is {age} years old\":\"User is {age} years old\"" + "}");

      var localizer = new JsonStringLocalizer(
         new InMemoryDistributedCache(),
         Substitute.For<ILibreTranslateService>(),
         new AutomaticTranslationSettings { DefaultLanguage = "en" },
         new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>()),
         Substitute.For<ILogger<JsonStringLocalizer>>());

      LocalizedString value = localizer["User is {age} years old", 42];

      Assert.Equal("User is 42 years old", value.Value);
   }

   [Fact]
   public void WhenAnonymousPlaceholderArgumentProvidedThenItFormatsByPropertyName()
   {
      SetCulture("en");
      WriteLocale("en.json", "{" + "\"User is {age} years old\":\"User is {age} years old\"" + "}");

      var localizer = new JsonStringLocalizer(
         new InMemoryDistributedCache(),
         Substitute.For<ILibreTranslateService>(),
         new AutomaticTranslationSettings { DefaultLanguage = "en" },
         new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>()),
         Substitute.For<ILogger<JsonStringLocalizer>>());

      LocalizedString value = localizer["User is {age} years old", new { age = 37 }];

      Assert.Equal("User is 37 years old", value.Value);
   }

   [Fact]
   public void WhenMissingPlaceholderKeyThenFallbackTranslationUsesReferenceValueAndRestoresPlaceholder()
   {
      SetCulture("cs-CZ");
      WriteLocale("cs.json", "{}");
      WriteLocale("en.json", "{}");

      var cache = new InMemoryDistributedCache();
      var translationService = Substitute.For<ILibreTranslateService>();
      translationService.TranslateTextAsync("User is 42 years old", "en", "cs-CZ")
         .Returns(Task.FromResult(new Response<TranslateResult>
         {
            Success = true,
            Data = new TranslateResult { TranslatedText = "Uživatel má 42 let" }
         }));

      var localizer = new JsonStringLocalizer(
         cache,
         translationService,
         new AutomaticTranslationSettings { DefaultLanguage = "en" },
         new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>()),
         Substitute.For<ILogger<JsonStringLocalizer>>());

      LocalizedString value = localizer["User is {age} years old", new { age = 42 }];

      Assert.Equal("Uživatel má 42 let", value.Value);
      Assert.Equal("Uživatel má {age} let", cache.GetString("locale_cs-CZ_User is {age} years old"));
   }

   [Fact]
   public void WhenValueMissingThenTranslationServiceProvidesFallback()
   {
      SetCulture("cs-CZ");
      WriteLocale("cs.json", "{}");
      WriteLocale("en.json", "{}");

      var cache = new InMemoryDistributedCache();
      var translationService = Substitute.For<ILibreTranslateService>();
      translationService.TranslateTextAsync("Missing", "en", "cs-CZ")
         .Returns(Task.FromResult(new Response<TranslateResult>
         {
            Success = true,
            Data = new TranslateResult { TranslatedText = "Prelozeno" }
         }));

      var logger = Substitute.For<ILogger<JsonStringLocalizer>>();
      var placeholderService = Substitute.For<IPlaceholderService>();
      var localizer = new JsonStringLocalizer(cache, translationService, new AutomaticTranslationSettings(), placeholderService, logger);

      LocalizedString value = localizer["Missing"];

      Assert.Equal("Prelozeno", value.Value);
      Assert.Equal("Prelozeno", cache.GetString("locale_cs-CZ_Missing"));
   }

   [Fact]
   public void WhenGetAllStringsCalledThenCultureSpecificFileIsUsed()
   {
      SetCulture("cs-CZ");
      WriteLocale("cs.json", "{" + "\"Hello\":\"Ahoj\",\"Bye\":\"Cau\"" + "}");
      WriteLocale("en.json", "{" + "\"Hello\":\"Hello\"" + "}");

      var localizer = new JsonStringLocalizer(
         new InMemoryDistributedCache(),
         Substitute.For<ILibreTranslateService>(),
         new AutomaticTranslationSettings(),
         Substitute.For<IPlaceholderService>(),
         Substitute.For<ILogger<JsonStringLocalizer>>());

      var values = localizer.GetAllStrings(includeParentCultures: false).ToDictionary(item => item.Name, item => item.Value);

      Assert.Equal("Ahoj", values["Hello"]);
      Assert.Equal("Cau", values["Bye"]);
   }

   [Fact]
   public void WhenKeyMissingThenDefaultDictionaryEntryIsCreatedAsKeyValue()
   {
      SetCulture("cs-CZ");
      WriteLocale("cs.json", "{}");
      WriteLocale("en.json", "{}");

      var cache = new InMemoryDistributedCache();
      var translationService = Substitute.For<ILibreTranslateService>();
      translationService.TranslateTextAsync("How are you", "en", "cs-CZ")
         .Returns(Task.FromResult(new Response<TranslateResult>
         {
            Success = true,
            Data = new TranslateResult { TranslatedText = "Jak se máš" }
         }));

      var settings = new AutomaticTranslationSettings { DefaultLanguage = "en" };
      var placeholderService = Substitute.For<IPlaceholderService>();
      var localizer = new JsonStringLocalizer(cache, translationService, settings, placeholderService, Substitute.For<ILogger<JsonStringLocalizer>>());

      _ = localizer["How are you"];

      string filePath = Path.Combine(_localesDirectory, "en.json");
      string content = File.ReadAllText(filePath);
      Dictionary<string, string> dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content)!;

      Assert.Equal("How are you", dict["How are you"]);
   }

   [Fact]
   public void WhenKeyMissingThenEntryIsAddedToConfiguredDefaultLanguage()
   {
      SetCulture("de-DE");
      WriteLocale("de.json", "{}");
      WriteLocale("cs.json", "{}");
      DeleteLocale("en.json");

      var cache = new InMemoryDistributedCache();
      var translationService = Substitute.For<ILibreTranslateService>();
      translationService.TranslateTextAsync("Dobrý den", "cs", "de-DE")
         .Returns(Task.FromResult(new Response<TranslateResult>
         {
            Success = true,
            Data = new TranslateResult { TranslatedText = "Guten Tag" }
         }));

      var settings = new AutomaticTranslationSettings { DefaultLanguage = "cs" };
      var placeholderService = Substitute.For<IPlaceholderService>();
      var localizer = new JsonStringLocalizer(cache, translationService, settings, placeholderService, Substitute.For<ILogger<JsonStringLocalizer>>());

      _ = localizer["Dobrý den"];

      string filePath = Path.Combine(_localesDirectory, "cs.json");
      string content = File.ReadAllText(filePath);
      Dictionary<string, string> dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content)!;

      Assert.Equal("Dobrý den", dict["Dobrý den"]);
   }

   [Fact]
   public void WhenLocalesInfrastructureMissingThenLocalizerCreatesDirectoryAndDefaultFile()
   {
      SetCulture("de-DE");

      string csPath = Path.Combine(_localesDirectory, "cs.json");
      string enPath = Path.Combine(_localesDirectory, "en.json");

      if(File.Exists(csPath))
      {
         File.Delete(csPath);
      }

      if(File.Exists(enPath))
      {
         File.Delete(enPath);
      }

       if(Directory.Exists(_localesDirectory))
       {
          Directory.Delete(_localesDirectory, recursive: true);
       }

      var localizer = new JsonStringLocalizer(
         new InMemoryDistributedCache(),
         Substitute.For<ILibreTranslateService>(),
         new AutomaticTranslationSettings { DefaultLanguage = "cs" },
         Substitute.For<IPlaceholderService>(),
         Substitute.For<ILogger<JsonStringLocalizer>>());

      _ = localizer.GetAllStrings(includeParentCultures: false).ToList();

      Assert.True(Directory.Exists(_localesDirectory));
      Assert.True(File.Exists(csPath));
      Assert.Equal("{}", File.ReadAllText(csPath));
   }

   [Fact]
   public void WhenCultureFileMissingThenGetAllStringsFallsBackToDefaultLanguage()
   {
      SetCulture("de-DE");
      DeleteLocale("de.json");
      WriteLocale("cs.json", "{" + "\"Hello\":\"Ahoj\"" + "}");
      DeleteLocale("en.json");

      var localizer = new JsonStringLocalizer(
         new InMemoryDistributedCache(),
         Substitute.For<ILibreTranslateService>(),
         new AutomaticTranslationSettings { DefaultLanguage = "cs" },
         Substitute.For<IPlaceholderService>(),
         Substitute.For<ILogger<JsonStringLocalizer>>());

      var values = localizer.GetAllStrings(includeParentCultures: false).ToList();

      Assert.Single(values);
      Assert.Equal("Ahoj", values[0].Value);
   }

   [Fact]
    public void WhenDialectFileExistsThenGetAllStringsPrefersDialectOverLanguage()
    {
       SetCulture("en-US");
       WriteLocale("en-US.json", "{" + "\"Hello\":\"Howdy\"" + "}");
       WriteLocale("en.json", "{" + "\"Hello\":\"Hello\"" + "}");

       var localizer = new JsonStringLocalizer(
          new InMemoryDistributedCache(),
          Substitute.For<ILibreTranslateService>(),
          new AutomaticTranslationSettings { DefaultLanguage = "en" },
          Substitute.For<IPlaceholderService>(),
          Substitute.For<ILogger<JsonStringLocalizer>>());

       var values = localizer.GetAllStrings(includeParentCultures: false).ToDictionary(item => item.Name, item => item.Value);

      Assert.Equal("Howdy", values["Hello"]);
   }

   [Fact]
    public void WhenDialectFileMissingThenGetAllStringsFallsBackToLanguageFile()
    {
       SetCulture("zh-CN");
       DeleteLocale("zh-CN.json");
       WriteLocale("zh.json", "{" + "\"Hello\":\"你好\"" + "}");

       var localizer = new JsonStringLocalizer(
          new InMemoryDistributedCache(),
          Substitute.For<ILibreTranslateService>(),
          new AutomaticTranslationSettings { DefaultLanguage = "en" },
          Substitute.For<IPlaceholderService>(),
          Substitute.For<ILogger<JsonStringLocalizer>>());

       var values = localizer.GetAllStrings(includeParentCultures: false).ToDictionary(item => item.Name, item => item.Value);

      Assert.Equal("你好", values["Hello"]);
   }

   public void Dispose()
   {
      foreach(var backup in _backups)
      {
         string path = Path.Combine(_localesDirectory, backup.Key);
         if(backup.Value is null)
         {
            if(File.Exists(path))
            {
               File.Delete(path);
            }
         }
         else
         {
            File.WriteAllText(path, backup.Value, Encoding.UTF8);
         }
      }

      CultureInfo.CurrentCulture = _originalCulture;
      CultureInfo.CurrentUICulture = _originalUiCulture;
      GC.SuppressFinalize(this);
   }

   private void BackupFile(string fileName)
   {
      string path = Path.Combine(_localesDirectory, fileName);
      _backups[fileName] = File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
   }

   private void WriteLocale(string fileName, string content)
   {
      File.WriteAllText(Path.Combine(_localesDirectory, fileName), content, Encoding.UTF8);
   }

   private void DeleteLocale(string fileName)
   {
      string path = Path.Combine(_localesDirectory, fileName);
      if(File.Exists(path))
      {
         File.Delete(path);
      }
   }

   private static void SetCulture(string cultureName)
   {
      CultureInfo culture = new(cultureName);
      CultureInfo.CurrentCulture = culture;
      CultureInfo.CurrentUICulture = culture;
   }

   private sealed class InMemoryDistributedCache : IDistributedCache
   {
      private readonly Dictionary<string, byte[]> _storage = new(StringComparer.Ordinal);

      public byte[]? Get(string key)
      {
         return _storage.TryGetValue(key, out byte[]? value) ? value : null;
      }

      public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
      {
         return Task.FromResult(Get(key));
      }

      public void Refresh(string key)
      {
      }

      public Task RefreshAsync(string key, CancellationToken token = default)
      {
         return Task.CompletedTask;
      }

      public void Remove(string key)
      {
         _storage.Remove(key);
      }

      public Task RemoveAsync(string key, CancellationToken token = default)
      {
         Remove(key);
         return Task.CompletedTask;
      }

      public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
      {
         _storage[key] = value;
      }

      public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
      {
         Set(key, value, options);
         return Task.CompletedTask;
      }
   }
}

[CollectionDefinition("JsonStringLocalizerTests", DisableParallelization = true)]
public sealed class JsonStringLocalizerTestsCollection;
