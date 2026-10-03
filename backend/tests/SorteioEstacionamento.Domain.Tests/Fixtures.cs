using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Domain.Tests;

internal static class Fixtures
{
    public static VagaSnapshot Gaveta(string codigo, CoberturaVaga cobertura, bool ehPcd = false) =>
        new(codigo, TipoVaga.Gaveta, cobertura, ehPcd);

    public static VagaSnapshot Simples(string codigo, CoberturaVaga cobertura, bool ehPcd = false) =>
        new(codigo, TipoVaga.Simples, cobertura, ehPcd);

    public static UnidadeSorteioContext Unidade(string id, params VagaSnapshot[] manuais) =>
        new(
            id,
            manuais.Select(v => new AtribuicaoSnapshot(id, v, OrigemAtribuicao.Manual)).ToList());

    public static byte[] Seed(byte valor = 1) => [.. Enumerable.Repeat(valor, 32)];

    public static PoolElegivel Pool(params VagaSnapshot[] cadastradas) =>
        PoolElegivel.Criar(cadastradas, []);

    public static PoolElegivel PoolComManuais(
        IEnumerable<VagaSnapshot> cadastradas,
        IEnumerable<AtribuicaoSnapshot> manuais) =>
        PoolElegivel.Criar(cadastradas, manuais);
}
