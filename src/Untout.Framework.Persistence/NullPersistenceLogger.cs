namespace Untout.Framework.Persistence;

using System;
using Untout.Framework.Persistence.Interfaces;

/// <summary>
/// Null implementation of IPersistenceLogger that does nothing.
/// Use this when logging is not needed (default behavior).
/// </summary>
public sealed class NullPersistenceLogger : IPersistenceLogger
{
    /// <summary>
    /// Singleton instance of NullPersistenceLogger.
    /// </summary>
    public static readonly NullPersistenceLogger Instance = new();

    private NullPersistenceLogger() { }

    /// <inheritdoc />
    public void LogDebug(string message) { }

    /// <inheritdoc />
    public void LogInformation(string message) { }

    /// <inheritdoc />
    public void LogWarning(string message) { }

    /// <inheritdoc />
    public void LogError(string message, Exception exception) { }

    /// <inheritdoc />
    public void LogQuery(string sql, object parameters = null) { }
}
