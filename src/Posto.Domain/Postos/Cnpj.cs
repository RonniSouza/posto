using Posto.Domain.Common;

namespace Posto.Domain.Postos;

/// <summary>
/// CNPJ normalizado (sem pontuação). Aceita o formato numérico e o alfanumérico,
/// adotado pela Receita Federal a partir de julho de 2026: 12 caracteres [0-9A-Z]
/// seguidos de 2 dígitos verificadores numéricos.
/// </summary>
public sealed record Cnpj
{
    private const int Tamanho = 14;
    private static readonly int[] PesosPrimeiroDigito = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] PesosSegundoDigito = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private Cnpj(string valor) => Valor = valor;

    public string Valor { get; }

    public static Result<Cnpj> Criar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return Result.Failure<Cnpj>(PostoErrors.CnpjVazio);
        }

        var normalizado = string.Concat(texto
            .Where(c => c is not ('.' or '/' or '-') && !char.IsWhiteSpace(c))
            .Select(char.ToUpperInvariant));

        return EhValido(normalizado)
            ? Result.Success(new Cnpj(normalizado))
            : Result.Failure<Cnpj>(PostoErrors.CnpjInvalido);
    }

    /// <summary>Formato de exibição: 00.000.000/0000-00.</summary>
    public string Formatado => $"{Valor[..2]}.{Valor[2..5]}.{Valor[5..8]}/{Valor[8..12]}-{Valor[12..]}";

    public override string ToString() => Valor;

    private static bool EhValido(string cnpj)
    {
        if (cnpj.Length != Tamanho
            || !cnpj[..12].All(c => char.IsAsciiDigit(c) || char.IsAsciiLetterUpper(c))
            || !cnpj[12..].All(char.IsAsciiDigit)
            || cnpj.All(c => c == cnpj[0]))
        {
            return false;
        }

        var primeiro = CalcularDigito(cnpj.AsSpan(0, 12), PesosPrimeiroDigito);
        var segundo = CalcularDigito(cnpj.AsSpan(0, 13), PesosSegundoDigito);

        return cnpj[12] - '0' == primeiro && cnpj[13] - '0' == segundo;
    }

    // Cada caractere vale (código ASCII - 48): dígitos valem 0-9 e letras A-Z valem 17-42.
    private static int CalcularDigito(ReadOnlySpan<char> prefixo, int[] pesos)
    {
        var soma = 0;
        for (var i = 0; i < prefixo.Length; i++)
        {
            soma += (prefixo[i] - '0') * pesos[i];
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
