using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Untout.Framework.Persistence.PostgreSql;
using Untout.Framework.Persistence.Interfaces;

namespace Untout.Framework.Persistence.Tests.PostgreSql;

public class TransactionScopeTests
{

    // Lightweight test doubles for DbConnection/DbTransaction to allow testing
    private class TestDbTransaction : DbTransaction
    {
        public bool CommitCalled { get; private set; }
        public bool RollbackCalled { get; private set; }

        public override void Commit() => CommitCalled = true;
        public override void Rollback() => RollbackCalled = true;
        public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
        protected override DbConnection DbConnection => null;
        public override Task CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCalled = true;
            return Task.CompletedTask;
        }
        public override Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            RollbackCalled = true;
            return Task.CompletedTask;
        }
    }

    private class TestDbConnection : DbConnection
    {
        private readonly TestDbTransaction _tx;
        public TestDbConnection(TestDbTransaction tx) => _tx = tx;

        public override string ConnectionString { get; set; }
        public override string Database => "TestDb";
        public override string DataSource => "Test";
        public override string ServerVersion => "1.0";
        public override ConnectionState State { get; } = ConnectionState.Open;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() { }
        public override void Open() { }
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => _tx;
        protected override DbCommand CreateDbCommand() => throw new NotSupportedException();
        
    }

    [Fact]
    public async Task NpgsqlTransactionScope_CommitAsync_CallsTransactionCommit_AndPreventsRollbackOnDispose()
    {
        // Arrange
        var testTx = new TestDbTransaction();
        var testConn = new TestDbConnection(testTx);

        var scope = new NpgsqlTransactionScope(testConn, testTx);

        // Act
        await scope.CommitAsync();
        await scope.DisposeAsync();

        // Assert
        Assert.True(testTx.CommitCalled);
        Assert.False(testTx.RollbackCalled);
    }

    [Fact]
    public async Task NpgsqlTransactionScope_DisposeAsync_RollsBackIfNotCommitted()
    {
        // Arrange
        var testTx = new TestDbTransaction();
        var testConn = new TestDbConnection(testTx);

        var scope = new NpgsqlTransactionScope(testConn, testTx);

        // Act
        await scope.DisposeAsync();

        // Assert
        Assert.True(testTx.RollbackCalled);
    }

    [Fact]
    public async Task TransactionScopeFactory_CreateAsync_ReturnsScope_WithConnectionAndTransaction()
    {
        // Arrange
        var testTx = new TestDbTransaction();
        var testConn = new TestDbConnection(testTx);

        var mockFactory = new Mock<IDbConnectionFactory>();
        mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(testConn);

        var factory = new TransactionScopeFactory(mockFactory.Object);

        // Act
        var scope = await factory.CreateAsync(IsolationLevel.ReadCommitted, CancellationToken.None);

        // Assert
        Assert.Same(testConn, scope.Connection);
        Assert.Same(testTx, scope.Transaction);
    }

    [Fact]
    public async Task DapperExecutor_ExecuteInTransactionAsync_CommitsOnSuccess()
    {
        // Arrange
        var mockScope = new Mock<IDbTransactionScope>();
        var mockConn = new Mock<IDbConnection>();
        var mockTx = new Mock<IDbTransaction>();

        mockScope.SetupGet(s => s.Connection).Returns(mockConn.Object);
        mockScope.SetupGet(s => s.Transaction).Returns(mockTx.Object);
        mockScope.Setup(s => s.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockScope.Setup(s => s.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mockScopeFactory = new Mock<ITransactionScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockScope.Object);

        var mockConnFactory = new Mock<IDbConnectionFactory>();

        var executor = new DapperExecutor(mockConnFactory.Object, mockScopeFactory.Object);

        // Act
        await executor.ExecuteInTransactionAsync(async (conn, tx) =>
        {
            // perform no DB work in unit test
            await Task.CompletedTask;
        });

        // Assert
        mockScope.Verify(s => s.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockScope.Verify(s => s.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DapperExecutor_ExecuteInTransactionAsync_RollsBackOnException()
    {
        // Arrange
        var mockScope = new Mock<IDbTransactionScope>();
        var mockConn = new Mock<IDbConnection>();
        var mockTx = new Mock<IDbTransaction>();

        mockScope.SetupGet(s => s.Connection).Returns(mockConn.Object);
        mockScope.SetupGet(s => s.Transaction).Returns(mockTx.Object);
        mockScope.Setup(s => s.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockScope.Setup(s => s.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mockScopeFactory = new Mock<ITransactionScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockScope.Object);

        var mockConnFactory = new Mock<IDbConnectionFactory>();

        var executor = new DapperExecutor(mockConnFactory.Object, mockScopeFactory.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await executor.ExecuteInTransactionAsync((conn, tx) => throw new InvalidOperationException("boom")));

        mockScope.Verify(s => s.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockScope.Verify(s => s.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
