# Untout.Framework.Persistence

A lightweight, reusable persistence framework for .NET 8 applications using Dapper and PostgreSQL.

## Overview

This framework extracts proven patterns from production codebases to reduce boilerplate repository code by 30-40%. It provides database-agnostic abstractions with PostgreSQL-specific implementations.

## Key Features

- **SQL Query Builder**: Generates CRUD queries with database-specific syntax (RETURNING clause for PostgreSQL)
- **Name Adapters**: Convention-based (snake_case) and attribute-based ([Table]/[Column]) mapping
- **Connection Factory**: Proper async connection lifecycle management
- **Generic Repository**: Base implementation for common CRUD operations
- **Clean Architecture**: Separation of interfaces and implementations

## Project Structure

```
src/
├── Untout.Framework.Persistence/          # Core abstractions (interfaces)
│   └── Interfaces/
│       ├── IEntity.cs                     # Base entity with Id
│       ├── IDbConnectionFactory.cs        # Connection factory
│       ├── ISqlQueryBuilder.cs            # Query builder interface
│       ├── IDbNameAdapter.cs              # Name mapping interface
│       └── IRepository.cs                 # Repository interface
│
└── Untout.Framework.Persistence.PostgreSql/  # PostgreSQL implementations
    ├── NpgsqlConnectionFactory.cs         # Npgsql connection factory
    ├── PostgreSqlQueryBuilder.cs          # RETURNING clause support
    ├── DapperRepository.cs                # Base repository with Dapper
    └── Adapters/
        ├── SnakeCaseAdapter.cs            # PascalCase -> snake_case
        └── AttributeAdapter.cs            # [Table]/[Column] attributes

tests/
└── Untout.Framework.Persistence.Tests/   # Unit tests
```

## Installation

```bash
# Coming soon to GitHub Packages
dotnet add package Untout.Framework.Persistence
dotnet add package Untout.Framework.Persistence.PostgreSql
```

For now, reference the projects directly:

```xml
<ItemGroup>
  <ProjectReference Include="..\untout-framework-persistence\src\Untout.Framework.Persistence\Untout.Framework.Persistence.csproj" />
  <ProjectReference Include="..\untout-framework-persistence\src\Untout.Framework.Persistence.PostgreSql\Untout.Framework.Persistence.PostgreSql.csproj" />
</ItemGroup>
```

## Quick Start

### 1. Define Your Entity

```csharp
using Untout.Framework.Persistence.Interfaces;

public class Article : IEntity<int>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### 2. Create Your Repository

```csharp
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql;

public class ArticleRepository : DapperRepository<int, Article>
{
    public ArticleRepository(
        IDbConnectionFactory connectionFactory,
        ISqlQueryBuilder<int, Article> queryBuilder)
        : base(connectionFactory, queryBuilder)
    {
    }

    // Add custom queries here if needed
    public async Task<IEnumerable<Article>> GetRecentAsync(int days)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM articles WHERE created_at > @Since ORDER BY created_at DESC";
        return await connection.QueryAsync<Article>(sql, new { Since = DateTime.UtcNow.AddDays(-days) });
    }
}
```

### 3. Register Services (Dependency Injection)

```csharp
// In Program.cs or Startup.cs
services.AddSingleton<IDbConnectionFactory>(
    new NpgsqlConnectionFactory("Host=localhost;Database=mydb;Username=user;Password=pass"));

// Choose your naming convention
services.AddSingleton<IDbNameAdapter, SnakeCaseAdapter>(); // PascalCase -> snake_case
// OR
services.AddSingleton<IDbNameAdapter, AttributeAdapter>(); // Use [Table]/[Column] attributes

// Register query builder
services.AddScoped<ISqlQueryBuilder<int, Article>, PostgreSqlQueryBuilder<int, Article>>();

// Register repository
services.AddScoped<ArticleRepository>();
```

### 4. Use in Your Services

```csharp
public class ArticleService
{
    private readonly ArticleRepository _repository;

    public ArticleService(ArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Article?> GetArticle(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Article> CreateArticle(Article article)
    {
        article.CreatedAt = DateTime.UtcNow;
        return await _repository.AddAsync(article);
    }
}
```

## Name Adapters

### SnakeCaseAdapter (Recommended for PostgreSQL)

Converts C# PascalCase to PostgreSQL snake_case:

```csharp
// C# Entity
public class NewsArticle { public int ArticleId { get; set; } }

// SQL (automatic conversion)
// Table: news_article
// Column: article_id
```

### AttributeAdapter

Uses Data Annotations attributes:

```csharp
using System.ComponentModel.DataAnnotations.Schema;

[Table("custom_table_name")]
public class Article
{
    [Column("article_id")]
    public int Id { get; set; }

    [Column("article_title")]
    public string Title { get; set; }
}
```

## Advanced Usage

### Custom Query Methods

Extend `DapperRepository` for custom queries:

```csharp
public class ArticleRepository : DapperRepository<int, Article>
{
    public async Task<IEnumerable<Article>> SearchByTitleAsync(string searchTerm)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM articles WHERE title ILIKE @SearchTerm";
        return await connection.QueryAsync<Article>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }
}
```

### Override Base Methods

```csharp
public override async Task<Article> AddAsync(Article entity, CancellationToken cancellationToken = default)
{
    // Add custom logic before/after insert
    entity.CreatedAt = DateTime.UtcNow;
    entity.Slug = GenerateSlug(entity.Title);

    return await base.AddAsync(entity, cancellationToken);
}
```

## Benefits

### Before (Manual SQL in each repository)

```csharp
public class ArticleRepository
{
    public async Task<Article?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM articles WHERE id = @Id";
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QuerySingleOrDefaultAsync<Article>(sql, new { Id = id });
    }

    public async Task<Article> AddAsync(Article article)
    {
        const string sql = @"
            INSERT INTO articles (title, content, created_at)
            VALUES (@Title, @Content, @CreatedAt)
            RETURNING id";
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        article.Id = await connection.ExecuteScalarAsync<int>(sql, article);
        return article;
    }

    // ... 3-5 more methods with similar boilerplate
}
```

### After (Using Framework)

```csharp
public class ArticleRepository : DapperRepository<int, Article>
{
    public ArticleRepository(
        IDbConnectionFactory connectionFactory,
        ISqlQueryBuilder<int, Article> queryBuilder)
        : base(connectionFactory, queryBuilder) { }

    // All CRUD methods inherited - zero boilerplate!
    // Only add custom queries specific to this entity
}
```

**Result**: 30-40% less code, easier to test, consistent patterns.

## Testing

Unit tests use xUnit and Moq:

```csharp
[Fact]
public void SnakeCaseAdapter_ConvertsTableName()
{
    var adapter = new SnakeCaseAdapter();
    var tableName = adapter.GetTableName<NewsArticle>();
    Assert.Equal("news_article", tableName);
}

[Fact]
public void PostgreSqlQueryBuilder_BuildsInsertWithReturning()
{
    var adapter = new SnakeCaseAdapter();
    var builder = new PostgreSqlQueryBuilder<int, Article>(adapter);
    var sql = builder.BuildInsert(new[] { "Title", "Content" });
    
    Assert.Contains("INSERT INTO articles", sql);
    Assert.Contains("RETURNING id", sql);
}
```

## Roadmap

- [x] Core interfaces (IEntity, IRepository, ISqlQueryBuilder, IDbNameAdapter)
- [x] PostgreSQL implementations (NpgsqlConnectionFactory, PostgreSqlQueryBuilder)
- [x] Name adapters (SnakeCaseAdapter, AttributeAdapter)
- [x] Base Dapper repository
- [ ] Unit tests (query builders, name adapters)
- [ ] Integration tests (with test database)
- [ ] SQL Server support (OUTPUT clause instead of RETURNING)
- [ ] MySQL support (LAST_INSERT_ID)
- [ ] GitHub Actions CI/CD pipeline
- [ ] NuGet package publishing
- [ ] Documentation site

## Contributing

This framework was extracted from production codebases at Lans Untout. Contributions welcome!

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

## License

MIT License - see LICENSE file for details

## Support

For issues and questions:
- GitHub Issues: https://github.com/lans-untout/untout-framework-persistence/issues
- Email: support@lansuntout.com

## Related Projects

- **Silatigui**: AI-powered news platform using this framework
- **Kissidougou**: Original repository that inspired this extraction

