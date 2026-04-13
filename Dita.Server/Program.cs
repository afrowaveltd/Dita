using Dita.Server.Logging;
using Dita.Server.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AspNetCore.App.SignalR.Extensions;
using Serilog.Sinks.SystemConsole.Themes;

const string ConsoleOutputTemplate = """
┌──────────────────────────────────────────────────────────────────────────────
│ {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}]
│ Source: {SourceContext}
│ Message: {Message:lj}
│ Properties: {Properties:j}
{Exception}└──────────────────────────────────────────────────────────────────────────────

""";

#if DEBUG
const bool IsDetailedLogging = true;
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

   if(!IsDetailedLogging)
   {
      return;
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
   builder.Services.AddRazorPages();
   builder.Services.AddSingleton<SettingsService>();

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