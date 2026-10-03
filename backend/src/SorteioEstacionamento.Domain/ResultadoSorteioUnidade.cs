namespace SorteioEstacionamento.Domain;

public sealed record ResultadoSorteioUnidade(
    string UnidadeId,
    IReadOnlyList<AtribuicaoSnapshot> AtribuicoesSorteio,
    StatusContemplacao Status);
