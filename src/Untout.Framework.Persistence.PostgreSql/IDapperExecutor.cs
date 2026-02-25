namespace Untout.Framework.Persistence.PostgreSql;

using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

/// <summary>
/// Thin abstraction over Dapper static extension methods to make them mockable in tests
/// </summary>
public interface IDapperExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(CommandDefinition command);
    Task<T> QuerySingleOrDefaultAsync<T>(CommandDefinition command);
    Task<int> ExecuteAsync(CommandDefinition command);
    Task<T> ExecuteScalarAsync<T>(CommandDefinition command);
}