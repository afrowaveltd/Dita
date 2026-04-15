using Afrowave.SharedTools.Api.Services;
using Dita.Server.Logging;
using Dita.Server.Services;
using Dita.Shared.Localization.Middlewares;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Localization;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AspNetCore.App.SignalR.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

const string ConsoleOutputTemplate = """
┌──────────────────────────────────────────────────────────────────────────────
│ {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}]
│ Source: {SourceContext}
│ Message: {Message:lj}
│ Properties: {Properties:j}
{Exception}└──────────────────────────────────────────────────────────────────────────────

""";

#if DEBUG
const LogEventLevel BootstrapMinimumLevel = LogEventLevel.Verbose;
const LogEventLevel ApplicationMinimumLevel = LogEventLevel.Verbose;
const LogEventLevel FrameworkMinimumLevel = LogEventLevel.Debug;
const LogEventLevel AspNetCoreMinimumLevel = LogEventLevel.Debug;
const LogEventLevel RequestSuccessLevel = LogEventLevel.Debug;
const LogEventLevel JsonFileMinimumLevel = LogEventLevel.Debug;
const double SlowRequestThresholdMs = 500;
#else
const bool IsDetailedLogging = false;
const LogEventLevel BootstrapMinimumLevel = LogEventLevel.Information;
const LogEventLevel ApplicationMinimumLevel = LogEventLevel.Information;
const LogEventLevel FrameworkMinimumLevel = LogEventLevel.Warning;
const LogEventLevel AspNetCoreMinimumLevel = LogEventLevel.Warning;
const LogEventLevel RequestSuccessLevel = LogEventLevel.Information;
const LogEventLevel JsonFileMinimumLevel = LogEventLevel.Information;
const double SlowRequestThresholdMs = 1000;
#endif
// Logging 
LogEventLevel GetRequestLogLevel(HttpContext httpContext, double elapsedMilliseconds, Exception? exception)
{
   if(exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
   {
      return LogEventLevel.Error;
   }

   if(httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest || elapsedMilliseconds >= SlowRequestThresholdMs)
   {
      return LogEventLevel.Warning;
   }

   return RequestSuccessLevel;
}

void EnrichRequestDiagnosticContext(Serilog.IDiagnosticContext diagnosticContext, HttpContext httpContext)
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

Log.Logger = new LoggerConfiguration()
   .MinimumLevel.Is(BootstrapMinimumLevel)
   .MinimumLevel.Override("Microsoft", FrameworkMinimumLevel)
   .MinimumLevel.Override("Microsoft.AspNetCore", AspNetCoreMinimumLevel)
   .MinimumLevel.Override("System", FrameworkMinimumLevel)
   .Enrich.FromLogContext()
#if DEBUG
   .WriteTo.Console(
      restrictedToMinimumLevel: LogEventLevel.Verbose,
      theme: AnsiConsoleTheme.Literate,
      outputTemplate: ConsoleOutputTemplate)
#endif
   .CreateBootstrapLogger();

try
{
   Log.Information("Starting web application");
   // Logging configuration and services
   WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
   LogStoragePaths logStoragePaths = LogStoragePaths.Create(builder.Environment.ContentRootPath);
   logStoragePaths.EnsureDirectories();
   LogStorageMaintenance.CleanupExpiredFiles(logStoragePaths);
   builder.Logging.ClearProviders();
   builder.Services.AddSingleton(logStoragePaths);
   builder.Services.AddSingleton<JsonArrayFileSink>();
   builder.Services.AddSingleton<SqliteLogSink>();
   builder.Services.AddDefaultSerilogHub();
   builder.Services.AddHostedService<LogCleanupService>();
   builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
      .MinimumLevel.Is(ApplicationMinimumLevel)
      .MinimumLevel.Override("Microsoft", FrameworkMinimumLevel)
      .MinimumLevel.Override("Microsoft.AspNetCore", AspNetCoreMinimumLevel)
      .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
      .MinimumLevel.Override("System", FrameworkMinimumLevel)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
      .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
#if DEBUG
      .WriteTo.Console(
         restrictedToMinimumLevel: LogEventLevel.Verbose,
         theme: AnsiConsoleTheme.Literate,
         outputTemplate: ConsoleOutputTemplate)
#endif
      .WriteTo.Sink(services.GetRequiredService<JsonArrayFileSink>(), restrictedToMinimumLevel: JsonFileMinimumLevel)
      .WriteTo.Sink(services.GetRequiredService<SqliteLogSink>(), restrictedToMinimumLevel: LogEventLevel.Warning)
      .WriteTo.SignalR(services, "ReceiveEvent"));

   // other services 

   builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
   {
      // Set property naming policy to camelCase
      options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

      // Allow complex object types like Lists<T> or other nested members
      options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

      // Add support for preserving references if needed (useful for circular references)
      options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

      // Customize any other settings as needed (e.g., number or date handling)
   });
   builder.Services.Configure<ForwardedHeadersOptions>(options =>
   {
      options.ForwardedHeaders =
      ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
   });

   builder.Services.AddControllers()
          .AddJsonOptions(options =>
          {
             // Set property naming policy to camelCase
             options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

             // Allow Lists and nested objects
             options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

             // Handle circular references if applicable
             options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
          })
          .AddXmlDataContractSerializerFormatters();

   builder.Services.AddRazorPages();
   builder.Services.AddSingleton<SettingsService>();
   builder.Services.AddSignalR();
   builder.Services.AddHttpClient();

   builder.Services.AddHttpContextAccessor();
   builder.Services.AddAntiforgery(options =>
   {
      options.HeaderName = "X-XSRF-TOKEN";
      options.Cookie.Name = "XSRF-TOKEN";
      options.Cookie.SecurePolicy = CookieSecurePolicy.None;
      options.Cookie.SameSite = SameSiteMode.Strict;
      options.Cookie.HttpOnly = true;
   });
   builder.Services.AddDistributedMemoryCache();
   builder.Services.AddLocalization();

   AutomaticTranslationSettings automaticTranslationSettings = builder.Configuration
      .GetSection(nameof(AutomaticTranslationSettings))
      .Get<AutomaticTranslationSettings>() ?? new AutomaticTranslationSettings();

   // middlewares
   builder.Services.AddTransient<LocalizationMiddleware>();

   // Singleton services
   builder.Services.AddSingleton(automaticTranslationSettings);
   builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
   builder.Services.AddSingleton<IHttpService, HttpService>();
   builder.Services.AddSingleton<ILibreTranslateHttpClientFactory, LibreTranslateHttpClientFactory>();
   builder.Services.AddSingleton<ILibreTranslateService, LibreTranslateService>();
   builder.Services.AddSingleton<ICookieService, CookieService>();

   // Scoped services

   // Transient services
   builder.Services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();


   WebApplication app = builder.Build();

   app.UseSerilogRequestLogging(options =>
   {
      options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
      options.GetLevel = GetRequestLogLevel;
      options.EnrichDiagnosticContext = EnrichRequestDiagnosticContext;
   });

   if(!app.Environment.IsDevelopment())
   {
      app.UseExceptionHandler("/Error");
      app.UseHsts();
      app.UseHttpsRedirection();
   }

   string[] supportedCultures = ["en"];



   app.UseRouting();

   app.UseAuthorization();

   app.MapStaticAssets();
   app.MapRazorPages()
      .WithStaticAssets();

   app.Run();
}
catch(Exception exception)
{
   Log.Fatal(exception, "Application terminated unexpectedly");
   throw;
}
finally
{
   Log.CloseAndFlush();
}