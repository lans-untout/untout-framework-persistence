
using Dapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql;
using Xunit;

namespace Untout.Framework.Persistence.Tests.Repositories;
public class DapperRepositoryTests
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Mock<IDbConnectionFactory> _mockFactory;
    private readonly Mock<ISqlQueryBuilder<int, TestEntity>> _mockQueryBuilder;
    private readonly Mock<IDapperExecutor> _mockDapperExecutor;

    public DapperRepositoryTests()
    {
        _mockConnection = new Mock<IDbConnection>();
        _mockFactory = new Mock<IDbConnectionFactory>();
        _mockQueryBuilder = new Mock<ISqlQueryBuilder<int, TestEntity>>();

        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);

        _mockDapperExecutor = new Mock<IDapperExecutor>();

    }

    [Fact]
    public async Task GetAllAsync_CallsQueryBuilder_AndReturnsEntities()
    {
        // Arrange
        var expectedSql = "SELECT * FROM test_entities";
        _mockQueryBuilder.Setup(b => b.BuildSelectAll())
            .Returns(expectedSql);

        var expectedEntities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1" },
            new TestEntity { Id = 2, Name = "Test2" }
        };

        _mockDapperExecutor.Setup(d => d.QueryAsync<TestEntity>(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(expectedEntities);

        // Act
        var repository = CreateRepository();
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal(expectedEntities[0].Name, result.First().Name);
        _mockQueryBuilder.Verify(b => b.BuildSelectAll(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_CallsQueryBuilder_AndReturnsEntity()
    {
        // Arrange
        var entityId = 42;
        var expectedSql = "SELECT * FROM test_entities WHERE id = @Id";
        _mockQueryBuilder.Setup(b => b.BuildSelectById(entityId))
            .Returns((expectedSql, new DynamicParameters(new { Id = entityId })));

        var expectedEntity = new TestEntity { Id = entityId, Name = "Test42" };
        _mockDapperExecutor.Setup(d => d.QuerySingleOrDefaultAsync<TestEntity>(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(expectedEntity);

        // Act
        var repository = CreateRepository();
        var result = await repository.GetByIdAsync(entityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entityId, result!.Id);
        Assert.Equal("Test42", result.Name);
        _mockQueryBuilder.Verify(b => b.BuildSelectById(entityId), Times.Once);
    }

    [Fact]
    public async Task AddAsync_CallsQueryBuilder_AndReturnsInsertedId()
    {
        // Arrange
        var entity = new TestEntity { Id = 0, Name = "NewEntity" };
        var expectedId = 123;
        var expectedSql = "INSERT INTO test_entities (name) VALUES (@Name) RETURNING id";

        _mockQueryBuilder.Setup(b => b.BuildInsert(It.IsAny<TestEntity>()))
            .Returns((expectedSql, new DynamicParameters(new { entity.Name })));

        _mockDapperExecutor.Setup(d => d.QuerySingleOrDefaultAsync<TestEntity>(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(new TestEntity { Id = expectedId, Name = entity.Name });

        var repository = CreateRepository();
        var result = await repository.AddAsync(entity);

        Assert.Equal(expectedId, result.Id);
        _mockQueryBuilder.Verify(b => b.BuildInsert(It.IsAny<TestEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsQueryBuilder_AndReturnsSuccess()
    {
        // Arrange
        var entity = new TestEntity { Id = 5, Name = "UpdatedEntity" };
        var expectedSql = "UPDATE test_entities SET name = @Name WHERE id = @Id";

        _mockQueryBuilder.Setup(b => b.BuildUpdate(It.IsAny<TestEntity>()))
            .Returns((expectedSql, new DynamicParameters(new { entity.Id, entity.Name })));

        _mockDapperExecutor.Setup(d => d.ExecuteAsync(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(1);

        var repository = CreateRepository();
        var result = await repository.UpdateAsync(entity);

        Assert.True(result);
        _mockQueryBuilder.Verify(b => b.BuildUpdate(It.IsAny<TestEntity>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsQueryBuilder_AndReturnsSuccess()
    {
        // Arrange
        var entityId = 77;
        var expectedSql = "DELETE FROM test_entities WHERE id = @Id";
        _mockQueryBuilder.Setup(b => b.BuildDelete(entityId))
            .Returns((expectedSql, new DynamicParameters(new { Id = entityId })));

        _mockDapperExecutor.Setup(d => d.ExecuteAsync(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(1);

        var repository = CreateRepository();
        var result = await repository.DeleteAsync(entityId);

        Assert.True(result);
        _mockQueryBuilder.Verify(b => b.BuildDelete(entityId), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ReturnsZero_WhenInsertFails()
    {
        // Arrange
        var entity = new TestEntity { Id = 0, Name = "FailEntity" };
        var expectedSql = "INSERT INTO test_entities (name) VALUES (@Name) RETURNING id";

        _mockQueryBuilder.Setup(b => b.BuildInsert(It.IsAny<TestEntity>()))
            .Returns((expectedSql, new DynamicParameters(new { entity.Name })));

        _mockDapperExecutor.Setup(d => d.ExecuteScalarAsync<int>(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(0);

        var repository = CreateRepository();
        var result = await repository.AddAsync(entity);

        Assert.Equal(0, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenNoRowsAffected()
    {
        // Arrange
        var entity = new TestEntity { Id = 999, Name = "NonExistentEntity" };
        var expectedSql = "UPDATE test_entities SET name = @Name WHERE id = @Id";

        _mockQueryBuilder.Setup(b => b.BuildUpdate(It.IsAny<TestEntity>()))
            .Returns((expectedSql, new DynamicParameters(new { entity.Id, entity.Name })));

        _mockDapperExecutor.Setup(d => d.ExecuteAsync(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(0);

        var repository = CreateRepository();
        var result = await repository.UpdateAsync(entity);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNoRowsAffected()
    {
        // Arrange
        var entityId = 999;
        var expectedSql = "DELETE FROM test_entities WHERE id = @Id";

        _mockQueryBuilder.Setup(b => b.BuildDelete(entityId))
            .Returns((expectedSql, new DynamicParameters(new { Id = entityId })));

        _mockDapperExecutor.Setup(d => d.ExecuteAsync(It.IsAny<CommandDefinition>()))
            .ReturnsAsync(0);

        var repository = CreateRepository();
        var result = await repository.DeleteAsync(entityId);

        Assert.False(result);
    }

    private IRepository<int, TestEntity> CreateRepository()
        => new DapperRepository<int, TestEntity>(
        _mockQueryBuilder.Object,
        _mockDapperExecutor.Object);

    // Test entity for repository tests
    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
