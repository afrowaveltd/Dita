using Dita.Server.Storage.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dita.Server.Storage.DesignTime;

/// <summary>
/// Design-time factory for <see cref="SqliteAppDbContext"/>.
/// Used by <c>dotnet ef</c> tooling to create migrations without a running application.
/// </summary>
/// <remarks>
/// The factory reads the connection string from the <c>DITA_SQLITE_CS</c> environment variable
/// or falls back to a local development database file.
/// </remarks>
internal sealed class SqliteDesignTimeFactory : IDesignTimeDbContextFactory<SqliteAppDbContext>
{
   /// <inheritdoc />
   public SqliteAppDbContext CreateDbContext(string[] args)
   {
      string connectionString = Environment.GetEnvironmentVariable("DITA_SQLITE_CS")
         ?? "Data Source=dita_design.db";

      DbContextOptionsBuilder<SqliteAppDbContext> optionsBuilder = new();
      optionsBuilder.UseSqlite(connectionString);

      return new SqliteAppDbContext(optionsBuilder.Options);
   }
}
