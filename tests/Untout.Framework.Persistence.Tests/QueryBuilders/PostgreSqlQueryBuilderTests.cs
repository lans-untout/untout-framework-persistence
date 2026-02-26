using System;
using System.Linq;
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql;
using Untout.Framework.Persistence.PostgreSql.Adapters;

namespace Untout.Framework.Persistence.Tests.QueryBuilders;

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
        Assert.Equal("SELECT id AS Id, title AS Title, content AS Content, created_at AS CreatedAt FROM test_article", sql);
    }

    [Fact]
    public void BuildSelectById_ReturnsQueryWithIdParameter()
    {
        // Act
        var (sql, parameters) = _builder.BuildSelectById(3);

        // Assert
        Assert.Equal("SELECT id AS Id, title AS Title, content AS Content, created_at AS CreatedAt FROM test_article WHERE id = @Id", sql);
        Assert.Contains("Id", parameters.ParameterNames);
        Assert.Equal(3, parameters.Get<int>("Id"));
    }

    [Fact]
    public void BuildInsert_IncludesReturningClause()
    {
        // Arrange
        var entity = new TestArticle
        {
            Title = "Test",
            Content = "Content",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var (sql, parameters) = _builder.BuildInsert(entity);

        // Assert
        Assert.Contains("INSERT INTO test_article", sql);
        Assert.Contains("(title, content, created_at)", sql);
        Assert.Contains("VALUES (@Title, @Content, @CreatedAt)", sql);
        Assert.Contains("RETURNING id", sql);
        Assert.Contains("Title", parameters.ParameterNames);
        Assert.Equal("Test", parameters.Get<string>("Title"));
        Assert.Contains("Content", parameters.ParameterNames);
        Assert.Equal("Content", parameters.Get<string>("Content"));
        Assert.Contains("CreatedAt", parameters.ParameterNames);
        Assert.Equal(entity.CreatedAt, parameters.Get<DateTime>("CreatedAt"));
    }

    [Fact]
    public void BuildInsert_ThrowsOnEmptyColumns()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.BuildInsert(null));
    }

    [Fact]
    public void BuildUpdate_IncludesSetClauseAndWhereId()
    {
        // Arrange
        var entity = new TestArticle
        {
            Id = 5,
            Title = "Updated",
            Content = "Updated content",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var (sql, parameters) = _builder.BuildUpdate(entity);

        // Assert
        Assert.Contains("UPDATE test_article", sql);
        Assert.Contains("SET title = @Title, content = @Content, created_at = @CreatedAt", sql);
        Assert.Contains("WHERE id = @Id", sql);
        Assert.Contains("Title", parameters.ParameterNames);
        Assert.Equal("Updated", parameters.Get<string>("Title"));
        Assert.Contains("Content", parameters.ParameterNames);
        Assert.Equal("Updated content", parameters.Get<string>("Content"));
        Assert.Contains("CreatedAt", parameters.ParameterNames);
        Assert.Equal(entity.CreatedAt, parameters.Get<DateTime>("CreatedAt"));
        Assert.Contains("Id", parameters.ParameterNames);
        Assert.Equal(5, parameters.Get<int>("Id"));
        
    }

    [Fact]
    public void BuildUpdate_ThrowsOnEmptyColumns()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.BuildUpdate(null));
    }

    [Fact]
    public void BuildDelete_ReturnsQueryWithIdParameter()
    {
        // Act
        var (sql, parameters) = _builder.BuildDelete(5);

        // Assert
        Assert.Equal("DELETE FROM test_article WHERE id = @Id", sql);
        Assert.Contains("Id", parameters.ParameterNames);
        Assert.Equal(5, parameters.Get<int>("Id")); 
    }

    private class TestArticle : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
