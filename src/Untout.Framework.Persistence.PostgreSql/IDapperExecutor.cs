namespace Untout.Framework.Persistence.PostgreSql;

using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Thin abstraction over Dapper static extension methods to make them mockable in tests
/// </summary>
public interface IDapperExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object? param = null);
    Task<T?> QuerySingleOrDefaultAsync<T>(IDbConnection connection, string sql, object? param = null);
    Task<int> ExecuteAsync(IDbConnection connection, string sql, object? param = null);
    Task<T?> ExecuteScalarAsync<T>(IDbConnection connection, string sql, object? param = null);
}
