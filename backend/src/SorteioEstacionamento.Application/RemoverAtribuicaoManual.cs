using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed record RemoverAtribuicaoManualCommand(Guid RodadaId, Guid AtribuicaoId);

public sealed class RemoverAtribuicaoManual
{
    private readonly IRodadaRepositorio _rodadas;

    public RemoverAtribuicaoManual(IRodadaRepositorio rodadas)
    {
        _rodadas = rodadas;
    }

    public async Task ExecutarAsync(
        RemoverAtribuicaoManualCommand comando,
        CancellationToken cancellationToken = default)
    {
        var rodada = await _rodadas.ObterPorIdAsync(comando.RodadaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Rodada não encontrada.");

        rodada.RemoverManual(comando.AtribuicaoId);
        await _rodadas.AtualizarAsync(rodada, cancellationToken);
    }
}
