using Microsoft.EntityFrameworkCore;
using Posto.Application.Abstractions;
using Posto.Domain.Postos;
using PostoEntity = Posto.Domain.Postos.Posto;

namespace Posto.Infrastructure.Persistence.Repositories;

internal sealed class PostoRepository(PostoDbContext context) : IPostoRepository
{
    public async Task<IReadOnlyList<PostoEntity>> ListarPorCnpjsAsync(IReadOnlyCollection<Cnpj> cnpjs, CancellationToken cancellationToken = default) =>
        await context.Postos
            .Where(p => cnpjs.Contains(p.Cnpj))
            .ToListAsync(cancellationToken);

    public void Adicionar(PostoEntity posto) => context.Postos.Add(posto);
}
