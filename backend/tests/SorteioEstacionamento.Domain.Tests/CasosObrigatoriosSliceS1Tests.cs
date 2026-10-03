using FluentAssertions;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Domain.Tests;

public sealed class CasosObrigatoriosSliceS1Tests
{
    [Fact]
    public void Caso1_FormaA_IncluiSomenteGavetaCoberta()
    {
        var coberta = Fixtures.Gaveta("G-C", CoberturaVaga.Coberta);
        var descoberta = Fixtures.Gaveta("G-D", CoberturaVaga.Descoberta);
        var pool = Fixtures.Pool(coberta, descoberta);
        var unidade = Fixtures.Unidade("101");

        var opcoes = pool.ListarOpcoes(unidade);

        opcoes.Should().ContainSingle()
            .Which.Should().BeOfType<OpcaoSorteio.FormaA>()
            .Which.Gaveta.Should().Be(coberta);
        opcoes.OfType<OpcaoSorteio.FormaA>().Should().NotContain(a => a.Gaveta.Codigo == "G-D");
    }

    [Fact]
    public void Caso2_FormaB_ParesDistintosComPeloMenosUmaCoberta()
    {
        var s1 = Fixtures.Simples("S1", CoberturaVaga.Coberta);
        var s2 = Fixtures.Simples("S2", CoberturaVaga.Descoberta);
        var s3 = Fixtures.Simples("S3", CoberturaVaga.Descoberta);
        var pool = Fixtures.Pool(s1, s2, s3);

        var opcoes = pool.ListarOpcoes(Fixtures.Unidade("101"));

        opcoes.Should().AllBeOfType<OpcaoSorteio.FormaB>();
        opcoes.Should().HaveCount(2);
        opcoes.Cast<OpcaoSorteio.FormaB>().Should().BeEquivalentTo(
        [
            new OpcaoSorteio.FormaB(s1, s2),
            new OpcaoSorteio.FormaB(s1, s3)
        ]);
        opcoes.Cast<OpcaoSorteio.FormaB>()
            .Should().NotContain(p => p.Primeira.Codigo == "S2" && p.Segunda.Codigo == "S3");
    }

    [Fact]
    public void Caso3_Complemento_Pendencia1NuncaGavetaERestringeCobertaSeManualDescoberta()
    {
        var gaveta = Fixtures.Gaveta("G1", CoberturaVaga.Coberta);
        var simplesCoberta = Fixtures.Simples("SC", CoberturaVaga.Coberta);
        var simplesDescoberta = Fixtures.Simples("SD", CoberturaVaga.Descoberta);
        var manualDescoberta = Fixtures.Simples("M1", CoberturaVaga.Descoberta);
        var unidade = Fixtures.Unidade("101", manualDescoberta);
        var pool = Fixtures.PoolComManuais(
            [gaveta, simplesCoberta, simplesDescoberta, manualDescoberta],
            unidade.AtribuicoesManuais);

        unidade.PendenciaLugares.Should().Be(1);

        var opcoes = pool.ListarOpcoes(unidade);

        opcoes.Should().ContainSingle()
            .Which.Should().BeOfType<OpcaoSorteio.Complemento>()
            .Which.Simples.Should().Be(simplesCoberta);
        opcoes.OfType<OpcaoSorteio.FormaA>().Should().BeEmpty();
        opcoes.Should().NotContain(o => o.Vagas.Any(v => v.Tipo == TipoVaga.Gaveta));
        opcoes.Should().NotContain(o => o.Vagas.Any(v => v.Codigo == "SD"));
    }

    [Fact]
    public void Caso3b_Complemento_ManualCobertaAceitaQualquerSimples()
    {
        var simplesCoberta = Fixtures.Simples("SC", CoberturaVaga.Coberta);
        var simplesDescoberta = Fixtures.Simples("SD", CoberturaVaga.Descoberta);
        var manualCoberta = Fixtures.Simples("M1", CoberturaVaga.Coberta);
        var unidade = Fixtures.Unidade("101", manualCoberta);
        var pool = Fixtures.PoolComManuais(
            [simplesCoberta, simplesDescoberta, manualCoberta],
            unidade.AtribuicoesManuais);

        var opcoes = pool.ListarOpcoes(unidade);

        opcoes.Should().HaveCount(2);
        opcoes.Should().AllBeOfType<OpcaoSorteio.Complemento>();
        opcoes.SelectMany(o => o.Vagas).Select(v => v.Codigo)
            .Should().BeEquivalentTo("SC", "SD");
    }

    [Fact]
    public void Caso4_UnidadeComManualGavetaNaoEntraNoSorteio()
    {
        var gavetaManual = Fixtures.Gaveta("G-M", CoberturaVaga.Coberta);
        var gavetaPool = Fixtures.Gaveta("G-P", CoberturaVaga.Coberta);
        var unidadeEncerrada = Fixtures.Unidade("101", gavetaManual);
        var unidadeAberta = Fixtures.Unidade("102");

        unidadeEncerrada.PendenciaLugares.Should().Be(0);

        var resultado = MotorSorteio.Executar(
            [gavetaManual, gavetaPool],
            [unidadeEncerrada, unidadeAberta],
            Fixtures.Seed());

        resultado.OrdemUnidades.Should().Equal("102");
        resultado.ResultadosPorUnidade.Should().Contain(r =>
            r.UnidadeId == "101"
            && r.AtribuicoesSorteio.Count == 0
            && r.Status == StatusContemplacao.Completa);
    }

    [Fact]
    public void Caso5_UnidadeComDuasSimplesManuaisEncerrada()
    {
        var m1 = Fixtures.Simples("M1", CoberturaVaga.Coberta);
        var m2 = Fixtures.Simples("M2", CoberturaVaga.Descoberta);
        var poolGaveta = Fixtures.Gaveta("G1", CoberturaVaga.Coberta);
        var unidadeEncerrada = Fixtures.Unidade("101", m1, m2);
        var unidadeAberta = Fixtures.Unidade("102");

        unidadeEncerrada.PendenciaLugares.Should().Be(0);

        var resultado = MotorSorteio.Executar(
            [m1, m2, poolGaveta],
            [unidadeEncerrada, unidadeAberta],
            Fixtures.Seed());

        resultado.OrdemUnidades.Should().Equal("102");
        var r101 = resultado.ResultadosPorUnidade.Single(r => r.UnidadeId == "101");
        r101.AtribuicoesSorteio.Should().BeEmpty();
        r101.Status.Should().Be(StatusContemplacao.Completa);
    }

    [Fact]
    public void Caso6_MesmaVagaNaoAtribuidaDuasVezes()
    {
        var g1 = Fixtures.Gaveta("G1", CoberturaVaga.Coberta);
        var s1 = Fixtures.Simples("S1", CoberturaVaga.Coberta);
        var s2 = Fixtures.Simples("S2", CoberturaVaga.Descoberta);
        var unidades = new[]
        {
            Fixtures.Unidade("101"),
            Fixtures.Unidade("102"),
            Fixtures.Unidade("103")
        };

        var resultado = MotorSorteio.Executar([g1, s1, s2], unidades, Fixtures.Seed());

        var codigos = resultado.ResultadosPorUnidade
            .SelectMany(r => r.AtribuicoesSorteio)
            .Select(a => a.Vaga.Codigo)
            .ToList();

        codigos.Should().OnlyHaveUniqueItems();
        codigos.Should().BeSubsetOf(["G1", "S1", "S2"]);
    }

    [Fact]
    public void Caso7_SeedFixoProduzResultadoReproduzivel()
    {
        var vagas = new[]
        {
            Fixtures.Gaveta("G1", CoberturaVaga.Coberta),
            Fixtures.Gaveta("G2", CoberturaVaga.Coberta),
            Fixtures.Simples("S1", CoberturaVaga.Coberta),
            Fixtures.Simples("S2", CoberturaVaga.Descoberta),
            Fixtures.Simples("S3", CoberturaVaga.Coberta)
        };
        var unidades = new[]
        {
            Fixtures.Unidade("101"),
            Fixtures.Unidade("102"),
            Fixtures.Unidade("201")
        };
        var seed = Fixtures.Seed(42);

        var a = MotorSorteio.Executar(vagas, unidades, seed);
        var b = MotorSorteio.Executar(vagas, unidades, seed);

        a.OrdemUnidades.Should().Equal(b.OrdemUnidades);
        a.ResultadosPorUnidade.Should().BeEquivalentTo(b.ResultadosPorUnidade, opts => opts.WithStrictOrdering());
    }

    [Fact]
    public void Caso8_DuasOpcoesElegiveisAparecemUmaVezCada()
    {
        var g1 = Fixtures.Gaveta("G1", CoberturaVaga.Coberta);
        var s1 = Fixtures.Simples("S1", CoberturaVaga.Coberta);
        var s2 = Fixtures.Simples("S2", CoberturaVaga.Descoberta);
        var pool = Fixtures.Pool(g1, s1, s2);

        var opcoes = pool.ListarOpcoes(Fixtures.Unidade("101"));

        opcoes.Should().HaveCount(2);
        opcoes.Should().OnlyHaveUniqueItems();
        opcoes.OfType<OpcaoSorteio.FormaA>().Should().ContainSingle(a => a.Gaveta.Codigo == "G1");
        opcoes.OfType<OpcaoSorteio.FormaB>().Should().ContainSingle(b =>
            b.Primeira.Codigo == "S1" && b.Segunda.Codigo == "S2");
        opcoes.OfType<OpcaoSorteio.FormaB>().Should().NotContain(b =>
            b.Primeira.Codigo == "S2" && b.Segunda.Codigo == "S1");
    }

    [Fact]
    public void Caso9_PoolEsgotado_NaoContempladaOuParcial()
    {
        var semVagas = MotorSorteio.Executar([], [Fixtures.Unidade("101")], Fixtures.Seed());
        semVagas.ResultadosPorUnidade.Single().Status.Should().Be(StatusContemplacao.NaoContemplada);
        semVagas.ResultadosPorUnidade.Single().AtribuicoesSorteio.Should().BeEmpty();

        var manual = Fixtures.Simples("M1", CoberturaVaga.Coberta);
        var somenteGaveta = Fixtures.Gaveta("G1", CoberturaVaga.Coberta);
        var parcial = MotorSorteio.Executar(
            [manual, somenteGaveta],
            [Fixtures.Unidade("101", manual)],
            Fixtures.Seed());

        var r = parcial.ResultadosPorUnidade.Single();
        r.Status.Should().Be(StatusContemplacao.Parcial);
        r.AtribuicoesSorteio.Should().BeEmpty();
    }

    [Fact]
    public void Caso10_ExcecaoPcd_DuasVagasPcdDescobertasSaoCompleta()
    {
        var pcd1 = Fixtures.Simples("PCD-1", CoberturaVaga.Descoberta, ehPcd: true);
        var pcd2 = Fixtures.Simples("PCD-2", CoberturaVaga.Descoberta, ehPcd: true);
        var atribuicoes = new[]
        {
            new AtribuicaoSnapshot("101", pcd1, OrigemAtribuicao.Manual),
            new AtribuicaoSnapshot("101", pcd2, OrigemAtribuicao.Manual)
        };

        CalculadorStatusContemplacao.Calcular(atribuicoes)
            .Should().Be(StatusContemplacao.Completa);

        ValidadorAtribuicaoManual.Validar([pcd1, pcd2]).EhValida.Should().BeTrue();
    }
}
