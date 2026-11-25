using Posto.Domain.Common;

namespace Posto.Application.Precos.ImportarAnp;

public static class AnpErrors
{
    public static readonly Error FonteVazia = new("Anp.FonteVazia", "A fonte da ANP não retornou nenhum registro.");

    public static readonly Error FiltroInvalido = new("Anp.FiltroInvalido", "Informe a UF e ao menos um município.");

    public static readonly Error ProdutoNaoSuportado = new("Anp.ProdutoNaoSuportado", "Produto da ANP não suportado.");

    public static readonly Error DataInvalida = new("Anp.DataInvalida", "Data da coleta fora do formato dd/MM/yyyy.");

    public static readonly Error ValorInvalido = new("Anp.ValorInvalido", "Valor de venda fora do formato 0,00.");
}
