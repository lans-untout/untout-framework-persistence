namespace Untout.Framework.Persistence.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Generic repository interface for CRUD operations
/// Provides standard data access patterns with async support
/// </summary>
/// <typeparam name="TKey">Entity primary key type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IRepository<TKey, TEntity> where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Retrieves all entities from the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity by its ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new entity into the database
    /// </summary>
    /// <param name="entity">Entity to insert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The inserted entity with ID populated</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the database
    /// </summary>
    /// <param name="entity">Entity with updated values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if update succeeded, false if entity not found</returns>
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by ID
    /// </summary>
    /// <param name="id">Entity ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion succeeded, false if entity not found</returns>
    Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}
