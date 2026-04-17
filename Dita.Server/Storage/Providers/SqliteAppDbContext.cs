using Microsoft.EntityFrameworkCore;

namespace Dita.Server.Storage.Providers;

/// <summary>
/// EF Core database context configured for the SQLite provider.
/// </summary>
/// <remarks>
/// Migrations for this provider are stored in <c>Storage/Migrations/SQLite/</c>.
/// Use <c>dotnet ef migrations add &lt;Name&gt; --context SqliteAppDbContext</c> to create a new migration.
/// </remarks>
public sealed class SqliteAppDbContext(DbContextOptions<SqliteAppDbContext> options) : AppDbContext(options)
{
}
