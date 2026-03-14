
# Untout.Framework.Persistence

## Quick Start

```csharp
// 1. Install packages
// dotnet add package Untout.Framework.Persistence.DependencyInjection

// 2. Define your entity
public class Article : IEntity<int>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

// 3. Register services in Program.cs
builder.Services
    .AddPostgreSqlPersistence(builder.Configuration.GetConnectionString("Default"))
    .AddRepository<int, Article>()
    .AddRepository<int, Tag>();

// 4. Inject and use in your services/controllers
public class ArticleService
{
    private readonly IRepository<int, Article> _repository;

    public ArticleService(IRepository<int, Article> repository)
    {
        _repository = repository;
    }

    public async Task<Article> CreateArticle(string title, string content)
    {
        var article = new Article { Title = title, Content = content };
        return await _repository.AddAsync(article);
    }
}
```

## Using DapperExecutor for Transactions

You can execute multiple operations in a single transaction using `DapperExecutor.ExecuteInTransactionAsync`. This ensures all operations either commit together or roll back on error.

**Example:**

```csharp
public class TransferService
{
    private readonly IDapperExecutor _executor;

    public TransferService(IDapperExecutor executor)
    {
        _executor = executor;
    }

    public async Task TransferFundsAsync(int fromAccountId, int toAccountId, decimal amount)
    {
        await _executor.ExecuteInTransactionAsync(async (conn, tx) =>
        {
            // Debit source account
            await conn.ExecuteAsync(
                "UPDATE accounts SET balance = balance - @Amount WHERE id = @Id AND balance >= @Amount",
                new { Id = fromAccountId, Amount = amount }, tx);

            // Credit destination account
            await conn.ExecuteAsync(
                "UPDATE accounts SET balance = balance + @Amount WHERE id = @Id",
                new { Id = toAccountId, Amount = amount }, tx);

            // Record transfer
            await conn.ExecuteAsync(
                "INSERT INTO transfers (from_id, to_id, amount, created_at) VALUES (@FromId, @ToId, @Amount, @CreatedAt)",
                new { FromId = fromAccountId, ToId = toAccountId, Amount = amount, CreatedAt = DateTime.UtcNow }, tx);
        });
    }
}
```

## Manual Setup (Without DI)

If you're not using dependency injection:

```csharp
// 1. Define your entity
public class Article : IEntity<int>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

// 2. Setup dependencies manually
var connectionFactory = new NpgsqlConnectionFactory("Host=localhost;Database=mydb;Username=user;Password=pass");
var nameAdapter = new SnakeCaseAdapter();
var queryBuilder = new PostgreSqlQueryBuilder<int, Article>(nameAdapter);
var executor = new DapperExecutor(connectionFactory);

// 3. Create repository
var repository = new DapperRepository<int, Article>(queryBuilder, executor);

// 4. Use DapperExecutor for transactions
await executor.ExecuteInTransactionAsync(async (conn, tx) =>
{
    await conn.ExecuteAsync("UPDATE accounts SET balance = balance - @Amount WHERE id = @Id", new { Id = 1, Amount = 100 }, tx);
    await conn.ExecuteAsync("UPDATE accounts SET balance = balance + @Amount WHERE id = @Id", new { Id = 2, Amount = 100 }, tx);
});

// 4. Use the repository
var article = new Article { Title = "Hello", Content = "World" };
await repository.AddAsync(article);
var allArticles = await repository.GetAllAsync();
```

### With Logging (DI)

Enable SQL query logging in development:

```csharp
// Option 1: Configure during setup
builder.Services.AddPostgreSqlPersistence(
    builder.Configuration.GetConnectionString("Default"),
    logger: ConsolePersistenceLogger.Instance);

// Option 2: Add logger separately (useful for environment-specific config)
builder.Services
    .AddPostgreSqlPersistence(builder.Configuration.GetConnectionString("Default"))
    .AddPersistenceLogger(
        builder.Environment.IsDevelopment() 
            ? ConsolePersistenceLogger.Instance 
            : NullPersistenceLogger.Instance)
    .AddRepository<int, Article>();
```

### With Logging (Manual Setup)

```csharp
var logger = ConsolePersistenceLogger.Instance;
var executor = new DapperExecutor(connectionFactory);
var repository = new DapperRepository<int, Article>(queryBuilder, executor, logger);
```

The framework provides:
- `IPersistenceLogger` - logging abstraction
- `NullPersistenceLogger` - no-op logger (default, zero overhead)
- `ConsolePersistenceLogger` - console logger for development

You can implement `IPersistenceLogger` to integrate with Serilog, NLog, or any logging framework.

**Note:** `AddQueryLogger` and `AddPersistenceLogger` are functionally identical—both register an `IPersistenceLogger` and replace any existing logger registration. Use whichever name you prefer.

See the `tests/` directory for more advanced usage and patterns.

Lightweight persistence building blocks for .NET 8 (Dapper + PostgreSQL).

- `Untout.Framework.Persistence`: core abstractions (interfaces)
- `Untout.Framework.Persistence.PostgreSql`: PostgreSQL implementation
- `Untout.Framework.Persistence.Tests`: unit tests / usage examples

## Project Structure

```
src/
├── Untout.Framework.Persistence/                 # Core abstractions
│   └── Interfaces/
│       ├── IEntity.cs
│       ├── IRepository.cs
│       ├── ISqlQueryBuilder.cs
│       ├── IDbConnectionFactory.cs
│       └── IDbNameAdapter.cs
└── Untout.Framework.Persistence.PostgreSql/      # PostgreSQL implementation
    ├── DapperRepository.cs
    ├── PostgreSqlQueryBuilder.cs
    ├── NpgsqlConnectionFactory.cs
    └── Adapters/
        ├── SnakeCaseAdapter.cs
        └── AttributeAdapter.cs

tests/
└── Untout.Framework.Persistence.Tests/
```

## How it works (mental model)

1. Call a repository method (Get/Add/Update/Delete).
2. Repository asks the query builder for SQL.
3. Repository gets a connection from the connection factory.
4. Dapper executes SQL with parameters.
5. Name adapter keeps table/column naming consistent with your database.

6. For transactions, use `DapperExecutor.ExecuteInTransactionAsync` to ensure atomicity.

## Getting started (minimal steps)

1. Provide a PostgreSQL connection string.
2. Choose a name adapter:
   - `SnakeCaseAdapter` (convention-based)
   - `AttributeAdapter` (uses `[Table]` / `[Column]`)
3. Register `PostgreSqlQueryBuilder<TKey, TEntity>` for your entities.
4. Create repositories by deriving from `DapperRepository<TKey, TEntity>`.

Working patterns and examples live in `tests/`.

## Installation

NuGet publishing is not enabled yet. For now, reference the projects directly:

- `src/Untout.Framework.Persistence/Untout.Framework.Persistence.csproj`
- `src/Untout.Framework.Persistence.PostgreSql/Untout.Framework.Persistence.PostgreSql.csproj`

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

MIT License - see `LICENSE` file for details.

## Support

For issues and questions:
- GitHub Issues: https://github.com/lans-untout/untout-framework-persistence/issues
- Email: support@lansuntout.com

## Related Projects

- **Silatigui**: AI-powered news platform using this framework
- **Kissidougou**: Original repository that inspired this extraction

