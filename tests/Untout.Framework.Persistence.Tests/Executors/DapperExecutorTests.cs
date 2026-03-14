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
using System.Data.Common;

public class DapperExecutorTests
{
    private readonly IDbConnection _mockConnection;
    private IDbConnectionFactory _dbConnectionFactory;
    private ITransactionScopeFactory _transactionScopeFactory;

    public DapperExecutorTests()
    {
        _dbConnectionFactory = Mock.Of<IDbConnectionFactory>();
        _mockConnection = Mock.Of<IDbConnection>();
        _transactionScopeFactory = Mock.Of<ITransactionScopeFactory>();
    }

    [Fact]
    public void Constructor_WithNullDbConnectionFactory_ThrowsArgumentNullException()
    {
        _dbConnectionFactory = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => GetExecutor());
    }

    [Fact]
    public void Constructor_WithNullTransactionScopeFactory_ThrowsArgumentNullException()
    {
        _transactionScopeFactory = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => GetExecutor());
    }

    [Fact]
    public void Constructor_WithValidFactory_Succeeds()
    {

        // Act
        var executor = GetExecutor();

        // Assert
        Assert.NotNull(executor);
    }

    [Fact]
    public async Task QueryAsync_CreatesConnectionFromFactory()
    {
        // Arrange
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection);
        Mock.Get(_mockConnection).Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(default), Times.Once);
    }

    [Fact]
    public async Task QueryAsync_PassesCancellationTokenToFactory()
    {
        // Arrange
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection);
        Mock.Get(_mockConnection).Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task QueryAsync_DisposesConnectionAfterExecution()
    {
        // Arrange
        var mockDisposableConnection = new Mock<IDbConnection>();
        mockDisposableConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDisposableConnection.Object);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var conn = new Mock<IDbConnection>();
                conn.Setup(c => c.State).Returns(ConnectionState.Open);
                return conn.Object;
            });

        var executor = GetExecutor();
        var command1 = new CommandDefinition("SELECT * FROM test1");
        var command2 = new CommandDefinition("SELECT * FROM test2");

        // Act
        try { await executor.QueryAsync<TestEntity>(command1); } catch { }
        try { await executor.QueryAsync<TestEntity>(command2); } catch { }

        // Assert - Factory should be called twice (one connection per operation)
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(default), Times.Exactly(2));
    }

    [Fact]
    public async Task QueryAsync_WhenFactoryThrows_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Connection failed");
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection);
        Mock.Get(_mockConnection).Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(default), Times.Once);
    }

    [Fact]
    public async Task QuerySingleOrDefaultAsync_PassesCancellationTokenToFactory()
    {
        // Arrange
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection);
        Mock.Get(_mockConnection).Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task QuerySingleOrDefaultAsync_DisposesConnectionAfterExecution()
    {
        // Arrange
        var mockDisposableConnection = new Mock<IDbConnection>();
        mockDisposableConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDisposableConnection.Object);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection);
        Mock.Get(_mockConnection).Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToFactory()
    {
        // Arrange
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection);
        Mock.Get(_mockConnection).Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DisposesConnectionAfterExecution()
    {
        // Arrange
        var mockDisposableConnection = new Mock<IDbConnection>();
        mockDisposableConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDisposableConnection.Object);

        var executor = GetExecutor();
        var command = new CommandDefinition("UPDATE test SET name = @Name", new DynamicParameters());

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

        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDisposableConnection.Object);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var conn = new Mock<IDbConnection>();
                conn.Setup(c => c.State).Returns(ConnectionState.Open);
                return conn.Object;
            });

        var executor = GetExecutor();
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var command1 = new CommandDefinition("UPDATE test SET name = @Name1", cancellationToken: cts1.Token);
        var command2 = new CommandDefinition("UPDATE test SET name = @Name2", cancellationToken: cts2.Token);

        // Act
        try { await executor.ExecuteAsync(command1); } catch { }
        try { await executor.ExecuteAsync(command2); } catch { }

        // Assert
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(cts1.Token), Times.Once);
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(cts2.Token), Times.Once);
    }

    [Fact]
    public async Task ExecuteScalarAsync_CreatesConnectionFromFactory()
    {
        // Arrange
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection);
        Mock.Get(_mockConnection).Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecuteScalarAsync_PassesCancellationTokenToFactory()
    {
        // Arrange
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection);
        Mock.Get(_mockConnection).Setup(c => c.State).Returns(ConnectionState.Open);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task ExecuteScalarAsync_DisposesConnectionAfterExecution()
    {
        // Arrange
        var mockDisposableConnection = new Mock<IDbConnection>();
        mockDisposableConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDisposableConnection.Object);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var conn = new Mock<IDbConnection>();
                conn.Setup(c => c.State).Returns(ConnectionState.Open);
                return conn.Object;
            });

        var executor = GetExecutor();
        var command1 = new CommandDefinition("INSERT INTO test (name) VALUES (@Name1) RETURNING id");
        var command2 = new CommandDefinition("INSERT INTO test (name) VALUES (@Name2) RETURNING id");

        // Act
        try { await executor.ExecuteScalarAsync<int>(command1); } catch { }
        try { await executor.ExecuteScalarAsync<int>(command2); } catch { }

        // Assert - Factory should be called twice (one connection per operation)
        Mock.Get(_dbConnectionFactory).Verify(f => f.CreateConnectionAsync(default), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteScalarAsync_WhenFactoryThrows_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Connection failed");
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var executor = GetExecutor();
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
        Mock.Get(_dbConnectionFactory).Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var executor = GetExecutor();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var command = new CommandDefinition("INSERT INTO test VALUES (@Name) RETURNING id", cancellationToken: cts.Token);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            executor.ExecuteScalarAsync<int>(command));
    }
    private DapperExecutor GetExecutor()
    {
        return new DapperExecutor(_dbConnectionFactory, _transactionScopeFactory);
    }

    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
