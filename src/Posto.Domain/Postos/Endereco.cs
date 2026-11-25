using Posto.Domain.Common;

namespace Posto.Domain.Postos;

public sealed record Endereco
{
    private Endereco(string logradouro, string? numero, string? complemento, string? bairro, string? cep, string municipio, string uf)
    {
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cep = cep;
        Municipio = municipio;
        Uf = uf;
    }

    public string Logradouro { get; }

    public string? Numero { get; }

    public string? Complemento { get; }

    public string? Bairro { get; }

    /// <summary>Somente dígitos (8), ou nulo quando a fonte não informa.</summary>
    public string? Cep { get; }

    public string Municipio { get; }

    public string Uf { get; }

    public static Result<Endereco> Criar(
        string? logradouro,
        string? numero,
        string? complemento,
        string? bairro,
        string? cep,
        string? municipio,
        string? uf)
    {
        if (string.IsNullOrWhiteSpace(logradouro))
        {
            return Result.Failure<Endereco>(PostoErrors.LogradouroVazio);
        }

        if (string.IsNullOrWhiteSpace(municipio))
        {
            return Result.Failure<Endereco>(PostoErrors.MunicipioVazio);
        }

        var ufNormalizada = uf?.Trim().ToUpperInvariant();
        if (ufNormalizada is not { Length: 2 } || !ufNormalizada.All(char.IsAsciiLetterUpper))
        {
            return Result.Failure<Endereco>(PostoErrors.UfInvalida);
        }

        string? cepNormalizado = null;
        if (!string.IsNullOrWhiteSpace(cep))
        {
            cepNormalizado = string.Concat(cep.Where(char.IsAsciiDigit));
            if (cepNormalizado.Length != 8)
            {
                return Result.Failure<Endereco>(PostoErrors.CepInvalido);
            }
        }

        return Result.Success(new Endereco(
            logradouro.Trim(),
            Opcional(numero),
            Opcional(complemento),
            Opcional(bairro),
            cepNormalizado,
            municipio.Trim(),
            ufNormalizada));
    }

    private static string? Opcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
