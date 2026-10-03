using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class ExecutarSorteio
{
    private readonly IRodadaRepositorio _rodadas;
    private readonly IVagaRepositorio _vagas;
    private readonly IUnidadeRepositorio _unidades;
    private readonly IGeradorSeedSorteio _geradorSeed;

    public ExecutarSorteio(
        IRodadaRepositorio rodadas,
        IVagaRepositorio vagas,
        IUnidadeRepositorio unidades,
        IGeradorSeedSorteio geradorSeed)
    {
        _rodadas = rodadas;
        _vagas = vagas;
        _unidades = unidades;
        _geradorSeed = geradorSeed;
    }

    public async Task<ResultadoSorteioDto> ExecutarAsync(
        Guid rodadaId,
        CancellationToken cancellationToken = default)
    {
        var rodada = await _rodadas.ObterPorIdAsync(rodadaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Rodada não encontrada.");

        var vagas = await _vagas.ListarTodasAsync(cancellationToken);
        var seed = _geradorSeed.Gerar32Bytes();
        var resultado = rodada.ExecutarSorteio(vagas, seed, DateTimeOffset.UtcNow);
        await _rodadas.AtualizarAsync(rodada, cancellationToken);

        var unidades = (await _unidades.ListarTodasAsync(cancellationToken))
            .ToDictionary(u => u.Id);
        return ResultadoSorteioDto.De(rodada, resultado, unidades);
    }
}
