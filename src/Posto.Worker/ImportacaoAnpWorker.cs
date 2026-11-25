using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Posto.Application.Precos.ImportarAnp;
using Posto.Infrastructure.Persistence;

namespace Posto.Worker;

public sealed class ImportacaoOptions
{
    public const string Secao = "Importacao";

    public string Uf { get; init; } = "PE";

    public IList<string> Municipios { get; init; } = [];

    /// <summary>A ANP publica uma vez por semana; checar diariamente pega a atualização sem pressa.</summary>
    public TimeSpan Intervalo { get; init; } = TimeSpan.FromDays(1);

    /// <summary>Importa uma vez e encerra (útil para rodar à mão: --Importacao:ExecutarUmaVez=true).</summary>
    public bool ExecutarUmaVez { get; init; }

    /// <summary>Aplica as migrations pendentes ao iniciar. Ok para ambiente local; em produção seria um passo do deploy.</summary>
    public bool AplicarMigrations { get; init; } = true;
}

public sealed partial class ImportacaoAnpWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ImportacaoOptions> options,
    IHostApplicationLifetime lifetime,
    ILogger<ImportacaoAnpWorker> logger) : BackgroundService
{
    private readonly ImportacaoOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.AplicarMigrations)
        {
            await AplicarMigrationsAsync(stoppingToken);
        }

        using var timer = new PeriodicTimer(_options.Intervalo);
        do
        {
            await ImportarAsync(stoppingToken);

            if (_options.ExecutarUmaVez)
            {
                lifetime.StopApplication();
                return;
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task AplicarMigrationsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PostoDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
        LogMigrationsAplicadas();
    }

    private async Task ImportarAsync(CancellationToken cancellationToken)
    {
        // Um escopo por execução: DbContext e repositórios novos a cada importação.
        await using var scope = scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ImportarPrecosAnpHandler>();

        try
        {
            var result = await handler.HandleAsync(new ImportarPrecosAnpCommand(_options.Uf, [.. _options.Municipios]), cancellationToken);

            if (result.IsFailure)
            {
                LogImportacaoFalhou(result.Error.Code, result.Error.Message);
                return;
            }

            var r = result.Value;
            LogImportacaoConcluida(r.RegistrosLidos, r.RegistrosDaRegiao, r.PostosCriados, r.PostosAtualizados, r.PrecosInseridos, r.PrecosJaExistentes, r.RegistrosInvalidos);
            foreach (var erro in r.AmostraDeErros)
            {
                LogRegistroInvalido(erro);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or System.Data.Common.DbException or DbUpdateException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            // Falha de infraestrutura (rede, arquivo, banco): registra e tenta de novo no próximo ciclo.
            LogErroInesperado(ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Migrations aplicadas")]
    private partial void LogMigrationsAplicadas();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Importação da ANP não realizada: {Codigo} - {Mensagem}")]
    private partial void LogImportacaoFalhou(string codigo, string mensagem);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Importação da ANP concluída: {Lidos} linhas lidas, {DaRegiao} da região; postos: {Criados} novos, {Atualizados} atualizados; preços: {Inseridos} novos, {Existentes} já existentes; {Invalidos} inválidos")]
    private partial void LogImportacaoConcluida(int lidos, int daRegiao, int criados, int atualizados, int inseridos, int existentes, int invalidos);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Linha da ANP ignorada: {Detalhe}")]
    private partial void LogRegistroInvalido(string detalhe);

    [LoggerMessage(Level = LogLevel.Error, Message = "Erro ao importar preços da ANP; nova tentativa no próximo ciclo")]
    private partial void LogErroInesperado(Exception ex);
}
