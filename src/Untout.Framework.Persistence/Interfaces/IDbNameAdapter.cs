namespace Untout.Framework.Persistence.Interfaces;

/// <summary>
/// Adapter for converting between C# naming conventions and database naming conventions
/// Supports both attribute-based and convention-based mapping
/// </summary>
public interface IDbNameAdapter
{
    /// <summary>
    /// Gets the database table name for an entity type
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <returns>Table name to use in SQL queries</returns>
    string GetTableName<T>() where T : class;

    /// <summary>
    /// Gets the database column name for a property
    /// </summary>
    /// <param name="propertyName">C# property name</param>
    /// <returns>Column name to use in SQL queries</returns>
    string GetColumnName(string propertyName);
}
