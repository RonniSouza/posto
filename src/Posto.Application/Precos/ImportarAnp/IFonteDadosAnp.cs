namespace Posto.Application.Precos.ImportarAnp;

/// <summary>Entrega as linhas publicadas pela ANP (implementada na Infrastructure via download do CSV).</summary>
public interface IFonteDadosAnp
{
    IAsyncEnumerable<RegistroPrecoAnp> LerRegistrosAsync(CancellationToken cancellationToken = default);
}
