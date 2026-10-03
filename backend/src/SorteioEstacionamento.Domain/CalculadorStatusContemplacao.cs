namespace SorteioEstacionamento.Domain;

public static class CalculadorStatusContemplacao
{
    public static StatusContemplacao Calcular(IReadOnlyList<AtribuicaoSnapshot> atribuicoes)
    {
        var lugares = atribuicoes.Sum(a => CalculadorLugaresFisicos.Calcular(a.Vaga));
        if (lugares <= 0)
            return StatusContemplacao.NaoContemplada;
        if (lugares == 1)
            return StatusContemplacao.Parcial;

        if (atribuicoes.Any(a => a.Vaga.Cobertura == CoberturaVaga.Coberta))
            return StatusContemplacao.Completa;

        var excecaoPcd = atribuicoes.Count == 2
            && atribuicoes.All(a => a.Origem == OrigemAtribuicao.Manual)
            && atribuicoes.All(a => a.Vaga.EhPcd);

        return excecaoPcd
            ? StatusContemplacao.Completa
            : StatusContemplacao.CompletaSemCobertura;
    }
}
