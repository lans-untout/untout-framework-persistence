namespace Untout.Framework.Persistence.Tests.Logging;

using System;
using Xunit;
using Untout.Framework.Persistence;
using Untout.Framework.Persistence.Interfaces;

public class NullPersistenceLoggerTests
{
    private readonly IPersistenceLogger _logger;

    public NullPersistenceLoggerTests()
    {
        _logger = NullPersistenceLogger.Instance;
    }

    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        // Arrange & Act
        var instance1 = NullPersistenceLogger.Instance;
        var instance2 = NullPersistenceLogger.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void LogDebug_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _logger.LogDebug("Test debug message");
    }

    [Fact]
    public void LogInformation_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _logger.LogInformation("Test info message");
    }

    [Fact]
    public void LogWarning_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _logger.LogWarning("Test warning message");
    }

    [Fact]
    public void LogError_DoesNotThrow()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert - should not throw
        _logger.LogError("Test error message", exception);
    }

    [Fact]
    public void LogQuery_WithoutParameters_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _logger.LogQuery("SELECT * FROM users");
    }

    [Fact]
    public void LogQuery_WithParameters_DoesNotThrow()
    {
        // Arrange
        var parameters = new { Id = 123, Name = "Test" };

        // Act & Assert - should not throw
        _logger.LogQuery("SELECT * FROM users WHERE id = @Id", parameters);
    }

    [Fact]
    public void LogDebug_WithNullMessage_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _logger.LogDebug(null);
    }

    [Fact]
    public void LogError_WithNullException_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _logger.LogError("Error occurred", null);
    }
}
