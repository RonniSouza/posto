using Posto.Domain.Common;

namespace Posto.Domain.Tests;

public class ResultTests
{
    [Fact]
    public void Success_DeveRetornarValor()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_DeveExporErroEImpedirAcessoAoValor()
    {
        var erro = new Error("Posto.NaoEncontrado", "Posto não encontrado.");

        var result = Result.Failure<int>(erro);

        Assert.True(result.IsFailure);
        Assert.Equal(erro, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
