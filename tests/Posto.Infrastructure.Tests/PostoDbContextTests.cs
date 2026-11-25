using Microsoft.EntityFrameworkCore;
using Posto.Domain.Postos;
using Posto.Domain.Precos;
using Posto.Infrastructure.Persistence;

namespace Posto.Infrastructure.Tests;

/// <summary>
/// Verifica o mapeamento e a tradução para SQL sem abrir conexão com o banco
/// (ToQueryString só gera o SQL). Testes contra um Postgres real virão com Testcontainers.
/// </summary>
public class PostoDbContextTests
{
    private static PostoDbContext CriarContexto()
    {
        var builder = new DbContextOptionsBuilder<PostoDbContext>();
        PostoDbContextOptions.Configurar(builder, "Host=localhost;Database=posto_teste");
        return new PostoDbContext(builder.Options);
    }

    [Fact]
    public void BuscaPorCnpjs_TraduzParaSqlComParametroDeTexto()
    {
        using var context = CriarContexto();
        var cnpjs = new[] { Cnpj.Criar("08726064000186").Value };

        var sql = context.Postos.Where(p => cnpjs.Contains(p.Cnpj)).ToQueryString();

        Assert.Contains("FROM postos", sql, StringComparison.Ordinal);
        Assert.Contains("p.cnpj = ANY", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void BuscaDeChavesDePreco_TraduzEnumComoTexto()
    {
        using var context = CriarContexto();
        var postoIds = new[] { Guid.CreateVersion7() };
        var desde = new DateOnly(2026, 10, 26);

        var sql = context.Precos
            .Where(p => postoIds.Contains(p.PostoId) && p.DataColeta >= desde && p.Fonte == FontePreco.Anp)
            .ToQueryString();

        Assert.Contains("FROM precos_combustivel", sql, StringComparison.Ordinal);
        Assert.Contains("p.fonte = 'Anp'", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void Modelo_TemIndicesDeIdempotenciaEGeoespacial()
    {
        using var context = CriarContexto();

        var preco = context.Model.FindEntityType(typeof(PrecoCombustivel))!;
        Assert.Contains(preco.GetIndexes(), i => i.IsUnique && i.Properties.Count == 4);

        var posto = context.Model.FindEntityType(typeof(Domain.Postos.Posto))!;
        Assert.Equal("geography (point, 4326)", posto.FindProperty("Localizacao")!.GetColumnType());
        Assert.Contains(posto.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == "Cnpj");
    }

    [Fact]
    public void Modelo_NaoTemMudancasPendentesEmRelacaoAsMigrations()
    {
        using var context = CriarContexto();

        Assert.False(context.Database.HasPendingModelChanges(), "Rode: dotnet ef migrations add <Nome> -p src/Posto.Infrastructure -s src/Posto.Infrastructure -o Persistence/Migrations");
    }
}
