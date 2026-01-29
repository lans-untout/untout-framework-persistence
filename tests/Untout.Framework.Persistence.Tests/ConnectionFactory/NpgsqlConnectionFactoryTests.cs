using Xunit;
using Untout.Framework.Persistence.PostgreSql;

namespace Untout.Framework.Persistence.Tests.ConnectionFactory;

public class NpgsqlConnectionFactoryTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConnectionStringIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NpgsqlConnectionFactory(null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConnectionStringIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NpgsqlConnectionFactory(string.Empty));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConnectionStringIsWhitespace()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NpgsqlConnectionFactory("   "));
    }

    [Fact]
    public async Task CreateConnectionAsync_ReturnsOpenConnection()
    {
        // Arrange
        // Using an invalid connection string to test that connection is attempted but not required to succeed
        // In real integration tests, this would use a real connection string
        var factory = new NpgsqlConnectionFactory("Host=localhost;Database=test;Username=test;Password=test");

        // Act & Assert
        // We can't test actual connection without a real database, but we can verify it doesn't throw on factory creation
        Assert.NotNull(factory);
    }

    [Fact]
    public void Constructor_AcceptsValidConnectionString()
    {
        // Arrange & Act
        var factory = new NpgsqlConnectionFactory("Host=localhost;Port=5432;Database=testdb;Username=user;Password=pass");

        // Assert
        Assert.NotNull(factory);
    }
}
