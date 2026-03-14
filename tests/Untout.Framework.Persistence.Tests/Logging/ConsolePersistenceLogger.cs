
using System;
using Untout.Framework.Persistence.Interfaces;

namespace Untout.Framework.Persistence.Tests.Logging;
/// <summary>
/// Console-based implementation of IPersistenceLogger for development and debugging.
/// Logs messages to the console with timestamps and log levels.
/// </summary>
public sealed class ConsolePersistenceLogger : IPersistenceLogger
{
    /// <summary>
    /// Singleton instance of ConsolePersistenceLogger.
    /// </summary>
    public static readonly ConsolePersistenceLogger Instance = new();

    private ConsolePersistenceLogger() { }

    /// <inheritdoc />
    public void LogDebug(string message)
    {
        WriteLog("DEBUG", message, ConsoleColor.Gray);
    }

    /// <inheritdoc />
    public void LogInformation(string message)
    {
        WriteLog("INFO", message, ConsoleColor.White);
    }

    /// <inheritdoc />
    public void LogWarning(string message)
    {
        WriteLog("WARN", message, ConsoleColor.Yellow);
    }

    /// <inheritdoc />
    public void LogError(string message, Exception exception)
    {
        WriteLog("ERROR", $"{message} | Exception: {exception.GetType().Name} - {exception.Message}", ConsoleColor.Red);
    }

    /// <inheritdoc />
    public void LogQuery(string sql, object parameters = null)
    {
        var paramInfo = parameters != null ? $" | Params: {parameters}" : string.Empty;
        WriteLog("SQL", $"{sql}{paramInfo}", ConsoleColor.Cyan);
    }

    private static void WriteLog(string level, string message, ConsoleColor color)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var originalColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{timestamp}] [{level}] {message}");
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }
}
