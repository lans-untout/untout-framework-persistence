using Npgsql;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Untout.Framework.Persistence.Interfaces;

namespace Untout.Framework.Persistence.PostgreSql;

/// <summary>
/// PostgreSQL connection factory implementation
/// Manages connection lifecycle using NpgsqlConnection
/// </summary>
internal class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Gets the connection string used by this factory.
    /// </summary>
    public string ConnectionString => _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqlConnectionFactory"/> class
    /// </summary>
    /// <param name="connectionString">PostgreSQL connection string</param>
    /// <exception cref="ArgumentNullException">Thrown if connectionString is null or empty</exception>
    public NpgsqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
