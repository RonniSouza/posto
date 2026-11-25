namespace Posto.Domain.Precos;

public enum TipoCombustivel
{
    Gasolina = 1,
    GasolinaAditivada = 2,
    Etanol = 3,
    Diesel = 4,
    DieselS10 = 5,
    Gnv = 6,
}

public enum UnidadeMedida
{
    Litro = 1,
    MetroCubico = 2,
}

public static class TipoCombustivelExtensions
{
    public static UnidadeMedida Unidade(this TipoCombustivel tipo) =>
        tipo == TipoCombustivel.Gnv ? UnidadeMedida.MetroCubico : UnidadeMedida.Litro;
}
