using FluentAssertions;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Domain.Tests;

public sealed class RodadaTransicoesTests
{
    private static readonly DateTimeOffset Agora = DateTimeOffset.UnixEpoch;
    private static byte[] Seed() => [.. Enumerable.Repeat((byte)7, 32)];

    [Fact]
    public void CriarComecaEmPreparacaoSemInscricoes()
    {
        var rodada = Rodada.Criar(2026, Agora);

        rodada.Estado.Should().Be(EstadoRodada.EmPreparacao);
        rodada.Inscricoes.Should().BeEmpty();
        rodada.Ano.Should().Be(2026);
    }

    [Fact]
    public void AtivarInscreveTodasAsUnidadesEVaiParaAtiva()
    {
        var u1 = Unidade.Criar("101");
        var u2 = Unidade.Criar("102");
        var rodada = Rodada.Criar(2026, Agora);

        rodada.Ativar([u1, u2], Agora);

        rodada.Estado.Should().Be(EstadoRodada.Ativa);
        rodada.Inscricoes.Select(i => i.UnidadeId).Should().BeEquivalentTo([u1.Id, u2.Id]);
        rodada.AtivadaEm.Should().Be(Agora);
    }

    [Fact]
    public void TransicaoInvalida_AtivarDuasVezes()
    {
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([Unidade.Criar("101")], Agora);

        var act = () => rodada.Ativar([Unidade.Criar("102")], Agora);

        act.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.RodadaEstadoInvalido);
    }

    [Fact]
    public void TransicaoInvalida_ExecutarForaDeAtiva()
    {
        var unidade = Unidade.Criar("101");
        var vaga = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);

        var emPreparacao = () => rodada.ExecutarSorteio([vaga], Seed(), Agora);
        emPreparacao.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.RodadaEstadoInvalido);

        rodada.Ativar([unidade], Agora);
        rodada.ExecutarSorteio([vaga], Seed(), Agora);

        var jaSorteada = () => rodada.ExecutarSorteio([vaga], Seed(), Agora);
        jaSorteada.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.RodadaEstadoInvalido);
    }

    [Fact]
    public void TransicaoInvalida_CancelarForaDeSorteada()
    {
        var rodada = Rodada.Criar(2026, Agora);
        var emPreparacao = () => rodada.CancelarSorteio(Agora);
        emPreparacao.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.RodadaEstadoInvalido);

        rodada.Ativar([Unidade.Criar("101")], Agora);
        var ativa = () => rodada.CancelarSorteio(Agora);
        ativa.Should().Throw<FalhaRegraNegocioException>()
            .Where(e => e.Codigo == CodigosErro.RodadaEstadoInvalido);
    }

    [Fact]
    public void CancelarSorteioRemoveAtribuicoesDeSorteioMantemManuaisEVoltaAtiva()
    {
        var unidade = Unidade.Criar("101");
        var outra = Unidade.Criar("102");
        var manual = Vaga.Criar("M1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var s1 = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var s2 = Vaga.Criar("S2", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade, outra], Agora);
        var atribuicaoManual = rodada.AtribuirManual(unidade.Id, manual);

        rodada.ExecutarSorteio([manual, s1, s2, g1], Seed(), Agora);
        rodada.Atribuicoes.Should().Contain(a => a.Origem == OrigemAtribuicao.Sorteio);

        rodada.CancelarSorteio(Agora.AddMinutes(1));

        rodada.Estado.Should().Be(EstadoRodada.Ativa);
        rodada.Atribuicoes.Should().ContainSingle()
            .Which.Id.Should().Be(atribuicaoManual.Id);
        rodada.Cancelamentos.Should().ContainSingle()
            .Which.SequenciaCancelada.Should().Be(1);
        rodada.Execucoes.Should().ContainSingle().Which.Sequencia.Should().Be(1);
    }

    [Fact]
    public void ReSorteioAposCancelamentoIncrementaSequencia()
    {
        var unidade = Unidade.Criar("101");
        var vaga = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);

        rodada.ExecutarSorteio([vaga], Seed(), Agora);
        rodada.CancelarSorteio(Agora);
        var segunda = rodada.ExecutarSorteio([vaga], Seed(), Agora.AddHours(1));

        segunda.Execucao.Sequencia.Should().Be(2);
        rodada.Execucoes.Select(e => e.Sequencia).Should().Equal(1, 2);
        rodada.Atribuicoes.Should().OnlyContain(a => a.Origem == OrigemAtribuicao.Sorteio);
        rodada.Estado.Should().Be(EstadoRodada.Sorteada);
    }
}
