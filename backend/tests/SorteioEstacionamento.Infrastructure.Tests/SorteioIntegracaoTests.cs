using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SorteioEstacionamento.Application;
using SorteioEstacionamento.Domain;
using SorteioEstacionamento.Infrastructure;
using SorteioEstacionamento.Infrastructure.Persistence;

namespace SorteioEstacionamento.Infrastructure.Tests;

public sealed class SorteioIntegracaoTests : IAsyncLifetime
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
    public async Task ExecutarSorteioPersisteExecucao_ExportCsvEPdfGeramArquivo()
    {
        var u1 = Unidade.Criar("101");
        var u2 = Unidade.Criar("102");
        var u3 = Unidade.Criar("201");
        var g1 = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var g2 = Vaga.Criar("G2", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var g3 = Vaga.Criar("G3", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var seed = Enumerable.Repeat((byte)3, 32).ToArray();
        var rodada = Rodada.Criar(2028, DateTimeOffset.Parse("2028-01-02T10:00:00Z"));

        await using (var db = new SorteioDbContext(_opcoes))
        {
            db.Unidades.AddRange(u1, u2, u3);
            db.Vagas.AddRange(g1, g2, g3);
            await db.SaveChangesAsync();
            await new RodadaRepositorio(db).AdicionarAsync(rodada);
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            await new AtivarRodadaAnual(new RodadaRepositorio(db), new UnidadeRepositorio(db))
                .ExecutarAsync(rodada.Id);
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            var resultado = await new ExecutarSorteio(
                new RodadaRepositorio(db),
                new VagaRepositorio(db),
                new UnidadeRepositorio(db),
                new GeradorSeedFixoIntegracao(seed)).ExecutarAsync(rodada.Id);

            resultado.Sequencia.Should().Be(1);
            resultado.Estado.Should().Be(EstadoRodada.Sorteada);
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            var persistida = await new RodadaRepositorio(db).ObterPorIdAsync(rodada.Id);
            persistida!.Execucoes.Should().ContainSingle();
            persistida.Execucoes[0].Seed.Should().Equal(seed);
            persistida.Execucoes[0].Resultado.ResultadosPorUnidade.Should().HaveCount(3);
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            var exportacao = new ExportacaoRodadaServico();
            var csv = await new ExportarRodadaCsv(
                new RodadaRepositorio(db),
                new UnidadeRepositorio(db),
                exportacao).ExecutarAsync(rodada.Id);
            var pdf = await new ExportarRodadaPdf(
                new RodadaRepositorio(db),
                new UnidadeRepositorio(db),
                exportacao).ExecutarAsync(rodada.Id);

            csv.NomeArquivo.Should().Be("sorteio-2028-v1.csv");
            csv.Conteudo.Should().NotBeEmpty();
            System.Text.Encoding.UTF8.GetString(csv.Conteudo).Should().Contain("ano_rodada;unidade");
            pdf.NomeArquivo.Should().Be("sorteio-2028-v1.pdf");
            pdf.Conteudo.Length.Should().BeGreaterThan(100);
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            await new CancelarSorteio(new RodadaRepositorio(db)).ExecutarAsync(rodada.Id);
            await new ExecutarSorteio(
                new RodadaRepositorio(db),
                new VagaRepositorio(db),
                new UnidadeRepositorio(db),
                new GeradorSeedFixoIntegracao(seed)).ExecutarAsync(rodada.Id);
        }

        await using (var db = new SorteioDbContext(_opcoes))
        {
            var persistida = await new RodadaRepositorio(db).ObterPorIdAsync(rodada.Id);
            persistida!.Execucoes.Should().HaveCount(2);
            persistida.Execucoes.Last().Sequencia.Should().Be(2);
        }
    }

    private sealed class GeradorSeedFixoIntegracao : IGeradorSeedSorteio
    {
        private readonly byte[] _seed;

        public GeradorSeedFixoIntegracao(byte[] seed) => _seed = seed;

        public byte[] Gerar32Bytes() => _seed.ToArray();
    }
}
