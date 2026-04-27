using Afrowave.SharedTools.Models.Localization;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Hubs;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.ScheduledTranslationService;
using Dita.Shared.Localization.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dita.Tests.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Tests for <see cref="BackendTranslationService"/> pipeline behavior.
/// </summary>
public class BackendTranslationServiceTests
{
   [Fact]
   public async Task WhenJsonStageHasNoChangesThenBackupSnapshotIsNotSaved()
   {
      string rootPath = CreateTempRoot();
      try
      {
         SeedCountries(rootPath, "[]");

         ILanguageService languageService = Substitute.For<ILanguageService>();
         ITranslationQueue translationQueue = new TranslationQueue();
         ILibreTranslateService translateService = Substitute.For<ILibreTranslateService>();

         ConfigureDefaultLanguageServiceResponses(languageService);

         translateService.ServerLatency().Returns(Response<int>.Ok(10));
         translateService.GetAvailableLanguagesAsync().Returns(Task.FromResult(Response<string[]>.Ok(["en", "cs"])));

         BackendTranslationService service = CreateService(
            rootPath,
            languageService,
            translationQueue,
            translateService);

         await service.RunAsync();

         await languageService.DidNotReceive().SaveOldTranslationAsync(Arg.Any<Dictionary<string, string>>());
      }
      finally
      {
         Directory.Delete(rootPath, true);
      }
   }

   [Fact]
   public async Task WhenRunAsyncIsCalledConcurrentlyThenSecondRunIsSkipped()
   {
      string rootPath = CreateTempRoot();
      try
      {
         SeedCountries(rootPath, "[]");

         ILanguageService languageService = Substitute.For<ILanguageService>();
         ITranslationQueue translationQueue = new TranslationQueue();
         ILibreTranslateService translateService = Substitute.For<ILibreTranslateService>();

         ConfigureDefaultLanguageServiceResponses(languageService);
         translateService.ServerLatency().Returns(Response<int>.Ok(10));

         TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
         int languageCalls = 0;

         translateService.GetAvailableLanguagesAsync().Returns(_ =>
         {
            int current = Interlocked.Increment(ref languageCalls);
            if(current == 1)
            {
               return WaitAndReturnLanguagesAsync(gate.Task);
            }

            return Task.FromResult(Response<string[]>.Ok(["en", "cs"]));
         });

         BackendTranslationService service = CreateService(
            rootPath,
            languageService,
            translationQueue,
            translateService);

         Task firstRun = service.RunAsync();
         await Task.Delay(50);

         Task secondRun = service.RunAsync();
         await secondRun;

         gate.SetResult();
         await firstRun;

         Assert.Equal(1, languageCalls);
      }
      finally
      {
         Directory.Delete(rootPath, true);
      }
   }

   private static async Task<Response<string[]>> WaitAndReturnLanguagesAsync(Task gate)
   {
      await gate;
      return Response<string[]>.Ok(["en", "cs"]);
   }

   private static void ConfigureDefaultLanguageServiceResponses(ILanguageService languageService)
   {
      languageService.Languages.Returns(new List<Language>());
      languageService.CreateMissingLanguageFilesAsync(Arg.Any<List<string>>())
         .Returns(Task.FromResult(new Dictionary<string, bool> { ["en"] = false, ["cs"] = false }));

      Dictionary<string, string> defaultDictionary = new(StringComparer.Ordinal)
      {
         ["Home page"] = "Home page"
      };

      languageService.GetDictionaryAsync("en")
         .Returns(Task.FromResult(Response<Dictionary<string, string>>.Ok(new Dictionary<string, string>(defaultDictionary, StringComparer.Ordinal))));
      languageService.GetDictionaryAsync("cs")
         .Returns(Task.FromResult(Response<Dictionary<string, string>>.Ok(new Dictionary<string, string>(StringComparer.Ordinal))));

      languageService.GetLastStored()
         .Returns(Task.FromResult(Response<Dictionary<string, string>>.Ok(new Dictionary<string, string>(defaultDictionary, StringComparer.Ordinal))));

      languageService.SaveDictionaryAsync(Arg.Any<SingleTranslation>())
         .Returns(Task.FromResult(Response<bool>.Ok(true)));
      languageService.SaveOldTranslationAsync(Arg.Any<Dictionary<string, string>>())
         .Returns(Task.FromResult(Response<bool>.Ok(true)));
   }

   private static BackendTranslationService CreateService(
      string rootPath,
      ILanguageService languageService,
      ITranslationQueue translationQueue,
      ILibreTranslateService translateService)
   {
      ILocalizationHubClient client = Substitute.For<ILocalizationHubClient>();
      IHubClients<ILocalizationHubClient> clients = Substitute.For<IHubClients<ILocalizationHubClient>>();
      clients.All.Returns(client);

      IHubContext<LocalizationHub, ILocalizationHubClient> hubContext = Substitute.For<IHubContext<LocalizationHub, ILocalizationHubClient>>();
      hubContext.Clients.Returns(clients);

      IConfiguration configuration = new ConfigurationBuilder()
         .AddInMemoryCollection(new Dictionary<string, string?>
         {
            ["AutomaticTranslationSettings:AppsettingsLoaded"] = "true",
            ["AutomaticTranslationSettings:DefaultLanguage"] = "en",
            ["AutomaticTranslationSettings:AutomaticRun"] = "true",
            ["AutomaticTranslationSettings:CheckingPeriod"] = "30",
            ["AutomaticTranslationSettings:WaitingTime"] = "00:00:00"
         })
         .Build();

      IHostEnvironment hostEnvironment = new TestHostEnvironment(rootPath);

      return new BackendTranslationService(
         languageService,
         translationQueue,
         hubContext,
         configuration,
         hostEnvironment,
         Substitute.For<IMarkdownTranslationService>(),
         Substitute.For<IMarkdownParserService>(),
         translateService,
         Substitute.For<ILogger<BackendTranslationService>>());
   }

   private static string CreateTempRoot()
   {
      string root = Path.Combine(Path.GetTempPath(), "dita-tests", Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(root);
      Directory.CreateDirectory(Path.Combine(root, "Jsons"));
      return root;
   }

   private static void SeedCountries(string rootPath, string json)
   {
      string countriesPath = Path.Combine(rootPath, "Jsons", "countries.json");
      File.WriteAllText(countriesPath, json);
   }

   /// <summary>
   /// Lightweight host environment for tests.
   /// </summary>
   private sealed class TestHostEnvironment(string rootPath) : IHostEnvironment
   {
      public string EnvironmentName { get; set; } = Environments.Development;
      public string ApplicationName { get; set; } = "Dita.Tests";
      public string ContentRootPath { get; set; } = rootPath;
      public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = default!;
   }
}
