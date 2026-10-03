namespace SorteioEstacionamento.Domain;

public sealed record ResultadoExecucaoSorteio(
    byte[] Seed,
    IReadOnlyList<string> OrdemUnidades,
    IReadOnlyList<ResultadoSorteioUnidade> ResultadosPorUnidade);
