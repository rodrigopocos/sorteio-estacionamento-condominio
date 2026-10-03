namespace SorteioEstacionamento.Domain;

public sealed record AtribuicaoSnapshot(
    string UnidadeId,
    VagaSnapshot Vaga,
    OrigemAtribuicao Origem);
