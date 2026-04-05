using Dita.Server.Logging;
using Dita.Server.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AspNetCore.App.SignalR.Extensions;
using Serilog.Sinks.SystemConsole.Themes;

// Define a custom output template for console logging

const string ConsoleOutputTemplate = """
┌──────────────────────────────────────────────────────────────────────────────
│ {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}]
│ Source: {SourceContext}
│ Message: {Message:lj}
│ Properties: {Properties:j}
{Exception}└──────────────────────────────────────────────────────────────────────────────

""";

Log.Logger = new LoggerConfiguration()
   .MinimumLevel.Verbose()
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
      .MinimumLevel.Verbose()
      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
      .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
      .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
      .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
#if DEBUG
      .WriteTo.Console(
         restrictedToMinimumLevel: LogEventLevel.Verbose,
         theme: AnsiConsoleTheme.Literate,
         outputTemplate: ConsoleOutputTemplate)
#endif
      .WriteTo.Sink(services.GetRequiredService<JsonArrayFileSink>(), restrictedToMinimumLevel: LogEventLevel.Information)
      .WriteTo.Sink(services.GetRequiredService<SqliteLogSink>(), restrictedToMinimumLevel: LogEventLevel.Warning)
      .WriteTo.SignalR(services, "ReceiveEvent"));
   builder.Services.AddRazorPages();
   builder.Services.AddSingleton<SettingsService>();

   WebApplication app = builder.Build();

   app.UseSerilogRequestLogging(options =>
   {
      options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
      options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
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
      };
   });

   // Configure the HTTP request pipeline.
   if(!app.Environment.IsDevelopment())
   {
      app.UseExceptionHandler("/Error");
      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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