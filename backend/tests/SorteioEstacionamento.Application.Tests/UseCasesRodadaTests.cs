using FluentAssertions;
using SorteioEstacionamento.Application;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application.Tests;

public sealed class UseCasesRodadaTests
{
    private static readonly DateTimeOffset Agora = DateTimeOffset.UnixEpoch;

    [Fact]
    public async Task CriarRodadaPersisteAnoUnicoEmPreparacao()
    {
        var rodadas = new RodadaRepositorioEmMemoria();
        var useCase = new CriarRodada(rodadas);

        var dto = await useCase.ExecutarAsync(new CriarRodadaCommand(2026));

        dto.Ano.Should().Be(2026);
        dto.Estado.Should().Be(EstadoRodada.EmPreparacao);
        dto.Inscricoes.Should().BeEmpty();
        dto.Atribuicoes.Should().BeEmpty();
        (await rodadas.ObterPorAnoAsync(2026))!.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task CriarRodadaAnoDuplicadoFalha()
    {
        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(Rodada.Criar(2026, Agora));
        var useCase = new CriarRodada(rodadas);

        var act = () => useCase.ExecutarAsync(new CriarRodadaCommand(2026));

        var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
        ex.Which.Codigo.Should().Be(CodigosErro.CadastroDuplicado);
    }

    [Fact]
    public async Task CriarRodadaAnoInvalidoFalha()
    {
        var useCase = new CriarRodada(new RodadaRepositorioEmMemoria());

        var act = () => useCase.ExecutarAsync(new CriarRodadaCommand(1999));

        var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
        ex.Which.Codigo.Should().Be(CodigosErro.CadastroInvalido);
    }

    [Fact]
    public async Task ObterDetalheRodadaInexistenteFalha()
    {
        var useCase = new ObterDetalheRodada(
            new RodadaRepositorioEmMemoria(),
            new UnidadeRepositorioEmMemoria(),
            new VagaRepositorioEmMemoria());

        var act = () => useCase.ExecutarAsync(Guid.NewGuid());

        var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
        ex.Which.Codigo.Should().Be(CodigosErro.NaoEncontrado);
    }

    [Fact]
    public async Task ObterDetalheIncluiInscricoesAtribuicoesEAvisos()
    {
        var unidade = Unidade.Criar("101");
        var s1 = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var s2 = Vaga.Criar("S2", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);
        rodada.AtribuirManual(unidade.Id, s1);
        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var obter = new ObterDetalheRodada(
            rodadas,
            new UnidadeRepositorioEmMemoria(unidade),
            new VagaRepositorioEmMemoria(s1, s2));

        var detalhe = await obter.ExecutarAsync(rodada.Id);

        detalhe.Estado.Should().Be(EstadoRodada.Ativa);
        detalhe.Inscricoes.Should().ContainSingle(i => i.UnidadeId == unidade.Id && i.Numero == "101");
        detalhe.Atribuicoes.Should().ContainSingle(a =>
            a.UnidadeId == unidade.Id && a.VagaCodigo == "S1" && a.Origem == OrigemAtribuicao.Manual);
        detalhe.Avisos.Select(a => a.Codigo).Should().Contain(CodigosAviso.CobertasInsuficientes);
        detalhe.Bloqueios.Should().BeEmpty();
    }

    [Fact]
    public async Task AtivarRodadaAnualInscreveTodasAsUnidadesDoCadastro()
    {
        var u1 = Unidade.Criar("101");
        var u2 = Unidade.Criar("102");
        var u3 = Unidade.Criar("201");
        var rodada = Rodada.Criar(2026, Agora);
        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var useCase = new AtivarRodadaAnual(rodadas, new UnidadeRepositorioEmMemoria(u1, u2, u3));

        await useCase.ExecutarAsync(rodada.Id);

        var persistida = await rodadas.ObterPorIdAsync(rodada.Id);
        persistida!.Estado.Should().Be(EstadoRodada.Ativa);
        persistida.Inscricoes.Select(i => i.UnidadeId).Should().BeEquivalentTo([u1.Id, u2.Id, u3.Id]);
    }

    [Fact]
    public async Task AtribuirERemoverVagaManualNaRodadaAtiva()
    {
        var unidade = Unidade.Criar("101");
        var vaga = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);
        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var vagas = new VagaRepositorioEmMemoria(vaga);
        var atribuir = new AtribuirVagaManual(rodadas, vagas);
        var remover = new RemoverAtribuicaoManual(rodadas);

        var atribuicaoId = await atribuir.ExecutarAsync(new AtribuirVagaManualCommand(rodada.Id, unidade.Id, vaga.Id));

        rodada.Atribuicoes.Should().ContainSingle(a => a.Id == atribuicaoId && a.Origem == OrigemAtribuicao.Manual);

        await remover.ExecutarAsync(new RemoverAtribuicaoManualCommand(rodada.Id, atribuicaoId));
        rodada.Atribuicoes.Should().BeEmpty();
    }

    [Fact]
    public async Task AtribuirManualEmRodadaSorteadaEhBloqueado()
    {
        var unidade = Unidade.Criar("101");
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var extra = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);
        rodada.ExecutarSorteio([g1, extra], [.. Enumerable.Repeat((byte)1, 32)], Agora);
        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var atribuir = new AtribuirVagaManual(rodadas, new VagaRepositorioEmMemoria(g1, extra));

        var act = () => atribuir.ExecutarAsync(new AtribuirVagaManualCommand(rodada.Id, unidade.Id, extra.Id));

        var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
        ex.Which.Codigo.Should().Be(CodigosErro.RodadaEstadoInvalido);
    }

    [Fact]
    public async Task AvaliarPreSorteioDevolveAvisosEstruturadosSemBloquear()
    {
        var unidade = Unidade.Criar("101");
        var s1 = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var s2 = Vaga.Criar("S2", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);
        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var avaliar = new AvaliarPreSorteio(rodadas, new VagaRepositorioEmMemoria(s1, s2));

        var resultado = await avaliar.ExecutarAsync(rodada.Id);

        resultado.PodeExecutar.Should().BeTrue();
        resultado.Bloqueios.Should().BeEmpty();
        resultado.Avisos.Should().NotBeEmpty();
        resultado.Avisos.Select(a => a.Codigo).Should().Contain(CodigosAviso.CobertasInsuficientes);
    }

    [Fact]
    public async Task RemoverRodadaApagaAgregadoELiberaAno()
    {
        var unidade = Unidade.Criar("101");
        var vaga = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([unidade], Agora);
        rodada.AtribuirManual(unidade.Id, vaga);
        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var remover = new RemoverRodada(rodadas);

        await remover.ExecutarAsync(rodada.Id);

        (await rodadas.ObterPorIdAsync(rodada.Id)).Should().BeNull();
        (await rodadas.ObterPorAnoAsync(2026)).Should().BeNull();
    }

    [Fact]
    public async Task RemoverRodadaInexistenteFalha()
    {
        var remover = new RemoverRodada(new RodadaRepositorioEmMemoria());

        var act = () => remover.ExecutarAsync(Guid.NewGuid());

        var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
        ex.Which.Codigo.Should().Be(CodigosErro.NaoEncontrado);
    }
}
