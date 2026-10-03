using FluentAssertions;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Domain.Tests;

public sealed class CalculadorLugaresFisicosTests
{
    [Fact]
    public void SimplesContaUmLugar()
    {
        CalculadorLugaresFisicos.Calcular(TipoVaga.Simples).Should().Be(1);
        CalculadorLugaresFisicos.Calcular(Fixtures.Simples("S1", CoberturaVaga.Coberta)).Should().Be(1);
    }

    [Fact]
    public void GavetaContaDoisLugares()
    {
        CalculadorLugaresFisicos.Calcular(TipoVaga.Gaveta).Should().Be(2);
        CalculadorLugaresFisicos.Calcular(Fixtures.Gaveta("G1", CoberturaVaga.Descoberta)).Should().Be(2);
    }
}
