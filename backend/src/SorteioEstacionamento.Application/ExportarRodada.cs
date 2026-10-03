using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class ExportarRodadaCsv
{
    private readonly IRodadaRepositorio _rodadas;
    private readonly IUnidadeRepositorio _unidades;
    private readonly IExportacaoRodada _exportacao;

    public ExportarRodadaCsv(
        IRodadaRepositorio rodadas,
        IUnidadeRepositorio unidades,
        IExportacaoRodada exportacao)
    {
        _rodadas = rodadas;
        _unidades = unidades;
        _exportacao = exportacao;
    }

    public async Task<ArquivoExportacaoDto> ExecutarAsync(
        Guid rodadaId,
        CancellationToken cancellationToken = default)
    {
        var (rodada, unidades) = await CarregarRodadaSorteadaAsync(rodadaId, cancellationToken);
        var linhas = ExportacaoRodadaMontador.Montar(rodada, unidades);
        var sequencia = rodada.Execucoes.Last().Sequencia;
        return _exportacao.GerarCsv(linhas, rodada.Ano, sequencia);
    }

    private async Task<(Rodada Rodada, IReadOnlyDictionary<Guid, Unidade> Unidades)> CarregarRodadaSorteadaAsync(
        Guid rodadaId,
        CancellationToken cancellationToken)
    {
        var rodada = await _rodadas.ObterPorIdAsync(rodadaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Rodada não encontrada.");

        if (rodada.Estado != EstadoRodada.Sorteada)
            throw new FalhaRegraNegocioException(
                CodigosErro.RodadaEstadoInvalido,
                "A exportação só está disponível após o sorteio da rodada.");

        if (rodada.Execucoes.Count == 0)
            throw new FalhaRegraNegocioException(
                CodigosErro.SorteioBloqueado,
                "Não há execução de sorteio para exportar.");

        var unidades = (await _unidades.ListarTodasAsync(cancellationToken))
            .ToDictionary(u => u.Id);
        return (rodada, unidades);
    }
}

public sealed class ExportarRodadaPdf
{
    private readonly IRodadaRepositorio _rodadas;
    private readonly IUnidadeRepositorio _unidades;
    private readonly IExportacaoRodada _exportacao;

    public ExportarRodadaPdf(
        IRodadaRepositorio rodadas,
        IUnidadeRepositorio unidades,
        IExportacaoRodada exportacao)
    {
        _rodadas = rodadas;
        _unidades = unidades;
        _exportacao = exportacao;
    }

    public async Task<ArquivoExportacaoDto> ExecutarAsync(
        Guid rodadaId,
        CancellationToken cancellationToken = default)
    {
        var rodada = await _rodadas.ObterPorIdAsync(rodadaId, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Rodada não encontrada.");

        if (rodada.Estado != EstadoRodada.Sorteada)
            throw new FalhaRegraNegocioException(
                CodigosErro.RodadaEstadoInvalido,
                "A exportação só está disponível após o sorteio da rodada.");

        var unidades = (await _unidades.ListarTodasAsync(cancellationToken))
            .ToDictionary(u => u.Id);
        var linhas = ExportacaoRodadaMontador.Montar(rodada, unidades);
        var cabecalho = ExportacaoRodadaMontador.MontarCabecalho(rodada, unidades);
        return _exportacao.GerarPdf(cabecalho, linhas);
    }
}
