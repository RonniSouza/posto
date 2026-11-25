using System.Globalization;
using Posto.Domain.Common;
using Posto.Domain.Postos;
using Posto.Domain.Precos;

namespace Posto.Application.Precos.ImportarAnp;

/// <summary>Linha da ANP já convertida para tipos do domínio.</summary>
public sealed record RegistroAnpValido(
    Cnpj Cnpj,
    string? RazaoSocial,
    string? Bandeira,
    Endereco Endereco,
    TipoCombustivel Combustivel,
    decimal Valor,
    DateOnly DataColeta);

/// <summary>Converte os textos do CSV da ANP (vírgula decimal, datas dd/MM/yyyy, produtos em maiúsculas).</summary>
public static class ConversorRegistroAnp
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly Dictionary<string, TipoCombustivel> Produtos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GASOLINA"] = TipoCombustivel.Gasolina,
        ["GASOLINA ADITIVADA"] = TipoCombustivel.GasolinaAditivada,
        ["ETANOL"] = TipoCombustivel.Etanol,
        ["DIESEL"] = TipoCombustivel.Diesel,
        ["DIESEL S10"] = TipoCombustivel.DieselS10,
        ["GNV"] = TipoCombustivel.Gnv,
    };

    public static Result<RegistroAnpValido> Converter(RegistroPrecoAnp registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        var cnpj = Cnpj.Criar(registro.Cnpj);
        if (cnpj.IsFailure)
        {
            return Result.Failure<RegistroAnpValido>(cnpj.Error);
        }

        var endereco = Endereco.Criar(
            registro.NomeRua,
            registro.NumeroRua,
            registro.Complemento,
            registro.Bairro,
            registro.Cep,
            registro.Municipio,
            registro.Uf);
        if (endereco.IsFailure)
        {
            return Result.Failure<RegistroAnpValido>(endereco.Error);
        }

        if (!Produtos.TryGetValue(registro.Produto?.Trim() ?? string.Empty, out var combustivel))
        {
            return Result.Failure<RegistroAnpValido>(AnpErrors.ProdutoNaoSuportado);
        }

        if (!DateOnly.TryParseExact(registro.DataColeta?.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
        {
            return Result.Failure<RegistroAnpValido>(AnpErrors.DataInvalida);
        }

        if (!decimal.TryParse(registro.ValorVenda?.Trim(), NumberStyles.AllowDecimalPoint, PtBr, out var valor))
        {
            return Result.Failure<RegistroAnpValido>(AnpErrors.ValorInvalido);
        }

        return Result.Success(new RegistroAnpValido(
            cnpj.Value,
            registro.Revenda,
            registro.Bandeira,
            endereco.Value,
            combustivel,
            valor,
            data));
    }
}
