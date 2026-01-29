# Untout.Framework.Persistence

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

