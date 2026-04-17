using Dita.Server.Storage.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dita.Server.Storage.DesignTime;

/// <summary>
/// Design-time factory for <see cref="SqlServerAppDbContext"/>.
/// Used by <c>dotnet ef</c> tooling to create migrations without a running application.
/// </summary>
/// <remarks>
/// The factory reads the connection string from the <c>DITA_SQLSERVER_CS</c> environment variable
/// or falls back to a local developer instance.
/// </remarks>
internal sealed class SqlServerDesignTimeFactory : IDesignTimeDbContextFactory<SqlServerAppDbContext>
{
   /// <inheritdoc />
   public SqlServerAppDbContext CreateDbContext(string[] args)
   {
      string connectionString = Environment.GetEnvironmentVariable("DITA_SQLSERVER_CS")
         ?? "Server=(localdb)\\mssqllocaldb;Database=DitaDev;Trusted_Connection=True;";

      DbContextOptionsBuilder<SqlServerAppDbContext> optionsBuilder = new();
      optionsBuilder.UseSqlServer(connectionString);

      return new SqlServerAppDbContext(optionsBuilder.Options);
   }
}
