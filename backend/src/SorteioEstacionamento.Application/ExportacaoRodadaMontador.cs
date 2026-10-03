using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public static class ExportacaoRodadaMontador
{
    public static IReadOnlyList<LinhaExportRodada> Montar(
        Rodada rodada,
        IReadOnlyDictionary<Guid, Unidade> unidadesPorId)
    {
        var linhas = new List<LinhaExportRodada>();
        var inscricoes = rodada.Inscricoes
            .Select(i => new
            {
                i.UnidadeId,
                Numero = unidadesPorId.TryGetValue(i.UnidadeId, out var unidade) ? unidade.Numero : ""
            })
            .OrderBy(x => x.Numero, StringComparer.Ordinal)
            .ToList();

        foreach (var inscricao in inscricoes)
        {
            var atribuicoes = rodada.Atribuicoes
                .Where(a => a.UnidadeId == inscricao.UnidadeId)
                .OrderBy(a => a.DadosVaga.Codigo, StringComparer.Ordinal)
                .ToList();

            var status = CalculadorStatusContemplacao.Calcular(
                atribuicoes.Select(a => a.ParaSnapshot()).ToList());
            var statusTexto = status.ToString();

            if (atribuicoes.Count == 0)
            {
                linhas.Add(new LinhaExportRodada(
                    rodada.Ano,
                    inscricao.Numero,
                    statusTexto,
                    "",
                    "",
                    "",
                    0,
                    ""));
                continue;
            }

            foreach (var atribuicao in atribuicoes)
            {
                var vaga = atribuicao.DadosVaga;
                linhas.Add(new LinhaExportRodada(
                    rodada.Ano,
                    inscricao.Numero,
                    statusTexto,
                    vaga.Codigo,
                    vaga.Tipo.ToString(),
                    vaga.Cobertura.ToString(),
                    CalculadorLugaresFisicos.Calcular(vaga),
                    atribuicao.Origem.ToString()));
            }
        }

        return linhas;
    }

    public static ExportacaoRodadaCabecalho MontarCabecalho(
        Rodada rodada,
        IReadOnlyDictionary<Guid, Unidade> unidadesPorId)
    {
        var execucao = rodada.Execucoes.LastOrDefault()
            ?? throw new FalhaRegraNegocioException(
                CodigosErro.SorteioBloqueado,
                "Não há execução de sorteio para exportar.");

        var totais = ContarStatusPorUnidade(rodada, unidadesPorId);
        return new ExportacaoRodadaCabecalho(
            rodada.Ano,
            execucao.Sequencia,
            execucao.ExecutadoEm,
            totais.Completas,
            totais.Parciais,
            totais.NaoContempladas);
    }

    private static (int Completas, int Parciais, int NaoContempladas) ContarStatusPorUnidade(
        Rodada rodada,
        IReadOnlyDictionary<Guid, Unidade> unidadesPorId)
    {
        var completas = 0;
        var parciais = 0;
        var naoContempladas = 0;

        foreach (var inscricao in rodada.Inscricoes)
        {
            var atribuicoes = rodada.Atribuicoes
                .Where(a => a.UnidadeId == inscricao.UnidadeId)
                .Select(a => a.ParaSnapshot())
                .ToList();
            var status = CalculadorStatusContemplacao.Calcular(atribuicoes);

            switch (status)
            {
                case StatusContemplacao.Completa:
                case StatusContemplacao.CompletaSemCobertura:
                    completas++;
                    break;
                case StatusContemplacao.Parcial:
                    parciais++;
                    break;
                default:
                    naoContempladas++;
                    break;
            }
        }

        return (completas, parciais, naoContempladas);
    }
}
