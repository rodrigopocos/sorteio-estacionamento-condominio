using FluentAssertions;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Domain.Tests;

public sealed class ValidadorAtribuicaoManualTests
{
    [Fact]
    public void RejeitaMisturaGavetaESimples()
    {
        var resultado = ValidadorAtribuicaoManual.Validar(
        [
            Fixtures.Gaveta("G1", CoberturaVaga.Coberta),
            Fixtures.Simples("S1", CoberturaVaga.Coberta)
        ]);

        resultado.EhValida.Should().BeFalse();
        resultado.CodigoErro.Should().Be(ValidadorAtribuicaoManual.CodigoRegraViolada);
    }

    [Fact]
    public void RejeitaDuasSimplesDescobertasSemExcecaoPcd()
    {
        var resultado = ValidadorAtribuicaoManual.Validar(
        [
            Fixtures.Simples("S1", CoberturaVaga.Descoberta),
            Fixtures.Simples("S2", CoberturaVaga.Descoberta)
        ]);

        resultado.EhValida.Should().BeFalse();
        resultado.CodigoErro.Should().Be(ValidadorAtribuicaoManual.CodigoRegraViolada);
    }

    [Fact]
    public void RejeitaGavetaDescobertaSemCobertura()
    {
        var resultado = ValidadorAtribuicaoManual.Validar(
        [
            Fixtures.Gaveta("G1", CoberturaVaga.Descoberta)
        ]);

        resultado.EhValida.Should().BeFalse();
    }

    [Fact]
    public void AceitaUmaSimplesParcial()
    {
        ValidadorAtribuicaoManual.Validar(
        [
            Fixtures.Simples("S1", CoberturaVaga.Descoberta)
        ]).EhValida.Should().BeTrue();
    }
}
