using Posto.Domain.Common;

namespace Posto.Domain.Postos;

public static class PostoErrors
{
    public static readonly Error CnpjVazio = new("Posto.CnpjVazio", "O CNPJ é obrigatório.");

    public static readonly Error CnpjInvalido = new("Posto.CnpjInvalido", "O CNPJ informado é inválido.");

    public static readonly Error RazaoSocialVazia = new("Posto.RazaoSocialVazia", "A razão social é obrigatória.");

    public static readonly Error LogradouroVazio = new("Posto.LogradouroVazio", "O logradouro é obrigatório.");

    public static readonly Error MunicipioVazio = new("Posto.MunicipioVazio", "O município é obrigatório.");

    public static readonly Error UfInvalida = new("Posto.UfInvalida", "A UF deve ter duas letras.");

    public static readonly Error CepInvalido = new("Posto.CepInvalido", "O CEP deve ter 8 dígitos.");

    public static readonly Error LocalizacaoInvalida = new(
        "Posto.LocalizacaoInvalida",
        "Latitude deve estar entre -90 e 90 e longitude entre -180 e 180.");
}
