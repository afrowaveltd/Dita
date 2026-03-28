using Dita.Server.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

const string ConsoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
   .MinimumLevel.Verbose()
   .Enrich.FromLogContext()
#if DEBUG
   .WriteTo.Console(
      restrictedToMinimumLevel: LogEventLevel.Verbose,
      theme: AnsiConsoleTheme.Code,
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
   builder.Services.AddSingleton<SqliteLogSink>();
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
         theme: AnsiConsoleTheme.Code,
         outputTemplate: ConsoleOutputTemplate)
#endif
      .WriteTo.File(
         formatter: new CompactJsonFormatter(),
         path: Path.Combine(logStoragePaths.TextDirectory, "server-.json"),
         restrictedToMinimumLevel: LogEventLevel.Information,
         rollingInterval: RollingInterval.Day,
         retainedFileCountLimit: logStoragePaths.RetentionDays,
         shared: true,
         flushToDiskInterval: TimeSpan.FromSeconds(1))
      .WriteTo.Sink(services.GetRequiredService<SqliteLogSink>(), restrictedToMinimumLevel: LogEventLevel.Warning));

   builder.Services.AddRazorPages();

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
