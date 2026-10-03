using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class RemoverRodada
{
    private readonly IRodadaRepositorio _rodadas;

    public RemoverRodada(IRodadaRepositorio rodadas) => _rodadas = rodadas;

    public async Task ExecutarAsync(Guid rodadaId, CancellationToken cancellationToken = default)
    {
        var rodada = await _rodadas.ObterPorIdAsync(rodadaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Rodada não encontrada.");

        await _rodadas.RemoverAsync(rodada, cancellationToken);
    }
}
