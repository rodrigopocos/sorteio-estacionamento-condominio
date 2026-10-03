using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class CancelarSorteio
{
    private readonly IRodadaRepositorio _rodadas;

    public CancelarSorteio(IRodadaRepositorio rodadas) => _rodadas = rodadas;

    public async Task ExecutarAsync(Guid rodadaId, CancellationToken cancellationToken = default)
    {
        var rodada = await _rodadas.ObterPorIdAsync(rodadaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Rodada não encontrada.");

        rodada.CancelarSorteio(DateTimeOffset.UtcNow);
        await _rodadas.AtualizarAsync(rodada, cancellationToken);
    }
}
