using System.Linq.Expressions;

namespace Dita.Server.Storage.Abstractions;

/// <summary>
/// Generic repository contract that provides basic CRUD operations for an entity type.
/// </summary>
/// <remarks>
/// Implementations are storage-agnostic — the same interface is fulfilled by EF Core, file-based,
/// and MongoDB backends. Business code should depend on this interface, never on a concrete implementation.
/// </remarks>
/// <typeparam name="T">The entity type. Must be a reference type.</typeparam>
public interface IRepository<T> where T : class
{
   /// <summary>Returns the entity with the given primary key, or <see langword="null"/> if not found.</summary>
   /// <param name="id">The primary key value.</param>
   /// <param name="cancellationToken">Token used to cancel the operation.</param>
   Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

   /// <summary>Returns all entities of type <typeparamref name="T"/>.</summary>
   /// <param name="cancellationToken">Token used to cancel the operation.</param>
   Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

   /// <summary>Returns all entities that satisfy the given <paramref name="predicate"/>.</summary>
   /// <param name="predicate">A filter expression applied to the entity set.</param>
   /// <param name="cancellationToken">Token used to cancel the operation.</param>
   Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

   /// <summary>Adds a new entity to the repository.</summary>
   /// <param name="entity">The entity to add.</param>
   /// <param name="cancellationToken">Token used to cancel the operation.</param>
   Task AddAsync(T entity, CancellationToken cancellationToken = default);

   /// <summary>Marks an existing entity as modified so its changes will be persisted on the next save.</summary>
   /// <param name="entity">The entity to update.</param>
   void Update(T entity);

   /// <summary>Marks an entity for deletion so it will be removed on the next save.</summary>
   /// <param name="entity">The entity to remove.</param>
   void Remove(T entity);
}
