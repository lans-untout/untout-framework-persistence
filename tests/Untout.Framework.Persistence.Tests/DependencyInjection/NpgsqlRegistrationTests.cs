using Microsoft.Extensions.DependencyInjection;
using Untout.Framework.Persistence.DependencyInjection;
using Untout.Framework.Persistence.Interfaces;
using Untout.Framework.Persistence.PostgreSql;

namespace Untout.Framework.Persistence.Tests.DependencyInjection;

public class NpgsqlRegistrationTests
{
    [Fact]
    public void AddPostgreSqlPersistence_RegistersNpgsqlConnectionFactory_WithProvidedConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=user;Password=pass";

        // Act
        services.AddPostgreSqlPersistence(connectionString);
        var provider = services.BuildServiceProvider();

        // Assert
        var factory = provider.GetService<IDbConnectionFactory>();
        Assert.NotNull(factory);
        Assert.IsType<NpgsqlConnectionFactory>(factory);

        var npgFactory = (NpgsqlConnectionFactory)factory;
        Assert.Equal(connectionString, npgFactory.ConnectionString);
    }
}
