using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed record CriarRodadaCommand(int Ano);

public sealed record RodadaResumoDto(
    Guid Id,
    int Ano,
    EstadoRodada Estado,
    DateTimeOffset CriadaEm,
    DateTimeOffset? AtivadaEm,
    DateTimeOffset? SorteadaEm);

public sealed record ContemplacaoUnidadeDto(
    Guid UnidadeId,
    string Numero,
    StatusContemplacao Status);

public sealed record AtribuirManualRequest(Guid UnidadeId, Guid VagaId);

public sealed record AtribuicaoCriadaDto(Guid Id);

public sealed record InscricaoDto(Guid UnidadeId, string Numero);

public sealed record AtribuicaoRodadaDto(
    Guid Id,
    Guid UnidadeId,
    string UnidadeNumero,
    Guid VagaId,
    string VagaCodigo,
    TipoVaga Tipo,
    CoberturaVaga Cobertura,
    bool EhPcd,
    OrigemAtribuicao Origem);

public sealed record RodadaDetalheDto(
    Guid Id,
    int Ano,
    EstadoRodada Estado,
    DateTimeOffset CriadaEm,
    DateTimeOffset? AtivadaEm,
    DateTimeOffset? SorteadaEm,
    IReadOnlyList<InscricaoDto> Inscricoes,
    IReadOnlyList<AtribuicaoRodadaDto> Atribuicoes,
    IReadOnlyList<AvisoDto> Avisos,
    IReadOnlyList<AvisoDto> Bloqueios,
    int? SequenciaVigente,
    IReadOnlyList<ContemplacaoUnidadeDto> Contemplacoes)
{
    public static RodadaDetalheDto De(
        Rodada rodada,
        IReadOnlyDictionary<Guid, Unidade> unidades,
        ResultadoValidacaoPreSorteio? validacao = null)
    {
        var inscricoes = rodada.Inscricoes
            .Select(i => new InscricaoDto(i.UnidadeId, NumeroDe(unidades, i.UnidadeId)))
            .OrderBy(i => i.Numero)
            .ToList();

        var atribuicoes = rodada.Atribuicoes
            .Select(a => new AtribuicaoRodadaDto(
                a.Id,
                a.UnidadeId,
                NumeroDe(unidades, a.UnidadeId),
                a.VagaId,
                a.DadosVaga.Codigo,
                a.DadosVaga.Tipo,
                a.DadosVaga.Cobertura,
                a.DadosVaga.EhPcd,
                a.Origem))
            .ToList();

        var avisos = validacao?.Avisos.Select(a => new AvisoDto(a.Codigo, a.Mensagem)).ToList()
            ?? [];
        var bloqueios = validacao?.Bloqueios.Select(b => new AvisoDto(b.Codigo, b.Mensagem)).ToList()
            ?? [];

        var sequencia = rodada.Execucoes.Count == 0
            ? (int?)null
            : rodada.Execucoes.Max(e => e.Sequencia);

        var contemplacoes = rodada.Estado != EstadoRodada.Sorteada
            ? []
            : rodada.Inscricoes
                .Select(i =>
                {
                    var snaps = rodada.Atribuicoes
                        .Where(a => a.UnidadeId == i.UnidadeId)
                        .Select(a => a.ParaSnapshot())
                        .ToList();
                    return new ContemplacaoUnidadeDto(
                        i.UnidadeId,
                        NumeroDe(unidades, i.UnidadeId),
                        CalculadorStatusContemplacao.Calcular(snaps));
                })
                .OrderBy(c => c.Numero)
                .ToList();

        return new RodadaDetalheDto(
            rodada.Id,
            rodada.Ano,
            rodada.Estado,
            rodada.CriadaEm,
            rodada.AtivadaEm,
            rodada.SorteadaEm,
            inscricoes,
            atribuicoes,
            avisos,
            bloqueios,
            sequencia,
            contemplacoes);
    }

    private static string NumeroDe(IReadOnlyDictionary<Guid, Unidade> unidades, Guid unidadeId) =>
        unidades.TryGetValue(unidadeId, out var unidade) ? unidade.Numero : "";
}
