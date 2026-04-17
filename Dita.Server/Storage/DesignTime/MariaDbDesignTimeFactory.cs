using Dita.Server.Storage.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Dita.Server.Storage.DesignTime;

/// <summary>
/// Design-time factory for <see cref="MariaDbAppDbContext"/>.
/// Used by <c>dotnet ef</c> tooling to create migrations without a running application.
/// </summary>
/// <remarks>
/// The factory reads the connection string from the <c>DITA_MARIADB_CS</c> environment variable
/// or falls back to a local developer instance.
/// The server version is auto-detected from the connection string.
/// </remarks>
internal sealed class MariaDbDesignTimeFactory : IDesignTimeDbContextFactory<MariaDbAppDbContext>
{
   /// <inheritdoc />
   public MariaDbAppDbContext CreateDbContext(string[] args)
   {
      string connectionString = Environment.GetEnvironmentVariable("DITA_MARIADB_CS")
         ?? "Server=localhost;Database=dita_dev;User=root;Password=root;";

      DbContextOptionsBuilder<MariaDbAppDbContext> optionsBuilder = new();
      optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

      return new MariaDbAppDbContext(optionsBuilder.Options);
   }
}
