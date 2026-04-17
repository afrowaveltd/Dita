using Microsoft.EntityFrameworkCore;

namespace Dita.Server.Storage.Providers;

/// <summary>
/// EF Core database context configured for the SQL Server provider.
/// </summary>
/// <remarks>
/// Migrations for this provider are stored in <c>Storage/Migrations/SqlServer/</c>.
/// Use <c>dotnet ef migrations add &lt;Name&gt; --context SqlServerAppDbContext</c> to create a new migration.
/// </remarks>
public sealed class SqlServerAppDbContext(DbContextOptions<SqlServerAppDbContext> options) : AppDbContext(options)
{
}
