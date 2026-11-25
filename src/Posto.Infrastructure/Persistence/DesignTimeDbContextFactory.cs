using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Posto.Infrastructure.Persistence;

/// <summary>
/// Usada só pelo dotnet-ef (migrations). Gerar uma migration não abre conexão;
/// aplicar (database update) usa a variável POSTO_CONNECTION_STRING ou o banco do docker-compose.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PostoDbContext>
{
    private const string ConexaoPadrao = "Host=localhost;Port=5432;Database=posto;Username=posto;Password=posto_dev";

    public PostoDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("POSTO_CONNECTION_STRING") ?? ConexaoPadrao;
        var builder = new DbContextOptionsBuilder<PostoDbContext>();
        PostoDbContextOptions.Configurar(builder, connectionString);
        return new PostoDbContext(builder.Options);
    }
}
