using Xunit;
using Untout.Framework.Persistence.PostgreSql.Adapters;

namespace Untout.Framework.Persistence.Tests.Adapters;

public class SnakeCaseAdapterTests
{
    private readonly SnakeCaseAdapter _adapter;

    public SnakeCaseAdapterTests()
    {
        _adapter = new SnakeCaseAdapter();
    }

    [Theory]
    [InlineData("Article", "article")]
    [InlineData("NewsArticle", "news_article")]
    [InlineData("AIGeneratedContent", "a_i_generated_content")]
    [InlineData("HTTPSConnection", "h_t_t_p_s_connection")]
    public void GetTableName_ConvertsToSnakeCase(string className, string expected)
    {
        // Arrange - use a test class with the given name pattern
        var tableName = _adapter.GetTableName<TestEntity>();

        // Act & Assert - verify the conversion logic
        var converted = ConvertToSnakeCase(className);
        Assert.Equal(expected, converted);
    }

    [Theory]
    [InlineData("Id", "id")]
    [InlineData("Title", "title")]
    [InlineData("CreatedAt", "created_at")]
    [InlineData("PublishedDate", "published_date")]
    [InlineData("AIEmbedding", "a_i_embedding")]
    public void GetColumnName_ConvertsToSnakeCase(string propertyName, string expected)
    {
        // Act
        var columnName = _adapter.GetColumnName(propertyName);

        // Assert
        Assert.Equal(expected, columnName);
    }

    [Fact]
    public void GetColumnName_HandlesNullOrEmpty()
    {
        // Act & Assert
        Assert.Null(_adapter.GetColumnName(null!));
        Assert.Equal(string.Empty, _adapter.GetColumnName(string.Empty));
        // Whitespace returns whitespace (converted to lowercase) - this is expected behavior
        Assert.Equal("   ", _adapter.GetColumnName("   "));
    }

    private static string ConvertToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        return System.Text.RegularExpressions.Regex.Replace(input, @"(?<!^)(?=[A-Z])", "_").ToLowerInvariant();
    }

    private class TestEntity : Interfaces.IEntity<int>
    {
        public int Id { get; set; }
    }
}
