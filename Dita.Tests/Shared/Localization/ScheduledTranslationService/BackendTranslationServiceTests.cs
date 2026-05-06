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
    public async Task WhenJsonTranslationContainsPlaceholderArtifactsThenSavedDictionaryKeepsCanonicalPlaceholders()
    {
        string rootPath = CreateTempRoot();
        try
        {
            SeedCountries(rootPath, "[]");

            ILanguageService languageService = Substitute.For<ILanguageService>();
            ILibreTranslateService translateService = Substitute.For<ILibreTranslateService>();

            languageService.Languages.Returns(new List<Language>());
            languageService.CreateMissingLanguageFilesAsync(Arg.Any<List<string>>())
                .Returns(Task.FromResult(new Dictionary<string, bool> { ["en"] = false, ["cs"] = false }));
            languageService.GetDictionaryAsync("en")
                .Returns(Task.FromResult(Response<Dictionary<string, string>>.Ok(new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Saved dictionary for '{language}' ({entryCount} entries)."] = "Saved dictionary for '{language}' ({entryCount} entries)."
                })));
            languageService.GetDictionaryAsync("cs")
                .Returns(Task.FromResult(Response<Dictionary<string, string>>.Ok(new Dictionary<string, string>(StringComparer.Ordinal))));
            languageService.GetLastStored()
                .Returns(Task.FromResult(Response<Dictionary<string, string>>.Ok(new Dictionary<string, string>(StringComparer.Ordinal))));
            languageService.SaveOldTranslationAsync(Arg.Any<Dictionary<string, string>>())
                .Returns(Task.FromResult(Response<bool>.Ok(true)));

            List<SingleTranslation> savedTranslations = [];
            languageService.SaveDictionaryAsync(Arg.Any<SingleTranslation>())
                .Returns(call =>
                {
                    savedTranslations.Add(call.ArgAt<SingleTranslation>(0));
                    return Task.FromResult(Response<bool>.Ok(true));
                });

            translateService.ServerLatency().Returns(Response<int>.Ok(10));
            translateService.GetAvailableLanguagesAsync().Returns(Task.FromResult(Response<string[]>.Ok(["en", "cs"])));
            translateService.TranslateTextAsync(Arg.Any<string>(), "en", "cs")
                .Returns(call =>
                {
                    string source = call.ArgAt<string>(0);
                    return Task.FromResult(Response<TranslateResult>.Ok(new TranslateResult
                    {
                        TranslatedText = source.Contains("\u27e60\u27e7", StringComparison.Ordinal)
                            ? "Uložený slovník pro 'CLAS0' (CLAS1 položek)."
                            : "Překlad"
                    }));
                });

            BackendTranslationService service = CreateService(
                rootPath,
                languageService,
                translateService);

            await service.RunAsync();

            SingleTranslation? savedCsTranslation = savedTranslations.LastOrDefault(translation =>
                translation.Language == "cs"
                && translation.Translations.ContainsKey("Saved dictionary for '{language}' ({entryCount} entries)."));

            Assert.NotNull(savedCsTranslation);
            Assert.Equal(
                "Uložený slovník pro '{language}' ({entryCount} položek).",
                savedCsTranslation!.Translations["Saved dictionary for '{language}' ({entryCount} entries)."]);
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

    [Fact]
    public async Task WhenAvailableLanguagesAreReportedThenRealtimeMessageUsesLocalizedLanguageNames()
    {
        string rootPath = CreateTempRoot();
        try
        {
            SeedCountries(rootPath, "[]");

            ILanguageService languageService = Substitute.For<ILanguageService>();
            languageService.GetLanguageDisplayName("en").Returns("English");
            languageService.GetLanguageDisplayName("cs").Returns("Czech");
            ConfigureDefaultLanguageServiceResponses(languageService);

            ILibreTranslateService translateService = Substitute.For<ILibreTranslateService>();
            translateService.ServerLatency().Returns(Response<int>.Ok(10));
            translateService.GetAvailableLanguagesAsync().Returns(Task.FromResult(Response<string[]>.Ok(["en", "cs"])));

            List<string> messages = [];
            BackendTranslationService service = CreateService(
                rootPath,
                languageService,
                translateService,
                onMessagePublished: message => messages.Add(message.Message));

            await service.RunAsync();

            Assert.Contains(messages, message =>
                message.Contains("English", StringComparison.Ordinal)
                && message.Contains("Czech", StringComparison.Ordinal));
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
        ILibreTranslateService translateService,
        Action<LocalizationHubMessage>? onMessagePublished = null)
    {
        ILocalizationHubClient client = Substitute.For<ILocalizationHubClient>();
        if(onMessagePublished is not null)
        {
            client.ReceiveLocalizationMessage(Arg.Any<LocalizationHubMessage>())
                .Returns(call =>
                {
                    onMessagePublished(call.ArgAt<LocalizationHubMessage>(0));
                    return Task.CompletedTask;
                });
        }

        IHubClients<ILocalizationHubClient> clients = Substitute.For<IHubClients<ILocalizationHubClient>>();
        clients.All.Returns(client);

        IHubContext<LocalizationHub, ILocalizationHubClient> hubContext = Substitute.For<IHubContext<LocalizationHub, ILocalizationHubClient>>();
        hubContext.Clients.Returns(clients);

        ISignalRPublisher signalRPublisher = new SignalRPublisher(hubContext);
        TranslationRetryService retryService = new TranslationRetryService(
            translateService,
            new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>()),
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
            languageService,
            settings,
            CreatePassThroughLocalizer<DocumentsTranslationService>(),
            Substitute.For<ILogger<DocumentsTranslationService>>());

        return new BackendTranslationService(
            configuration,
            languageService,
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
            object[] values = call.ArgAt<object[]>(1);
            string formatted = key;

            foreach(object? value in values)
            {
                int start = formatted.IndexOf('{', StringComparison.Ordinal);
                int end = start < 0 ? -1 : formatted.IndexOf('}', start + 1);
                if(start < 0 || end < 0)
                {
                    break;
                }

                formatted = string.Concat(
                    formatted.AsSpan(0, start),
                    value?.ToString() ?? string.Empty,
                    formatted.AsSpan(end + 1));
            }

            return new LocalizedString(key, formatted, resourceNotFound: false);
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
