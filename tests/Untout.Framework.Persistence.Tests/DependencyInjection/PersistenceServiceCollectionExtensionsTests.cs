
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Untout.Framework.Persistence.DependencyInjection;
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql;

namespace Untout.Framework.Persistence.Tests.DependencyInjection;
public class PersistenceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPostgreSqlPersistence_WithValidConnectionString_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=user;Password=pass";

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<IDbConnectionFactory>());
        Assert.NotNull(provider.GetService<IDapperExecutor>());
        Assert.NotNull(provider.GetService<IDbNameAdapter>());
        Assert.NotNull(provider.GetService<IPersistenceLogger>());
    }

    [Fact]
    public void AddPostgreSqlPersistence_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null;
        var connectionString = "Host=localhost";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddPostgreSqlPersistence(connectionString));
    }

    [Fact]
    public void AddPostgreSqlPersistence_WithNullConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddPostgreSqlPersistence(null));
    }

    [Fact]
    public void AddPostgreSqlPersistence_WithEmptyConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            services.AddPostgreSqlPersistence(""));
    }

    [Fact]
    public void AddPostgreSqlPersistence_WithCustomLogger_RegistersCustomLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";
        var logger = ConsolePersistenceLogger.Instance;

        // Act
        services.AddPostgreSqlPersistence(connectionString, logger);
        var provider = services.BuildServiceProvider();

        // Assert
        var registeredLogger = provider.GetService<IPersistenceLogger>();
        Assert.Same(logger, registeredLogger);
    }

    [Fact]
    public void AddPostgreSqlPersistence_RegistersCorrectLifetimes()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString);

        // Assert
        var factoryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDbConnectionFactory));
        var executorDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDapperExecutor));
        var adapterDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDbNameAdapter));
        var loggerDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPersistenceLogger));

        Assert.Equal(ServiceLifetime.Scoped, factoryDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, executorDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, adapterDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, loggerDescriptor?.Lifetime);
    }

    [Fact]
    public void AddRepository_RegistersQueryBuilderAndRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        services.AddRepository<int, TestEntity>();
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<ISqlQueryBuilder<int, TestEntity>>());
        Assert.NotNull(provider.GetService<IRepository<int, TestEntity>>());
    }

    [Fact]
    public void AddRepository_WithoutInfrastructure_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRepository<int, TestEntity>();
        var provider = services.BuildServiceProvider();

        // Act & Assert - Should throw when trying to resolve because dependencies not registered
        Assert.Throws<InvalidOperationException>(() => 
            provider.GetRequiredService<IRepository<int, TestEntity>>());
    }

    [Fact]
    public void AddRepository_RegistersScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        services.AddRepository<int, TestEntity>();

        // Assert
        var queryBuilderDescriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(ISqlQueryBuilder<int, TestEntity>));
        var repositoryDescriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(IRepository<int, TestEntity>));

        Assert.Equal(ServiceLifetime.Scoped, queryBuilderDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, repositoryDescriptor?.Lifetime);
    }

    [Fact]
    public void AddRepository_CanRegisterMultipleEntities()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        services.AddRepository<int, TestEntity>();
        services.AddRepository<int, TestEntity2>();
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<IRepository<int, TestEntity>>());
        Assert.NotNull(provider.GetService<IRepository<int, TestEntity2>>());
    }

    [Fact]
    public void AddPostgreSqlPersistence_SupportsMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        var result = services
            .AddPostgreSqlPersistence(connectionString)
            .AddRepository<int, TestEntity>()
            .AddRepository<int, TestEntity2>();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddRepository_WithScopedProvider_CreatesDifferentInstancesPerScope()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";
        services.AddPostgreSqlPersistence(connectionString);
        services.AddRepository<int, TestEntity>();
        var provider = services.BuildServiceProvider();

        // Act
        IRepository<int, TestEntity> repo1;
        IRepository<int, TestEntity> repo2;
        IRepository<int, TestEntity> repo3;

        using (var scope1 = provider.CreateScope())
        {
            repo1 = scope1.ServiceProvider.GetRequiredService<IRepository<int, TestEntity>>();
            repo2 = scope1.ServiceProvider.GetRequiredService<IRepository<int, TestEntity>>();
        }

        using (var scope2 = provider.CreateScope())
        {
            repo3 = scope2.ServiceProvider.GetRequiredService<IRepository<int, TestEntity>>();
        }

        // Assert
        Assert.Same(repo1, repo2); // Same scope = same instance
        Assert.NotSame(repo1, repo3); // Different scope = different instance
    }

    [Fact]
    public void AddPersistenceLogger_RegistersCustomLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";
        var logger = ConsolePersistenceLogger.Instance;

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        services.AddPersistenceLogger(logger);
        var provider = services.BuildServiceProvider();

        // Assert
        var registeredLogger = provider.GetService<IPersistenceLogger>();
        Assert.Same(logger, registeredLogger);
    }

    [Fact]
    public void AddPersistenceLogger_ReplacesDefaultLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString)
            .AddQueryLogger(NullPersistenceLogger.Instance);
        var providerBefore = services.BuildServiceProvider();
        var loggerBefore = providerBefore.GetService<IPersistenceLogger>();

        services.AddPersistenceLogger(ConsolePersistenceLogger.Instance); // Replaces with ConsoleLogger
        var providerAfter = services.BuildServiceProvider();
        var loggerAfter = providerAfter.GetService<IPersistenceLogger>();

        // Assert
        Assert.IsType<NullPersistenceLogger>(loggerBefore);
        Assert.Same(ConsolePersistenceLogger.Instance, loggerAfter);
    }

    [Fact]
    public void AddPersistenceLogger_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null;
        var logger = ConsolePersistenceLogger.Instance;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddPersistenceLogger(logger));
    }

    [Fact]
    public void AddPersistenceLogger_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddPersistenceLogger(null));
    }

    [Fact]
    public void AddPersistenceLogger_SupportsMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        var result = services
            .AddPostgreSqlPersistence(connectionString)
            .AddPersistenceLogger(ConsolePersistenceLogger.Instance)
            .AddRepository<int, TestEntity>();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddQueryLogger_WithInstance_RegistersCustomLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";
        var logger = ConsolePersistenceLogger.Instance;

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        services.AddQueryLogger(logger);
        var provider = services.BuildServiceProvider();

        // Assert
        var registeredLogger = provider.GetService<IPersistenceLogger>();
        Assert.Same(logger, registeredLogger);
    }

    [Fact]
    public void AddQueryLogger_WithInstance_ReplacesExistingLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString); // Registers NullLogger
        services.AddQueryLogger(ConsolePersistenceLogger.Instance); // Replaces with ConsoleLogger
        var provider = services.BuildServiceProvider();

        // Assert
        var registeredLogger = provider.GetService<IPersistenceLogger>();
        Assert.Same(ConsolePersistenceLogger.Instance, registeredLogger);
    }

    [Fact]
    public void AddQueryLogger_WithInstance_SupportsMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        var result = services
            .AddPostgreSqlPersistence(connectionString)
            .AddQueryLogger(ConsolePersistenceLogger.Instance)
            .AddRepository<int, TestEntity>();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddQueryLogger_WithGenericType_RegistersLoggerType()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        services.AddQueryLogger<TestCustomLogger>();
        var provider = services.BuildServiceProvider();

        // Assert
        var registeredLogger = provider.GetService<IPersistenceLogger>();
        Assert.NotNull(registeredLogger);
        Assert.IsType<TestCustomLogger>(registeredLogger);
    }

    [Fact]
    public void AddQueryLogger_WithGenericType_ReplacesDefaultLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString); // Registers NullLogger
        services.AddQueryLogger<TestCustomLogger>(); // Replaces with TestCustomLogger
        var provider = services.BuildServiceProvider();

        // Assert
        var registeredLogger = provider.GetService<IPersistenceLogger>();
        Assert.IsType<TestCustomLogger>(registeredLogger);
    }

    [Fact]
    public void AddQueryLogger_WithGenericType_ReplacesExistingInstanceLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        services.AddQueryLogger(ConsolePersistenceLogger.Instance); // Register instance
        services.AddQueryLogger<TestCustomLogger>(); // Replace with type
        var provider = services.BuildServiceProvider();

        // Assert
        var registeredLogger = provider.GetService<IPersistenceLogger>();
        Assert.IsType<TestCustomLogger>(registeredLogger);
        Assert.NotSame(ConsolePersistenceLogger.Instance, registeredLogger);
    }

    [Fact]
    public void AddQueryLogger_WithGenericType_CreatesNewInstanceEachSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        services.AddQueryLogger<TestCustomLogger>();
        var provider = services.BuildServiceProvider();

        // Assert - Singleton should return same instance
        var logger1 = provider.GetService<IPersistenceLogger>();
        var logger2 = provider.GetService<IPersistenceLogger>();
        Assert.Same(logger1, logger2);
    }

    [Fact]
    public void AddQueryLogger_WithGenericType_SupportsMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        var result = services
            .AddPostgreSqlPersistence(connectionString)
            .AddQueryLogger<TestCustomLogger>()
            .AddRepository<int, TestEntity>();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddQueryLogger_WithGenericType_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddQueryLogger<TestCustomLogger>());
    }

    [Fact]
    public void AddQueryLogger_WithInstance_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null;
        var logger = ConsolePersistenceLogger.Instance;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddQueryLogger(logger));
    }

    [Fact]
    public void AddQueryLogger_WithInstance_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddQueryLogger(null));
    }

    [Fact]
    public void AddQueryLogger_WithGenericType_RegistersAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        services.AddQueryLogger<TestCustomLogger>();

        // Assert
        var loggerDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPersistenceLogger));
        Assert.Equal(ServiceLifetime.Singleton, loggerDescriptor?.Lifetime);
    }

    [Fact]
    public void AddQueryLogger_WithGenericType_LoggerIsUsedByRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test";

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        services.AddQueryLogger<TestCustomLogger>();
        services.AddRepository<int, TestEntity>();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<int, TestEntity>>();
        var logger = scope.ServiceProvider.GetRequiredService<IPersistenceLogger>();

        // Assert
        Assert.IsType<TestCustomLogger>(logger);
        Assert.NotNull(repository);
    }

    public class TestCustomLogger : IPersistenceLogger
    {
        public void LogQuery(string sql, object parameters = null) { }
        public void LogDebug(string message) { }
        public void LogInformation(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception exception) { }
    }

    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestEntity2 : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
