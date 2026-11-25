using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Posto.Application.Precos.ImportarAnp;

namespace Posto.Infrastructure.Anp;

/// <summary>
/// Lê o CSV da ANP: UTF-8 com BOM, separador ';', primeira linha com cabeçalho.
/// Usa CsvHelper porque alguns campos vêm entre aspas contendo ';' (ex.: "LOTE: 04;").
/// </summary>
internal static class LeitorCsvAnp
{
    private static readonly CsvConfiguration Configuracao = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ";",
        TrimOptions = TrimOptions.Trim,
        // Coluna ausente vira nulo (os mapeamentos são Optional); o ConversorRegistroAnp rejeita a linha se faltar algo obrigatório.
        MissingFieldFound = null,
        HeaderValidated = null,
        BadDataFound = null,
    };

    public static async IAsyncEnumerable<RegistroPrecoAnp> LerAsync(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvReader(reader, Configuracao);
        csv.Context.RegisterClassMap<MapaRegistroAnp>();

        await foreach (var registro in csv.GetRecordsAsync<RegistroPrecoAnp>(cancellationToken))
        {
            yield return registro;
        }
    }

    private sealed class MapaRegistroAnp : ClassMap<RegistroPrecoAnp>
    {
        public MapaRegistroAnp()
        {
            Parameter("Uf").Name("Estado - Sigla").Optional();
            Parameter("Municipio").Name("Municipio").Optional();
            Parameter("Revenda").Name("Revenda").Optional();
            Parameter("Cnpj").Name("CNPJ da Revenda").Optional();
            Parameter("NomeRua").Name("Nome da Rua").Optional();
            Parameter("NumeroRua").Name("Numero Rua").Optional();
            Parameter("Complemento").Name("Complemento").Optional();
            Parameter("Bairro").Name("Bairro").Optional();
            Parameter("Cep").Name("Cep").Optional();
            Parameter("Produto").Name("Produto").Optional();
            Parameter("DataColeta").Name("Data da Coleta").Optional();
            Parameter("ValorVenda").Name("Valor de Venda").Optional();
            Parameter("Bandeira").Name("Bandeira").Optional();
        }
    }
}
