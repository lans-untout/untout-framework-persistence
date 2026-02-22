namespace Untout.Framework.Persistence.PostgreSql;

using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

/// <summary>
/// Thin abstraction over Dapper static extension methods to make them mockable in tests
/// </summary>
public interface IDapperExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, CommandDefinition command);
    Task<T> QuerySingleOrDefaultAsync<T>(IDbConnection connection, CommandDefinition command);
    Task<int> ExecuteAsync(IDbConnection connection, CommandDefinition command);
    Task<T> ExecuteScalarAsync<T>(IDbConnection connection, CommandDefinition command);
}
