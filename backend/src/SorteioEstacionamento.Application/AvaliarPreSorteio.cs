using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class AvaliarPreSorteio
{
    private readonly IRodadaRepositorio _rodadas;
    private readonly IVagaRepositorio _vagas;

    public AvaliarPreSorteio(IRodadaRepositorio rodadas, IVagaRepositorio vagas)
    {
        _rodadas = rodadas;
        _vagas = vagas;
    }

    public async Task<ResultadoValidacaoPreSorteio> ExecutarAsync(
        Guid rodadaId,
        CancellationToken cancellationToken = default)
    {
        var rodada = await _rodadas.ObterPorIdAsync(rodadaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Rodada não encontrada.");

        var vagas = await _vagas.ListarTodasAsync(cancellationToken);
        return ValidadorPreSorteio.Validar(rodada, vagas);
    }
}
