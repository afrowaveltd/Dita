using Afrowave.SharedTools.Api.Services;
using Dita.Server.Logging;
using Dita.Server.Models.Settings;
using Dita.Server.Services;
using Dita.Server.Startup;
using Dita.Server.Storage;
using Dita.Shared.Localization.Middlewares;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Localization;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AspNetCore.App.SignalR.Extensions;
using Serilog.Sinks.SystemConsole.Themes;
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
const LogEventLevel BootstrapMinimumLevel = LogEventLevel.Information;
const LogEventLevel ApplicationMinimumLevel = LogEventLevel.Information;
const LogEventLevel FrameworkMinimumLevel = LogEventLevel.Warning;
const LogEventLevel AspNetCoreMinimumLevel = LogEventLevel.Warning;
const LogEventLevel RequestSuccessLevel = LogEventLevel.Information;
const LogEventLevel JsonFileMinimumLevel = LogEventLevel.Information;
const double SlowRequestThresholdMs = 1000;
#endif

// Bootstrap logger used before DI container is fully built.
Log.Logger = new LoggerConfiguration()
   .MinimumLevel.Is(BootstrapMinimumLevel)
   .MinimumLevel.Override("Microsoft", FrameworkMinimumLevel)
   .MinimumLevel.Override("Microsoft.AspNetCore", AspNetCoreMinimumLevel)
   .MinimumLevel.Override("System", FrameworkMinimumLevel)
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

   // Host + logging setup.
   WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
   int retentionDaysInfo = builder.Configuration.GetValue<int>("Logging:RetentionDaysInfo", LogStoragePaths.DefaultRetentionDaysInfo);
   int retentionDaysWarning = builder.Configuration.GetValue<int>("Logging:RetentionDaysWarning", LogStoragePaths.DefaultRetentionDaysWarning);
   LogStoragePaths logStoragePaths = LogStoragePaths.Create(builder.Environment.ContentRootPath, retentionDaysInfo, retentionDaysWarning);
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

   // Serialization and transport settings.
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
   builder.Services.AddOpenApi("Dita");
   builder.Services.AddRazorPages();
   builder.Services.AddSingleton<SettingsService>();
   builder.Services.AddSignalR()
      .AddJsonProtocol(o =>
   {

      o.PayloadSerializerOptions.PropertyNamingPolicy = null;
   });
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
   Log.Information("Appsettings.json file loaded: {@AutomaticTranslationSettings}", automaticTranslationSettings);

   builder.Services.AddSingleton<ICookieService, CookieService>();
   // Middleware + localization services.
   builder.Services.AddTransient<LocalizationMiddleware>();
   builder.Services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();

   // Singleton services
   StorageSettings? storageSettings = builder.Configuration
         .GetSection("Storage")
         .Get<StorageSettings>();
   builder.Services.AddSingleton(automaticTranslationSettings);
   builder.Services.AddSingleton<IHttpService, HttpService>();
   builder.Services.AddSingleton<ILanguageService, LanguageService>();
   builder.Services.AddSingleton<ILibreTranslateHttpClientFactory, LibreTranslateHttpClientFactory>();
   builder.Services.AddSingleton<ILibreTranslateService, LibreTranslateService>();
   builder.Services.AddSingleton<ITranslationQueue, TranslationQueue>();

   // Markdown translation services
   builder.Services.AddSingleton<IMarkdownParserService, MarkdownParserService>();
   builder.Services.AddSingleton<IMarkdownReconstructorService, MarkdownReconstructorService>();
   builder.Services.AddSingleton<IMarkdownTranslationService, MarkdownTranslationService>();

   // Storage: the provider is selected via Storage:StorageType in appsettings.json.
   // Changing the type and connection string is all that is needed to switch backends.
   // EF Core providers also apply pending migrations automatically at startup (see UseMigrationsAsync below).

   if(storageSettings == null)
   {
      Log.Error("Failed to load storage settings from configuration. Please check appsettings.json file.");
      throw new InvalidOperationException("Storage settings are not configured properly.");
   }

   Log.Information("Registering storage provider: {StorageType}", storageSettings.StorageType);
   builder.Services.AddSingleton<StorageSettings>(storageSettings);
   builder.Services.AddStorage(storageSettings);

   WebApplication app = builder.Build();

   app.UseSerilogRequestLogging(options =>
   {
      options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
      options.GetLevel = (httpContext, elapsed, exception) =>
         ProgramPipelineHelpers.GetRequestLogLevel(httpContext, elapsed, exception, SlowRequestThresholdMs, RequestSuccessLevel);
      options.EnrichDiagnosticContext = ProgramPipelineHelpers.EnrichRequestDiagnosticContext;
   });

   if(!app.Environment.IsDevelopment())
   {
      app.UseExceptionHandler("/Error");
      app.UseHsts();
      app.UseHttpsRedirection();
   }

   // Localization culture resolution.
   string defaultCulture = ProgramPipelineHelpers.NormalizeCultureCode(automaticTranslationSettings.DefaultLanguage);
   string[] supportedCultures;
   ILanguageService languageService = app.Services.GetRequiredService<ILanguageService>();
   ILibreTranslateService libreTranslateService = app.Services.GetRequiredService<ILibreTranslateService>();
   var languages = await libreTranslateService.GetAvailableLanguagesAsync();

   if(languages.Success && languages.Data.Length > 0)
   {
      var createResult = await languageService.CreateMissingLanguageFilesAsync([.. languages.Data]);
      foreach(var result in createResult)
      {
         if(result.Value)
         {
            Console.WriteLine($"Created missing language file for '{result.Key}'.");
         }
      }
   }

   supportedCultures = languageService.TranslationsPresented() ?? [defaultCulture];
   supportedCultures = [.. supportedCultures
      .Select(ProgramPipelineHelpers.NormalizeCultureCode)
      .Where(static culture => !string.IsNullOrWhiteSpace(culture))
      .Distinct(StringComparer.OrdinalIgnoreCase)];

   if(supportedCultures.Length == 0)
   {
      supportedCultures = [defaultCulture];
   }

   app.UseMiddleware<LocalizationMiddleware>();
   app.UseRequestLocalization(options =>
   {
      options.AddSupportedCultures(supportedCultures)
          .AddSupportedUICultures(supportedCultures)
          .SetDefaultCulture(defaultCulture)
          .ApplyCurrentCultureToResponseHeaders = true;
   });

   app.UseRouting();
   app.UseAuthentication();
   app.UseAuthorization();
   app.MapOpenApi()
        .CacheOutput();
   app.MapScalarApiReference(options =>
   {
      _ = options
          .WithTitle("Dita Open API Explorer")
          .WithTheme(ScalarTheme.Mars)
          .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
   });

   app.MapStaticAssets();
   app.MapRazorPages()
      .WithStaticAssets();

   // Apply any pending EF Core migrations before accepting traffic.
   // This is a no-op for file-based and MongoDB storage backends.
   await app.UseMigrationsAsync(storageSettings.StorageType);

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