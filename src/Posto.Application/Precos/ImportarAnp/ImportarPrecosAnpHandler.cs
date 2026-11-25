using Posto.Application.Abstractions;
using Posto.Domain.Common;
using Posto.Domain.Postos;
using Posto.Domain.Precos;
using PostoEntity = Posto.Domain.Postos.Posto;

namespace Posto.Application.Precos.ImportarAnp;

/// <summary>
/// Importa os preços da ANP para os municípios pedidos. É idempotente: rodar de novo com o
/// mesmo arquivo não duplica preços (a chave é posto + combustível + data + fonte).
/// </summary>
public sealed class ImportarPrecosAnpHandler(
    IFonteDadosAnp fonte,
    IPostoRepository postos,
    IPrecoCombustivelRepository precos,
    IUnitOfWork unitOfWork,
    TimeProvider relogio)
{
    private const int MaximoErrosNaAmostra = 20;

    // O Brasil não tem horário de verão desde 2019; Recife fica sempre em UTC-3.
    private static readonly TimeSpan FusoRecife = TimeSpan.FromHours(-3);

    public async Task<Result<ResumoImportacaoAnp>> HandleAsync(ImportarPrecosAnpCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Uf) || command.Municipios.Count == 0)
        {
            return Result.Failure<ResumoImportacaoAnp>(AnpErrors.FiltroInvalido);
        }

        var uf = command.Uf.Trim();
        var municipios = command.Municipios.Select(m => m.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // 1. Lê a fonte em streaming e guarda só as linhas da região (alguns milhares de 47 mil).
        var lidos = 0;
        var daRegiao = 0;
        var validos = new List<RegistroAnpValido>();
        var erros = new List<string>();
        var invalidos = 0;

        await foreach (var registro in fonte.LerRegistrosAsync(cancellationToken))
        {
            lidos++;
            if (!string.Equals(registro.Uf?.Trim(), uf, StringComparison.OrdinalIgnoreCase)
                || !municipios.Contains(registro.Municipio?.Trim() ?? string.Empty))
            {
                continue;
            }

            daRegiao++;
            var convertido = ConversorRegistroAnp.Converter(registro);
            if (convertido.IsSuccess)
            {
                validos.Add(convertido.Value);
                continue;
            }

            invalidos++;
            if (erros.Count < MaximoErrosNaAmostra)
            {
                erros.Add($"{registro.Cnpj?.Trim()} {registro.Produto} {registro.DataColeta}: {convertido.Error.Message}");
            }
        }

        if (lidos == 0)
        {
            return Result.Failure<ResumoImportacaoAnp>(AnpErrors.FonteVazia);
        }

        var hoje = DateOnly.FromDateTime(relogio.GetUtcNow().ToOffset(FusoRecife).DateTime);

        // 2. Cria ou atualiza os postos, usando o cadastro da coleta mais recente de cada CNPJ.
        var (postoPorCnpj, criados, atualizados) = await SincronizarPostosAsync(validos, cancellationToken);

        // 3. Insere só os preços que ainda não existem.
        var (inseridos, existentes) = await InserirPrecosAsync(validos, postoPorCnpj, hoje, erros, cancellationToken);
        invalidos += validos.Count - inseridos - existentes;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ResumoImportacaoAnp(
            lidos,
            daRegiao,
            invalidos,
            criados,
            atualizados,
            inseridos,
            existentes,
            erros));
    }

    private async Task<(Dictionary<Cnpj, PostoEntity> PostoPorCnpj, int Criados, int Atualizados)> SincronizarPostosAsync(
        List<RegistroAnpValido> validos,
        CancellationToken cancellationToken)
    {
        var maisRecentePorCnpj = validos
            .GroupBy(r => r.Cnpj)
            .ToDictionary(g => g.Key, g => g.MaxBy(r => r.DataColeta)!);

        var existentes = await postos.ListarPorCnpjsAsync(maisRecentePorCnpj.Keys, cancellationToken);
        var postoPorCnpj = existentes.ToDictionary(p => p.Cnpj);
        int criados = 0, atualizados = 0;

        foreach (var (cnpj, registro) in maisRecentePorCnpj)
        {
            if (postoPorCnpj.TryGetValue(cnpj, out var posto))
            {
                if (posto.AtualizarCadastro(registro.RazaoSocial, registro.Bandeira, registro.Endereco).IsSuccess)
                {
                    atualizados++;
                }

                continue;
            }

            var novo = PostoEntity.Criar(cnpj, registro.RazaoSocial, registro.Bandeira, registro.Endereco);
            if (novo.IsSuccess)
            {
                postos.Adicionar(novo.Value);
                postoPorCnpj[cnpj] = novo.Value;
                criados++;
            }
        }

        return (postoPorCnpj, criados, atualizados);
    }

    private async Task<(int Inseridos, int Existentes)> InserirPrecosAsync(
        List<RegistroAnpValido> validos,
        Dictionary<Cnpj, PostoEntity> postoPorCnpj,
        DateOnly hoje,
        List<string> erros,
        CancellationToken cancellationToken)
    {
        if (validos.Count == 0)
        {
            return (0, 0);
        }

        var desde = validos.Min(r => r.DataColeta);
        var postoIds = postoPorCnpj.Values.Select(p => p.Id).ToList();
        var chaves = (await precos.ListarChavesAsync(postoIds, desde, FontePreco.Anp, cancellationToken)).ToHashSet();

        var novos = new List<PrecoCombustivel>();
        var existentes = 0;

        foreach (var registro in validos)
        {
            if (!postoPorCnpj.TryGetValue(registro.Cnpj, out var posto))
            {
                // Posto rejeitado pelo domínio (ex.: sem razão social): seus preços também ficam de fora.
                AnotarErro(erros, registro, PostoErrors.RazaoSocialVazia);
                continue;
            }

            // HashSet.Add devolve false quando a chave já existe, no banco ou repetida no próprio arquivo.
            if (!chaves.Add(new ChavePreco(posto.Id, registro.Combustivel, registro.DataColeta)))
            {
                existentes++;
                continue;
            }

            var preco = PrecoCombustivel.Criar(posto.Id, registro.Combustivel, registro.Valor, registro.DataColeta, FontePreco.Anp, hoje);
            if (preco.IsSuccess)
            {
                novos.Add(preco.Value);
            }
            else
            {
                AnotarErro(erros, registro, preco.Error);
            }
        }

        precos.AdicionarVarios(novos);
        return (novos.Count, existentes);
    }

    private static void AnotarErro(List<string> erros, RegistroAnpValido registro, Error erro)
    {
        if (erros.Count < MaximoErrosNaAmostra)
        {
            erros.Add($"{registro.Cnpj} {registro.Combustivel} {registro.DataColeta:dd/MM/yyyy}: {erro.Message}");
        }
    }
}
