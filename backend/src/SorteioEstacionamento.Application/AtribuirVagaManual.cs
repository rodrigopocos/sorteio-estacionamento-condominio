using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed record AtribuirVagaManualCommand(Guid RodadaId, Guid UnidadeId, Guid VagaId);

public sealed class AtribuirVagaManual
{
    private readonly IRodadaRepositorio _rodadas;
    private readonly IVagaRepositorio _vagas;

    public AtribuirVagaManual(IRodadaRepositorio rodadas, IVagaRepositorio vagas)
    {
        _rodadas = rodadas;
        _vagas = vagas;
    }

    public async Task<Guid> ExecutarAsync(
        AtribuirVagaManualCommand comando,
        CancellationToken cancellationToken = default)
    {
        var rodada = await _rodadas.ObterPorIdAsync(comando.RodadaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Rodada não encontrada.");

        var vaga = await _vagas.ObterPorIdAsync(comando.VagaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Vaga não encontrada.");

        var atribuicao = rodada.AtribuirManual(comando.UnidadeId, vaga);
        await _rodadas.AtualizarAsync(rodada, cancellationToken);
        return atribuicao.Id;
    }
}
