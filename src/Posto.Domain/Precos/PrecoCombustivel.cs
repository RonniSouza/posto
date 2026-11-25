using Posto.Domain.Common;

namespace Posto.Domain.Precos;

/// <summary>
/// Preço de venda de um combustível em um posto, numa data de coleta, segundo uma fonte.
/// É um agregado separado do Posto: o histórico cresce sem limite e não precisa ser
/// carregado junto com o cadastro.
/// </summary>
public sealed class PrecoCombustivel : Entity<Guid>
{
    /// <summary>Teto de sanidade, em reais, para barrar erros de digitação na fonte.</summary>
    public const decimal ValorMaximo = 100m;

    private PrecoCombustivel(Guid id, Guid postoId, TipoCombustivel combustivel, decimal valor, DateOnly dataColeta, FontePreco fonte)
        : base(id)
    {
        PostoId = postoId;
        Combustivel = combustivel;
        Valor = valor;
        DataColeta = dataColeta;
        Fonte = fonte;
    }

    // Usado pelo EF Core ao materializar a entidade.
    private PrecoCombustivel()
        : base(Guid.Empty)
    {
    }

    public Guid PostoId { get; private set; }

    public TipoCombustivel Combustivel { get; private set; }

    /// <summary>Preço em reais por <see cref="Unidade"/>.</summary>
    public decimal Valor { get; private set; }

    public DateOnly DataColeta { get; private set; }

    public FontePreco Fonte { get; private set; }

    public UnidadeMedida Unidade => Combustivel.Unidade();

    /// <param name="hoje">Data atual no fuso do posto; coletas posteriores são rejeitadas.</param>
    public static Result<PrecoCombustivel> Criar(
        Guid postoId,
        TipoCombustivel combustivel,
        decimal valor,
        DateOnly dataColeta,
        FontePreco fonte,
        DateOnly hoje)
    {
        if (postoId == Guid.Empty)
        {
            return Result.Failure<PrecoCombustivel>(PrecoErrors.PostoObrigatorio);
        }

        if (!Enum.IsDefined(combustivel))
        {
            return Result.Failure<PrecoCombustivel>(PrecoErrors.CombustivelInvalido);
        }

        if (!Enum.IsDefined(fonte))
        {
            return Result.Failure<PrecoCombustivel>(PrecoErrors.FonteInvalida);
        }

        if (valor is <= 0 or >= ValorMaximo)
        {
            return Result.Failure<PrecoCombustivel>(PrecoErrors.ValorInvalido);
        }

        if (dataColeta > hoje)
        {
            return Result.Failure<PrecoCombustivel>(PrecoErrors.DataNoFuturo);
        }

        return Result.Success(new PrecoCombustivel(Guid.CreateVersion7(), postoId, combustivel, valor, dataColeta, fonte));
    }
}
