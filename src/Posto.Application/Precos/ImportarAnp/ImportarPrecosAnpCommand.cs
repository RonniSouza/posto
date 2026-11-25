namespace Posto.Application.Precos.ImportarAnp;

/// <param name="Uf">Sigla do estado, ex.: PE.</param>
/// <param name="Municipios">Nomes como a ANP publica (maiúsculas, sem acento), ex.: RECIFE, OLINDA.</param>
public sealed record ImportarPrecosAnpCommand(string Uf, IReadOnlyCollection<string> Municipios);

public sealed record ResumoImportacaoAnp(
    int RegistrosLidos,
    int RegistrosDaRegiao,
    int RegistrosInvalidos,
    int PostosCriados,
    int PostosAtualizados,
    int PrecosInseridos,
    int PrecosJaExistentes,
    IReadOnlyList<string> AmostraDeErros);
