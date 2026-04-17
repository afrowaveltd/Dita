using Microsoft.EntityFrameworkCore;

namespace Dita.Server.Storage;

/// <summary>
/// Base EF Core database context shared by all provider-specific subclasses.
/// </summary>
/// <remarks>
/// Add entity <see cref="DbSet{TEntity}"/> properties here as the data model grows.
/// Each supported database provider has its own subclass in <c>Storage/Providers/</c> that configures
/// provider-specific options and points EF Core migrations to the correct sub-folder under
/// <c>Storage/Migrations/</c>.
/// </remarks>
public abstract class AppDbContext(DbContextOptions options) : DbContext(options)
{
   // ── Entities ────────────────────────────────────────────────────────────────
   // Add DbSet<T> properties here when data model entities are defined.
   // Example:
   //   public DbSet<UserEntity> Users => Set<UserEntity>();

   // ── Model configuration ─────────────────────────────────────────────────────
   /// <inheritdoc />
   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);

      // Apply all IEntityTypeConfiguration<T> implementations found in this assembly.
      modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
   }
}
