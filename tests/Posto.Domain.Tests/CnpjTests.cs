using Posto.Domain.Postos;

namespace Posto.Domain.Tests;

public class CnpjTests
{
    [Theory]
    [InlineData(" 08.726.064/0001-86", "08726064000186")] // formato da ANP, com espaço na frente
    [InlineData("08726064000186", "08726064000186")]
    [InlineData("12.ABC.345/01DE-35", "12ABC34501DE35")] // exemplo oficial de CNPJ alfanumérico
    [InlineData("12abc34501de35", "12ABC34501DE35")]
    public void Criar_ComCnpjValido_Normaliza(string entrada, string esperado)
    {
        var result = Cnpj.Criar(entrada);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value.Valor);
    }

    [Theory]
    [InlineData("08.726.064/0001-87")] // dígito verificador errado
    [InlineData("0872606400018")] // 13 caracteres
    [InlineData("11111111111111")] // todos iguais
    [InlineData("12ABC34501DE3A")] // dígito verificador não numérico
    public void Criar_ComCnpjInvalido_Falha(string entrada)
    {
        var result = Cnpj.Criar(entrada);

        Assert.Equal(PostoErrors.CnpjInvalido, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Criar_Vazio_Falha(string? entrada)
    {
        Assert.Equal(PostoErrors.CnpjVazio, Cnpj.Criar(entrada).Error);
    }

    [Fact]
    public void Formatado_AplicaMascara()
    {
        Assert.Equal("08.726.064/0001-86", Cnpj.Criar("08726064000186").Value.Formatado);
    }

    [Fact]
    public void CnpjsIguais_SaoIguais()
    {
        Assert.Equal(Cnpj.Criar("08.726.064/0001-86").Value, Cnpj.Criar("08726064000186").Value);
    }
}
