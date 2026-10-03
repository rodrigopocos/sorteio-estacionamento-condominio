namespace SorteioEstacionamento.Domain;

public sealed record VagaSnapshot(
    string Codigo,
    TipoVaga Tipo,
    CoberturaVaga Cobertura,
    bool EhPcd);
