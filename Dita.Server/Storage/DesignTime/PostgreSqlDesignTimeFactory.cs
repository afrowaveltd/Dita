using Dita.Server.Storage.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dita.Server.Storage.DesignTime;

/// <summary>
/// Design-time factory for <see cref="PostgreSqlAppDbContext"/>.
/// Used by <c>dotnet ef</c> tooling to create migrations without a running application.
/// </summary>
/// <remarks>
/// The factory reads the connection string from the <c>DITA_POSTGRES_CS</c> environment variable
/// or falls back to a local developer instance.
/// </remarks>
internal sealed class PostgreSqlDesignTimeFactory : IDesignTimeDbContextFactory<PostgreSqlAppDbContext>
{
   /// <inheritdoc />
   public PostgreSqlAppDbContext CreateDbContext(string[] args)
   {
      string connectionString = Environment.GetEnvironmentVariable("DITA_POSTGRES_CS")
         ?? "Host=localhost;Database=dita_dev;Username=postgres;Password=postgres";

      DbContextOptionsBuilder<PostgreSqlAppDbContext> optionsBuilder = new();
      optionsBuilder.UseNpgsql(connectionString);

      return new PostgreSqlAppDbContext(optionsBuilder.Options);
   }
}
