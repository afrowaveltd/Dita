using Dita.Server.Models.Enums;
using Dita.Server.Models.Settings;
using Dita.Server.Storage.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Dita.Server.Storage;

/// <summary>
/// Extension methods that register the storage layer and apply database migrations at application startup.
/// </summary>
public static class StorageExtensions
{
   /// <summary>
   /// Registers the appropriate database context and related storage services based on the
   /// <see cref="StorageSettings"/> configuration read from <c>appsettings.json</c>.
   /// </summary>
   /// <param name="services">The application service collection.</param>
   /// <param name="storageSettings">The storage configuration section from <c>appsettings.json</c>.</param>
   /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
   /// <exception cref="InvalidOperationException">
   /// Thrown when a database connection string is required but not configured.
   /// </exception>
   public static IServiceCollection AddStorage(this IServiceCollection services, StorageSettings storageSettings)
   {
      ArgumentNullException.ThrowIfNull(storageSettings);

      switch(storageSettings.StorageType)
      {
         case StorageType.EFCoreSqlite:
            RegisterSqlite(services, storageSettings);
            break;

         case StorageType.EFCoreSqlServer:
            RegisterSqlServer(services, storageSettings);
            break;

         case StorageType.EFCorePostgres:
            RegisterPostgreSql(services, storageSettings);
            break;

         case StorageType.EFCoreMariaDb:
            RegisterMariaDb(services, storageSettings);
            break;

         case StorageType.MongoDb:
            // MongoDB does not use EF Core — register the driver client here when ready.
            // services.AddSingleton<IMongoClient>(new MongoClient(storageSettings.ConnectionString));
            break;

         case StorageType.JsonFiles:
         case StorageType.AjisFiles:
            // File-based storage — no EF context needed.
            break;

         default:
            throw new InvalidOperationException($"Unsupported storage type: {storageSettings.StorageType}");
      }

      return services;
   }

   /// <summary>
   /// Applies any pending EF Core migrations for the active database provider at application startup.
   /// </summary>
   /// <remarks>
   /// This method is a no-op for non-EF storage backends (file-based, MongoDB).
   /// If the migration fails the application logs a fatal error and re-throws so the host shuts down cleanly
   /// instead of starting with an inconsistent schema.
   /// </remarks>
   /// <param name="app">The built web application.</param>
   /// <param name="storageType">The storage type selected in <c>appsettings.json</c>.</param>
   public static async Task UseMigrationsAsync(this WebApplication app, StorageType storageType)
   {
      if(!IsEfCoreProvider(storageType))
      {
         return;
      }

      ILogger<WebApplication> logger = app.Services.GetRequiredService<ILogger<WebApplication>>();

      try
      {
         await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
         AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

         IEnumerable<string> pending = await context.Database.GetPendingMigrationsAsync();
         string[] pendingMigrations = pending as string[] ?? pending.ToArray();

         if(pendingMigrations.Length == 0)
         {
            logger.LogInformation("Database schema is up to date — no pending migrations for {Provider}.", storageType);
            return;
         }

         logger.LogInformation(
            "Applying {Count} pending migration(s) for {Provider}: {Migrations}",
            pendingMigrations.Length,
            storageType,
            string.Join(", ", pendingMigrations));

         await context.Database.MigrateAsync();

         logger.LogInformation("Database migrations applied successfully for {Provider}.", storageType);
      }
      catch(Exception ex)
      {
         logger.LogCritical(ex, "Failed to apply database migrations for {Provider}. The application cannot start.", storageType);
         throw;
      }
   }

   // ── Private helpers ──────────────────────────────────────────────────────────

   private static void RegisterSqlite(IServiceCollection services, StorageSettings settings)
   {
      string connectionString = RequireConnectionString(settings, StorageType.EFCoreSqlite);

      services.AddDbContext<SqliteAppDbContext>(o => o.UseSqlite(connectionString));

      // Register the base type so UseMigrationsAsync can resolve it generically.
      services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<SqliteAppDbContext>());
   }

   private static void RegisterSqlServer(IServiceCollection services, StorageSettings settings)
   {
      string connectionString = RequireConnectionString(settings, StorageType.EFCoreSqlServer);

      services.AddDbContext<SqlServerAppDbContext>(o => o.UseSqlServer(connectionString));

      services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<SqlServerAppDbContext>());
   }

   private static void RegisterPostgreSql(IServiceCollection services, StorageSettings settings)
   {
      string connectionString = RequireConnectionString(settings, StorageType.EFCorePostgres);

      services.AddDbContext<PostgreSqlAppDbContext>(o => o.UseNpgsql(connectionString));

      services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<PostgreSqlAppDbContext>());
   }

   private static void RegisterMariaDb(IServiceCollection services, StorageSettings settings)
   {
      string connectionString = RequireConnectionString(settings, StorageType.EFCoreMariaDb);

      services.AddDbContext<MariaDbAppDbContext>(o =>
         o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

      services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<MariaDbAppDbContext>());
   }

   private static string RequireConnectionString(StorageSettings settings, StorageType storageType)
   {
      if(string.IsNullOrWhiteSpace(settings.ConnectionString))
      {
         throw new InvalidOperationException(
            $"A connection string is required for storage type '{storageType}' but none was provided in the configuration. " +
            $"Set 'Storage:ConnectionString' in appsettings.json.");
      }

      return settings.ConnectionString;
   }

   private static bool IsEfCoreProvider(StorageType storageType) => storageType
      is StorageType.EFCoreSqlite
      or StorageType.EFCoreSqlServer
      or StorageType.EFCorePostgres
      or StorageType.EFCoreMariaDb;
}
