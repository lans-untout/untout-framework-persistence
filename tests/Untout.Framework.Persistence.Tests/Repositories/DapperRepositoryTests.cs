namespace Untout.Framework.Persistence.Tests.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Moq;
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql;
using Xunit;

public class DapperRepositoryTests
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Mock<IDbConnectionFactory> _mockFactory;
    private readonly Mock<ISqlQueryBuilder<int, TestEntity>> _mockQueryBuilder;
    private readonly Mock<IDbNameAdapter> _mockNameAdapter;
    private readonly TestRepository _repository;

    public DapperRepositoryTests()
    {
        _mockConnection = new Mock<IDbConnection>();
        _mockFactory = new Mock<IDbConnectionFactory>();
        _mockQueryBuilder = new Mock<ISqlQueryBuilder<int, TestEntity>>();
        _mockNameAdapter = new Mock<IDbNameAdapter>();

        _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConnection.Object);

        _repository = new TestRepository(_mockFactory.Object, _mockQueryBuilder.Object, _mockNameAdapter.Object);
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

        // Arrange DapperExecutor mock to avoid mocking extension methods on IDbConnection
        var mockDapper = new Mock<IDapperExecutor>();
        mockDapper.Setup(d => d.QueryAsync<TestEntity>(_mockConnection.Object, expectedSql, null))
            .ReturnsAsync(expectedEntities);

        // Recreate repository with DapperExecutor mock
        var repository = new TestRepository(_mockFactory.Object, _mockQueryBuilder.Object, _mockNameAdapter.Object, mockDapper.Object);

        // Act
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
        _mockQueryBuilder.Setup(b => b.BuildSelectById())
            .Returns(expectedSql);

        var expectedEntity = new TestEntity { Id = entityId, Name = "Test42" };

        var mockDapper = new Mock<IDapperExecutor>();
        mockDapper.Setup(d => d.QuerySingleOrDefaultAsync<TestEntity>(_mockConnection.Object, expectedSql, It.IsAny<object>()))
            .ReturnsAsync(expectedEntity);

        var repository = new TestRepository(_mockFactory.Object, _mockQueryBuilder.Object, _mockNameAdapter.Object, mockDapper.Object);

        // Act
        var result = await repository.GetByIdAsync(entityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entityId, result!.Id);
        Assert.Equal("Test42", result.Name);
        _mockQueryBuilder.Verify(b => b.BuildSelectById(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_CallsQueryBuilder_AndReturnsInsertedId()
    {
        // Arrange
        var entity = new TestEntity { Id = 0, Name = "NewEntity" };
        var expectedId = 123;
        var expectedSql = "INSERT INTO test_entities (name) VALUES (@Name) RETURNING id";

        _mockQueryBuilder.Setup(b => b.BuildInsert(It.IsAny<IEnumerable<string>>()))
            .Returns(expectedSql);

        var mockDapper = new Mock<IDapperExecutor>();
        mockDapper.Setup(d => d.ExecuteScalarAsync<int>(_mockConnection.Object, expectedSql, It.IsAny<object>()))
            .ReturnsAsync(expectedId);

        var repository = new TestRepository(_mockFactory.Object, _mockQueryBuilder.Object, _mockNameAdapter.Object, mockDapper.Object);

        var result = await repository.AddAsync(entity);

        Assert.Equal(expectedId, result.Id);
        _mockQueryBuilder.Verify(b => b.BuildInsert(It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsQueryBuilder_AndReturnsSuccess()
    {
        // Arrange
        var entity = new TestEntity { Id = 5, Name = "UpdatedEntity" };
        var expectedSql = "UPDATE test_entities SET name = @Name WHERE id = @Id";

        _mockQueryBuilder.Setup(b => b.BuildUpdate(It.IsAny<IEnumerable<string>>()))
            .Returns(expectedSql);

        var mockDapper = new Mock<IDapperExecutor>();
        mockDapper.Setup(d => d.ExecuteAsync(_mockConnection.Object, expectedSql, It.IsAny<object>()))
            .ReturnsAsync(1);

        var repository = new TestRepository(_mockFactory.Object, _mockQueryBuilder.Object, _mockNameAdapter.Object, mockDapper.Object);

        var result = await repository.UpdateAsync(entity);

        Assert.True(result);
        _mockQueryBuilder.Verify(b => b.BuildUpdate(It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsQueryBuilder_AndReturnsSuccess()
    {
        // Arrange
        var entityId = 77;
        var expectedSql = "DELETE FROM test_entities WHERE id = @Id";

        _mockQueryBuilder.Setup(b => b.BuildDelete())
            .Returns(expectedSql);

        var mockDapper = new Mock<IDapperExecutor>();
        mockDapper.Setup(d => d.ExecuteAsync(_mockConnection.Object, expectedSql, It.IsAny<object>()))
            .ReturnsAsync(1);

        var repository = new TestRepository(_mockFactory.Object, _mockQueryBuilder.Object, _mockNameAdapter.Object, mockDapper.Object);

        var result = await repository.DeleteAsync(entityId);

        Assert.True(result);
        _mockQueryBuilder.Verify(b => b.BuildDelete(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ReturnsZero_WhenInsertFails()
    {
        // Arrange
        var entity = new TestEntity { Id = 0, Name = "FailEntity" };
        var expectedSql = "INSERT INTO test_entities (name) VALUES (@Name) RETURNING id";

        _mockQueryBuilder.Setup(b => b.BuildInsert(It.IsAny<IEnumerable<string>>()))
            .Returns(expectedSql);

        var mockDapper = new Mock<IDapperExecutor>();
        mockDapper.Setup(d => d.ExecuteScalarAsync<int>(_mockConnection.Object, expectedSql, It.IsAny<object>()))
            .ReturnsAsync(0);

        var repository = new TestRepository(_mockFactory.Object, _mockQueryBuilder.Object, _mockNameAdapter.Object, mockDapper.Object);

        var result = await repository.AddAsync(entity);

        Assert.Equal(0, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenNoRowsAffected()
    {
        // Arrange
        var entity = new TestEntity { Id = 999, Name = "NonExistentEntity" };
        var expectedSql = "UPDATE test_entities SET name = @Name WHERE id = @Id";

        _mockQueryBuilder.Setup(b => b.BuildUpdate(It.IsAny<IEnumerable<string>>()))
            .Returns(expectedSql);

        var mockDapper = new Mock<IDapperExecutor>();
        mockDapper.Setup(d => d.ExecuteAsync(_mockConnection.Object, expectedSql, It.IsAny<object>()))
            .ReturnsAsync(0);

        var repository = new TestRepository(_mockFactory.Object, _mockQueryBuilder.Object, _mockNameAdapter.Object, mockDapper.Object);

        var result = await repository.UpdateAsync(entity);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNoRowsAffected()
    {
        // Arrange
        var entityId = 999;
        var expectedSql = "DELETE FROM test_entities WHERE id = @Id";

        _mockQueryBuilder.Setup(b => b.BuildDelete())
            .Returns(expectedSql);

        var mockDapper = new Mock<IDapperExecutor>();
        mockDapper.Setup(d => d.ExecuteAsync(_mockConnection.Object, expectedSql, It.IsAny<object>()))
            .ReturnsAsync(0);

        var repository = new TestRepository(_mockFactory.Object, _mockQueryBuilder.Object, _mockNameAdapter.Object, mockDapper.Object);

        var result = await repository.DeleteAsync(entityId);

        Assert.False(result);
    }

    private static int GetIdFromDynamicParams(object parameters)
    {
        if (parameters is DynamicParameters dynamicParams)
        {
            try
            {
                return dynamicParams.Get<int>("Id");
            }
            catch (ArgumentException)
            {
                // Parameter might be named "@Id" instead of "Id"
                return dynamicParams.Get<int>("@Id");
            }
        }
        return 0;
    }

    // Test entity for repository tests
    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Test repository implementation
    public class TestRepository : DapperRepository<int, TestEntity>
    {
        public TestRepository(
            IDbConnectionFactory connectionFactory,
            ISqlQueryBuilder<int, TestEntity> queryBuilder,
            IDbNameAdapter nameAdapter,
            IDapperExecutor? dapper = null)
            : base(connectionFactory, queryBuilder, dapper)
        {
        }
    }
}
