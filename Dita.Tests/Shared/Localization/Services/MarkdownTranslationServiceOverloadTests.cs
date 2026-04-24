using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dita.Tests.Shared.Localization.Services;

/// <summary>
/// Tests for the <see cref="MarkdownTranslationService"/> overload methods.
/// </summary>
public class MarkdownTranslationServiceOverloadTests
{
   [Fact]
   public async Task TranslateMarkdownAsync_WithoutParameters_UsesDefaultLanguageAndExcludesIgnored()
   {
      // Arrange
      AutomaticTranslationSettings settings = new()
      {
         DefaultLanguage = "en",
         IgnoredLanguages = ["fr", "de"]
      };

      ILibreTranslateService mockTranslateService = Substitute.For<ILibreTranslateService>();
      mockTranslateService
         .GetAvailableLanguagesAsync()
         .Returns(new Response<string[]>
         {
            Success = true,
            Data = ["en", "cs", "fr", "de", "es", "it"]
         });

      mockTranslateService
         .TranslateTextAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
         .Returns(callInfo =>
         {
            string text = callInfo.ArgAt<string>(0);
            string target = callInfo.ArgAt<string>(2);
            return Task.FromResult(new Response<TranslateResult>
            {
               Success = true,
               Data = new TranslateResult { TranslatedText = $"[{target}] {text}" }
            });
         });

      IMarkdownParserService mockParser = Substitute.For<IMarkdownParserService>();
      mockParser
         .ExtractTranslatableBlocks(Arg.Any<string>())
         .Returns(callInfo =>
         {
            string content = callInfo.ArgAt<string>(0);
            return new List<MarkdownTranslatableBlock>
            {
               new()
               {
                  StartLine = 0,
                  EndLine = 0,
                  BlockType = "Paragraph",
                  OriginalText = content,
                  Key = Guid.NewGuid()
               }
            };
         });

      IMarkdownReconstructorService mockReconstructor = Substitute.For<IMarkdownReconstructorService>();
      mockReconstructor
         .Reconstruct(Arg.Any<string>(), Arg.Any<List<MarkdownTranslatableBlock>>())
         .Returns(callInfo =>
         {
            List<MarkdownTranslatableBlock> blocks = callInfo.ArgAt<List<MarkdownTranslatableBlock>>(1);
            var translatedBlock = blocks.FirstOrDefault(b => b.IsTranslated);
            return translatedBlock?.TranslatedText ?? callInfo.ArgAt<string>(0);
         });

      MarkdownTranslationService service = new(
         mockParser,
         mockReconstructor,
         mockTranslateService,
         settings,
         NullLogger<MarkdownTranslationService>.Instance);

      string markdown = "Hello World";

      // Act
      Dictionary<string, string> result = await service.TranslateMarkdownAsync(markdown);

      // Assert
      Assert.NotEmpty(result);
      Assert.Equal(3, result.Count); // cs, es, it (excluded: en, fr, de)
      Assert.Contains("cs", result.Keys);
      Assert.Contains("es", result.Keys);
      Assert.Contains("it", result.Keys);
      Assert.DoesNotContain("en", result.Keys);
      Assert.DoesNotContain("fr", result.Keys);
      Assert.DoesNotContain("de", result.Keys);
   }

   [Fact]
   public async Task TranslateMarkdownAsync_WithoutParameters_WhenNoLanguagesAvailable_ReturnsEmpty()
   {
      // Arrange
      AutomaticTranslationSettings settings = new()
      {
         DefaultLanguage = "en",
         IgnoredLanguages = []
      };

      ILibreTranslateService mockTranslateService = Substitute.For<ILibreTranslateService>();
      mockTranslateService
         .GetAvailableLanguagesAsync()
         .Returns(new Response<string[]>
         {
            Success = false,
            Message = "Service unavailable"
         });

      IMarkdownParserService mockParser = Substitute.For<IMarkdownParserService>();
      IMarkdownReconstructorService mockReconstructor = Substitute.For<IMarkdownReconstructorService>();

      MarkdownTranslationService service = new(
         mockParser,
         mockReconstructor,
         mockTranslateService,
         settings,
         NullLogger<MarkdownTranslationService>.Instance);

      // Act
      Dictionary<string, string> result = await service.TranslateMarkdownAsync("Hello World");

      // Assert
      Assert.Empty(result);
   }

   [Fact]
   public async Task TranslateMarkdownAsync_WithoutParameters_WhenAllLanguagesIgnored_ReturnsEmpty()
   {
      // Arrange
      AutomaticTranslationSettings settings = new()
      {
         DefaultLanguage = "en",
         IgnoredLanguages = ["cs", "de", "fr"]
      };

      ILibreTranslateService mockTranslateService = Substitute.For<ILibreTranslateService>();
      mockTranslateService
         .GetAvailableLanguagesAsync()
         .Returns(new Response<string[]>
         {
            Success = true,
            Data = ["en", "cs", "de", "fr"] // All are either default or ignored
         });

      IMarkdownReconstructorService mockReconstructor = Substitute.For<IMarkdownReconstructorService>();
      IMarkdownParserService mockParser = Substitute.For<IMarkdownParserService>();

      MarkdownTranslationService service = new(
         mockParser,
         mockReconstructor,
         mockTranslateService,
         settings,
         NullLogger<MarkdownTranslationService>.Instance);

      // Act
      Dictionary<string, string> result = await service.TranslateMarkdownAsync("Hello World");

      // Assert
      Assert.Empty(result);
   }
}
