namespace SorteioEstacionamento.Domain;

public sealed class PoolElegivel
{
    private readonly Dictionary<string, VagaSnapshot> _disponiveis;

    private PoolElegivel(Dictionary<string, VagaSnapshot> disponiveis)
    {
        _disponiveis = disponiveis;
    }

    public IReadOnlyCollection<VagaSnapshot> VagasDisponiveis => _disponiveis.Values;

    public static PoolElegivel Criar(
        IEnumerable<VagaSnapshot> cadastradas,
        IEnumerable<AtribuicaoSnapshot> jaAtribuidas)
    {
        var ocupadas = jaAtribuidas
            .Select(a => a.Vaga.Codigo)
            .ToHashSet(StringComparer.Ordinal);

        var disponiveis = cadastradas
            .Where(v => !ocupadas.Contains(v.Codigo))
            .ToDictionary(v => v.Codigo, StringComparer.Ordinal);

        return new PoolElegivel(disponiveis);
    }

    public IReadOnlyList<OpcaoSorteio> ListarOpcoes(UnidadeSorteioContext unidade)
    {
        var pendencia = CalculadorPendencia.Calcular(unidade.AtribuicoesManuais);
        if (pendencia <= 0)
            return [];
        if (pendencia == 1)
            return ListarComplemento(unidade);

        return [.. ListarFormaA(), .. ListarFormaB()];
    }

    public void Consumir(OpcaoSorteio opcao)
    {
        foreach (var vaga in opcao.Vagas)
        {
            if (!_disponiveis.Remove(vaga.Codigo))
                throw new InvalidOperationException($"Vaga {vaga.Codigo} não está no pool.");
        }
    }

    private IEnumerable<OpcaoSorteio> ListarFormaA()
    {
        return _disponiveis.Values
            .Where(v => v.Tipo == TipoVaga.Gaveta && v.Cobertura == CoberturaVaga.Coberta)
            .OrderBy(v => v.Codigo, StringComparer.Ordinal)
            .Select(v => (OpcaoSorteio)new OpcaoSorteio.FormaA(v));
    }

    private IEnumerable<OpcaoSorteio> ListarFormaB()
    {
        var simples = _disponiveis.Values
            .Where(v => v.Tipo == TipoVaga.Simples)
            .OrderBy(v => v.Codigo, StringComparer.Ordinal)
            .ToList();

        var pares = new List<OpcaoSorteio>();
        for (var i = 0; i < simples.Count; i++)
        {
            for (var j = i + 1; j < simples.Count; j++)
            {
                var a = simples[i];
                var b = simples[j];
                if (a.Cobertura == CoberturaVaga.Coberta || b.Cobertura == CoberturaVaga.Coberta)
                    pares.Add(new OpcaoSorteio.FormaB(a, b));
            }
        }

        return pares;
    }

    private IReadOnlyList<OpcaoSorteio> ListarComplemento(UnidadeSorteioContext unidade)
    {
        var manual = unidade.AtribuicoesManuais.Select(a => a.Vaga).Single();
        var exigeCoberta = manual.Cobertura == CoberturaVaga.Descoberta;

        return _disponiveis.Values
            .Where(v => v.Tipo == TipoVaga.Simples)
            .Where(v => !exigeCoberta || v.Cobertura == CoberturaVaga.Coberta)
            .OrderBy(v => v.Codigo, StringComparer.Ordinal)
            .Select(v => (OpcaoSorteio)new OpcaoSorteio.Complemento(v))
            .ToList();
    }
}
