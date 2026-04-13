using Afrowave.SharedTools.Api.Services;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Dita.Tests.Shared.Localization.Middlewares;

public sealed class LocalizationMiddlewareTests : IDisposable
{
   private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
   private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

   [Fact]
   public async Task WhenCookieContainsValidCultureThenHeaderAndThreadCulturesAreUpdated()
   {
      var cookieService = Substitute.For<ICookieService>();
      cookieService.ReadResponse("Language").Returns(new Response<string> { Data = "cs-CZ", Success = true });
      var middleware = new LocalizationMiddleware(Substitute.For<ILogger<LocalizationMiddleware>>(), cookieService);
      var context = new DefaultHttpContext();
      var nextCalled = false;
      string? capturedCulture = null;
      string? capturedUiCulture = null;

      await middleware.InvokeAsync(context, _ =>
      {
         nextCalled = true;
         capturedCulture = CultureInfo.CurrentCulture.Name;
         capturedUiCulture = CultureInfo.CurrentUICulture.Name;
         return Task.CompletedTask;
      });

      Assert.True(nextCalled);
      Assert.Equal("cs-CZ", context.Request.Headers.AcceptLanguage.ToString());
      Assert.Equal("cs-CZ", capturedCulture);
      Assert.Equal("cs-CZ", capturedUiCulture);
   }

   [Fact]
   public async Task WhenCookieContainsUnknownCultureThenMiddlewareFallsBackToEnglish()
   {
      var cookieService = Substitute.For<ICookieService>();
      cookieService.ReadResponse("Language").Returns(new Response<string> { Data = "xx-Invalid", Success = true });
      var middleware = new LocalizationMiddleware(Substitute.For<ILogger<LocalizationMiddleware>>(), cookieService);
      var context = new DefaultHttpContext();
      string? capturedCulture = null;
      string? capturedUiCulture = null;

      await middleware.InvokeAsync(context, _ =>
      {
         capturedCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
         capturedUiCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
         return Task.CompletedTask;
      });

      Assert.Equal("xx-Invalid", context.Request.Headers.AcceptLanguage.ToString());
      Assert.Equal("en", capturedCulture);
      Assert.Equal("en", capturedUiCulture);
   }

   public void Dispose()
   {
      CultureInfo.CurrentCulture = _originalCulture;
      CultureInfo.CurrentUICulture = _originalUiCulture;
      GC.SuppressFinalize(this);
   }
}