using Posto.Domain.Precos;

namespace Posto.Domain.Tests;

public class PrecoCombustivelTests
{
    private static readonly DateOnly Hoje = new(2026, 11, 25);
    private static readonly Guid PostoId = Guid.CreateVersion7();

    [Fact]
    public void Criar_ComDadosValidos_CriaPreco()
    {
        var result = PrecoCombustivel.Criar(PostoId, TipoCombustivel.Gasolina, 6.85m, Hoje.AddDays(-3), FontePreco.Anp, Hoje);

        Assert.True(result.IsSuccess);
        Assert.Equal(6.85m, result.Value.Valor);
        Assert.Equal(UnidadeMedida.Litro, result.Value.Unidade);
    }

    [Fact]
    public void Gnv_EhVendidoPorMetroCubico()
    {
        Assert.Equal(UnidadeMedida.MetroCubico, TipoCombustivel.Gnv.Unidade());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public void Criar_ComValorForaDaFaixa_Falha(decimal valor)
    {
        var result = PrecoCombustivel.Criar(PostoId, TipoCombustivel.Etanol, valor, Hoje, FontePreco.Anp, Hoje);

        Assert.Equal(PrecoErrors.ValorInvalido, result.Error);
    }

    [Fact]
    public void Criar_ComDataNoFuturo_Falha()
    {
        var result = PrecoCombustivel.Criar(PostoId, TipoCombustivel.Etanol, 4.5m, Hoje.AddDays(1), FontePreco.Anp, Hoje);

        Assert.Equal(PrecoErrors.DataNoFuturo, result.Error);
    }

    [Fact]
    public void Criar_SemPosto_Falha()
    {
        var result = PrecoCombustivel.Criar(Guid.Empty, TipoCombustivel.Etanol, 4.5m, Hoje, FontePreco.Anp, Hoje);

        Assert.Equal(PrecoErrors.PostoObrigatorio, result.Error);
    }

    [Fact]
    public void Criar_ComCombustivelDesconhecido_Falha()
    {
        var result = PrecoCombustivel.Criar(PostoId, (TipoCombustivel)99, 4.5m, Hoje, FontePreco.Anp, Hoje);

        Assert.Equal(PrecoErrors.CombustivelInvalido, result.Error);
    }
}
