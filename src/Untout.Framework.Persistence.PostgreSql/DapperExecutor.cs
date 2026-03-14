using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Untout.Framework.Persistence.Interfaces;

namespace Untout.Framework.Persistence.PostgreSql;

/// <summary>
/// Default implementation of IDapperExecutor that creates and disposes connections per operation.
/// Registered as Scoped in DI - one instance per HTTP request or scope.
/// Thread-safe because it's stateless (creates connections per call).
/// Supports explicit transaction scopes for multi-operation transactions.
/// </summary>
internal sealed class DapperExecutor : IDapperExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITransactionScopeFactory _transactionScopeFactory;

    public DapperExecutor(IDbConnectionFactory connectionFactory, ITransactionScopeFactory transactionScopeFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(transactionScopeFactory);
        _connectionFactory = connectionFactory;
        _transactionScopeFactory = transactionScopeFactory;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> QueryAsync<T>(CommandDefinition command)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.QueryAsync<T>(command);
    }

    /// <inheritdoc />
    public async Task<T> QuerySingleOrDefaultAsync<T>(CommandDefinition command)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.QuerySingleOrDefaultAsync<T>(command);
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(CommandDefinition command)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.ExecuteAsync(command);
    }

    /// <inheritdoc />
    public async Task<T> ExecuteScalarAsync<T>(CommandDefinition command)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.ExecuteScalarAsync<T>(command);
    }

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(
        Func<IDbConnection, IDbTransaction, Task> operations,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);

        await using var scope = await BeginTransactionInternalAsync(isolationLevel, cancellationToken);

        try
        {
            await operations(scope.Connection, scope.Transaction);
            await scope.CommitAsync(cancellationToken);
        }
        catch
        {
            await scope.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<IDbConnection, IDbTransaction, Task<T>> operations,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);

        await using var scope = await BeginTransactionInternalAsync(isolationLevel, cancellationToken);

        try
        {
            var result = await operations(scope.Connection, scope.Transaction);
            await scope.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await scope.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<IDbTransactionScope> BeginTransactionInternalAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken)
    {
        return await _transactionScopeFactory.CreateAsync(isolationLevel, cancellationToken);
    }
}