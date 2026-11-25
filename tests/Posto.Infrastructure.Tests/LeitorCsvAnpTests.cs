using System.Text;
using Posto.Application.Precos.ImportarAnp;
using Posto.Infrastructure.Anp;

namespace Posto.Infrastructure.Tests;

public class LeitorCsvAnpTests
{
    private static async Task<List<RegistroPrecoAnp>> LerFixtureAsync()
    {
        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "anp-amostra.csv"));
        var registros = new List<RegistroPrecoAnp>();
        await foreach (var registro in LeitorCsvAnp.LerAsync(stream))
        {
            registros.Add(registro);
        }

        return registros;
    }

    [Fact]
    public async Task LeTodasAsLinhasMenosOCabecalho()
    {
        var registros = await LerFixtureAsync();

        Assert.Equal(5, registros.Count);
    }

    [Fact]
    public async Task MapeiaColunasPeloNomeDoCabecalho_IgnorandoBom()
    {
        var primeiro = (await LerFixtureAsync())[0];

        Assert.Equal("PE", primeiro.Uf); // primeira coluna útil vem logo depois do BOM
        Assert.Equal("OLINDA", primeiro.Municipio);
        Assert.Equal("R. J. COMBUSTÍVEIS E LUBRIFICANTES LTDA.", primeiro.Revenda); // acento preservado
        Assert.Equal("08.726.064/0001-86", primeiro.Cnpj); // espaço inicial removido
        Assert.Equal("AVENIDA FAGUNDES VARELA", primeiro.NomeRua);
        Assert.Equal("229", primeiro.NumeroRua);
        Assert.Equal("JARDIM ATLANTICO", primeiro.Bairro);
        Assert.Equal("53140-080", primeiro.Cep);
        Assert.Equal("GASOLINA", primeiro.Produto);
        Assert.Equal("29/10/2026", primeiro.DataColeta);
        Assert.Equal("6,85", primeiro.ValorVenda);
        Assert.Equal("VIBRA", primeiro.Bandeira);
    }

    [Fact]
    public async Task CampoEntreAspasComPontoEVirgula_NaoDesalinhaColunas()
    {
        var brasilia = (await LerFixtureAsync()).Single(r => r.Municipio == "BRASILIA");

        Assert.Equal("LOTE: 04;", brasilia.Complemento);
        Assert.Equal("NORTE (AGUAS CLARAS)", brasilia.Bairro);
        Assert.Equal("6,19", brasilia.ValorVenda);
    }

    [Fact]
    public async Task LinhasDoCsv_ConvertemParaODominio()
    {
        var registros = await LerFixtureAsync();

        Assert.All(registros, r => Assert.True(ConversorRegistroAnp.Converter(r).IsSuccess, r.ToString()));
    }

    [Fact]
    public async Task ArquivoSemBomEComColunasAMenos_LeCamposAusentesComoNulo()
    {
        const string csv = "Estado - Sigla;Municipio;CNPJ da Revenda;Produto\nPE;RECIFE;05.557.705/0001-28;GNV\n";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var registros = new List<RegistroPrecoAnp>();
        await foreach (var registro in LeitorCsvAnp.LerAsync(stream))
        {
            registros.Add(registro);
        }

        var unico = Assert.Single(registros);
        Assert.Equal("RECIFE", unico.Municipio);
        Assert.Null(unico.ValorVenda);
    }
}
