namespace Untout.Framework.Persistence.PostgreSql;

using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Untout.Framework.Persistence.Interfaces;
using System;

internal class DapperExecutor : IDapperExecutor
{
    private readonly IDbConnection connection;
    private IDbConnectionFactory _connectionFactory;

    public DapperExecutor(IDbConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory, nameof(connectionFactory));
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(CommandDefinition command)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.QueryAsync<T>(command);
    }

    public async Task<T> QuerySingleOrDefaultAsync<T>(CommandDefinition command)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.QuerySingleOrDefaultAsync<T>(command);
    }

    public async Task<int> ExecuteAsync(CommandDefinition command)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.ExecuteAsync(command);
    }

    public async Task<T> ExecuteScalarAsync<T>(CommandDefinition command)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(command.CancellationToken);
        return await connection.ExecuteScalarAsync<T>(command);
    }
}