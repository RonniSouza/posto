using Posto.Application.Precos.ImportarAnp;
using Posto.Domain.Precos;

namespace Posto.Application.Tests;

public class ImportarPrecosAnpHandlerTests
{
    private const string CnpjOlinda = " 08.726.064/0001-86";
    private const string CnpjRecife = " 04.155.302/0001-90";

    // 25/11/2026 01:00 UTC ainda é 24/11 em Recife (UTC-3).
    private static readonly RelogioFixo Relogio = new(new DateTimeOffset(2026, 11, 25, 1, 0, 0, TimeSpan.Zero));

    private static readonly ImportarPrecosAnpCommand RecifeOlinda = new("PE", ["RECIFE", "OLINDA"]);

    private static RegistroPrecoAnp Linha(
        string cnpj = CnpjOlinda,
        string municipio = "OLINDA",
        string uf = "PE",
        string produto = "GASOLINA",
        string data = "29/10/2026",
        string valor = "6,85",
        string revenda = "R. J. COMBUSTÍVEIS E LUBRIFICANTES LTDA.",
        string bandeira = "VIBRA") =>
        new(uf, municipio, revenda, cnpj, "AVENIDA FAGUNDES VARELA", "229", "", "JARDIM ATLANTICO", "53140-080", produto, data, valor, bandeira);

    private static ImportarPrecosAnpHandler Handler(BancoFake banco, params RegistroPrecoAnp[] linhas) =>
        new(new FonteFake(linhas), banco, banco, banco, Relogio);

    [Fact]
    public async Task Importa_SoOsMunicipiosPedidos()
    {
        var banco = new BancoFake();
        var handler = Handler(
            banco,
            Linha(),
            Linha(cnpj: CnpjRecife, municipio: "RECIFE", produto: "ETANOL", valor: "4,85"),
            Linha(cnpj: " 88.587.589/0001-17", municipio: "CANOAS", uf: "RS"),
            Linha(cnpj: " 88.587.589/0001-17", municipio: "OLINDA", uf: "SP")); // mesmo nome de município em outra UF é ignorado

        var result = await handler.HandleAsync(RecifeOlinda);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.RegistrosLidos);
        Assert.Equal(2, result.Value.RegistrosDaRegiao);
        Assert.Equal(2, result.Value.PostosCriados);
        Assert.Equal(2, result.Value.PrecosInseridos);
        Assert.Equal(2, banco.Postos.Count);
        Assert.Contains(banco.Precos, p => p.Combustivel == TipoCombustivel.Etanol && p.Valor == 4.85m);
    }

    [Fact]
    public async Task VariosProdutosDoMesmoPosto_CriamUmPosto()
    {
        var banco = new BancoFake();
        var handler = Handler(
            banco,
            Linha(produto: "GASOLINA"),
            Linha(produto: "GASOLINA ADITIVADA", valor: "6,99"),
            Linha(produto: "GNV", valor: "4,89"));

        var result = await handler.HandleAsync(RecifeOlinda);

        Assert.Equal(1, result.Value.PostosCriados);
        Assert.Equal(3, result.Value.PrecosInseridos);
        Assert.Single(banco.Postos);
        Assert.Equal(UnidadeMedida.MetroCubico, banco.Precos.Single(p => p.Combustivel == TipoCombustivel.Gnv).Unidade);
    }

    [Fact]
    public async Task RodarDuasVezes_NaoDuplicaPrecos()
    {
        var banco = new BancoFake();
        var linhas = new[] { Linha(), Linha(produto: "ETANOL", valor: "4,85") };

        await Handler(banco, linhas).HandleAsync(RecifeOlinda);
        var segunda = await Handler(banco, linhas).HandleAsync(RecifeOlinda);

        Assert.Equal(0, segunda.Value.PostosCriados);
        Assert.Equal(1, segunda.Value.PostosAtualizados);
        Assert.Equal(0, segunda.Value.PrecosInseridos);
        Assert.Equal(2, segunda.Value.PrecosJaExistentes);
        Assert.Single(banco.Postos);
        Assert.Equal(2, banco.Precos.Count);
    }

    [Fact]
    public async Task LinhaRepetidaNoArquivo_EntraUmaVez()
    {
        var banco = new BancoFake();

        var result = await Handler(banco, Linha(), Linha()).HandleAsync(RecifeOlinda);

        Assert.Equal(1, result.Value.PrecosInseridos);
        Assert.Equal(1, result.Value.PrecosJaExistentes);
    }

    [Fact]
    public async Task NovaSemana_AdicionaPrecoEAtualizaCadastroComColetaMaisRecente()
    {
        var banco = new BancoFake();
        await Handler(banco, Linha(data: "29/10/2026", bandeira: "BRANCA")).HandleAsync(RecifeOlinda);

        var result = await Handler(banco, Linha(data: "05/11/2026", valor: "6,79", bandeira: "IPIRANGA")).HandleAsync(RecifeOlinda);

        Assert.Equal(1, result.Value.PrecosInseridos);
        Assert.Equal(2, banco.Precos.Count);
        Assert.Equal("IPIRANGA", banco.Postos.Single().Bandeira);
    }

    [Theory]
    [InlineData(CnpjOlinda, "GLP", "29/10/2026", "6,85")] // produto fora do escopo
    [InlineData(CnpjOlinda, "GASOLINA", "2026-10-29", "6,85")] // data em outro formato
    [InlineData(CnpjOlinda, "GASOLINA", "29/10/2026", "6.85.1")] // valor ilegível
    [InlineData(" 08.726.064/0001-00", "GASOLINA", "29/10/2026", "6,85")] // CNPJ inválido
    [InlineData(CnpjOlinda, "GASOLINA", "25/11/2026", "6,85")] // amanhã no fuso de Recife
    public async Task LinhaInvalida_EhContadaENaoInterrompeAImportacao(string cnpj, string produto, string data, string valor)
    {
        var banco = new BancoFake();
        var handler = Handler(
            banco,
            Linha(cnpj: cnpj, produto: produto, data: data, valor: valor),
            Linha(cnpj: CnpjRecife, municipio: "RECIFE"));

        var result = await handler.HandleAsync(RecifeOlinda);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.RegistrosInvalidos);
        Assert.Equal(1, result.Value.PrecosInseridos);
        Assert.Single(result.Value.AmostraDeErros);
    }

    [Fact]
    public async Task FonteVazia_RetornaFalha()
    {
        var banco = new BancoFake();

        var result = await Handler(banco).HandleAsync(RecifeOlinda);

        Assert.Equal(AnpErrors.FonteVazia, result.Error);
        Assert.Equal(0, banco.SaveChangesChamadas);
    }

    [Fact]
    public async Task FiltroSemMunicipios_RetornaFalha()
    {
        var result = await Handler(new BancoFake(), Linha()).HandleAsync(new ImportarPrecosAnpCommand("PE", []));

        Assert.Equal(AnpErrors.FiltroInvalido, result.Error);
    }

    [Fact]
    public async Task SalvaTudoNumaUnicaTransacao()
    {
        var banco = new BancoFake();

        await Handler(banco, Linha(), Linha(cnpj: CnpjRecife, municipio: "RECIFE")).HandleAsync(RecifeOlinda);

        Assert.Equal(1, banco.SaveChangesChamadas);
    }
}
