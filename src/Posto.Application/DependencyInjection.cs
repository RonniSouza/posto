using Microsoft.Extensions.DependencyInjection;

namespace Posto.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Handlers de casos de uso e validações serão registrados aqui.
        return services;
    }
}
