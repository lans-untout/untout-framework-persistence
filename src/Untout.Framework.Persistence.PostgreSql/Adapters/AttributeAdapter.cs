namespace Untout.Framework.Persistence.PostgreSql.Adapters;

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Untout.Framework.Persistence.Interfaces;

/// <summary>
/// Attribute-based name adapter that uses [Table] and [Column] attributes
/// Falls back to property/class name if no attribute is present
/// </summary>
public class AttributeAdapter : IDbNameAdapter
{
    /// <inheritdoc />
    public string GetTableName<T>() where T : class
    {
        var tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
        return tableAttr?.Name ?? typeof(T).Name;
    }

    /// <inheritdoc />
    public string GetColumnName(string propertyName)
    {
        // This is a simplified version - in real use, you'd pass the PropertyInfo
        // or cache property metadata for better performance
        return propertyName;
    }

    /// <summary>
    /// Gets column name for a specific property (overload with PropertyInfo)
    /// </summary>
    /// <param name="property">Property metadata</param>
    /// <returns>Column name from [Column] attribute or property name</returns>
    public string GetColumnName(PropertyInfo property)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
        return columnAttr?.Name ?? property.Name;
    }
}
