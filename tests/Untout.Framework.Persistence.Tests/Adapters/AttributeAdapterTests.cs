using System.ComponentModel.DataAnnotations.Schema;
using Xunit;
using Untout.Framework.Persistence.PostgreSql.Adapters;
using Untout.Framework.Persistence.Interfaces;

namespace Untout.Framework.Persistence.Tests.Adapters;

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
    public void GetColumnName_WithPropertyInfo_ReturnsAttributeValue_WhenColumnAttributePresent()
    {
        // Arrange
        var property = typeof(EntityWithColumnAttribute).GetProperty(nameof(EntityWithColumnAttribute.CustomField))!;

        // Act
        var columnName = _adapter.GetColumnName(property);

        // Assert
        Assert.Equal("custom_column", columnName);
    }

    [Fact]
    public void GetColumnName_WithPropertyInfo_ReturnsPropertyName_WhenNoColumnAttribute()
    {
        // Arrange
        var property = typeof(EntityWithColumnAttribute).GetProperty(nameof(EntityWithColumnAttribute.RegularField))!;

        // Act
        var columnName = _adapter.GetColumnName(property);

        // Assert
        Assert.Equal("RegularField", columnName);
    }

    [Fact]
    public void GetColumnName_WithPropertyInfo_ThrowsArgumentNullException_WhenPropertyIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _adapter.GetColumnName((System.Reflection.PropertyInfo)null!));
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
