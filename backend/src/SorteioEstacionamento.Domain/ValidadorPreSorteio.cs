namespace SorteioEstacionamento.Domain;

public sealed record AvisoPreSorteio(string Codigo, string Mensagem);

public sealed record BloqueioPreSorteio(string Codigo, string Mensagem);

public sealed record ResultadoValidacaoPreSorteio(
    IReadOnlyList<BloqueioPreSorteio> Bloqueios,
    IReadOnlyList<AvisoPreSorteio> Avisos)
{
    public bool PodeExecutar => Bloqueios.Count == 0;
}

public static class ValidadorPreSorteio
{
    public static ResultadoValidacaoPreSorteio Validar(
        Rodada rodada,
        IReadOnlyList<Vaga> vagasCadastradas)
    {
        var bloqueios = new List<BloqueioPreSorteio>();
        var avisos = new List<AvisoPreSorteio>();

        if (rodada.Estado != EstadoRodada.Ativa)
        {
            bloqueios.Add(new BloqueioPreSorteio(
                CodigosErro.RodadaEstadoInvalido,
                "A rodada precisa estar ativa para executar o sorteio."));
            return new ResultadoValidacaoPreSorteio(bloqueios, avisos);
        }

        if (rodada.Inscricoes.Count == 0)
        {
            bloqueios.Add(new BloqueioPreSorteio(
                CodigosErro.SorteioBloqueado,
                "Não há unidades inscritas para executar o sorteio."));
        }

        var vagasDuplicadas = rodada.Atribuicoes
            .GroupBy(a => a.VagaId)
            .Any(g => g.Count() > 1);
        if (vagasDuplicadas)
        {
            bloqueios.Add(new BloqueioPreSorteio(
                CodigosErro.VagaIndisponivel,
                "Há vaga atribuída mais de uma vez nesta rodada."));
        }

        foreach (var grupo in rodada.Atribuicoes
                     .Where(a => a.Origem == OrigemAtribuicao.Manual)
                     .GroupBy(a => a.UnidadeId))
        {
            if (!rodada.Inscricoes.Any(i => i.UnidadeId == grupo.Key))
            {
                bloqueios.Add(new BloqueioPreSorteio(
                    CodigosErro.SorteioBloqueado,
                    "Há atribuição manual de unidade não inscrita."));
                continue;
            }

            var validacao = ValidadorAtribuicaoManual.Validar(grupo.Select(a => a.DadosVaga).ToList());
            if (!validacao.EhValida)
            {
                bloqueios.Add(new BloqueioPreSorteio(
                    validacao.CodigoErro ?? CodigosErro.ManualRegraViolada,
                    "Há atribuição manual que viola as regras da rodada."));
            }
        }

        var lugaresCadastrados = vagasCadastradas.Sum(v => CalculadorLugaresFisicos.Calcular(v.Tipo));
        var lugaresNecessarios = rodada.Inscricoes.Count * CalculadorPendencia.DireitoLugares;
        if (rodada.Inscricoes.Count > 0 && lugaresCadastrados < lugaresNecessarios)
        {
            bloqueios.Add(new BloqueioPreSorteio(
                CodigosErro.CapacidadeInsuficiente,
                "A capacidade cadastrada é menor que 2 lugares por unidade inscrita."));
        }

        if (bloqueios.Count > 0)
            return new ResultadoValidacaoPreSorteio(bloqueios, avisos);

        AdicionarAvisos(rodada, vagasCadastradas, avisos);
        return new ResultadoValidacaoPreSorteio(bloqueios, avisos);
    }

    private static void AdicionarAvisos(
        Rodada rodada,
        IReadOnlyList<Vaga> vagasCadastradas,
        List<AvisoPreSorteio> avisos)
    {
        var ocupadas = rodada.Atribuicoes.Select(a => a.VagaId).ToHashSet();
        var pool = vagasCadastradas.Where(v => !ocupadas.Contains(v.Id)).ToList();
        var contextos = rodada.ContextosSorteio();

        var precisamCoberta = contextos.Count(c => PrecisaCobertaDoPool(c));
        var cobertasNoPool = pool.Count(v => v.Cobertura == CoberturaVaga.Coberta);
        if (cobertasNoPool < precisamCoberta)
        {
            avisos.Add(new AvisoPreSorteio(
                CodigosAviso.CobertasInsuficientes,
                "A quantidade de vagas cobertas no pool pode ser insuficiente para completar todas as unidades pendentes."));
        }

        var pendencia2 = contextos.Count(c => c.PendenciaLugares == 2);
        var gavetasCobertas = pool.Count(v => v.Tipo == TipoVaga.Gaveta && v.Cobertura == CoberturaVaga.Coberta);
        var simples = pool.Where(v => v.Tipo == TipoVaga.Simples).ToList();
        var simplesCobertas = simples.Count(v => v.Cobertura == CoberturaVaga.Coberta);
        var paresDisjuntos = Math.Min(simples.Count / 2, simplesCobertas);
        if (gavetasCobertas + paresDisjuntos < pendencia2)
        {
            avisos.Add(new AvisoPreSorteio(
                CodigosAviso.OpcoesPendencia2Insuficientes,
                "Gavetas cobertas e pares simples possíveis podem não atender todas as unidades com pendência 2."));
        }
    }

    private static bool PrecisaCobertaDoPool(UnidadeSorteioContext unidade)
    {
        if (unidade.PendenciaLugares <= 0)
            return false;

        return !unidade.AtribuicoesManuais.Any(a => a.Vaga.Cobertura == CoberturaVaga.Coberta);
    }
}
