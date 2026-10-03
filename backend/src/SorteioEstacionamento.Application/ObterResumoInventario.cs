using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class ObterResumoInventario
{
    private readonly IUnidadeRepositorio _unidades;
    private readonly IVagaRepositorio _vagas;

    public ObterResumoInventario(IUnidadeRepositorio unidades, IVagaRepositorio vagas)
    {
        _unidades = unidades;
        _vagas = vagas;
    }

    public async Task<ResumoInventarioDto> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        var unidades = await _unidades.ListarTodasAsync(cancellationToken);
        var vagas = await _vagas.ListarTodasAsync(cancellationToken);
        var lugares = vagas.Sum(v => CalculadorLugaresFisicos.Calcular(v.Tipo));
        return new ResumoInventarioDto(unidades.Count, vagas.Count, lugares, vagas.Count(v => v.EhPcd));
    }
}
