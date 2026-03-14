
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace Untout.Framework.Persistence.Interfaces;

/// <summary>
/// Thin abstraction over Dapper static extension methods to make them mockable in tests.
/// Supports both per-operation connections and explicit transaction scopes.
/// </summary>
public interface IDapperExecutor
{
    /// <summary>
    /// Executes a query and returns multiple results.
    /// </summary>
    Task<IEnumerable<T>> QueryAsync<T>(CommandDefinition command);

    /// <summary>
    /// Executes a query and returns a single result or default.
    /// </summary>
    Task<T> QuerySingleOrDefaultAsync<T>(CommandDefinition command);

    /// <summary>
    /// Executes a command and returns the number of affected rows.
    /// </summary>
    Task<int> ExecuteAsync(CommandDefinition command);

    /// <summary>
    /// Executes a command and returns a scalar value.
    /// </summary>
    Task<T> ExecuteScalarAsync<T>(CommandDefinition command);

    /// <summary>
    /// Executes multiple operations within a transaction scope.
    /// Automatically commits on success, rolls back on exception.
    /// </summary>
    /// <param name="operations">The operations to execute within the transaction.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteInTransactionAsync(
        Func<IDbConnection, IDbTransaction, Task> operations,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes multiple operations within a transaction scope and returns a result.
    /// Automatically commits on success, rolls back on exception.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(
        Func<IDbConnection, IDbTransaction, Task<T>> operations,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}