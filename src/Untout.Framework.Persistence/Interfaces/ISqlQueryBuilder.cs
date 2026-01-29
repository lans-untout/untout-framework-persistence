namespace Untout.Framework.Persistence.Interfaces;

using System.Collections.Generic;

/// <summary>
/// SQL query builder abstraction for database-agnostic query generation
/// Handles differences in SQL dialects (e.g., RETURNING vs OUTPUT clauses)
/// </summary>
/// <typeparam name="TKey">Entity primary key type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface ISqlQueryBuilder<TKey, TEntity> where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Builds a SELECT query to retrieve all entities
    /// </summary>
    /// <returns>SQL query string</returns>
    string BuildSelectAll();

    /// <summary>
    /// Builds a SELECT query to retrieve an entity by ID
    /// </summary>
    /// <returns>SQL query string with @Id parameter</returns>
    string BuildSelectById();

    /// <summary>
    /// Builds an INSERT query with database-specific syntax for returning the inserted ID
    /// PostgreSQL uses RETURNING clause, SQL Server uses OUTPUT clause
    /// </summary>
    /// <param name="columns">Column names to insert (excludes Id for auto-increment)</param>
    /// <returns>SQL query string with parameters for each column</returns>
    string BuildInsert(IEnumerable<string> columns);

    /// <summary>
    /// Builds an UPDATE query by ID
    /// </summary>
    /// <param name="columns">Column names to update (excludes Id)</param>
    /// <returns>SQL query string with @Id parameter and parameters for each column</returns>
    string BuildUpdate(IEnumerable<string> columns);

    /// <summary>
    /// Builds a DELETE query by ID
    /// </summary>
    /// <returns>SQL query string with @Id parameter</returns>
    string BuildDelete();
}
