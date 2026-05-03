using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.ScheduledTranslationService;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Logging;

namespace Dita.Tests.Shared.Localization.Services;

public sealed class LocalizationApiServiceTests
{
   [Fact]
   public async Task LocalizeAsync_WhenTargetDictionaryContainsPhrase_ReturnsDictionaryValueWithoutWriting()
   {
      ILanguageService languageService = Substitute.For<ILanguageService>();
      languageService.GetDictionaryAsync("cs")
         .Returns(Task.FromResult(Response<Dictionary<string, string>>.Ok(new Dictionary<string, string>
         {
            ["Hello"] = "Ahoj"
         })));

      IPlaceholderService placeholderService = CreatePlaceholderService();
      var service = new LocalizeService(
         languageService,
         placeholderService,
         new AutomaticTranslationSettings { DefaultLanguage = "en" },
         Substitute.For<ILogger<LocalizeService>>());

      Response<TextLocalizationResponse> response = await service.LocalizeAsync(new TextLocalizationRequest
      {
         Text = "Hello",
         TargetLanguage = "cs"
      });

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal("Ahoj", response.Data.LocalizedText);
      Assert.Equal(TextResolutionSource.TargetDictionary, response.Data.ResolvedFrom);
      await languageService.DidNotReceiveWithAnyArgs().AddTranslationEntryAsync(default!, default!, default!);
   }

   [Fact]
   public async Task LocalizeAsync_WhenPhraseIsMissing_AddsPhraseToDefaultDictionary()
   {
      ILanguageService languageService = Substitute.For<ILanguageService>();
      languageService.GetDictionaryAsync("cs")
         .Returns(Task.FromResult(Response<Dictionary<string, string>>.Fail("Dictionary file not found.")));
      languageService.GetDictionaryAsync("en")
         .Returns(Task.FromResult(Response<Dictionary<string, string>>.Ok(new Dictionary<string, string>())));
      languageService.CreateMissingLanguageFilesAsync(Arg.Any<List<string>>())
         .Returns(Task.FromResult(new Dictionary<string, bool> { ["en"] = false }));
      languageService.AddTranslationEntryAsync("en", "New phrase", "New phrase")
         .Returns(Task.FromResult(Response<bool>.Ok(true)));

      var service = new LocalizeService(
         languageService,
         CreatePlaceholderService(),
         new AutomaticTranslationSettings { DefaultLanguage = "en" },
         Substitute.For<ILogger<LocalizeService>>());

      Response<TextLocalizationResponse> response = await service.LocalizeAsync(new TextLocalizationRequest
      {
         Text = "New phrase",
         TargetLanguage = "cs"
      });

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.True(response.Data.AddedToDefaultDictionary);
      Assert.Equal("New phrase", response.Data.LocalizedText);
      Assert.Equal(TextResolutionSource.DefaultDictionaryCreated, response.Data.ResolvedFrom);
      await languageService.Received(1).AddTranslationEntryAsync("en", "New phrase", "New phrase");
   }

   [Fact]
   public async Task TranslateAsync_WhenTargetDictionaryContainsPhrase_ReturnsDictionaryValueWithoutServerCall()
   {
      ILanguageService languageService = Substitute.For<ILanguageService>();
      languageService.GetDictionaryAsync("cs")
         .Returns(Task.FromResult(Response<Dictionary<string, string>>.Ok(new Dictionary<string, string>
         {
            ["Hello"] = "Ahoj"
         })));

      ILibreTranslateService libreTranslateService = Substitute.For<ILibreTranslateService>();
      IPlaceholderService placeholderService = CreatePlaceholderService();
      TranslationRetryService retryService = CreateRetryService(libreTranslateService, placeholderService);

      var service = new TranslateService(
         languageService,
         libreTranslateService,
         retryService,
         placeholderService,
         new AutomaticTranslationSettings { DefaultLanguage = "en" },
         Substitute.For<ILogger<TranslateService>>());

      Response<TextTranslationResponse> response = await service.TranslateAsync(new TextTranslationRequest
      {
         Text = "Hello",
         SourceLanguage = "en",
         TargetLanguage = "cs"
      });

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal("Ahoj", response.Data.TranslatedText);
      Assert.False(response.Data.TranslationServerUsed);
      await libreTranslateService.DidNotReceiveWithAnyArgs().TranslateTextAsync(default!, default!, default!);
      await languageService.DidNotReceiveWithAnyArgs().AddTranslationEntryAsync(default!, default!, default!);
   }

   [Fact]
   public async Task TranslateAsync_WhenPhraseIsMissing_UsesServerWithoutWritingDictionary()
   {
      ILanguageService languageService = Substitute.For<ILanguageService>();
      languageService.GetDictionaryAsync("cs")
         .Returns(Task.FromResult(Response<Dictionary<string, string>>.Fail("Dictionary file not found.")));

      ILibreTranslateService libreTranslateService = Substitute.For<ILibreTranslateService>();
      libreTranslateService.TranslateTextAsync("Hello", "en", "cs")
         .Returns(Task.FromResult(Response<TranslateResult>.Ok(new TranslateResult { TranslatedText = "Ahoj" })));

      IPlaceholderService placeholderService = CreatePlaceholderService();
      TranslationRetryService retryService = CreateRetryService(libreTranslateService, placeholderService);

      var service = new TranslateService(
         languageService,
         libreTranslateService,
         retryService,
         placeholderService,
         new AutomaticTranslationSettings { DefaultLanguage = "en" },
         Substitute.For<ILogger<TranslateService>>());

      Response<TextTranslationResponse> response = await service.TranslateAsync(new TextTranslationRequest
      {
         Text = "Hello",
         SourceLanguage = "en",
         TargetLanguage = "cs"
      });

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal("Ahoj", response.Data.TranslatedText);
      Assert.True(response.Data.TranslationServerUsed);
      Assert.Equal(TextResolutionSource.TranslationServer, response.Data.ResolvedFrom);
      await languageService.DidNotReceiveWithAnyArgs().AddTranslationEntryAsync(default!, default!, default!);
   }

   private static IPlaceholderService CreatePlaceholderService()
   {
      IPlaceholderService placeholderService = Substitute.For<IPlaceholderService>();

      placeholderService.Format(Arg.Any<string>(), Arg.Any<Dictionary<string, string>?>())
         .Returns(call => call.ArgAt<string>(0));
      placeholderService.PrepareForTranslation(Arg.Any<string>())
         .Returns(call => (call.ArgAt<string>(0), (Func<string, string>)(translated => translated)));
      placeholderService.HasPlaceholders(Arg.Any<string>())
         .Returns(false);

      return placeholderService;
   }

   private static TranslationRetryService CreateRetryService(
      ILibreTranslateService libreTranslateService,
      IPlaceholderService placeholderService)
      => new(
         libreTranslateService,
         placeholderService,
         Substitute.For<ILogger<TranslationRetryService>>(),
         stageMaxRetries: 0,
         stageRetryDelaySeconds: 0);
}
