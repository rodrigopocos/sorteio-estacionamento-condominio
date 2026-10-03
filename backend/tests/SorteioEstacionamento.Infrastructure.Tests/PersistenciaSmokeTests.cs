using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SorteioEstacionamento.Application;
using SorteioEstacionamento.Domain;
using SorteioEstacionamento.Infrastructure;
using SorteioEstacionamento.Infrastructure.Persistence;

namespace SorteioEstacionamento.Infrastructure.Tests;

public sealed class PersistenciaSmokeTests : IAsyncLifetime
{
    private SqliteConnection _conexao = null!;
    private DbContextOptions<SorteioDbContext> _opcoes = null!;

    public async Task InitializeAsync()
    {
        _conexao = new SqliteConnection("Data Source=:memory:");
        await _conexao.OpenAsync();
        _opcoes = new DbContextOptionsBuilder<SorteioDbContext>().UseSqlite(_conexao).Options;
        await using var db = new SorteioDbContext(_opcoes);
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _conexao.DisposeAsync();

    [Fact]
    public async Task CriarRodada_AnoUnicoPersisteEDuplicataFalha()
    {
        Guid rodadaId;
        await using (var db = new SorteioDbContext(_opcoes))
        {
            var dto = await new CriarRodada(new RodadaRepositorio(db))
                .ExecutarAsync(new CriarRodadaCommand(2027));
            rodadaId = dto.Id;
            dto.Estado.Should().Be(EstadoRodada.EmPreparacao);
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            var persistida = await new RodadaRepositorio(db).ObterPorAnoAsync(2027);
            persistida!.Id.Should().Be(rodadaId);

            var act = () => new CriarRodada(new RodadaRepositorio(db))
                .ExecutarAsync(new CriarRodadaCommand(2027));
            var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
            ex.Which.Codigo.Should().Be(CodigosErro.CadastroDuplicado);
        }
    }

    [Fact]
    public async Task RoundtripRodada_AtivarManualSorteioCancelar()
    {
        var u1 = Unidade.Criar("101");
        var u2 = Unidade.Criar("102");
        var manual = Vaga.Criar("M1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var s1 = Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta);
        var s2 = Vaga.Criar("S2", TipoVaga.Simples, CoberturaVaga.Descoberta);
        var agora = DateTimeOffset.Parse("2026-03-10T12:00:00Z");
        var seed = Enumerable.Repeat((byte)11, 32).ToArray();

        var rodada = Rodada.Criar(2026, agora);
        await using (var db = new SorteioDbContext(_opcoes))
        {
            db.Unidades.AddRange(u1, u2);
            db.Vagas.AddRange(manual, g1, s1, s2);
            await db.SaveChangesAsync();
            await new RodadaRepositorio(db).AdicionarAsync(rodada);
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            var ativar = new AtivarRodadaAnual(new RodadaRepositorio(db), new UnidadeRepositorio(db));
            await ativar.ExecutarAsync(rodada.Id);
        }

        Guid atribuicaoManualId;
        await using (var db = new SorteioDbContext(_opcoes))
        {
            var atribuir = new AtribuirVagaManual(new RodadaRepositorio(db), new VagaRepositorio(db));
            atribuicaoManualId = await atribuir.ExecutarAsync(
                new AtribuirVagaManualCommand(rodada.Id, u1.Id, manual.Id));
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            var repo = new RodadaRepositorio(db);
            var persistida = await repo.ObterPorIdAsync(rodada.Id);
            var vagas = await new VagaRepositorio(db).ListarTodasAsync();
            persistida!.ExecutarSorteio(vagas, seed, agora);
            await repo.AtualizarAsync(persistida);
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            var persistida = await new RodadaRepositorio(db).ObterPorIdAsync(rodada.Id);
            persistida!.Estado.Should().Be(EstadoRodada.Sorteada);
            persistida.Inscricoes.Should().HaveCount(2);
            persistida.Atribuicoes.Should().Contain(a => a.Id == atribuicaoManualId && a.Origem == OrigemAtribuicao.Manual);
            persistida.Atribuicoes.Should().Contain(a => a.Origem == OrigemAtribuicao.Sorteio);
            persistida.Execucoes.Should().ContainSingle();
            persistida.Execucoes[0].Sequencia.Should().Be(1);
            persistida.Execucoes[0].Resultado.OrdemUnidades.Should().NotBeEmpty();
            persistida.Atribuicoes.Where(a => a.Origem == OrigemAtribuicao.Sorteio)
                .Should().NotContain(a => a.VagaId == manual.Id);

            persistida.CancelarSorteio(agora.AddHours(1));
            await new RodadaRepositorio(db).AtualizarAsync(persistida);
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            var persistida = await new RodadaRepositorio(db).ObterPorIdAsync(rodada.Id);
            persistida!.Estado.Should().Be(EstadoRodada.Ativa);
            persistida.Atribuicoes.Should().ContainSingle(a => a.Origem == OrigemAtribuicao.Manual);
            persistida.Atribuicoes.Should().NotContain(a => a.Origem == OrigemAtribuicao.Sorteio);
            persistida.Execucoes.Should().ContainSingle().Which.Sequencia.Should().Be(1);
            persistida.Cancelamentos.Should().ContainSingle()
                .Which.SequenciaCancelada.Should().Be(1);
        }
    }
}
