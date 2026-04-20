using Afrowave.SharedTools.Api.Services;
using Dita.Shared.Localization.Models;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace Dita.Shared.Localization.Middlewares;

/// <summary>
/// Middleware for managing application localization based on language settings.
/// </summary>
/// <remarks>
/// This middleware reads and writes language preferences from cookies and sets the current culture for the HTTP
/// context. If no language is explicitly set, the configured default language from
/// <see cref="AutomaticTranslationSettings.DefaultLanguage"/> is used.
/// </remarks>
public class LocalizationMiddleware(ICookieService cookie, AutomaticTranslationSettings settings) : IMiddleware
{
   private readonly ICookieService _cookie = cookie;
   private readonly AutomaticTranslationSettings _settings = settings;

   /// <summary>
   /// Processes an HTTP request and sets the culture based on the preferred language.
   /// </summary>
   /// <param name="context">The HTTP context of the current request.</param>
   /// <param name="next">The delegate for invoking the next middleware in the pipeline.</param>
   /// <returns>An asynchronous task representing the middleware processing.</returns>
   /// <remarks>
   /// The middleware performs the following steps: 1. Attempts to read the language from cookies.
   /// 2. If not present, parses the primary language from the Accept-Language header.
   /// 3. If neither source is available, uses the configured default language.
   /// 4. Verifies that the selected culture exists and applies it to the current thread.
   /// </remarks>
   public async Task InvokeAsync(HttpContext context, RequestDelegate next)
   {
      string defaultCulture = ResolveDefaultCulture();

      string? cultureKey = _cookie.ReadResponse("Language").Data;
      cultureKey = string.IsNullOrWhiteSpace(cultureKey)
         ? GetPrimaryLanguageFromHeader(context.Request.Headers["Accept-Language"].ToString() ?? "en")
         : cultureKey;
      cultureKey = string.IsNullOrWhiteSpace(cultureKey) ? defaultCulture : cultureKey;

      if (CultureExists(cultureKey))
      {
         CultureInfo culture = new(cultureKey);
         Thread.CurrentThread.CurrentCulture = culture;
         Thread.CurrentThread.CurrentUICulture = culture;
      }
      else
      {
         CultureInfo fallbackCulture = new(defaultCulture);
         Thread.CurrentThread.CurrentCulture = fallbackCulture;
         Thread.CurrentThread.CurrentUICulture = fallbackCulture;
         cultureKey = fallbackCulture.Name;
      }

      context.Request.Headers["Accept-Language"] = cultureKey;
      _cookie.Write("Language", cultureKey);

      await next(context);
   }

   private string ResolveDefaultCulture()
   {
      string configured = _settings.DefaultLanguage;
      if (string.IsNullOrWhiteSpace(configured))
      {
         return "en";
      }

      try
      {
         return CultureInfo.GetCultureInfo(configured).Name;
      }
      catch (CultureNotFoundException)
      {
         return "en";
      }
   }

   private static string? GetPrimaryLanguageFromHeader(string headerValue)
   {
      if (string.IsNullOrWhiteSpace(headerValue))
      {
         return null;
      }

      string firstPart = headerValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;
      if (string.IsNullOrWhiteSpace(firstPart))
      {
         return null;
      }

      string culturePart = firstPart.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;
      return string.IsNullOrWhiteSpace(culturePart) ? null : culturePart;
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