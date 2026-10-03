using FluentAssertions;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Domain.Tests;

public sealed class RodadaAtribuicaoManualTests
{
    private static readonly DateTimeOffset Agora = DateTimeOffset.UnixEpoch;
    private static byte[] Seed() => [.. Enumerable.Repeat((byte)3, 32)];

    [Fact]
    public void ManualEmRodadaSorteadaEhBloqueado()
    {
        var unidade = Unidade.Criar("101");
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var extra = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);
        rodada.ExecutarSorteio([g1, extra], Seed(), Agora);

        var act = () => rodada.AtribuirManual(unidade.Id, extra);

        act.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.RodadaEstadoInvalido);
    }

    [Fact]
    public void RemoverManualEmRodadaSorteadaEhBloqueado()
    {
        var unidade = Unidade.Criar("101");
        var outra = Unidade.Criar("102");
        var manual = Vaga.Criar("M1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var s1 = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var s2 = Vaga.Criar("S2", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade, outra], Agora);
        var atribuicao = rodada.AtribuirManual(unidade.Id, manual);
        rodada.ExecutarSorteio([manual, g1, s1, s2], Seed(), Agora);

        var act = () => rodada.RemoverManual(atribuicao.Id);

        act.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.RodadaEstadoInvalido);
    }

    [Fact]
    public void RejeitaUnidadeNaoInscritaEVagaDuplicada()
    {
        var inscrita = Unidade.Criar("101");
        var fora = Unidade.Criar("999");
        var vaga = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([inscrita], Agora);

        var naoInscrita = () => rodada.AtribuirManual(fora.Id, vaga);
        naoInscrita.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.SorteioBloqueado);

        rodada.AtribuirManual(inscrita.Id, vaga);
        var duplicada = () => rodada.AtribuirManual(inscrita.Id, vaga);
        duplicada.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.VagaIndisponivel);
    }

    [Fact]
    public void RejeitaMisturaGavetaComSimples()
    {
        var unidade = Unidade.Criar("101");
        var gaveta = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var simples = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);
        rodada.AtribuirManual(unidade.Id, gaveta);

        var act = () => rodada.AtribuirManual(unidade.Id, simples);

        act.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.ManualRegraViolada);
    }

    [Fact]
    public void VagaManualNaoEntraNoSorteio()
    {
        var u1 = Unidade.Criar("101");
        var u2 = Unidade.Criar("102");
        var manual = Vaga.Criar("PCD1", TipoVaga.Simples, CoberturaVaga.Coberta, ehPcd: true);
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var s1 = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var s2 = Vaga.Criar("S2", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([u1, u2], Agora);
        rodada.AtribuirManual(u1.Id, manual);

        rodada.ExecutarSorteio([manual, g1, s1, s2], Seed(), Agora);

        rodada.Atribuicoes.Where(a => a.Origem == OrigemAtribuicao.Sorteio)
            .Should().NotContain(a => a.VagaId == manual.Id);
        rodada.Atribuicoes.Should().Contain(a =>
            a.UnidadeId == u1.Id && a.Origem == OrigemAtribuicao.Manual && a.VagaId == manual.Id);
    }

    [Fact]
    public void RemoverManualLiberaAVaga()
    {
        var unidade = Unidade.Criar("101");
        var vaga = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);
        var atribuicao = rodada.AtribuirManual(unidade.Id, vaga);

        rodada.RemoverManual(atribuicao.Id);

        rodada.Atribuicoes.Should().BeEmpty();
        rodada.AtribuirManual(unidade.Id, vaga).VagaId.Should().Be(vaga.Id);
    }
}
