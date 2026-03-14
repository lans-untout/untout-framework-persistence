using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Untout.Framework.Persistence.Interfaces;

namespace Untout.Framework.Persistence.PostgreSql;

/// <summary>
/// PostgreSQL implementation of IDbTransactionScope.
/// Manages a connection and transaction lifecycle.
/// </summary>
internal sealed class NpgsqlTransactionScope : IDbTransactionScope
{
    private readonly DbConnection _connection;
    private readonly DbTransaction _transaction;
    private volatile bool _committed;
    private bool _disposed;

    /// <inheritdoc />
    public IDbConnection Connection => _connection;

    /// <inheritdoc />
    public IDbTransaction Transaction => _transaction;

    public NpgsqlTransactionScope(DbConnection connection, DbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_committed)
        {
            throw new InvalidOperationException("Transaction has already been committed.");
        }

        await _transaction.CommitAsync(cancellationToken);
        _committed = true;
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_committed)
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (!_committed)
        {
            try
            {
                await _transaction.RollbackAsync();
            }
            catch
            {
                // Ignore rollback errors during disposal
            }
        }

        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
        _disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (!_committed)
        {
            try
            {
                _transaction.Rollback();
            }
            catch
            {
                // Ignore rollback errors during disposal
            }
        }

        _transaction.Dispose();
        _connection.Dispose();
        _disposed = true;
    }
}
