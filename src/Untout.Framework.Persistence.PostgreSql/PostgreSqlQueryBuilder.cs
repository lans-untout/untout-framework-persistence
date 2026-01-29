using System;
using System.Collections.Generic;
using System.Linq;
using Untout.Framework.Persistence.Interfaces;

namespace Untout.Framework.Persistence.PostgreSql;

/// <summary>
/// PostgreSQL-specific SQL query builder
/// Uses RETURNING clause for INSERT operations (PostgreSQL 8.2+)
/// </summary>
/// <typeparam name="TKey">Entity primary key type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public class PostgreSqlQueryBuilder<TKey, TEntity> : ISqlQueryBuilder<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
{
    private readonly IDbNameAdapter _nameAdapter;
    private readonly string _tableName;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlQueryBuilder{TKey, TEntity}"/> class
    /// </summary>
    /// <param name="nameAdapter">Name adapter for table/column mapping</param>
    public PostgreSqlQueryBuilder(IDbNameAdapter nameAdapter)
    {
        _nameAdapter = nameAdapter ?? throw new ArgumentNullException(nameof(nameAdapter));
        _tableName = nameAdapter.GetTableName<TEntity>();
    }

    /// <inheritdoc />
    public string BuildSelectAll()
    {
        return $"SELECT * FROM {_tableName}";
    }

    /// <inheritdoc />
    public string BuildSelectById()
    {
        var idColumn = _nameAdapter.GetColumnName(nameof(IEntity<TKey>.Id));
        return $"SELECT * FROM {_tableName} WHERE {idColumn} = @Id";
    }

    /// <inheritdoc />
    public string BuildInsert(IEnumerable<string> columns)
    {
        if (columns == null || !columns.Any())
        {
            throw new ArgumentException("Columns cannot be null or empty", nameof(columns));
        }

        var columnList = string.Join(", ", columns.Select(_nameAdapter.GetColumnName));
        var parameterList = string.Join(", ", columns.Select(c => $"@{c}"));
        var idColumn = _nameAdapter.GetColumnName(nameof(IEntity<TKey>.Id));

        // PostgreSQL RETURNING clause returns the inserted ID
        return $"INSERT INTO {_tableName} ({columnList}) VALUES ({parameterList}) RETURNING {idColumn}";
    }

    /// <inheritdoc />
    public string BuildUpdate(IEnumerable<string> columns)
    {
        if (columns == null || !columns.Any())
        {
            throw new ArgumentException("Columns cannot be null or empty", nameof(columns));
        }

        var setClause = string.Join(", ", columns.Select(c =>
            $"{_nameAdapter.GetColumnName(c)} = @{c}"));
        var idColumn = _nameAdapter.GetColumnName(nameof(IEntity<TKey>.Id));

        return $"UPDATE {_tableName} SET {setClause} WHERE {idColumn} = @Id";
    }

    /// <inheritdoc />
    public string BuildDelete()
    {
        var idColumn = _nameAdapter.GetColumnName(nameof(IEntity<TKey>.Id));
        return $"DELETE FROM {_tableName} WHERE {idColumn} = @Id";
    }
}
