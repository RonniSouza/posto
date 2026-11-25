using Posto.Domain.Common;

namespace Posto.Domain.Precos;

public static class PrecoErrors
{
    public static readonly Error PostoObrigatorio = new("Preco.PostoObrigatorio", "O preço precisa estar associado a um posto.");

    public static readonly Error CombustivelInvalido = new("Preco.CombustivelInvalido", "Tipo de combustível desconhecido.");

    public static readonly Error FonteInvalida = new("Preco.FonteInvalida", "Fonte de preço desconhecida.");

    public static readonly Error ValorInvalido = new(
        "Preco.ValorInvalido",
        $"O valor deve ser maior que zero e menor que {PrecoCombustivel.ValorMaximo}.");

    public static readonly Error DataNoFuturo = new("Preco.DataNoFuturo", "A data da coleta não pode estar no futuro.");
}
