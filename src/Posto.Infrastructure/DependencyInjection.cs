using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Posto.Application.Abstractions;
using Posto.Application.Precos.ImportarAnp;
using Posto.Infrastructure.Anp;
using Posto.Infrastructure.Persistence;
using Posto.Infrastructure.Persistence.Repositories;

namespace Posto.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(PostoDbContextOptions.ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{PostoDbContextOptions.ConnectionStringName}' não configurada.");

        services.AddDbContext<PostoDbContext>(options => PostoDbContextOptions.Configurar(options, connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PostoDbContext>());
        services.AddScoped<IPostoRepository, PostoRepository>();
        services.AddScoped<IPrecoCombustivelRepository, PrecoCombustivelRepository>();

        services.AddOptions<AnpOptions>().Bind(configuration.GetSection(AnpOptions.Secao));
        services.AddHttpClient<IFonteDadosAnp, FonteDadosAnp>(http =>
        {
            http.Timeout = TimeSpan.FromMinutes(5);
            http.DefaultRequestHeaders.UserAgent.ParseAdd("ProjetoPosto/1.0");
        });

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
