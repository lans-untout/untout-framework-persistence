namespace Untout.Framework.Persistence.PostgreSql;

using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

internal class DapperExecutor : IDapperExecutor
{
    public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object? param = null)
        => connection.QueryAsync<T>(sql, param);

    public Task<T?> QuerySingleOrDefaultAsync<T>(IDbConnection connection, string sql, object? param = null)
        => connection.QuerySingleOrDefaultAsync<T>(sql, param);

    public Task<int> ExecuteAsync(IDbConnection connection, string sql, object? param = null)
        => connection.ExecuteAsync(sql, param);

    public Task<T?> ExecuteScalarAsync<T>(IDbConnection connection, string sql, object? param = null)
        => connection.ExecuteScalarAsync<T>(sql, param);
}
