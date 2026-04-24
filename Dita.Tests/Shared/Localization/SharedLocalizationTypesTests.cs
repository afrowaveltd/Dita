using Dita.Shared.Identity.Enums;
using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Hubs;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;

namespace Dita.Tests.Shared.Localization;

public class SharedLocalizationTypesTests
{
   [Fact]
   public void WhenComparisonValuesReadThenExpectedOrderIsPreserved()
   {
      var values = Enum.GetValues<Comparison>();

      Assert.Equal(
      [
         Comparison.Equal,
         Comparison.Greater,
         Comparison.GreaterOrEqual,
         Comparison.Less,
         Comparison.LessOrEqual,
         Comparison.Between,
         Comparison.Any
      ], values);
   }

   [Fact]
   public void WhenGenderValuesReadThenExpectedValuesAreDefined()
   {
      var values = Enum.GetValues<Gender>();

      Assert.Equal([Gender.Male, Gender.Female, Gender.Neutral, Gender.Other], values);
   }

   [Fact]
   public void WhenLoginResponseValuesReadThenExpectedValuesAreDefined()
   {
      var values = Enum.GetValues<LoginResponse>();

      Assert.Equal(
      [
         LoginResponse.Success,
         LoginResponse.InvalidCredentials,
         LoginResponse.UserNotFound,
         LoginResponse.LockedOut,
         LoginResponse.Banned,
         LoginResponse.TwoFactorRequired,
         LoginResponse.UnknownError
      ], values);
   }

   [Fact]
   public void WhenAutomaticTranslationSettingsCreatedThenDefaultValuesAreApplied()
   {
      var settings = new AutomaticTranslationSettings();

      Assert.Equal("http://localhost:5000", settings.Address);
      Assert.False(settings.NeedsKey);
      Assert.Equal(string.Empty, settings.Key);
      Assert.Equal("en", settings.DefaultLanguage);
      Assert.Empty(settings.IgnoredLanguages);
      Assert.Equal(["/Docs"], settings.MarkdownRoots);
      Assert.False(settings.AutomaticRun);
      Assert.Equal(TimeSpan.Zero, settings.WaitingTime);
      Assert.Equal(30, settings.CheckingPeriod);
      Assert.Equal("/translate", settings.TranslateEndpoint);
      Assert.Equal("/translate_file", settings.TranslateFileEndpoint);
      Assert.Equal("/languages", settings.LanguagesEndpoint);
      Assert.Equal("/detect", settings.DetectLanguageEndpoint);
   }

   [Fact]
   public void WhenComparisonConditionCreatedThenDefaultValuesAreApplied()
   {
      var condition = new ComparisonCondition();

      Assert.Equal(Comparison.Equal, condition.Compare);
      Assert.Empty(condition.Values);
      Assert.False(condition.IsOr);
   }

   [Fact]
   public void WhenDetectRequestSerializedThenJsonPropertyNamesMatchContract()
   {
      var request = new DetectRequest
      {
         ApiKey = "secret",
         Query = "Ahoj"
      };

      var json = JsonSerializer.Serialize(request);

      Assert.Contains("\"api_key\":\"secret\"", json);
      Assert.Contains("\"q\":\"Ahoj\"", json);
   }

   [Fact]
   public void WhenDetectionsCreatedThenDefaultValuesAreApplied()
   {
      var detections = new Detections();

      Assert.Equal(0, detections.Confidence);
      Assert.Equal(string.Empty, detections.Language);
   }

   [Fact]
   public void WhenErrorResponseCreatedThenMessageIsCopiedToError()
   {
      var response = new ErrorResponse("Translation failed");

      Assert.Equal("Translation failed", response.Error);
   }

   [Fact]
   public void WhenLibreLanguageCreatedThenDefaultValuesAreApplied()
   {
      var language = new LibreLanguage();

      Assert.Equal(string.Empty, language.Code);
      Assert.Equal(string.Empty, language.Name);
      Assert.Empty(language.Targets);
   }

   [Fact]
   public void WhenSingleTranslationCreatedThenDefaultValuesAreApplied()
   {
      var translation = new SingleTranslation();

      Assert.Equal(string.Empty, translation.Language);
      Assert.Empty(translation.Translations);
   }

   [Fact]
   public void WhenTranslateFileRequestConfiguredThenValuesPersist()
   {
      var file = Substitute.For<IFormFile>();
      var request = new TranslateFileRequest
      {
         Api_key = "key",
         File = file,
         Source = "cs",
         Target = "de"
      };

      Assert.Equal("key", request.Api_key);
      Assert.Same(file, request.File);
      Assert.Equal("cs", request.Source);
      Assert.Equal("de", request.Target);
   }

   [Fact]
   public void WhenTranslateFileResultCreatedThenDefaultValueIsEmpty()
   {
      var result = new TranslateFileResult();

      Assert.Equal(string.Empty, result.TranslatedFileUrl);
   }

   [Fact]
   public void WhenTranslateRequestSerializedThenJsonPropertyNamesMatchContract()
   {
      var request = new TranslateRequest
      {
         Alternatives = 2,
         ApiKey = "secret",
         Format = "html",
         Query = "Hello",
         Source = "en",
         Target = "cs"
      };

      var json = JsonSerializer.Serialize(request);

      Assert.Contains("\"api_key\":\"secret\"", json);
      Assert.Contains("\"q\":\"Hello\"", json);
      Assert.Contains("\"Alternatives\":2", json);
   }

   [Fact]
   public void WhenTranslateResultCreatedThenDefaultValuesAreApplied()
   {
      var result = new TranslateResult();

      Assert.Empty(result.Alternatives);
      Assert.NotNull(result.DetectedLanguage);
      Assert.Equal(string.Empty, result.TranslatedText);
   }

   [Fact]
   public void WhenTranslationCreatedThenDefaultValueIsEmpty()
   {
      var translation = new Translation();

      Assert.Equal(string.Empty, translation.TranslatedText);
   }

   [Fact]
   public void WhenLocalizationHubCreatedThenItIsSignalRHub()
   {
      var hub = new LocalizationHub();

      Assert.IsAssignableFrom<Hub>(hub);
   }

   [Fact]
   public void WhenLibreTranslateHttpClientFactoryCreatedThenClientUsesConfiguredBaseAddress()
   {
      var settings = new AutomaticTranslationSettings { Address = "https://translate.example/" };
      var factory = new LibreTranslateHttpClientFactory(settings);

      using var client = factory.LibreClient;

      Assert.Equal(new Uri("https://translate.example/"), client.BaseAddress);
   }

   [Fact]
   public void WhenJsonStringLocalizerFactoryCreateWithResourceSourceCalledThenJsonStringLocalizerIsReturned()
   {
      var cache = Substitute.For<IDistributedCache>();
      var libreTranslate = Substitute.For<ILibreTranslateService>();
      var settings = new AutomaticTranslationSettings();
      var logger = Substitute.For<ILogger<JsonStringLocalizer>>();
      var factory = new JsonStringLocalizerFactory(cache, libreTranslate, settings, logger);

      var localizer = factory.Create(typeof(SharedLocalizationTypesTests));

      Assert.IsType<JsonStringLocalizer>(localizer);
   }

   [Fact]
   public void WhenJsonStringLocalizerFactoryCreateWithBaseNameCalledThenJsonStringLocalizerIsReturned()
   {
      var cache = Substitute.For<IDistributedCache>();
      var libreTranslate = Substitute.For<ILibreTranslateService>();
      var settings = new AutomaticTranslationSettings();
      var logger = Substitute.For<ILogger<JsonStringLocalizer>>();
      var factory = new JsonStringLocalizerFactory(cache, libreTranslate, settings, logger);

      var localizer = factory.Create("base", "location");

      Assert.IsType<JsonStringLocalizer>(localizer);
   }
}