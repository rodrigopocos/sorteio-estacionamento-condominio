namespace SorteioEstacionamento.Domain;

public readonly record struct ResultadoValidacao(bool EhValida, string? CodigoErro)
{
    public static ResultadoValidacao Ok() => new(true, null);

    public static ResultadoValidacao Erro(string codigo) => new(false, codigo);
}

public static class ValidadorAtribuicaoManual
{
    public const string CodigoRegraViolada = "MANUAL_REGRA_VIOLADA";

    public static ResultadoValidacao Validar(IReadOnlyList<VagaSnapshot> vagasManuais)
    {
        if (vagasManuais.Count == 0)
            return ResultadoValidacao.Ok();

        var temGaveta = vagasManuais.Any(v => v.Tipo == TipoVaga.Gaveta);
        var temSimples = vagasManuais.Any(v => v.Tipo == TipoVaga.Simples);
        if (temGaveta && temSimples)
            return ResultadoValidacao.Erro(CodigoRegraViolada);

        var lugares = vagasManuais.Sum(CalculadorLugaresFisicos.Calcular);
        if (lugares > CalculadorPendencia.DireitoLugares)
            return ResultadoValidacao.Erro(CodigoRegraViolada);

        if (lugares == CalculadorPendencia.DireitoLugares)
        {
            var temCoberta = vagasManuais.Any(v => v.Cobertura == CoberturaVaga.Coberta);
            var excecaoPcd = vagasManuais.Count == 2 && vagasManuais.All(v => v.EhPcd);
            if (!temCoberta && !excecaoPcd)
                return ResultadoValidacao.Erro(CodigoRegraViolada);
        }

        return ResultadoValidacao.Ok();
    }
}
