using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class ObterDetalheRodada
{
    private readonly IRodadaRepositorio _rodadas;
    private readonly IUnidadeRepositorio _unidades;
    private readonly IVagaRepositorio _vagas;

    public ObterDetalheRodada(
        IRodadaRepositorio rodadas,
        IUnidadeRepositorio unidades,
        IVagaRepositorio vagas)
    {
        _rodadas = rodadas;
        _unidades = unidades;
        _vagas = vagas;
    }

    public async Task<RodadaDetalheDto> ExecutarAsync(
        Guid rodadaId,
        CancellationToken cancellationToken = default)
    {
        var rodada = await _rodadas.ObterPorIdAsync(rodadaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Rodada não encontrada.");

        var unidades = (await _unidades.ListarTodasAsync(cancellationToken))
            .ToDictionary(u => u.Id);

        ResultadoValidacaoPreSorteio? validacao = null;
        if (rodada.Estado == EstadoRodada.Ativa)
        {
            var vagas = await _vagas.ListarTodasAsync(cancellationToken);
            validacao = ValidadorPreSorteio.Validar(rodada, vagas);
        }

        return RodadaDetalheDto.De(rodada, unidades, validacao);
    }
}
