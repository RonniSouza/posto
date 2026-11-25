namespace Posto.Application.Precos.ImportarAnp;

/// <summary>
/// Uma linha do Levantamento de Preços de Combustíveis da ANP, com os campos como texto,
/// do jeito que vieram do arquivo. A conversão e a validação ficam em <see cref="ConversorRegistroAnp"/>.
/// </summary>
public sealed record RegistroPrecoAnp(
    string? Uf,
    string? Municipio,
    string? Revenda,
    string? Cnpj,
    string? NomeRua,
    string? NumeroRua,
    string? Complemento,
    string? Bairro,
    string? Cep,
    string? Produto,
    string? DataColeta,
    string? ValorVenda,
    string? Bandeira);
