namespace Untout.Framework.Persistence.Interfaces;

using System;

/// <summary>
/// Logging abstraction for persistence operations.
/// Can be replaced with Serilog, NLog, or other logging frameworks in consuming applications.
/// </summary>
public interface IPersistenceLogger
{
    /// <summary>
    /// Logs a debug-level message (e.g., SQL queries, parameters).
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogDebug(string message);

    /// <summary>
    /// Logs an informational message (e.g., operation completed).
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogInformation(string message);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogWarning(string message);

    /// <summary>
    /// Logs an error with exception details.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="exception">The exception that occurred.</param>
    void LogError(string message, Exception exception);

    /// <summary>
    /// Logs a SQL query execution.
    /// </summary>
    /// <param name="sql">The SQL query being executed.</param>
    /// <param name="parameters">The parameters (optional).</param>
    void LogQuery(string sql, object parameters = null);
}
