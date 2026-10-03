using FluentAssertions;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Domain.Tests;

public sealed class CalculadorPendenciaTests
{
    [Fact]
    public void SemAtribuicaoPendenciaDois()
    {
        CalculadorPendencia.Calcular([]).Should().Be(2);
    }

    [Fact]
    public void UmaSimplesPendenciaUm()
    {
        var atribuicoes = Fixtures.Unidade("101", Fixtures.Simples("S1", CoberturaVaga.Coberta))
            .AtribuicoesManuais;
        CalculadorPendencia.Calcular(atribuicoes).Should().Be(1);
    }

    [Fact]
    public void GavetaOuDuasSimplesPendenciaZero()
    {
        var gaveta = Fixtures.Unidade("101", Fixtures.Gaveta("G1", CoberturaVaga.Coberta)).AtribuicoesManuais;
        var duas = Fixtures.Unidade(
            "102",
            Fixtures.Simples("S1", CoberturaVaga.Coberta),
            Fixtures.Simples("S2", CoberturaVaga.Descoberta)).AtribuicoesManuais;

        CalculadorPendencia.Calcular(gaveta).Should().Be(0);
        CalculadorPendencia.Calcular(duas).Should().Be(0);
    }
}
