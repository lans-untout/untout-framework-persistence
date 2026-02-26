
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Untout.Framework.Persistence;
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql;
using Xunit;

namespace Untout.Framework.Persistence.Tests.Repositories;
public class DapperRepositoryWithLoggingTests
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Mock<IDbConnectionFactory> _mockFactory;
    private readonly Mock<ISqlQueryBuilder<int, TestEntity>> _mockQueryBuilder;
    private readonly Mock<IDapperExecutor> _mockDapperExecutor;
    private readonly Mock<IPersistenceLogger> _mockLogger;

    public DapperRepositoryWithLoggingTests()
    {
        _mockConnection = new Mock<IDbConnection>();
        _mockFactory = new Mock<IDbConnectionFactory>();
        _mockQueryBuilder = new Mock<ISqlQueryBuilder<int, TestEntity>>();
        _mockDapperExecutor = new Mock<IDapperExecutor>();
        _mockLogger = new Mock<IPersistenceLogger>();

        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);
    }

    [Fact]
    public async Task GetAllAsync_LogsQueryAndResult()
    {
        // Arrange
        var expectedSql = "SELECT * FROM test_entities";
        _mockQueryBuilder.Setup(b => b.BuildSelectAll())
            .Returns(expectedSql);

        var expectedEntities = new List<TestEntity>
        {
            new() { Id = 1, Name = "Test1" },
            new() { Id = 2, Name = "Test2" }
        };

        _mockDapperExecutor.Setup(d => d.QueryAsync<TestEntity>(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(expectedEntities);

        var repository = CreateRepository();

        // Act
        await repository.GetAllAsync();

        // Assert
        _mockLogger.Verify(l => l.LogQuery(expectedSql, null), Times.Once);
        _mockLogger.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("GetAllAsync") && s.Contains("2 rows"))), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_LogsQueryWithParametersAndResult()
    {
        // Arrange
        var entityId = 42;
        var expectedSql = "SELECT * FROM test_entities WHERE id = @Id";
        _mockQueryBuilder.Setup(b => b.BuildSelectById(entityId))
            .Returns((expectedSql, new DynamicParameters(new { Id = entityId })));

        var expectedEntity = new TestEntity { Id = entityId, Name = "Test42" };

        _mockDapperExecutor.Setup(d => d.QuerySingleOrDefaultAsync<TestEntity>(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(expectedEntity);

        var repository = CreateRepository();

        // Act
        await repository.GetByIdAsync(entityId);

        // Assert
        _mockLogger.Verify(l => l.LogQuery(expectedSql, It.IsAny<object>()), Times.Once);
        _mockLogger.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("GetByIdAsync") && s.Contains("42") && s.Contains("1 row"))), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_LogsNullResult()
    {
        // Arrange
        var entityId = 999;
        var expectedSql = "SELECT * FROM test_entities WHERE id = @Id";
        _mockQueryBuilder.Setup(b => b.BuildSelectById(entityId))
            .Returns((expectedSql, new DynamicParameters(new { Id = entityId })));

        _mockDapperExecutor.Setup(d => d.QuerySingleOrDefaultAsync<TestEntity>(It.IsAny<CommandDefinition>()))
            .ReturnsAsync((TestEntity)null);

        var repository = CreateRepository();

        // Act
        await repository.GetByIdAsync(entityId);

        // Assert
        _mockLogger.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("null"))), Times.Once);
    }

    [Fact]
    public async Task AddAsync_LogsQueryAndInsertedId()
    {
        // Arrange
        var entity = new TestEntity { Id = 0, Name = "NewEntity" };
        var expectedId = 123;
        var expectedSql = "INSERT INTO test_entities (name) VALUES (@Name) RETURNING id";

        _mockQueryBuilder.Setup(b => b.BuildInsert(It.IsAny<TestEntity>()))
            .Returns((expectedSql, new DynamicParameters(new { entity.Name })));

        _mockDapperExecutor.Setup(d => d.ExecuteScalarAsync<int>(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(expectedId);

        var repository = CreateRepository();

        // Act
        await repository.AddAsync(entity);

        // Assert
        _mockLogger.Verify(l => l.LogQuery(expectedSql, It.IsAny<object>()), Times.Once);
        _mockLogger.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("AddAsync") && s.Contains("123"))), Times.Once);
    }

    [Fact]
    public async Task AddAsync_LogsWarningWhenNoIdReturned()
    {
        // Arrange
        var entity = new TestEntity { Id = 0, Name = "FailEntity" };
        var expectedSql = "INSERT INTO test_entities (name) VALUES (@Name) RETURNING id";

        _mockQueryBuilder.Setup(b => b.BuildInsert(It.IsAny<TestEntity>()))
            .Returns((expectedSql, new DynamicParameters(new { entity.Name })));

        _mockDapperExecutor.Setup(d => d.ExecuteScalarAsync<int>(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(default(int));

        var repository = CreateRepository();

        // Act
        await repository.AddAsync(entity);

        // Assert
        _mockLogger.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("AddAsync") && s.Contains("did not return"))), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_LogsQueryAndAffectedRows()
    {
        // Arrange
        var entity = new TestEntity { Id = 5, Name = "UpdatedEntity" };
        var expectedSql = "UPDATE test_entities SET name = @Name WHERE id = @Id";

        _mockQueryBuilder.Setup(b => b.BuildUpdate(It.IsAny<TestEntity>()))
            .Returns((expectedSql, new DynamicParameters(new { entity.Id, entity.Name })));

        _mockDapperExecutor.Setup(d => d.ExecuteAsync(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(1);

        var repository = CreateRepository();

        // Act
        await repository.UpdateAsync(entity);

        // Assert
        _mockLogger.Verify(l => l.LogQuery(expectedSql, It.IsAny<object>()), Times.Once);
        _mockLogger.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("UpdateAsync") && s.Contains("1 rows"))), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_LogsQueryAndAffectedRows()
    {
        // Arrange
        var entityId = 77;
        var expectedSql = "DELETE FROM test_entities WHERE id = @Id";

        _mockQueryBuilder.Setup(b => b.BuildDelete(entityId))
            .Returns((expectedSql, new DynamicParameters(new { Id = entityId })));

        _mockDapperExecutor.Setup(d => d.ExecuteAsync(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(1);

        var repository = CreateRepository();

        // Act
        await repository.DeleteAsync(entityId);

        // Assert
        _mockLogger.Verify(l => l.LogQuery(expectedSql, It.IsAny<object>()), Times.Once);
        _mockLogger.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("DeleteAsync") && s.Contains("77") && s.Contains("1 rows"))), Times.Once);
    }

    [Fact]
    public void Constructor_WithNullLogger_UsesNullLogger()
    {
        // Arrange & Act
        var repository = new DapperRepository<int, TestEntity>(
            _mockQueryBuilder.Object,
            _mockDapperExecutor.Object
        );

        // Assert - should not throw, NullLogger is used by default
        Assert.NotNull(repository);
    }

    [Fact]
    public async Task Repository_WithNullLogger_DoesNotThrow()
    {
        // Arrange
        var expectedSql = "SELECT * FROM test_entities";
        _mockQueryBuilder.Setup(b => b.BuildSelectAll())
            .Returns(expectedSql);

        _mockDapperExecutor.Setup(d => d.QueryAsync<TestEntity>(It.IsAny<CommandDefinition>()))
            .ReturnsAsync([]);

        var repository = new DapperRepository<int, TestEntity>(
            _mockQueryBuilder.Object,
            _mockDapperExecutor.Object
        );

        // Act & Assert - should not throw
        await repository.GetAllAsync();
    }

    private IRepository<int, TestEntity> CreateRepository()
        => new DapperRepository<int, TestEntity>(
            _mockQueryBuilder.Object,
            _mockDapperExecutor.Object,
            _mockLogger.Object);

    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
