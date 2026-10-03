using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed record ResultadoUnidadeSorteioDto(
    Guid UnidadeId,
    StatusContemplacao Status,
    IReadOnlyList<AtribuicaoRodadaDto> AtribuicoesSorteio);

public sealed record ResultadoSorteioDto(
    Guid RodadaId,
    int Sequencia,
    string SeedHex,
    DateTimeOffset ExecutadoEm,
    EstadoRodada Estado,
    IReadOnlyList<ResultadoUnidadeSorteioDto> ResultadosPorUnidade,
    IReadOnlyList<AvisoDto> Avisos)
{
    public static ResultadoSorteioDto De(
        Rodada rodada,
        ResultadoExecucaoRodada resultado,
        IReadOnlyDictionary<Guid, Unidade> unidades)
    {
        var execucao = resultado.Execucao;
        var resultados = execucao.Resultado.ResultadosPorUnidade
            .Select(u =>
            {
                var unidadeId = Guid.Parse(u.UnidadeId);
                var atribuicoes = u.AtribuicoesSorteio
                    .Select(a => new AtribuicaoRodadaDto(
                        Guid.Empty,
                        unidadeId,
                        NumeroDe(unidades, unidadeId),
                        Guid.Empty,
                        a.Vaga.Codigo,
                        a.Vaga.Tipo,
                        a.Vaga.Cobertura,
                        a.Vaga.EhPcd,
                        OrigemAtribuicao.Sorteio))
                    .ToList();
                return new ResultadoUnidadeSorteioDto(unidadeId, u.Status, atribuicoes);
            })
            .ToList();

        return new ResultadoSorteioDto(
            rodada.Id,
            execucao.Sequencia,
            Convert.ToHexString(execucao.Seed),
            execucao.ExecutadoEm,
            rodada.Estado,
            resultados,
            resultado.Avisos.Select(a => new AvisoDto(a.Codigo, a.Mensagem)).ToList());
    }

    private static string NumeroDe(IReadOnlyDictionary<Guid, Unidade> unidades, Guid unidadeId) =>
        unidades.TryGetValue(unidadeId, out var unidade) ? unidade.Numero : "";
}
