namespace Untout.Framework.Persistence.Tests.QueryBuilders;

using Xunit;
using Untout.Framework.Persistence.PostgreSql;
using Untout.Framework.Persistence.PostgreSql.Adapters;
using Untout.Framework.Persistence.Interfaces;

public class PostgreSqlQueryBuilderTests
{
    private readonly SnakeCaseAdapter _adapter;
    private readonly PostgreSqlQueryBuilder<int, TestArticle> _builder;

    public PostgreSqlQueryBuilderTests()
    {
        _adapter = new SnakeCaseAdapter();
        _builder = new PostgreSqlQueryBuilder<int, TestArticle>(_adapter);
    }

    [Fact]
    public void BuildSelectAll_ReturnsCorrectQuery()
    {
        // Act
        var sql = _builder.BuildSelectAll();

        // Assert
        Assert.Equal("SELECT * FROM test_article", sql);
    }

    [Fact]
    public void BuildSelectById_ReturnsQueryWithIdParameter()
    {
        // Act
        var sql = _builder.BuildSelectById();

        // Assert
        Assert.Contains("SELECT * FROM test_article", sql);
        Assert.Contains("WHERE id = @Id", sql);
    }

    [Fact]
    public void BuildInsert_IncludesReturningClause()
    {
        // Arrange
        var columns = new[] { "Title", "Content", "CreatedAt" };

        // Act
        var sql = _builder.BuildInsert(columns);

        // Assert
        Assert.Contains("INSERT INTO test_article", sql);
        Assert.Contains("(title, content, created_at)", sql);
        Assert.Contains("VALUES (@Title, @Content, @CreatedAt)", sql);
        Assert.Contains("RETURNING id", sql);
    }

    [Fact]
    public void BuildInsert_ThrowsOnEmptyColumns()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _builder.BuildInsert(Array.Empty<string>()));
    }

    [Fact]
    public void BuildUpdate_IncludesSetClauseAndWhereId()
    {
        // Arrange
        var columns = new[] { "Title", "Content" };

        // Act
        var sql = _builder.BuildUpdate(columns);

        // Assert
        Assert.Contains("UPDATE test_article", sql);
        Assert.Contains("SET title = @Title, content = @Content", sql);
        Assert.Contains("WHERE id = @Id", sql);
    }

    [Fact]
    public void BuildUpdate_ThrowsOnEmptyColumns()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _builder.BuildUpdate(Array.Empty<string>()));
    }

    [Fact]
    public void BuildDelete_ReturnsQueryWithIdParameter()
    {
        // Act
        var sql = _builder.BuildDelete();

        // Assert
        Assert.Equal("DELETE FROM test_article WHERE id = @Id", sql);
    }

    private class TestArticle : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
