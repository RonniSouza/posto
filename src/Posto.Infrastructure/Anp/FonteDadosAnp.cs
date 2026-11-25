using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Posto.Application.Precos.ImportarAnp;

namespace Posto.Infrastructure.Anp;

/// <summary>Baixa (ou abre, se for caminho local) cada arquivo configurado e lê as linhas em streaming.</summary>
internal sealed partial class FonteDadosAnp(HttpClient http, IOptions<AnpOptions> options, ILogger<FonteDadosAnp> logger) : IFonteDadosAnp
{
    public async IAsyncEnumerable<RegistroPrecoAnp> LerRegistrosAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var arquivo in options.Value.Arquivos)
        {
            LogLendoArquivo(arquivo);
            await using var stream = await AbrirAsync(arquivo, cancellationToken);

            await foreach (var registro in LeitorCsvAnp.LerAsync(stream, cancellationToken))
            {
                yield return registro;
            }
        }
    }

    private async Task<Stream> AbrirAsync(string arquivo, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(arquivo, UriKind.Absolute, out var uri) || uri.IsFile)
        {
            return File.OpenRead(arquivo);
        }

        // ResponseHeadersRead: começa a ler antes de o arquivo (~8 MB) terminar de baixar.
        var resposta = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        resposta.EnsureSuccessStatusCode();
        return await resposta.Content.ReadAsStreamAsync(cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Lendo arquivo da ANP: {Arquivo}")]
    private partial void LogLendoArquivo(string arquivo);
}
