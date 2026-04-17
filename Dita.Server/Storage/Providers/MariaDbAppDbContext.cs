using Microsoft.EntityFrameworkCore;

namespace Dita.Server.Storage.Providers;

/// <summary>
/// EF Core database context configured for the MariaDB provider (Pomelo).
/// </summary>
/// <remarks>
/// Migrations for this provider are stored in <c>Storage/Migrations/MariaDB/</c>.
/// Use <c>dotnet ef migrations add &lt;Name&gt; --context MariaDbAppDbContext</c> to create a new migration.
/// </remarks>
public sealed class MariaDbAppDbContext(DbContextOptions<MariaDbAppDbContext> options) : AppDbContext(options)
{
}
