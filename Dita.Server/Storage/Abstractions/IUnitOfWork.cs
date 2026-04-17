namespace Dita.Server.Storage.Abstractions;

/// <summary>
/// Represents a unit of work that groups multiple repository operations into a single atomic transaction.
/// </summary>
/// <remarks>
/// Call <see cref="SaveChangesAsync"/> once after all mutations to flush the changes to the underlying store.
/// EF Core implementations wrap the EF change tracker; file-based and MongoDB implementations may use their
/// own transaction equivalents.
/// </remarks>
public interface IUnitOfWork : IAsyncDisposable
{
   /// <summary>
   /// Persists all pending changes to the underlying storage provider.
   /// </summary>
   /// <param name="cancellationToken">Token used to cancel the operation.</param>
   /// <returns>The number of state entries written to the store.</returns>
   Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
