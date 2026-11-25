using Posto.Domain.Postos;
using PostoEntity = Posto.Domain.Postos.Posto;

namespace Posto.Domain.Tests;

public class PostoTests
{
    private static readonly Cnpj CnpjValido = Cnpj.Criar("08726064000186").Value;

    private static Endereco EnderecoValido() =>
        Endereco.Criar("AVENIDA FAGUNDES VARELA", "229", "", "JARDIM ATLANTICO", "53140-080", "OLINDA", "pe").Value;

    [Fact]
    public void Criar_ComDadosValidos_CriaPostoSemLocalizacao()
    {
        var result = PostoEntity.Criar(CnpjValido, "  R. J. COMBUSTÍVEIS LTDA.  ", "VIBRA", EnderecoValido());

        Assert.True(result.IsSuccess);
        var posto = result.Value;
        Assert.NotEqual(Guid.Empty, posto.Id);
        Assert.Equal("R. J. COMBUSTÍVEIS LTDA.", posto.RazaoSocial);
        Assert.Null(posto.Localizacao);
    }

    [Fact]
    public void Criar_SemRazaoSocial_Falha()
    {
        var result = PostoEntity.Criar(CnpjValido, " ", "VIBRA", EnderecoValido());

        Assert.Equal(PostoErrors.RazaoSocialVazia, result.Error);
    }

    [Fact]
    public void DefinirLocalizacao_UsaLongitudeComoXeSrid4326()
    {
        var posto = PostoEntity.Criar(CnpjValido, "POSTO", null, EnderecoValido()).Value;

        var result = posto.DefinirLocalizacao(latitude: -8.0089, longitude: -34.8553);

        Assert.True(result.IsSuccess);
        Assert.Equal(-34.8553, posto.Localizacao!.X);
        Assert.Equal(-8.0089, posto.Localizacao.Y);
        Assert.Equal(4326, posto.Localizacao.SRID);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    public void DefinirLocalizacao_ForaDosLimites_Falha(double latitude, double longitude)
    {
        var posto = PostoEntity.Criar(CnpjValido, "POSTO", null, EnderecoValido()).Value;

        Assert.Equal(PostoErrors.LocalizacaoInvalida, posto.DefinirLocalizacao(latitude, longitude).Error);
    }

    [Fact]
    public void AtualizarCadastro_TrocaDados()
    {
        var posto = PostoEntity.Criar(CnpjValido, "NOME ANTIGO", "BRANCA", EnderecoValido()).Value;

        var result = posto.AtualizarCadastro("NOME NOVO", "IPIRANGA", EnderecoValido());

        Assert.True(result.IsSuccess);
        Assert.Equal("NOME NOVO", posto.RazaoSocial);
        Assert.Equal("IPIRANGA", posto.Bandeira);
    }
}

public class EnderecoTests
{
    [Fact]
    public void Criar_NormalizaCepUfECamposOpcionais()
    {
        var endereco = Endereco.Criar(" RUA A ", " ", "", null, "53140-080", "OLINDA", "pe").Value;

        Assert.Equal("RUA A", endereco.Logradouro);
        Assert.Null(endereco.Numero);
        Assert.Null(endereco.Complemento);
        Assert.Equal("53140080", endereco.Cep);
        Assert.Equal("PE", endereco.Uf);
    }

    [Theory]
    [InlineData("", "RECIFE", "PE", "50000-000", "Posto.LogradouroVazio")]
    [InlineData("RUA A", "", "PE", "50000-000", "Posto.MunicipioVazio")]
    [InlineData("RUA A", "RECIFE", "PER", "50000-000", "Posto.UfInvalida")]
    [InlineData("RUA A", "RECIFE", "PE", "5000-000", "Posto.CepInvalido")]
    public void Criar_ComDadoInvalido_Falha(string logradouro, string municipio, string uf, string cep, string codigoErro)
    {
        var result = Endereco.Criar(logradouro, null, null, null, cep, municipio, uf);

        Assert.Equal(codigoErro, result.Error.Code);
    }
}
