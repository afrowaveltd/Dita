using Microsoft.EntityFrameworkCore;

namespace Dita.Server.Storage.Providers;

/// <summary>
/// EF Core database context configured for the PostgreSQL provider (Npgsql).
/// </summary>
/// <remarks>
/// Migrations for this provider are stored in <c>Storage/Migrations/PostgreSQL/</c>.
/// Use <c>dotnet ef migrations add &lt;Name&gt; --context PostgreSqlAppDbContext</c> to create a new migration.
/// </remarks>
public sealed class PostgreSqlAppDbContext(DbContextOptions<PostgreSqlAppDbContext> options) : AppDbContext(options)
{
}
