using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    private readonly string _idColumn;
    private readonly Dictionary<string, PropertyInfo> _properties;
    private readonly Dictionary<string, string> _columnNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlQueryBuilder{TKey, TEntity}"/> class
    /// </summary>
    /// <param name="nameAdapter">Name adapter for table/column mapping</param>
    public PostgreSqlQueryBuilder(IDbNameAdapter nameAdapter)
    {
        _nameAdapter = nameAdapter ?? throw new ArgumentNullException(nameof(nameAdapter));
        _tableName = nameAdapter.GetTableName<TEntity>();
        _idColumn = nameAdapter.GetColumnName<TEntity>(nameof(IEntity<TKey>.Id));

        // Cache properties (exclude Id for inserts/updates)
        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.Name != nameof(IEntity<TKey>.Id))
            .ToList();

        // Dual cache: PropertyInfo for fast value access + column names for SQL generation
        _properties = properties.ToDictionary(p => p.Name, p => p);
        _columnNames = properties.ToDictionary(p => p.Name, p => nameAdapter.GetColumnName<TEntity>(p.Name));
    }

    /// <inheritdoc />
    public string BuildSelectAll()
    {
        var colsAndAs = string.Join(", ", _columnNames.Select(kv => $"{kv.Value} AS {kv.Key}"));
        return $"SELECT {_idColumn} AS Id, {colsAndAs} FROM {_tableName}";
    }

    /// <inheritdoc />
    public (string Sql, DynamicParameters Parameters) BuildSelectById(TKey id)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);
        var colsAndAs = string.Join(", ", _columnNames.Select(kv => $"{kv.Value} AS {kv.Key}"));
        return ($"SELECT {_idColumn} AS Id, {colsAndAs} FROM {_tableName} WHERE {_idColumn} = @Id", parameters);
    }

    /// <inheritdoc />
    public (string Sql, DynamicParameters Parameters) BuildInsert(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        var parameters = new DynamicParameters();
        foreach (var kv in _properties)
        {
            var value = kv.Value.GetValue(entity);
            parameters.Add(kv.Key, value);
        }

        var columnList = string.Join(", ", _columnNames.Values);
        var parameterList = string.Join(", ", _columnNames.Keys.Select(c => $"@{c}"));

        var colsAndAs = string.Join(", ", _columnNames.Select(kv => $"{kv.Value} AS {kv.Key}"));
        return ($"INSERT INTO {_tableName} ({columnList}) VALUES ({parameterList}) RETURNING {_idColumn} AS Id, {colsAndAs}", parameters);
    }

    /// <inheritdoc />
    public (string Sql, DynamicParameters Parameters) BuildUpdate(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        var parameters = new DynamicParameters();
        foreach (var kv in _properties)
        {
            var value = kv.Value.GetValue(entity);
            parameters.Add(kv.Key, value);
        }
        parameters.Add("Id", entity.Id);

        var setClause = string.Join(", ", _columnNames.Select(kv => $"{kv.Value} = @{kv.Key}"));

        return ($"UPDATE {_tableName} SET {setClause} WHERE {_idColumn} = @Id", parameters);
    }

    /// <inheritdoc />
    public (string Sql, DynamicParameters Parameters) BuildDelete(TKey id)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);
        return ($"DELETE FROM {_tableName} WHERE {_idColumn} = @Id", parameters);
    }
}
