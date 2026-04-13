using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

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
      var localizer = new JsonStringLocalizer(cache, translationService, logger);

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
      var localizer = new JsonStringLocalizer(cache, translationService, logger);

      LocalizedString value = localizer["Hello"];

      Assert.Equal("Ahoj", value.Value);
      Assert.Equal("Ahoj", cache.GetString("locale_cs-CZ_Hello"));
   }

   [Fact]
   public void WhenValueMissingThenTranslationServiceProvidesFallback()
   {
      SetCulture("cs-CZ");
      WriteLocale("cs.json", "{}");
      WriteLocale("en.json", "{}");

      var cache = new InMemoryDistributedCache();
      var translationService = Substitute.For<ILibreTranslateService>();
      translationService.TranslateTextAsync("Missing", "en", "cs")
         .Returns(Task.FromResult(new Response<TranslateResult>
         {
            Success = true,
            Data = new TranslateResult { TranslatedText = "Prelozeno" }
         }));

      var logger = Substitute.For<ILogger<JsonStringLocalizer>>();
      var localizer = new JsonStringLocalizer(cache, translationService, logger);

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

      var localizer = new JsonStringLocalizer(new InMemoryDistributedCache(), Substitute.For<ILibreTranslateService>(), Substitute.For<ILogger<JsonStringLocalizer>>());

      var values = localizer.GetAllStrings(includeParentCultures: false).ToDictionary(item => item.Name, item => item.Value);

      Assert.Equal("Ahoj", values["Hello"]);
      Assert.Equal("Cau", values["Bye"]);
   }

   [Fact]
   public void WhenCultureFileMissingThenGetAllStringsFallsBackToEnglish()
   {
      SetCulture("de-DE");
      DeleteLocale("de.json");
      WriteLocale("en.json", "{" + "\"Hello\":\"Hello\"" + "}");

      var localizer = new JsonStringLocalizer(new InMemoryDistributedCache(), Substitute.For<ILibreTranslateService>(), Substitute.For<ILogger<JsonStringLocalizer>>());

      var values = localizer.GetAllStrings(includeParentCultures: false).ToList();

      Assert.Single(values);
      Assert.Equal("Hello", values[0].Value);
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