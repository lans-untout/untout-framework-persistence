namespace Untout.Framework.Persistence.PostgreSql;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Untout.Framework.Persistence.Interfaces;

/// <summary>
/// Default implementation of IDapperExecutor that creates and disposes connections per operation.
/// Registered as Scoped in DI - one instance per HTTP request or scope.
/// Thread-safe because it's stateless (creates connections per call).
/// </summary>
public sealed class DapperExecutor : IDapperExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperExecutor(IDbConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory, nameof(connectionFactory));
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(CommandDefinition command)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.QueryAsync<T>(command);
    }

    public async Task<T> QuerySingleOrDefaultAsync<T>(CommandDefinition command)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.QuerySingleOrDefaultAsync<T>(command);
    }

    public async Task<int> ExecuteAsync(CommandDefinition command)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.ExecuteAsync(command);
    }

    public async Task<T> ExecuteScalarAsync<T>(CommandDefinition command)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.ExecuteScalarAsync<T>(command);
    }
}