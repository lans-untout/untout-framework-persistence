namespace Untout.Framework.Persistence.PostgreSql.Adapters;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Untout.Framework.Persistence.Interfaces;

/// <summary>
/// Convention-based name adapter that converts PascalCase to snake_case
/// Used for PostgreSQL naming conventions (e.g., ArticleId -> article_id)
/// </summary>
public class SnakeCaseAdapter : IDbNameAdapter
{
    private static readonly Regex PascalCaseRegex = new(@"(?<!^)(?=[A-Z])", RegexOptions.Compiled);

    /// <inheritdoc />
    public string GetTableName<T>() where T : class
    {
        // Check for [Table] attribute first
        var tableAttr = typeof(T).GetCustomAttributes(typeof(TableAttribute), false);
        if (tableAttr.Length > 0)
        {
            return ((TableAttribute)tableAttr[0]).Name;
        }

        // Convert class name to snake_case
        return ToSnakeCase(typeof(T).Name);
    }

    /// <inheritdoc />
    public string GetColumnName(string propertyName)
    {
        return ToSnakeCase(propertyName);
    }

    /// <summary>
    /// Converts PascalCase string to snake_case
    /// </summary>
    /// <param name="input">PascalCase string</param>
    /// <returns>snake_case string</returns>
    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return PascalCaseRegex.Replace(input, "_").ToLowerInvariant();
    }
}
