using Afrowave.SharedTools.Api.Services;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Middlewares;
using Dita.Shared.Localization.Models;
using Microsoft.AspNetCore.Http;
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
      var middleware = new LocalizationMiddleware(cookieService, new AutomaticTranslationSettings { DefaultLanguage = "en" });
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
   public async Task WhenCookieContainsUnknownCultureThenMiddlewareFallsBackToDefaultLanguage()
   {
      var cookieService = Substitute.For<ICookieService>();
      cookieService.ReadResponse("Language").Returns(new Response<string> { Data = "xx-Invalid", Success = true });
      var middleware = new LocalizationMiddleware(cookieService, new AutomaticTranslationSettings { DefaultLanguage = "cs-CZ" });
      var context = new DefaultHttpContext();
      string? capturedCulture = null;
      string? capturedUiCulture = null;

      await middleware.InvokeAsync(context, _ =>
      {
         capturedCulture = CultureInfo.CurrentCulture.Name;
         capturedUiCulture = CultureInfo.CurrentUICulture.Name;
         return Task.CompletedTask;
      });

      Assert.Equal("cs-CZ", context.Request.Headers.AcceptLanguage.ToString());
      Assert.Equal("cs-CZ", capturedCulture);
      Assert.Equal("cs-CZ", capturedUiCulture);
   }

   [Fact]
   public async Task WhenCookieIsMissingThenMiddlewareUsesPrimaryAcceptLanguageDialect()
   {
      var cookieService = Substitute.For<ICookieService>();
      cookieService.ReadResponse("Language").Returns(new Response<string> { Data = string.Empty, Success = true });
      var middleware = new LocalizationMiddleware(cookieService, new AutomaticTranslationSettings { DefaultLanguage = "en" });
      var context = new DefaultHttpContext();
      context.Request.Headers.AcceptLanguage = "cs-CZ,cs;q=0.9,en-US;q=0.8";
      string? capturedCulture = null;

      await middleware.InvokeAsync(context, _ =>
      {
         capturedCulture = CultureInfo.CurrentUICulture.Name;
         return Task.CompletedTask;
      });

      Assert.Equal("cs-CZ", context.Request.Headers.AcceptLanguage.ToString());
      Assert.Equal("cs-CZ", capturedCulture);
   }

   public void Dispose()
   {
      CultureInfo.CurrentCulture = _originalCulture;
      CultureInfo.CurrentUICulture = _originalUiCulture;
      GC.SuppressFinalize(this);
   }
}