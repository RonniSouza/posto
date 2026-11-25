using Posto.Application;
using Posto.Infrastructure;
using Posto.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddOptions<ImportacaoOptions>()
    .Bind(builder.Configuration.GetSection(ImportacaoOptions.Secao))
    .Validate(o => o.Municipios.Count > 0 && o.Intervalo > TimeSpan.Zero, "Configure Importacao:Municipios e um Intervalo positivo.")
    .ValidateOnStart();
builder.Services.AddHostedService<ImportacaoAnpWorker>();

var host = builder.Build();
host.Run();
