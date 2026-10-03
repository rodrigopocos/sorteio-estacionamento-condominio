using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class CriarRodada
{
    private readonly IRodadaRepositorio _rodadas;

    public CriarRodada(IRodadaRepositorio rodadas) => _rodadas = rodadas;

    public async Task<RodadaDetalheDto> ExecutarAsync(
        CriarRodadaCommand comando,
        CancellationToken cancellationToken = default)
    {
        if (comando.Ano < 2000)
            throw new FalhaRegraNegocioException(CodigosErro.CadastroInvalido, "Ano da rodada inválido.");

        var existente = await _rodadas.ObterPorAnoAsync(comando.Ano, cancellationToken);
        if (existente is not null)
            throw new FalhaRegraNegocioException(CodigosErro.CadastroDuplicado, "Já existe rodada para este ano.");

        var rodada = Rodada.Criar(comando.Ano, DateTimeOffset.UtcNow);
        await _rodadas.AdicionarAsync(rodada, cancellationToken);
        return RodadaDetalheDto.De(rodada, new Dictionary<Guid, Unidade>());
    }
}
