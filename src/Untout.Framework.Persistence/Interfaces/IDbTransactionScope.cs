using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Untout.Framework.Persistence.Interfaces;

/// <summary>
/// Represents an active database transaction scope.
/// Dispose to rollback, call CommitAsync to commit.
/// </summary>
internal interface IDbTransactionScope : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the underlying database connection for this transaction scope.
    /// </summary>
    IDbConnection Connection { get; }

    /// <summary>
    /// Gets the underlying database transaction.
    /// </summary>
    IDbTransaction Transaction { get; }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
