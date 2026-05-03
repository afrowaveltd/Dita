using Afrowave.SharedTools.Models.Localization;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Hubs;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.ScheduledTranslationService;
using Dita.Shared.Localization.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
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
            ILibreTranslateService translateService = Substitute.For<ILibreTranslateService>();

            ConfigureDefaultLanguageServiceResponses(languageService);

            translateService.ServerLatency().Returns(Response<int>.Ok(10));
            translateService.GetAvailableLanguagesAsync().Returns(Task.FromResult(Response<string[]>.Ok(["en", "cs"])));

            BackendTranslationService service = CreateService(
                rootPath,
                languageService,
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
            ILibreTranslateService translateService = Substitute.For<ILibreTranslateService>();

            ConfigureDefaultLanguageServiceResponses(languageService);
            translateService.ServerLatency().Returns(Response<int>.Ok(10));

            TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
            int languageCalls = 0;

            translateService.GetAvailableLanguagesAsync().Returns(_ =>
            {
                int current = Interlocked.Increment(ref languageCalls);
                if (current == 1)
                {
                    return WaitAndReturnLanguagesAsync(gate.Task);
                }

                return Task.FromResult(Response<string[]>.Ok(["en", "cs"]));
            });

            BackendTranslationService service = CreateService(
                rootPath,
                languageService,
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

    [Fact]
    public async Task WhenServerLatencyFailsThenPipelineFails()
    {
        string rootPath = CreateTempRoot();
        try
        {
            SeedCountries(rootPath, "[]");

            ILanguageService languageService = Substitute.For<ILanguageService>();
            ILibreTranslateService translateService = Substitute.For<ILibreTranslateService>();

            ConfigureDefaultLanguageServiceResponses(languageService);
            translateService.ServerLatency().Returns(Response<int>.Fail("Server unreachable"));
            translateService.GetAvailableLanguagesAsync().Returns(Task.FromResult(Response<string[]>.Ok(["en", "cs"])));

            BackendTranslationService service = CreateService(
                rootPath,
                languageService,
                translateService);

            await service.RunAsync();

            // Pipeline should fail early and not attempt to save old translations
            await languageService.DidNotReceive().SaveOldTranslationAsync(Arg.Any<Dictionary<string, string>>());
        }
        finally
        {
            Directory.Delete(rootPath, true);
        }
    }

    [Fact]
    public async Task WhenAvailableLanguagesIsEmptyThenPipelineFails()
    {
        string rootPath = CreateTempRoot();
        try
        {
            SeedCountries(rootPath, "[]");

            ILanguageService languageService = Substitute.For<ILanguageService>();
            ILibreTranslateService translateService = Substitute.For<ILibreTranslateService>();

            ConfigureDefaultLanguageServiceResponses(languageService);
            translateService.ServerLatency().Returns(Response<int>.Ok(10));
            translateService.GetAvailableLanguagesAsync().Returns(Task.FromResult(Response<string[]>.Ok([])));

            BackendTranslationService service = CreateService(
                rootPath,
                languageService,
                translateService);

            await service.RunAsync();

            // Pipeline should fail because no languages are available
            await languageService.DidNotReceive().SaveOldTranslationAsync(Arg.Any<Dictionary<string, string>>());
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
        ILibreTranslateService translateService)
    {
        ILocalizationHubClient client = Substitute.For<ILocalizationHubClient>();
        IHubClients<ILocalizationHubClient> clients = Substitute.For<IHubClients<ILocalizationHubClient>>();
        clients.All.Returns(client);

        IHubContext<LocalizationHub, ILocalizationHubClient> hubContext = Substitute.For<IHubContext<LocalizationHub, ILocalizationHubClient>>();
        hubContext.Clients.Returns(clients);

        ISignalRPublisher signalRPublisher = new SignalRPublisher(hubContext);
        TranslationRetryService retryService = new TranslationRetryService(
            translateService,
            Substitute.For<IPlaceholderService>(),
            Substitute.For<ILogger<TranslationRetryService>>(),
            stageMaxRetries: 1,
            stageRetryDelaySeconds: 0);

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
        AutomaticTranslationSettings settings = new() { DefaultLanguage = "en", AppsettingsLoaded = true };

        ICountriesTranslationService countriesService = new CountriesTranslationService(
            languageService,
            translateService,
            signalRPublisher,
            retryService,
            hostEnvironment,
            CreatePassThroughLocalizer<CountriesTranslationService>(),
            Substitute.For<ILogger<CountriesTranslationService>>());

        ILocalizationTranslationService localizationService = new LocalizationTranslationService(
            languageService,
            retryService,
            signalRPublisher,
            CreatePassThroughLocalizer<LocalizationTranslationService>(),
            Substitute.For<ILogger<LocalizationTranslationService>>());

        IDocumentsTranslationService documentsService = new DocumentsTranslationService(
            Substitute.For<IMarkdownReconstructorService>(),
            Substitute.For<IMarkdownParserService>(),
            retryService,
            signalRPublisher,
            hostEnvironment,
            settings,
            CreatePassThroughLocalizer<DocumentsTranslationService>(),
            Substitute.For<ILogger<DocumentsTranslationService>>());

        return new BackendTranslationService(
            configuration,
            translateService,
            signalRPublisher,
            countriesService,
            localizationService,
            CreatePassThroughLocalizer<BackendTranslationService>(),
            documentsService,
            Substitute.For<ILogger<BackendTranslationService>>());
    }

    private static IStringLocalizer<T> CreatePassThroughLocalizer<T>()
    {
        var localizer = Substitute.For<IStringLocalizer<T>>();

        localizer[Arg.Any<string>()].Returns(call =>
        {
            string key = call.ArgAt<string>(0);
            return new LocalizedString(key, key, resourceNotFound: false);
        });

        localizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(call =>
        {
            string key = call.ArgAt<string>(0);
            return new LocalizedString(key, key, resourceNotFound: false);
        });

        return localizer;
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
