using Microsoft.Extensions.DependencyInjection;
using Posto.Application.Precos.ImportarAnp;

namespace Posto.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ImportarPrecosAnpHandler>();
        return services;
    }
}
