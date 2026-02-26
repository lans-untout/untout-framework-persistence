namespace Untout.Framework.Persistence.Tests.Executors;

using Moq;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql;
using Xunit;

public class DapperExecutorTests
{
    private readonly Mock<IDbConnectionFactory> _mockFactory;
    private readonly Mock<IDbConnection> _mockConnection;

    public DapperExecutorTests()
    {
        _mockFactory = new Mock<IDbConnectionFactory>();
        _mockConnection = new Mock<IDbConnection>();
    }

    [Fact]
    public void Constructor_WithNullFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DapperExecutor(null));
    }

    [Fact]
    public void Constructor_WithValidFactory_Succeeds()
    {
        // Act
        var executor = new DapperExecutor(_mockFactory.Object);

        // Assert
        Assert.NotNull(executor);
    }

    [Fact]
    public async Task QueryAsync_CreatesConnectionFromFactory()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("SELECT * FROM test");

        // Act
        try
        {
            await executor.QueryAsync<TestEntity>(command);
        }
        catch
        {
            // Expected - we can't mock Dapper extension methods
        }

        // Assert
        _mockFactory.Verify(f => f.CreateConnectionAsync(default), Times.Once);
    }

    [Fact]
    public async Task QueryAsync_PassesCancellationTokenToFactory()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = new DapperExecutor(_mockFactory.Object);
        var cts = new CancellationTokenSource();
        var command = new CommandDefinition("SELECT * FROM test", cancellationToken: cts.Token);

        // Act
        try
        {
            await executor.QueryAsync<TestEntity>(command);
        }
        catch
        {
            // Expected - we can't mock Dapper extension methods
        }

        // Assert
        _mockFactory.Verify(f => f.CreateConnectionAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task QueryAsync_DisposesConnectionAfterExecution()
    {
        // Arrange
        var mockDisposableConnection = new Mock<IDbConnection>();
        mockDisposableConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDisposableConnection.Object);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("SELECT * FROM test");

        // Act
        try
        {
            await executor.QueryAsync<TestEntity>(command);
        }
        catch
        {
            // Expected - Dapper extension methods can't be mocked
        }

        // Assert - Connection should be disposed
        mockDisposableConnection.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public async Task QueryAsync_CreatesNewConnectionForEachCall()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var conn = new Mock<IDbConnection>();
                conn.Setup(c => c.State).Returns(ConnectionState.Open);
                return conn.Object;
            });

        var executor = new DapperExecutor(_mockFactory.Object);
        var command1 = new CommandDefinition("SELECT * FROM test1");
        var command2 = new CommandDefinition("SELECT * FROM test2");

        // Act
        try { await executor.QueryAsync<TestEntity>(command1); } catch { }
        try { await executor.QueryAsync<TestEntity>(command2); } catch { }

        // Assert - Factory should be called twice (one connection per operation)
        _mockFactory.Verify(f => f.CreateConnectionAsync(default), Times.Exactly(2));
    }

    [Fact]
    public async Task QueryAsync_WhenFactoryThrows_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Connection failed");
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("SELECT * FROM test");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            executor.QueryAsync<TestEntity>(command));
        Assert.Equal("Connection failed", exception.Message);
    }

    [Fact]
    public async Task QueryAsync_WithCancelledToken_PropagatesCancellation()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var executor = new DapperExecutor(_mockFactory.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var command = new CommandDefinition("SELECT * FROM test", cancellationToken: cts.Token);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            executor.QueryAsync<TestEntity>(command));
    }

    [Fact]
    public async Task QuerySingleOrDefaultAsync_CreatesConnectionFromFactory()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("SELECT * FROM test WHERE id = @Id");

        // Act
        try
        {
            await executor.QuerySingleOrDefaultAsync<TestEntity>(command);
        }
        catch
        {
            // Expected - we can't mock Dapper extension methods
        }

        // Assert
        _mockFactory.Verify(f => f.CreateConnectionAsync(default), Times.Once);
    }

    [Fact]
    public async Task QuerySingleOrDefaultAsync_PassesCancellationTokenToFactory()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = new DapperExecutor(_mockFactory.Object);
        var cts = new CancellationTokenSource();
        var command = new CommandDefinition("SELECT * FROM test WHERE id = @Id", cancellationToken: cts.Token);

        // Act
        try
        {
            await executor.QuerySingleOrDefaultAsync<TestEntity>(command);
        }
        catch
        {
            // Expected - we can't mock Dapper extension methods
        }

        // Assert
        _mockFactory.Verify(f => f.CreateConnectionAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task QuerySingleOrDefaultAsync_DisposesConnectionAfterExecution()
    {
        // Arrange
        var mockDisposableConnection = new Mock<IDbConnection>();
        mockDisposableConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDisposableConnection.Object);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("SELECT * FROM test WHERE id = @Id");

        // Act
        try
        {
            await executor.QuerySingleOrDefaultAsync<TestEntity>(command);
        }
        catch
        {
            // Expected - Dapper extension methods can't be mocked
        }

        // Assert - Connection should be disposed
        mockDisposableConnection.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public async Task QuerySingleOrDefaultAsync_WhenFactoryThrows_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Connection failed");
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("SELECT * FROM test WHERE id = @Id");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            executor.QuerySingleOrDefaultAsync<TestEntity>(command));
        Assert.Equal("Connection failed", exception.Message);
    }

    [Fact]
    public async Task QuerySingleOrDefaultAsync_WithCancelledToken_PropagatesCancellation()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var executor = new DapperExecutor(_mockFactory.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var command = new CommandDefinition("SELECT * FROM test WHERE id = @Id", cancellationToken: cts.Token);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            executor.QuerySingleOrDefaultAsync<TestEntity>(command));
    }

    [Fact]
    public async Task ExecuteAsync_CreatesConnectionFromFactory()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("UPDATE test SET name = @Name");

        // Act
        try
        {
            await executor.ExecuteAsync(command);
        }
        catch
        {
            // Expected - we can't mock Dapper extension methods
        }

        // Assert
        _mockFactory.Verify(f => f.CreateConnectionAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToFactory()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = new DapperExecutor(_mockFactory.Object);
        var cts = new CancellationTokenSource();
        var command = new CommandDefinition("UPDATE test SET name = @Name", cancellationToken: cts.Token);

        // Act
        try
        {
            await executor.ExecuteAsync(command);
        }
        catch
        {
            // Expected - we can't mock Dapper extension methods
        }

        // Assert
        _mockFactory.Verify(f => f.CreateConnectionAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DisposesConnectionAfterExecution()
    {
        // Arrange
        var mockDisposableConnection = new Mock<IDbConnection>();
        mockDisposableConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDisposableConnection.Object);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("UPDATE test SET name = @Name");

        // Act
        try
        {
            await executor.ExecuteAsync(command);
        }
        catch
        {
            // Expected - Dapper extension methods can't be mocked
        }

        // Assert - Connection should be disposed
        mockDisposableConnection.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DisposesConnectionEvenOnException()
    {
        // Arrange
        var mockDisposableConnection = new Mock<IDbConnection>();
        mockDisposableConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDisposableConnection.Object);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("INVALID SQL");

        // Act
        try
        {
            await executor.ExecuteAsync(command);
        }
        catch
        {
            // Expected - invalid SQL or mocking limitations
        }

        // Assert - Connection should still be disposed
        mockDisposableConnection.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFactoryThrows_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Connection failed");
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("DELETE FROM test WHERE id = @Id");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            executor.ExecuteAsync(command));
        Assert.Equal("Connection failed", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancelledToken_PropagatesCancellation()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var executor = new DapperExecutor(_mockFactory.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var command = new CommandDefinition("UPDATE test SET name = @Name", cancellationToken: cts.Token);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            executor.ExecuteAsync(command));
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentCancellationTokens_PassesCorrectTokens()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var conn = new Mock<IDbConnection>();
                conn.Setup(c => c.State).Returns(ConnectionState.Open);
                return conn.Object;
            });

        var executor = new DapperExecutor(_mockFactory.Object);
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var command1 = new CommandDefinition("UPDATE test SET name = @Name1", cancellationToken: cts1.Token);
        var command2 = new CommandDefinition("UPDATE test SET name = @Name2", cancellationToken: cts2.Token);

        // Act
        try { await executor.ExecuteAsync(command1); } catch { }
        try { await executor.ExecuteAsync(command2); } catch { }

        // Assert
        _mockFactory.Verify(f => f.CreateConnectionAsync(cts1.Token), Times.Once);
        _mockFactory.Verify(f => f.CreateConnectionAsync(cts2.Token), Times.Once);
    }

    [Fact]
    public async Task ExecuteScalarAsync_CreatesConnectionFromFactory()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("INSERT INTO test (name) VALUES (@Name) RETURNING id");

        // Act
        try
        {
            await executor.ExecuteScalarAsync<int>(command);
        }
        catch
        {
            // Expected - we can't mock Dapper extension methods
        }

        // Assert
        _mockFactory.Verify(f => f.CreateConnectionAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecuteScalarAsync_PassesCancellationTokenToFactory()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = new DapperExecutor(_mockFactory.Object);
        var cts = new CancellationTokenSource();
        var command = new CommandDefinition("INSERT INTO test (name) VALUES (@Name) RETURNING id", cancellationToken: cts.Token);

        // Act
        try
        {
            await executor.ExecuteScalarAsync<int>(command);
        }
        catch
        {
            // Expected - we can't mock Dapper extension methods
        }

        // Assert
        _mockFactory.Verify(f => f.CreateConnectionAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task ExecuteScalarAsync_DisposesConnectionAfterExecution()
    {
        // Arrange
        var mockDisposableConnection = new Mock<IDbConnection>();
        mockDisposableConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDisposableConnection.Object);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("INSERT INTO test (name) VALUES (@Name) RETURNING id");

        // Act
        try
        {
            await executor.ExecuteScalarAsync<int>(command);
        }
        catch
        {
            // Expected - Dapper extension methods can't be mocked
        }

        // Assert - Connection should be disposed
        mockDisposableConnection.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public async Task ExecuteScalarAsync_CreatesNewConnectionForEachCall()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var conn = new Mock<IDbConnection>();
                conn.Setup(c => c.State).Returns(ConnectionState.Open);
                return conn.Object;
            });

        var executor = new DapperExecutor(_mockFactory.Object);
        var command1 = new CommandDefinition("INSERT INTO test (name) VALUES (@Name1) RETURNING id");
        var command2 = new CommandDefinition("INSERT INTO test (name) VALUES (@Name2) RETURNING id");

        // Act
        try { await executor.ExecuteScalarAsync<int>(command1); } catch { }
        try { await executor.ExecuteScalarAsync<int>(command2); } catch { }

        // Assert - Factory should be called twice (one connection per operation)
        _mockFactory.Verify(f => f.CreateConnectionAsync(default), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteScalarAsync_WhenFactoryThrows_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Connection failed");
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var executor = new DapperExecutor(_mockFactory.Object);
        var command = new CommandDefinition("INSERT INTO test (name) VALUES (@Name) RETURNING id");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            executor.ExecuteScalarAsync<int>(command));
        Assert.Equal("Connection failed", exception.Message);
    }

    [Fact]
    public async Task ExecuteScalarAsync_WithCancelledToken_PropagatesCancellation()
    {
        // Arrange
        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var executor = new DapperExecutor(_mockFactory.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var command = new CommandDefinition("INSERT INTO test VALUES (@Name) RETURNING id", cancellationToken: cts.Token);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            executor.ExecuteScalarAsync<int>(command));
    }

    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
