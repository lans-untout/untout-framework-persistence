using System.ComponentModel.DataAnnotations.Schema;
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql.Adapters;

namespace Untout.Framework.Persistence.Tests.Adapters;

public class AttributeAdapterTests
{
    private readonly IDbNameAdapter _adapter;

    public AttributeAdapterTests()
    {
        _adapter = new SnakeCaseAdapter();
    }

    [Fact]
    public void GetTableName_ReturnsAttributeValue_WhenTableAttributePresent()
    {
        // Act
        var tableName = _adapter.GetTableName<EntityWithTableAttribute>();

        // Assert
        Assert.Equal("custom_table", tableName);
    }

    [Fact]
    public void GetTableName_ReturnsClassName_WhenNoTableAttribute()
    {
        // Act
        var tableName = _adapter.GetTableName<EntityWithoutAttribute>();

        // Assert
        Assert.Equal("entity_without_attribute", tableName);
    }

    [Fact]
    public void GetColumnName_ReturnsPropertyName_WhenNoAttribute()
    {
        // Arrange
        var propertyName = "SomeProperty";

        // Act
        var columnName = _adapter.GetColumnName<EntityWithoutAttribute>(propertyName);

        // Assert
        Assert.Equal("some_property", columnName);
    }

    [Fact]
    public void GetColumnName_ReturnsPropertyName_AsExpected()
    {
        // Arrange & Act
        var columnName1 = _adapter.GetColumnName<EntityWithColumnAttribute>("CustomField");
        var columnName2 = _adapter.GetColumnName<EntityWithColumnAttribute>("RegularField");
        var columnName3 = _adapter.GetColumnName<EntityWithoutAttribute>("SomeProperty");

        // Assert
        Assert.Equal("custom_column", columnName1);
        Assert.Equal("regular_field", columnName2);
        Assert.Equal("some_property", columnName3);
    }

    // Test entities
    [Table("custom_table")]
    private class EntityWithTableAttribute : IEntity<int>
    {
        public int Id { get; set; }
    }

    private class EntityWithoutAttribute : IEntity<int>
    {
        public int Id { get; set; }
    }

    private class EntityWithColumnAttribute : IEntity<int>
    {
        public int Id { get; set; }

        [Column("custom_column")]
        public string CustomField { get; set; } = string.Empty;

        public string RegularField { get; set; } = string.Empty;
    }
}
