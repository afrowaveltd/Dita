using Afrowave.SharedTools.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Dita.Shared.Localization.Middlewares;

/// <summary>
/// Middleware for managing application localization based on language settings.
/// </summary>
/// <remarks>
/// This middleware reads and writes language preferences from cookies and sets the current culture
/// for the HTTP context. If no language is explicitly set, the default language "en" is used.
/// </remarks>
public class LocalizationMiddleware(ILogger<LocalizationMiddleware> logger, ICookieService cookie)
{
   ILogger<LocalizationMiddleware> _logger = logger;
   ICookieService _cookie = cookie;

   /// <summary>
   /// Processes an HTTP request and sets the culture based on the preferred language.
   /// </summary>
   /// <param name="context">The HTTP context of the current request.</param>
   /// <param name="next">The delegate for invoking the next middleware in the pipeline.</param>
   /// <returns>An asynchronous task representing the middleware processing.</returns>
   /// <remarks>
   /// The middleware performs the following steps:
   /// 1. Attempts to read the language from cookies.
   /// 2. If not in cookies, reads Accept-Language from the request and saves it to cookies.
   /// 3. If neither source is available, uses the default language "en".
   /// 4. Verifies that the given culture exists, and if so, sets it for the current thread.
   /// </remarks>
   public async Task InvokeAsync(HttpContext context, RequestDelegate next)
   {
      string? cultureKey;

      if(_cookie.ReadResponse("Language").Data != null
           || _cookie.ReadResponse("Language").Data != string.Empty)
      {
         cultureKey = _cookie.ReadResponse("Language").Data;
         context.Request.Headers["Accept-Language"] = cultureKey;

      }
      else if(context.Request.Headers["Accept-Language"] != string.Empty)
      {
         cultureKey = context.Request.Headers["Accept-Language"];
         _cookie.Write("Language", cultureKey ?? "en");
      }
      else
      {
         cultureKey = "en";
         context.Request.Headers["Accept-Language"] = cultureKey;
         _cookie.Write("Language", cultureKey);
      }

      if(CultureExists(cultureKey ?? "en"))
      {
#pragma warning disable CS8604 // Může jít o argument s odkazem null.
         CultureInfo culture = new(cultureKey);
#pragma warning restore CS8604 // Může jít o argument s odkazem null.
         Thread.CurrentThread.CurrentCulture = culture;
         Thread.CurrentThread.CurrentUICulture = culture;
      }
      else
      {
         CultureInfo culture = new("en");
         Thread.CurrentThread.CurrentCulture = culture;
         Thread.CurrentThread.CurrentUICulture = culture;
      }

      await next(context);
   }

   /// <summary>
   /// Verifies whether the specified culture name exists in the system.
   /// </summary>
   /// <param name="cultureName">The culture name to verify (e.g., "en", "cs", "de-DE").</param>
   /// <returns><c>true</c> if the culture exists; otherwise <c>false</c>.</returns>
   private static bool CultureExists(string cultureName)
   {
      return CultureInfo.GetCultures(CultureTypes.AllCultures).Any(culture => string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
   }
}