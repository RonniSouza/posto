using Microsoft.EntityFrameworkCore;
using Posto.Application.Abstractions;
using Posto.Domain.Precos;

namespace Posto.Infrastructure.Persistence.Repositories;

internal sealed class PrecoCombustivelRepository(PostoDbContext context) : IPrecoCombustivelRepository
{
    public async Task<IReadOnlySet<ChavePreco>> ListarChavesAsync(
        IReadOnlyCollection<Guid> postoIds,
        DateOnly desde,
        FontePreco fonte,
        CancellationToken cancellationToken = default)
    {
        var chaves = await context.Precos
            .AsNoTracking()
            .Where(p => postoIds.Contains(p.PostoId) && p.DataColeta >= desde && p.Fonte == fonte)
            .Select(p => new ChavePreco(p.PostoId, p.Combustivel, p.DataColeta))
            .ToListAsync(cancellationToken);

        return chaves.ToHashSet();
    }

    public void AdicionarVarios(IEnumerable<PrecoCombustivel> precos) => context.Precos.AddRange(precos);
}
