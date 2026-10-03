using FluentAssertions;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Domain.Tests;

public sealed class CalculadorStatusContemplacaoTests
{
    [Fact]
    public void ZeroLugaresNaoContemplada()
    {
        CalculadorStatusContemplacao.Calcular([]).Should().Be(StatusContemplacao.NaoContemplada);
    }

    [Fact]
    public void UmLugarParcial()
    {
        var atribuicoes = Fixtures.Unidade("101", Fixtures.Simples("S1", CoberturaVaga.Coberta))
            .AtribuicoesManuais;
        CalculadorStatusContemplacao.Calcular(atribuicoes).Should().Be(StatusContemplacao.Parcial);
    }

    [Fact]
    public void DoisLugaresComCobertaCompleta()
    {
        var atribuicoes = Fixtures.Unidade(
            "101",
            Fixtures.Simples("S1", CoberturaVaga.Coberta),
            Fixtures.Simples("S2", CoberturaVaga.Descoberta)).AtribuicoesManuais;

        CalculadorStatusContemplacao.Calcular(atribuicoes).Should().Be(StatusContemplacao.Completa);
    }

    [Fact]
    public void DoisLugaresSemCobertaSemPcd_CompletaSemCobertura()
    {
        var atribuicoes = new[]
        {
            new AtribuicaoSnapshot("101", Fixtures.Simples("S1", CoberturaVaga.Descoberta), OrigemAtribuicao.Manual),
            new AtribuicaoSnapshot("101", Fixtures.Simples("S2", CoberturaVaga.Descoberta), OrigemAtribuicao.Sorteio)
        };

        CalculadorStatusContemplacao.Calcular(atribuicoes)
            .Should().Be(StatusContemplacao.CompletaSemCobertura);
    }
}
