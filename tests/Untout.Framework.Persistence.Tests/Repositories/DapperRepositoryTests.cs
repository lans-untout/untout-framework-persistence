using Moq;
using Xunit;
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql;
using System.Data;

namespace Untout.Framework.Persistence.Tests.Repositories;

public class DapperRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _mockConnectionFactory;
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Mock<ISqlQueryBuilder<int, TestEntity>> _mockQueryBuilder;
    private readonly TestRepository _repository;

    public DapperRepositoryTests()
    {
        _mockConnectionFactory = new Mock<IDbConnectionFactory>();
        _mockConnection = new Mock<IDbConnection>();
        _mockQueryBuilder = new Mock<ISqlQueryBuilder<int, TestEntity>>();

        _mockConnectionFactory
            .Setup(f => f.CreateConnectionAsync(default))
            .ReturnsAsync(_mockConnection.Object);

        _repository = new TestRepository(_mockConnectionFactory.Object, _mockQueryBuilder.Object);
    }

    [Fact]
    public async Task GetAllAsync_CallsQueryBuilderAndConnection()
    {
        // Arrange
        _mockQueryBuilder.Setup(b => b.BuildSelectAll()).Returns("SELECT * FROM test_entity");

        // Act
        await _repository.GetAllAsync();

        // Assert
        _mockQueryBuilder.Verify(b => b.BuildSelectAll(), Times.Once);
        _mockConnectionFactory.Verify(f => f.CreateConnectionAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_BuildsCorrectQuery()
    {
        // Arrange
        var entityId = 42;
        _mockQueryBuilder.Setup(b => b.BuildSelectById()).Returns("SELECT * FROM test_entity WHERE id = @Id");

        // Act
        await _repository.GetByIdAsync(entityId);

        // Assert
        _mockQueryBuilder.Verify(b => b.BuildSelectById(), Times.Once);
        _mockConnectionFactory.Verify(f => f.CreateConnectionAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentNullException_WhenEntityIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateAsync(null!));
    }

    [Fact]
    public async Task AddAsync_CallsInsertBuilder()
    {
        // Arrange
        var entity = new TestEntity { Id = 0, Name = "Test" };
        var columns = new[] { "Name" };
        _mockQueryBuilder.Setup(b => b.BuildInsert(It.IsAny<IEnumerable<string>>()))
            .Returns("INSERT INTO test_entity (name) VALUES (@Name) RETURNING id");

        // Act
        await _repository.AddAsync(entity);

        // Assert
        _mockQueryBuilder.Verify(b => b.BuildInsert(It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsUpdateBuilder()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated" };
        _mockQueryBuilder.Setup(b => b.BuildUpdate(It.IsAny<IEnumerable<string>>()))
            .Returns("UPDATE test_entity SET name = @Name WHERE id = @Id");

        // Act
        await _repository.UpdateAsync(entity);

        // Assert
        _mockQueryBuilder.Verify(b => b.BuildUpdate(It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteBuilder()
    {
        // Arrange
        var entityId = 5;
        _mockQueryBuilder.Setup(b => b.BuildDelete()).Returns("DELETE FROM test_entity WHERE id = @Id");

        // Act
        await _repository.DeleteAsync(entityId);

        // Assert
        _mockQueryBuilder.Verify(b => b.BuildDelete(), Times.Once);
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
            ISqlQueryBuilder<int, TestEntity> queryBuilder)
            : base(connectionFactory, queryBuilder)
        {
        }
    }
}
