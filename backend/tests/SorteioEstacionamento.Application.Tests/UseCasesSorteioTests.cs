using FluentAssertions;
using SorteioEstacionamento.Application;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application.Tests;

public sealed class UseCasesSorteioTests
{
    private static readonly DateTimeOffset Agora = DateTimeOffset.UnixEpoch;
    private static readonly byte[] Seed = Enumerable.Repeat((byte)7, 32).ToArray();

    [Fact]
    public async Task ExecutarSorteioTresUnidadesPersisteExecucaoESeed()
    {
        var u1 = Unidade.Criar("101");
        var u2 = Unidade.Criar("102");
        var u3 = Unidade.Criar("201");
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var g2 = Vaga.Criar("G2", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var g3 = Vaga.Criar("G3", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([u1, u2, u3], Agora);

        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var executar = new ExecutarSorteio(
            rodadas,
            new VagaRepositorioEmMemoria(g1, g2, g3),
            new UnidadeRepositorioEmMemoria(u1, u2, u3),
            new GeradorSeedFixo(Seed));

        var resultado = await executar.ExecutarAsync(rodada.Id);

        resultado.Sequencia.Should().Be(1);
        resultado.SeedHex.Should().Be(Convert.ToHexString(Seed));
        resultado.Estado.Should().Be(EstadoRodada.Sorteada);
        resultado.ResultadosPorUnidade.Should().HaveCount(3);

        var persistida = await rodadas.ObterPorIdAsync(rodada.Id);
        persistida!.Execucoes.Should().ContainSingle().Which.Sequencia.Should().Be(1);
        persistida.Atribuicoes.Should().OnlyContain(a => a.Origem == OrigemAtribuicao.Sorteio);
        persistida.Atribuicoes.Select(a => a.VagaId).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task CancelarSorteioRemoveSomenteAtribuicoesDeSorteio()
    {
        var u1 = Unidade.Criar("101");
        var u2 = Unidade.Criar("102");
        var manual = Vaga.Criar("M1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var s1 = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([u1, u2], Agora);
        rodada.AtribuirManual(u1.Id, manual);

        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var vagas = new VagaRepositorioEmMemoria(manual, g1, s1);
        await new ExecutarSorteio(rodadas, vagas, new UnidadeRepositorioEmMemoria(u1, u2), new GeradorSeedFixo(Seed))
            .ExecutarAsync(rodada.Id);

        await new CancelarSorteio(rodadas).ExecutarAsync(rodada.Id);

        var persistida = await rodadas.ObterPorIdAsync(rodada.Id);
        persistida!.Estado.Should().Be(EstadoRodada.Ativa);
        persistida.Atribuicoes.Should().ContainSingle(a => a.Origem == OrigemAtribuicao.Manual);
        persistida.Cancelamentos.Should().ContainSingle().Which.SequenciaCancelada.Should().Be(1);
    }

    [Fact]
    public async Task ResorteioAposCancelamentoIncrementaSequencia()
    {
        var u1 = Unidade.Criar("101");
        var u2 = Unidade.Criar("102");
        var u3 = Unidade.Criar("201");
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var g2 = Vaga.Criar("G2", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var g3 = Vaga.Criar("G3", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([u1, u2, u3], Agora);

        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var vagas = new VagaRepositorioEmMemoria(g1, g2, g3);
        var unidades = new UnidadeRepositorioEmMemoria(u1, u2, u3);
        var executar = new ExecutarSorteio(rodadas, vagas, unidades, new GeradorSeedFixo(Seed));

        await executar.ExecutarAsync(rodada.Id);
        await new CancelarSorteio(rodadas).ExecutarAsync(rodada.Id);

        var segundoSeed = Enumerable.Repeat((byte)9, 32).ToArray();
        var resultado = await new ExecutarSorteio(rodadas, vagas, unidades, new GeradorSeedFixo(segundoSeed))
            .ExecutarAsync(rodada.Id);

        resultado.Sequencia.Should().Be(2);
        resultado.SeedHex.Should().Be(Convert.ToHexString(segundoSeed));

        var persistida = await rodadas.ObterPorIdAsync(rodada.Id);
        persistida!.Execucoes.Should().HaveCount(2);
        persistida.Execucoes.Select(e => e.Sequencia).Should().BeEquivalentTo([1, 2]);
        persistida.Atribuicoes.Should().OnlyContain(a => a.Origem == OrigemAtribuicao.Sorteio);
    }

    [Fact]
    public async Task ExportarCsvEmRodadaAtivaEhBloqueado()
    {
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([Unidade.Criar("101")], Agora);
        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var exportar = new ExportarRodadaCsv(
            rodadas,
            new UnidadeRepositorioEmMemoria(),
            new ExportacaoRodadaFake());

        var act = () => exportar.ExecutarAsync(rodada.Id);

        var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
        ex.Which.Codigo.Should().Be(CodigosErro.RodadaEstadoInvalido);
    }

    [Fact]
    public void MontadorExportGeraLinhasPorVagaEGavetaComDoisLugares()
    {
        var u1 = Unidade.Criar("101");
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([u1], Agora);
        rodada.ExecutarSorteio([g1], Seed, Agora);

        var linhas = ExportacaoRodadaMontador.Montar(rodada, new Dictionary<Guid, Unidade> { [u1.Id] = u1 });

        linhas.Should().ContainSingle();
        linhas[0].Unidade.Should().Be("101");
        linhas[0].StatusContemplacao.Should().Be(nameof(StatusContemplacao.Completa));
        linhas[0].VagaCodigo.Should().Be("G1");
        linhas[0].TipoVaga.Should().Be(nameof(TipoVaga.Gaveta));
        linhas[0].LugaresFisicos.Should().Be(2);
        linhas[0].Origem.Should().Be(nameof(OrigemAtribuicao.Sorteio));
    }

    [Fact]
    public async Task ExportarCsvDelegaMontagemAoServicoDeInfra()
    {
        var u1 = Unidade.Criar("101");
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var rodada = Rodada.Criar(2026, Agora);
        rodada.Ativar([u1], Agora);
        rodada.ExecutarSorteio([g1], Seed, Agora);

        var rodadas = new RodadaRepositorioEmMemoria();
        await rodadas.AdicionarAsync(rodada);
        var exportacao = new ExportacaoRodadaFake();
        var arquivo = await new ExportarRodadaCsv(
            rodadas,
            new UnidadeRepositorioEmMemoria(u1),
            exportacao).ExecutarAsync(rodada.Id);

        arquivo.NomeArquivo.Should().Be("sorteio-2026-v1.csv");
        exportacao.UltimasLinhasCsv.Should().NotBeNull().And.ContainSingle();
    }
}
