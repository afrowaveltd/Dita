using Microsoft.AspNetCore.Http;
using Serilog.Events;
using System.Globalization;

namespace Dita.Server.Startup;

/// <summary>
/// Provides reusable helper methods for the application startup pipeline.
/// </summary>
internal static class ProgramPipelineHelpers
{
   /// <summary>
   /// Resolves the Serilog event level for request logging based on response status, elapsed time and exceptions.
   /// </summary>
   /// <param name="httpContext">The current HTTP context.</param>
   /// <param name="elapsedMilliseconds">Elapsed request processing time in milliseconds.</param>
   /// <param name="exception">The exception thrown by the request pipeline, if any.</param>
   /// <param name="slowRequestThresholdMs">Threshold above which request duration is considered slow.</param>
   /// <param name="requestSuccessLevel">Log level used for successful requests.</param>
   /// <returns>The resulting log level for the request event.</returns>
   public static LogEventLevel GetRequestLogLevel(
      HttpContext httpContext,
      double elapsedMilliseconds,
      Exception? exception,
      double slowRequestThresholdMs,
      LogEventLevel requestSuccessLevel)
   {
      if(exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
      {
         return LogEventLevel.Error;
      }

      if(httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest || elapsedMilliseconds >= slowRequestThresholdMs)
      {
         return LogEventLevel.Warning;
      }

      return requestSuccessLevel;
   }

   /// <summary>
   /// Adds useful request diagnostics to Serilog's diagnostic context.
   /// </summary>
   /// <param name="diagnosticContext">The Serilog diagnostic context.</param>
   /// <param name="httpContext">The current HTTP context.</param>
   public static void EnrichRequestDiagnosticContext(Serilog.IDiagnosticContext diagnosticContext, HttpContext httpContext)
   {
      diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
      diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
      diagnosticContext.Set("RequestProtocol", httpContext.Request.Protocol);
      diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);

      string userAgent = httpContext.Request.Headers.UserAgent.ToString();
      if(!string.IsNullOrWhiteSpace(userAgent))
      {
         diagnosticContext.Set("UserAgent", userAgent);
      }

      string? clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
      if(!string.IsNullOrWhiteSpace(clientIp))
      {
         diagnosticContext.Set("ClientIp", clientIp);
      }

      diagnosticContext.Set("ConnectionId", httpContext.Connection.Id);

      string endpointName = httpContext.GetEndpoint()?.DisplayName ?? string.Empty;
      if(!string.IsNullOrWhiteSpace(endpointName))
      {
         diagnosticContext.Set("EndpointName", endpointName);
      }

      string queryString = httpContext.Request.QueryString.Value ?? string.Empty;
      if(!string.IsNullOrWhiteSpace(queryString))
      {
         diagnosticContext.Set("RequestQueryString", queryString);
      }

      string? contentType = httpContext.Request.ContentType;
      if(!string.IsNullOrWhiteSpace(contentType))
      {
         diagnosticContext.Set("RequestContentType", contentType);
      }

      if(httpContext.Request.ContentLength is long contentLength)
      {
         diagnosticContext.Set("RequestContentLength", contentLength);
      }
   }

   /// <summary>
   /// Normalizes a locale code into canonical .NET culture name when possible.
   /// </summary>
   /// <param name="localeCode">Locale code to normalize.</param>
   /// <returns>Normalized culture name, or the original value when not recognized.</returns>
   public static string NormalizeCultureCode(string localeCode)
   {
      try
      {
         return CultureInfo.GetCultureInfo(localeCode).Name;
      }
      catch(CultureNotFoundException)
      {
         return localeCode;
      }
   }
}
