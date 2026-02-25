namespace Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using Untout.Framework.Persistence;
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql;
using Untout.Framework.Persistence.PostgreSql.Adapters;

/// <summary>
/// Extension methods for registering Untout.Framework.Persistence services in dependency injection.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL persistence infrastructure services to the dependency injection container.
    /// Registers connection factory, executor, name adapter, and default logger.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">PostgreSQL connection string.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <exception cref="ArgumentException">Thrown when connectionString is null or whitespace.</exception>
    public static IServiceCollection AddPostgreSqlPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Connection factory (scoped - one per request/scope)
        services.AddScoped<IDbConnectionFactory>(_ => 
            new NpgsqlConnectionFactory(connectionString));
        
        // Dapper executor (scoped - manages connections per request)
        services.AddScoped<IDapperExecutor, DapperExecutor>();
        
        // Name adapter (singleton - stateless)
        services.AddSingleton<IDbNameAdapter, SnakeCaseAdapter>();

        var existingDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPersistenceLogger));
        if (existingDescriptor == null)
        {
            services.AddSingleton<IPersistenceLogger>(NullPersistenceLogger.Instance);
        }

        return services;
    }

    public static IServiceCollection AddQueryLogger(
        this IServiceCollection services,
        IPersistenceLogger persistenceLogger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(persistenceLogger);

        // Remove existing logger registration if any
        var existingDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPersistenceLogger));
        if (existingDescriptor != null)
        {
            services.Remove(existingDescriptor);
        }

        services.AddSingleton(persistenceLogger);
        return services;
    }

    /// <summary>
    /// Adds PostgreSQL persistence infrastructure with custom logger.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">PostgreSQL connection string.</param>
    /// <param name="logger">Custom logger instance (e.g., ConsolePersistenceLogger.Instance).</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddPostgreSqlPersistence(
        this IServiceCollection services,
        string connectionString,
        IPersistenceLogger logger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(logger);

        services.AddScoped<IDbConnectionFactory>(_ => 
            new NpgsqlConnectionFactory(connectionString));
        
        services.AddScoped<IDapperExecutor, DapperExecutor>();
        services.AddSingleton<IDbNameAdapter, SnakeCaseAdapter>();
        services.AddSingleton(logger);

        return services;
    }

    /// <summary>
    /// Registers a repository for a specific entity type.
    /// Automatically registers the query builder and repository implementation.
    /// </summary>
    /// <typeparam name="TKey">Entity primary key type.</typeparam>
    /// <typeparam name="TEntity">Entity type implementing IEntity.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddRepository<TKey, TEntity>(
        this IServiceCollection services)
        where TEntity : class, IEntity<TKey>
    {
        ArgumentNullException.ThrowIfNull(services);

        // Query builder (scoped - uses name adapter)
        services.AddScoped<ISqlQueryBuilder<TKey, TEntity>>(sp =>
            new PostgreSqlQueryBuilder<TKey, TEntity>(
                sp.GetRequiredService<IDbNameAdapter>()));

        // Repository (scoped - uses query builder, executor, logger)
        services.AddScoped<IRepository<TKey, TEntity>>(sp =>
            new DapperRepository<TKey, TEntity>(
                sp.GetRequiredService<ISqlQueryBuilder<TKey, TEntity>>(),
                sp.GetRequiredService<IDapperExecutor>(),
                sp.GetRequiredService<IPersistenceLogger>()));

        return services;
    }

    /// <summary>
    /// Registers a custom logger instance for persistence operations.
    /// Replaces the default NullPersistenceLogger.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="logger">The logger instance to register.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddPersistenceLogger(
        this IServiceCollection services,
        IPersistenceLogger logger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(logger);

        // Remove existing logger registration if any
        var existingDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPersistenceLogger));
        if (existingDescriptor != null)
        {
            services.Remove(existingDescriptor);
        }

        // Register new logger as singleton
        services.AddSingleton(logger);

        return services;
    }
}
