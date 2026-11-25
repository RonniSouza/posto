using System.Runtime.CompilerServices;
using Posto.Application.Abstractions;
using Posto.Application.Precos.ImportarAnp;
using Posto.Domain.Postos;
using Posto.Domain.Precos;
using PostoEntity = Posto.Domain.Postos.Posto;

namespace Posto.Application.Tests;

internal sealed class FonteFake(params RegistroPrecoAnp[] registros) : IFonteDadosAnp
{
    public async IAsyncEnumerable<RegistroPrecoAnp> LerRegistrosAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var registro in registros)
        {
            await Task.Yield();
            yield return registro;
        }
    }
}

/// <summary>Simula o banco: o que foi adicionado só "persiste" no SaveChanges.</summary>
internal sealed class BancoFake : IPostoRepository, IPrecoCombustivelRepository, IUnitOfWork
{
    private readonly List<PostoEntity> _postosPendentes = [];
    private readonly List<PrecoCombustivel> _precosPendentes = [];

    public List<PostoEntity> Postos { get; } = [];

    public List<PrecoCombustivel> Precos { get; } = [];

    public int SaveChangesChamadas { get; private set; }

    public Task<IReadOnlyList<PostoEntity>> ListarPorCnpjsAsync(IReadOnlyCollection<Cnpj> cnpjs, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PostoEntity>>(Postos.Where(p => cnpjs.Contains(p.Cnpj)).ToList());

    public void Adicionar(PostoEntity posto) => _postosPendentes.Add(posto);

    public Task<IReadOnlySet<ChavePreco>> ListarChavesAsync(
        IReadOnlyCollection<Guid> postoIds,
        DateOnly desde,
        FontePreco fonte,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlySet<ChavePreco>>(Precos
            .Where(p => postoIds.Contains(p.PostoId) && p.DataColeta >= desde && p.Fonte == fonte)
            .Select(p => new ChavePreco(p.PostoId, p.Combustivel, p.DataColeta))
            .ToHashSet());

    public void AdicionarVarios(IEnumerable<PrecoCombustivel> precos) => _precosPendentes.AddRange(precos);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesChamadas++;
        var total = _postosPendentes.Count + _precosPendentes.Count;
        Postos.AddRange(_postosPendentes);
        Precos.AddRange(_precosPendentes);
        _postosPendentes.Clear();
        _precosPendentes.Clear();
        return Task.FromResult(total);
    }
}

internal sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => agora;
}
