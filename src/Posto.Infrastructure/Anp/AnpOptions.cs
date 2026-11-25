namespace Posto.Infrastructure.Anp;

public sealed class AnpOptions
{
    public const string Secao = "Anp";

    /// <summary>
    /// Arquivos "últimas 4 semanas" do Levantamento de Preços de Combustíveis. Aceita URL http(s)
    /// ou caminho local (útil para importar um arquivo baixado sem internet).
    /// </summary>
    public IList<string> Arquivos { get; init; } = [];
}
