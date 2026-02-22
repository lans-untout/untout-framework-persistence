using System;
using System.IO;

namespace Untout.Framework.Persistence.Tests.Logging;

public class ConsolePersistenceLoggerTests : IDisposable
{
    private readonly ConsolePersistenceLogger _logger;
    private readonly StringWriter _consoleOutput;
    private readonly TextWriter _originalOutput;

    public ConsolePersistenceLoggerTests()
    {
        _logger = ConsolePersistenceLogger.Instance;
        _originalOutput = Console.Out;
        _consoleOutput = new StringWriter();
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _consoleOutput?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        // Arrange & Act
        var instance1 = ConsolePersistenceLogger.Instance;
        var instance2 = ConsolePersistenceLogger.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void LogDebug_WritesToConsole()
    {
        // Arrange
        var message = "Debug test message";

        // Act
        _logger.LogDebug(message);

        // Assert
        var output = _consoleOutput.ToString();
        Assert.Contains("[DEBUG]", output);
        Assert.Contains(message, output);
    }

    [Fact]
    public void LogInformation_WritesToConsole()
    {
        // Arrange
        var message = "Info test message";

        // Act
        _logger.LogInformation(message);

        // Assert
        var output = _consoleOutput.ToString();
        Assert.Contains("[INFO]", output);
        Assert.Contains(message, output);
    }

    [Fact]
    public void LogWarning_WritesToConsole()
    {
        // Arrange
        var message = "Warning test message";

        // Act
        _logger.LogWarning(message);

        // Assert
        var output = _consoleOutput.ToString();
        Assert.Contains("[WARN]", output);
        Assert.Contains(message, output);
    }

    [Fact]
    public void LogError_WritesToConsoleWithException()
    {
        // Arrange
        var message = "Error test message";
        var exception = new InvalidOperationException("Test exception");

        // Act
        _logger.LogError(message, exception);

        // Assert
        var output = _consoleOutput.ToString();
        Assert.Contains("[ERROR]", output);
        Assert.Contains(message, output);
        Assert.Contains("InvalidOperationException", output);
        Assert.Contains("Test exception", output);
    }

    [Fact]
    public void LogQuery_WithoutParameters_WritesToConsole()
    {
        // Arrange
        var sql = "SELECT * FROM users";

        // Act
        _logger.LogQuery(sql);

        // Assert
        var output = _consoleOutput.ToString();
        Assert.Contains("[SQL]", output);
        Assert.Contains(sql, output);
    }

    [Fact]
    public void LogQuery_WithParameters_WritesToConsoleWithParams()
    {
        // Arrange
        var sql = "SELECT * FROM users WHERE id = @Id";
        var parameters = new { Id = 123 };

        // Act
        _logger.LogQuery(sql, parameters);

        // Assert
        var output = _consoleOutput.ToString();
        Assert.Contains("[SQL]", output);
        Assert.Contains(sql, output);
        Assert.Contains("Params:", output);
    }

    [Fact]
    public void AllLogMethods_IncludeTimestamp()
    {
        // Act
        _logger.LogDebug("Test");
        _logger.LogInformation("Test");
        _logger.LogWarning("Test");
        _logger.LogError("Test", new Exception());
        _logger.LogQuery("SELECT 1");

        // Assert
        var output = _consoleOutput.ToString();
        var lines = output.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            // Each line should start with a timestamp in format [yyyy-MM-dd HH:mm:ss.fff]
            Assert.Matches(@"^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\]", line);
        }
    }

    [Fact]
    public void MultipleLogCalls_ProduceMultipleLines()
    {
        // Act
        _logger.LogDebug("Message 1");
        _logger.LogInformation("Message 2");
        _logger.LogWarning("Message 3");

        // Assert
        var output = _consoleOutput.ToString();
        var lines = output.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 3);
    }
}
