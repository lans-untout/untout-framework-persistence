namespace Untout.Framework.Persistence.Tests.Adapters;

using System.ComponentModel.DataAnnotations.Schema;
using Xunit;
using Untout.Framework.Persistence.PostgreSql.Adapters;
using Untout.Framework.Persistence.Interfaces;

public class AttributeAdapterTests
{
    private readonly AttributeAdapter _adapter;

    public AttributeAdapterTests()
    {
        _adapter = new AttributeAdapter();
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
        Assert.Equal("EntityWithoutAttribute", tableName);
    }

    [Fact]
    public void GetColumnName_ReturnsPropertyName_WhenNoAttribute()
    {
        // Arrange
        var propertyName = "SomeProperty";

        // Act
        var columnName = _adapter.GetColumnName(propertyName);

        // Assert
        Assert.Equal(propertyName, columnName);
    }

    [Fact]
    public void GetColumnName_ReturnsPropertyName_AsExpected()
    {
        // Arrange & Act
        var columnName1 = _adapter.GetColumnName("CustomField");
        var columnName2 = _adapter.GetColumnName("RegularField");
        var columnName3 = _adapter.GetColumnName("SomeProperty");

        // Assert
        Assert.Equal("CustomField", columnName1);
        Assert.Equal("RegularField", columnName2);
        Assert.Equal("SomeProperty", columnName3);
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
}
