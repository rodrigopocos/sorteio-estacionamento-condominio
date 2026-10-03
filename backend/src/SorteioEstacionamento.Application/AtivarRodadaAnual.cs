using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class AtivarRodadaAnual
{
    private readonly IRodadaRepositorio _rodadas;
    private readonly IUnidadeRepositorio _unidades;

    public AtivarRodadaAnual(IRodadaRepositorio rodadas, IUnidadeRepositorio unidades)
    {
        _rodadas = rodadas;
        _unidades = unidades;
    }

    public async Task ExecutarAsync(Guid rodadaId, CancellationToken cancellationToken = default)
    {
        var rodada = await ObterRodadaAsync(rodadaId, cancellationToken);
        var unidades = await _unidades.ListarTodasAsync(cancellationToken);
        rodada.Ativar(unidades, DateTimeOffset.UtcNow);
        await _rodadas.AtualizarAsync(rodada, cancellationToken);
    }

    private async Task<Rodada> ObterRodadaAsync(Guid rodadaId, CancellationToken cancellationToken)
    {
        return await _rodadas.ObterPorIdAsync(rodadaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Rodada não encontrada.");
    }
}
