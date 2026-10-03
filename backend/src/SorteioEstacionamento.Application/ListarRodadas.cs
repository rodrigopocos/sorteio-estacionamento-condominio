namespace SorteioEstacionamento.Application;

public sealed class ListarRodadas
{
    private readonly IRodadaRepositorio _rodadas;

    public ListarRodadas(IRodadaRepositorio rodadas) => _rodadas = rodadas;

    public async Task<IReadOnlyList<RodadaResumoDto>> ExecutarAsync(
        CancellationToken cancellationToken = default)
    {
        var itens = await _rodadas.ListarAsync(cancellationToken);
        return itens
            .Select(r => new RodadaResumoDto(
                r.Id,
                r.Ano,
                r.Estado,
                r.CriadaEm,
                r.AtivadaEm,
                r.SorteadaEm))
            .ToList();
    }
}
