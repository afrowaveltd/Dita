using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace Dita.Tests.Shared.Localization.Services;

public class LibreTranslateServiceTests
{
   [Fact]
   public async Task WhenDetectLanguageSucceedsThenResponseIsDeserializedAndApiKeyIsSent()
   {
      var handler = new QueueHttpMessageHandler();
      handler.Enqueue(async request =>
      {
         string body = await request.Content!.ReadAsStringAsync();
         Assert.Equal("https://translate.example/detect", request.RequestUri!.ToString());
         Assert.Contains("q=Ahoj", body);
         Assert.Contains("api_key=secret", body);
         return CreateJsonResponse("{" + "\"confidence\":99,\"language\":\"cs\"" + "}");
      });

      var service = CreateService(handler, new AutomaticTranslationSettings
      {
         Address = "https://translate.example",
         NeedsKey = true,
         Key = "secret"
      });

      Response<Detections> response = await service.DetectLanguageAsync("Ahoj");

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal(99, response.Data!.Confidence);
      Assert.Equal("cs", response.Data.Language);
   }

   [Fact]
   public async Task WhenGetAvailableLanguagesSucceedsThenCodesAreReturned()
   {
      var handler = new QueueHttpMessageHandler();
      handler.Enqueue(_ => Task.FromResult(CreateJsonResponse("[{\"code\":\"en\",\"name\":\"English\",\"targets\":[\"cs\"]},{\"code\":\"cs\",\"name\":\"Czech\",\"targets\":[\"en\"]}]")));
      var service = CreateService(handler);

      Response<string[]> response = await service.GetAvailableLanguagesAsync();

      Assert.True(response.Success);
      Assert.Equal(["en", "cs"], response.Data);
   }

   [Fact]
   public async Task WhenGetAvailableLanguagesReturnsEmptyListThenFailureIsReturned()
   {
      var handler = new QueueHttpMessageHandler();
      handler.Enqueue(_ => Task.FromResult(CreateJsonResponse("[]")));
      var service = CreateService(handler);

      Response<string[]> response = await service.GetAvailableLanguagesAsync();

      Assert.False(response.Success);
      Assert.Equal("No languages found.", response.Message);
   }

   [Fact]
   public async Task WhenTranslateFileWithoutSourceCalledThenAutoSourceIsSubmitted()
   {
      var handler = new QueueHttpMessageHandler();
      handler.Enqueue(async request =>
      {
         string body = await request.Content!.ReadAsStringAsync();
         Assert.Equal("https://translate.example/translate_file", request.RequestUri!.ToString());
         Assert.Contains("name=source", body);
         Assert.Contains("auto", body);
         Assert.Contains("name=target", body);
         Assert.Contains("de", body);
         Assert.Contains("sample.txt", body);
         return CreateJsonResponse("{" + "\"translatedFileUrl\":\"https://translate.example/files/sample-de.txt\"" + "}");
      });

      var service = CreateService(handler);
      await using MemoryStream stream = new(Encoding.UTF8.GetBytes("hello"));

      Response<TranslateFileResult> response = await service.TranslateFileAsync(stream, "de", "sample.txt");

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal("https://translate.example/files/sample-de.txt", response.Data!.TranslatedFileUrl);
   }

   [Fact]
   public async Task WhenTranslateTextSucceedsThenTranslatedTextIsReturned()
   {
      var handler = new QueueHttpMessageHandler();
      handler.Enqueue(_ => Task.FromResult(CreateJsonResponse("{" + "\"translatedText\":\"Ahoj\"" + "}")));
      var service = CreateService(handler);

      Response<TranslateResult> response = await service.TranslateTextAsync("Hello", "en", "cs");

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal("Ahoj", response.Data!.TranslatedText);
   }

   [Fact]
   public async Task WhenTranslationMatchesOriginalWithMixedCaseThenLowercaseRetryResultIsUsed()
   {
      var handler = new QueueHttpMessageHandler();
      handler.Enqueue(_ => Task.FromResult(CreateJsonResponse("{" + "\"translatedText\":\"Hello\"" + "}")));
      handler.Enqueue(_ => Task.FromResult(CreateJsonResponse("{" + "\"translatedText\":\"Ahoj\"" + "}")));
      var service = CreateService(handler);

      Response<TranslateResult> response = await service.TranslateTextAsync("Hello", "en", "cs");

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal("Ahoj", response.Data!.TranslatedText);
      Assert.Equal(2, handler.CallCount);
   }

   [Fact]
   public async Task WhenTranslateTextReturnsNullPayloadThenFailureIsReturned()
   {
      var handler = new QueueHttpMessageHandler();
      handler.Enqueue(_ => Task.FromResult(CreateJsonResponse("null")));
      var service = CreateService(handler);

      Response<TranslateResult> response = await service.TranslateTextAsync("Hello", "en", "cs");

      Assert.False(response.Success);
      Assert.Equal("Failed to deserialize translation result.", response.Message);
   }

   [Fact]
   public async Task WhenSourceAndTargetLanguageAreSameThenTranslateTextReturnsOriginalTextWithoutHttpCall()
   {
      var handler = new QueueHttpMessageHandler();
      var service = CreateService(handler);

      Response<TranslateResult> response = await service.TranslateTextAsync("Ahoj", "cs", "cs");

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal("Ahoj", response.Data!.TranslatedText);
      Assert.Equal(0, handler.CallCount);
   }

   [Fact]
   public async Task WhenSourceAndTargetLanguageAreCultureVariantsThenTranslateTextReturnsOriginalTextWithoutHttpCall()
   {
      var handler = new QueueHttpMessageHandler();
      var service = CreateService(handler);

      Response<TranslateResult> response = await service.TranslateTextAsync("Hello", "en-US", "en");

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal("Hello", response.Data!.TranslatedText);
      Assert.Equal(0, handler.CallCount);
   }

   [Fact]
   public async Task WhenTranslateTextGetsBadGatewayThenRetryAndSucceed()
   {
      var handler = new QueueHttpMessageHandler();
      handler.Enqueue(_ => Task.FromResult(CreateResponse(HttpStatusCode.BadGateway, "<html>502 Bad Gateway</html>")));
      handler.Enqueue(_ => Task.FromResult(CreateJsonResponse("{" + "\"translatedText\":\"Ahoj\"" + "}")));
      var service = CreateService(handler);

      Response<TranslateResult> response = await service.TranslateTextAsync("Hello", "en", "cs");

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal("Ahoj", response.Data!.TranslatedText);
      Assert.Equal(2, handler.CallCount);
   }

   [Fact]
   public async Task WhenTranslateTextGetsNonRetryableStatusThenFailWithoutRetry()
   {
      var handler = new QueueHttpMessageHandler();
      handler.Enqueue(_ => Task.FromResult(CreateResponse(HttpStatusCode.BadRequest, "bad request")));
      var service = CreateService(handler);

      Response<TranslateResult> response = await service.TranslateTextAsync("Hello", "en", "cs");

      Assert.False(response.Success);
      Assert.Contains("400", response.Message);
      Assert.Equal(1, handler.CallCount);
   }

   [Fact]
   public async Task WhenTranslateTextThrowsTransientExceptionThenRetryAndSucceed()
   {
      var handler = new QueueHttpMessageHandler();
      handler.Enqueue(_ => throw new HttpRequestException("network glitch"));
      handler.Enqueue(_ => Task.FromResult(CreateJsonResponse("{" + "\"translatedText\":\"Ahoj\"" + "}")));
      var service = CreateService(handler);

      Response<TranslateResult> response = await service.TranslateTextAsync("Hello", "en", "cs");

      Assert.True(response.Success);
      Assert.NotNull(response.Data);
      Assert.Equal("Ahoj", response.Data!.TranslatedText);
      Assert.Equal(2, handler.CallCount);
   }

   private static LibreTranslateService CreateService(QueueHttpMessageHandler handler, AutomaticTranslationSettings? settings = null)
   {
      settings ??= new AutomaticTranslationSettings { Address = "https://translate.example" };
      HttpClient client = new(handler)
      {
         BaseAddress = new Uri(settings.Address)
      };

      return new LibreTranslateService(
         settings,
         new StubLibreTranslateHttpClientFactory(client),
         Substitute.For<ILogger<LibreTranslateService>>());
   }

   private static HttpResponseMessage CreateJsonResponse(string content)
   {
      return new HttpResponseMessage(HttpStatusCode.OK)
      {
         Content = new StringContent(content, Encoding.UTF8, "application/json")
      };
   }

   private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string content)
   {
      return new HttpResponseMessage(statusCode)
      {
         Content = new StringContent(content, Encoding.UTF8, "text/plain")
      };
   }

   private sealed class StubLibreTranslateHttpClientFactory(HttpClient client) : ILibreTranslateHttpClientFactory
   {
      public HttpClient LibreClient => client;
   }

   private sealed class QueueHttpMessageHandler : HttpMessageHandler
   {
      private readonly Queue<Func<HttpRequestMessage, Task<HttpResponseMessage>>> _responses = new();

      public int CallCount { get; private set; }

      public void Enqueue(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory)
      {
         _responses.Enqueue(responseFactory);
      }

      protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      {
         CallCount++;

         if(_responses.Count == 0)
         {
            throw new InvalidOperationException("No queued response was configured.");
         }

         return _responses.Dequeue()(request);
      }
   }
}