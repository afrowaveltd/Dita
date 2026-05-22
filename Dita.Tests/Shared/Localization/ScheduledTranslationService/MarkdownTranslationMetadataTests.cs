using System.Security.Cryptography;
using System.Text;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Dita.Shared.Localization.ScheduledTranslationService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Dita.Tests.Shared.Localization.ScheduledTranslationService;

public class MarkdownTranslationMetadataTests
{
    [Fact]
    public void IsStale_WhenOnlyLineEndingsChange_ReturnsFalse()
    {
        MarkdownTranslationMetadata metadata = new()
        {
            SourceHash = MarkdownTranslationMetadata.ComputeSourceHash("# Title\n\nText\n")
        };

        Assert.False(metadata.IsStale("# Title\r\n\r\nText\r\n"));
        Assert.False(metadata.IsStale("# Title\r\rText\r"));
    }

    [Fact]
    public void IsStale_WhenLegacyCrlfHashMatchesCurrentContent_ReturnsFalse()
    {
        MarkdownTranslationMetadata metadata = new()
        {
            SourceHash = ComputeRawHash("# Title\r\n\r\nText\r\n")
        };

        Assert.False(metadata.IsStale("# Title\r\n\r\nText\r\n"));
        Assert.False(metadata.IsStale("# Title\n\nText\n"));
    }

    [Fact]
    public void IsFullyTranslated_WithExpectedBlockCount_RequiresMatchingCount()
    {
        MarkdownTranslationMetadata metadata = new()
        {
            LanguageBlockStatus = new Dictionary<string, List<bool>>
            {
                ["cs"] = [true, true],
                ["de"] = []
            }
        };

        Assert.True(metadata.IsFullyTranslated("cs", 2));
        Assert.False(metadata.IsFullyTranslated("cs", 3));
        Assert.True(metadata.IsFullyTranslated("de", 0));
    }

    [Fact]
    public async Task RunAsync_WhenAddingMissingLanguage_PreservesSkippedLanguageMetadata()
    {
        string rootPath = CreateTempRoot();
        try
        {
            string docsPath = Path.Combine(rootPath, "Docs");
            Directory.CreateDirectory(docsPath);

            string sourcePath = Path.Combine(docsPath, "en.md");
            await File.WriteAllTextAsync(sourcePath, "# Hello\n\nWorld\n", Encoding.UTF8);
            await File.WriteAllTextAsync(Path.Combine(docsPath, "cs.md"), "# Ahoj\n\nSvet\n", Encoding.UTF8);

            new MarkdownTranslationMetadata
            {
                SourceHash = MarkdownTranslationMetadata.ComputeSourceHash("# Hello\n\nWorld\n"),
                LanguageBlockStatus = new Dictionary<string, List<bool>>
                {
                    ["cs"] = [true, true]
                }
            }.Save(sourcePath);

            ILibreTranslateService translateService = Substitute.For<ILibreTranslateService>();
            translateService
                .TranslateTextAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(call =>
                {
                    string text = call.ArgAt<string>(0);
                    string target = call.ArgAt<string>(2);
                    return Task.FromResult(Response<TranslateResult>.Ok(new TranslateResult
                    {
                        TranslatedText = $"[{target}] {text}"
                    }));
                });

            DocumentsTranslationService service = CreateService(rootPath, translateService);
            StoringReport storingReport = new();

            await service.RunAsync(["cs", "de"], storingReport, Guid.NewGuid());

            MarkdownTranslationMetadata? metadata = MarkdownTranslationMetadata.Load(sourcePath);

            Assert.NotNull(metadata);
            Assert.True(metadata.IsFullyTranslated("cs", 2));
            Assert.True(metadata.IsFullyTranslated("de", 2));
            Assert.True(File.Exists(Path.Combine(docsPath, "de.md")));

            translateService.ClearReceivedCalls();

            await service.RunAsync(["cs", "de"], storingReport, Guid.NewGuid());

            await translateService.DidNotReceive()
                .TranslateTextAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }
        finally
        {
            Directory.Delete(rootPath, true);
        }
    }

    private static DocumentsTranslationService CreateService(string rootPath, ILibreTranslateService translateService)
    {
        IMarkdownParserService parser = Substitute.For<IMarkdownParserService>();
        parser
            .ExtractTranslatableBlocks(Arg.Any<string>())
            .Returns([
                new MarkdownTranslatableBlock
                {
                    Key = Guid.NewGuid(),
                    OriginalText = "Hello",
                    TranslatedText = "Hello",
                    StartLine = 0,
                    EndLine = 0,
                    BlockType = "Heading"
                },
                new MarkdownTranslatableBlock
                {
                    Key = Guid.NewGuid(),
                    OriginalText = "World",
                    TranslatedText = "World",
                    StartLine = 2,
                    EndLine = 2,
                    BlockType = "Paragraph"
                }
            ]);

        IMarkdownReconstructorService reconstructor = Substitute.For<IMarkdownReconstructorService>();
        reconstructor
            .Reconstruct(Arg.Any<string>(), Arg.Any<List<MarkdownTranslatableBlock>>())
            .Returns(call =>
            {
                string content = call.ArgAt<string>(0);
                List<MarkdownTranslatableBlock> blocks = call.ArgAt<List<MarkdownTranslatableBlock>>(1);

                foreach (MarkdownTranslatableBlock block in blocks)
                {
                    content = content.Replace(block.OriginalText, block.TranslatedText, StringComparison.Ordinal);
                }

                return content;
            });

        ISignalRPublisher publisher = Substitute.For<ISignalRPublisher>();
        publisher
            .PublishStageAsync(
                Arg.Any<Guid>(),
                Arg.Any<ProcessStage>(),
                Arg.Any<MarkdownTranslationsReport>(),
                Arg.Any<LocalizationMessageType>(),
                Arg.Any<string>(),
                Arg.Any<bool>())
            .Returns(Task.CompletedTask);
        publisher
            .PublishMessageAsync(
                Arg.Any<Guid>(),
                Arg.Any<LocalizationMessageType>(),
                Arg.Any<ProcessStage>(),
                Arg.Any<string>(),
                Arg.Any<object?>(),
                Arg.Any<bool>())
            .Returns(Task.CompletedTask);

        TranslationRetryService retryService = new(
            translateService,
            new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>()),
            Substitute.For<ILogger<TranslationRetryService>>(),
            stageMaxRetries: 0,
            stageRetryDelaySeconds: 0);

        ILanguageService languageService = Substitute.For<ILanguageService>();
        languageService.GetLanguageDisplayName(Arg.Any<string>()).Returns(call => call.ArgAt<string>(0));

        return new DocumentsTranslationService(
            reconstructor,
            parser,
            retryService,
            publisher,
            new TestHostEnvironment(rootPath),
            languageService,
            new AutomaticTranslationSettings
            {
                DefaultLanguage = "en",
                MarkdownRoots = ["/Docs"]
            },
            CreatePassThroughLocalizer<DocumentsTranslationService>(),
            Substitute.For<ILogger<DocumentsTranslationService>>());
    }

    private static string CreateTempRoot()
    {
        string root = Path.Combine(Path.GetTempPath(), "dita-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static IStringLocalizer<T> CreatePassThroughLocalizer<T>()
    {
        IStringLocalizer<T> localizer = Substitute.For<IStringLocalizer<T>>();

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

    private static string ComputeRawHash(string content)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }

    private sealed class TestHostEnvironment(string rootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Dita.Tests";
        public string ContentRootPath { get; set; } = rootPath;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = default!;
    }
}
