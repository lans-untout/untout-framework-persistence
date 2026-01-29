namespace Untout.Framework.Persistence.Interfaces;

using System.Data;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Factory for creating database connections
/// Ensures proper connection lifecycle management and supports async operations
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates and opens a new database connection
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>An opened database connection that should be disposed by the caller</returns>
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
