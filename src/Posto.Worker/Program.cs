using Posto.Application;
using Posto.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// Na Fase 1: job agendado de importação do CSV da ANP.

var host = builder.Build();
host.Run();
