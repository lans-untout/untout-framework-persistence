namespace Untout.Framework.Persistence.Interfaces;

/// <summary>
/// Base interface for all entities with a strongly-typed primary key
/// </summary>
/// <typeparam name="TKey">The type of the primary key (int, Guid, etc.)</typeparam>
public interface IEntity<TKey>
{
    /// <summary>
    /// The primary key of the entity
    /// </summary>
    TKey Id { get; set; }
}
