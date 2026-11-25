using Microsoft.EntityFrameworkCore;

namespace Posto.Infrastructure.Persistence;

internal static class PostoDbContextOptions
{
    public const string ConnectionStringName = "Posto";

    /// <summary>Configuração única do provider, usada pela aplicação e pelo dotnet-ef.</summary>
    public static DbContextOptionsBuilder Configurar(DbContextOptionsBuilder builder, string connectionString) =>
        builder
            .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention();
}
