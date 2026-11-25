using Posto.Domain.Postos;
using PostoEntity = Posto.Domain.Postos.Posto;

namespace Posto.Application.Abstractions;

public interface IPostoRepository
{
    Task<IReadOnlyList<PostoEntity>> ListarPorCnpjsAsync(IReadOnlyCollection<Cnpj> cnpjs, CancellationToken cancellationToken = default);

    void Adicionar(PostoEntity posto);
}
