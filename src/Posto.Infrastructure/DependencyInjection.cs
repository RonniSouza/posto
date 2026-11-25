using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Posto.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // EF Core + PostgreSQL/PostGIS e o importador da ANP entram aqui na Fase 1.
        return services;
    }
}
