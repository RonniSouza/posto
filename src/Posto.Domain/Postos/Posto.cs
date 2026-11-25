using NetTopologySuite.Geometries;
using Posto.Domain.Common;

namespace Posto.Domain.Postos;

/// <summary>
/// Posto revendedor de combustíveis, identificado pelo CNPJ.
/// </summary>
/// <remarks>
/// A classe tem o mesmo nome do namespace raiz (<c>Posto</c>). Fora deste namespace,
/// referencie-a por um alias (<c>using PostoEntity = Posto.Domain.Postos.Posto;</c>),
/// senão o compilador entende <c>Posto</c> como o namespace.
/// </remarks>
public sealed class Posto : Entity<Guid>
{
    /// <summary>WGS 84, o sistema de coordenadas do GPS (latitude/longitude).</summary>
    public const int Srid = 4326;

    private Posto(Guid id, Cnpj cnpj, string razaoSocial, string? bandeira, Endereco endereco)
        : base(id)
    {
        Cnpj = cnpj;
        RazaoSocial = razaoSocial;
        Bandeira = bandeira;
        Endereco = endereco;
    }

    // Usado pelo EF Core ao materializar a entidade.
    private Posto()
        : base(Guid.Empty)
    {
    }

    public Cnpj Cnpj { get; private set; } = null!;

    public string RazaoSocial { get; private set; } = null!;

    /// <summary>Distribuidora (VIBRA, IPIRANGA, RAIZEN...) ou BRANCA, quando não tem bandeira.</summary>
    public string? Bandeira { get; private set; }

    public Endereco Endereco { get; private set; } = null!;

    /// <summary>
    /// Ponto geográfico do posto. A ANP não publica coordenadas, então fica nulo
    /// até o endereço ser geocodificado.
    /// </summary>
    public Point? Localizacao { get; private set; }

    public static Result<Posto> Criar(Cnpj cnpj, string? razaoSocial, string? bandeira, Endereco endereco)
    {
        ArgumentNullException.ThrowIfNull(cnpj);
        ArgumentNullException.ThrowIfNull(endereco);

        if (string.IsNullOrWhiteSpace(razaoSocial))
        {
            return Result.Failure<Posto>(PostoErrors.RazaoSocialVazia);
        }

        return Result.Success(new Posto(Guid.CreateVersion7(), cnpj, razaoSocial.Trim(), Opcional(bandeira), endereco));
    }

    public Result AtualizarCadastro(string? razaoSocial, string? bandeira, Endereco endereco)
    {
        ArgumentNullException.ThrowIfNull(endereco);

        if (string.IsNullOrWhiteSpace(razaoSocial))
        {
            return Result.Failure(PostoErrors.RazaoSocialVazia);
        }

        RazaoSocial = razaoSocial.Trim();
        Bandeira = Opcional(bandeira);
        Endereco = endereco;
        return Result.Success();
    }

    public Result DefinirLocalizacao(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
        {
            return Result.Failure(PostoErrors.LocalizacaoInvalida);
        }

        // No NetTopologySuite, X é a longitude e Y é a latitude.
        Localizacao = new Point(longitude, latitude) { SRID = Srid };
        return Result.Success();
    }

    private static string? Opcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
