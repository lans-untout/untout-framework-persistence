namespace Untout.Framework.Persistence.PostgreSql;

using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

internal class DapperExecutor : IDapperExecutor
{
    public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, CommandDefinition command)
        => connection.QueryAsync<T>(command);

    public Task<T> QuerySingleOrDefaultAsync<T>(IDbConnection connection, CommandDefinition command)
        => connection.QuerySingleOrDefaultAsync<T>(command);
    public Task<int> ExecuteAsync(IDbConnection connection, CommandDefinition command)
        => connection.ExecuteAsync(command);

    public Task<T> ExecuteScalarAsync<T>(IDbConnection connection, CommandDefinition command)
        => connection.ExecuteScalarAsync<T>(command);
}
