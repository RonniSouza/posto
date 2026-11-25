using Posto.Domain.Precos;

namespace Posto.Application.Abstractions;

/// <summary>Identifica um preço de forma única: um valor por posto, combustível, data e fonte.</summary>
public readonly record struct ChavePreco(Guid PostoId, TipoCombustivel Combustivel, DateOnly DataColeta);

public interface IPrecoCombustivelRepository
{
    /// <summary>Chaves dos preços já gravados para os postos informados, a partir de <paramref name="desde"/>.</summary>
    Task<IReadOnlySet<ChavePreco>> ListarChavesAsync(
        IReadOnlyCollection<Guid> postoIds,
        DateOnly desde,
        FontePreco fonte,
        CancellationToken cancellationToken = default);

    void AdicionarVarios(IEnumerable<PrecoCombustivel> precos);
}
