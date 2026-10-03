using FluentAssertions;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Domain.Tests;

public sealed class ValidadorPreSorteioTests
{
    private static readonly DateTimeOffset Agora = DateTimeOffset.UnixEpoch;
    private static byte[] Seed() => [.. Enumerable.Repeat((byte)9, 32)];

    [Fact]
    public void BloqueiaCapacidadeMenorQueDoisLugaresPorInscrita()
    {
        var u1 = Unidade.Criar("101");
        var u2 = Unidade.Criar("102");
        var unica = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([u1, u2], Agora);

        var resultado = ValidadorPreSorteio.Validar(rodada, [unica]);

        resultado.PodeExecutar.Should().BeFalse();
        resultado.Bloqueios.Should().Contain(b => b.Codigo == CodigosErro.CapacidadeInsuficiente);

        var act = () => rodada.ExecutarSorteio([unica], Seed(), Agora);
        act.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.CapacidadeInsuficiente);
    }

    [Fact]
    public void BloqueiaSorteioSemUnidadesInscritas()
    {
        var vaga = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([], Agora);

        var resultado = ValidadorPreSorteio.Validar(rodada, [vaga]);

        resultado.PodeExecutar.Should().BeFalse();
        resultado.Bloqueios.Should().Contain(b => b.Codigo == CodigosErro.SorteioBloqueado);
    }

    [Fact]
    public void AvisoCobertasInsuficientesNaoBloqueia()
    {
        var unidade = Unidade.Criar("101");
        var manual = Vaga.Criar("M1", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var complemento = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);
        rodada.AtribuirManual(unidade.Id, manual);

        var resultado = ValidadorPreSorteio.Validar(rodada, [manual, complemento]);

        resultado.PodeExecutar.Should().BeTrue();
        resultado.Avisos.Should().Contain(a => a.Codigo == CodigosAviso.CobertasInsuficientes);
        resultado.Bloqueios.Should().BeEmpty();
    }

    [Fact]
    public void AvisoOpcoesPendencia2InsuficientesNaoBloqueia()
    {
        var u1 = Unidade.Criar("101");
        var u2 = Unidade.Criar("102");
        var gDesc = Vaga.Criar("G-D", TipoVaga.Gaveta, CoberturaVaga.Descoberta);
        var s1 = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var s2 = Vaga.Criar("S2", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([u1, u2], Agora);

        var resultado = ValidadorPreSorteio.Validar(rodada, [gDesc, s1, s2]);

        resultado.PodeExecutar.Should().BeTrue();
        resultado.Avisos.Should().Contain(a => a.Codigo == CodigosAviso.OpcoesPendencia2Insuficientes);
        resultado.Avisos.Should().Contain(a => a.Codigo == CodigosAviso.CobertasInsuficientes);
    }

    [Fact]
    public void ExecutarComAvisoConcluiSorteioParcial()
    {
        var unidade = Unidade.Criar("101");
        var s1 = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var s2 = Vaga.Criar("S2", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);

        var execucao = rodada.ExecutarSorteio([s1, s2], Seed(), Agora);

        execucao.Avisos.Should().NotBeEmpty();
        rodada.Estado.Should().Be(EstadoRodada.Sorteada);
        execucao.Execucao.Resultado.ResultadosPorUnidade.Single().Status
            .Should().Be(StatusContemplacao.NaoContemplada);
    }
}
