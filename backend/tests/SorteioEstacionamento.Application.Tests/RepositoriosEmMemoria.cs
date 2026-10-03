using SorteioEstacionamento.Application;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application.Tests;

internal sealed class RodadaRepositorioEmMemoria : IRodadaRepositorio
{
    private readonly Dictionary<Guid, Rodada> _itens = [];

    public Task<Rodada?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_itens.TryGetValue(id, out var rodada) ? rodada : null);

    public Task<Rodada?> ObterPorAnoAsync(int ano, CancellationToken cancellationToken = default) =>
        Task.FromResult(_itens.Values.FirstOrDefault(r => r.Ano == ano));

    public Task<IReadOnlyList<Rodada>> ListarAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Rodada>>(_itens.Values.OrderByDescending(r => r.Ano).ToList());

    public Task AdicionarAsync(Rodada rodada, CancellationToken cancellationToken = default)
    {
        _itens[rodada.Id] = rodada;
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Rodada rodada, CancellationToken cancellationToken = default)
    {
        _itens[rodada.Id] = rodada;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Rodada rodada, CancellationToken cancellationToken = default)
    {
        _itens.Remove(rodada.Id);
        return Task.CompletedTask;
    }
}

internal sealed class UnidadeRepositorioEmMemoria : IUnidadeRepositorio
{
    private readonly List<Unidade> _itens;

    public UnidadeRepositorioEmMemoria(params Unidade[] unidades) => _itens = [.. unidades];

    public Task<Unidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_itens.FirstOrDefault(u => u.Id == id));

    public Task<Unidade?> ObterPorNumeroAsync(string numero, CancellationToken cancellationToken = default) =>
        Task.FromResult(_itens.FirstOrDefault(u => u.Numero == numero.Trim()));

    public Task<IReadOnlyList<Unidade>> ListarTodasAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Unidade>>(_itens);

    public Task AdicionarAsync(Unidade unidade, CancellationToken cancellationToken = default)
    {
        _itens.Add(unidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Unidade unidade, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RemoverAsync(Unidade unidade, CancellationToken cancellationToken = default)
    {
        _itens.Remove(unidade);
        return Task.CompletedTask;
    }
}

internal sealed class VagaRepositorioEmMemoria : IVagaRepositorio
{
    private readonly List<Vaga> _itens;

    public VagaRepositorioEmMemoria(params Vaga[] vagas) => _itens = [.. vagas];

    public Task<Vaga?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_itens.FirstOrDefault(v => v.Id == id));

    public Task<Vaga?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default) =>
        Task.FromResult(_itens.FirstOrDefault(v => v.Codigo == codigo.Trim()));

    public Task<IReadOnlyList<Vaga>> ListarTodasAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Vaga>>(_itens);

    public Task AdicionarAsync(Vaga vaga, CancellationToken cancellationToken = default)
    {
        _itens.Add(vaga);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Vaga vaga, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RemoverAsync(Vaga vaga, CancellationToken cancellationToken = default)
    {
        _itens.Remove(vaga);
        return Task.CompletedTask;
    }
}

internal sealed class GeradorSeedFixo : IGeradorSeedSorteio
{
    private readonly byte[] _seed;

    public GeradorSeedFixo(byte[] seed) => _seed = seed;

    public byte[] Gerar32Bytes() => _seed.ToArray();
}

internal sealed class ExportacaoRodadaFake : IExportacaoRodada
{
    public IReadOnlyList<LinhaExportRodada>? UltimasLinhasCsv { get; private set; }

    public ArquivoExportacaoDto GerarCsv(IReadOnlyList<LinhaExportRodada> linhas, int ano, int sequencia)
    {
        UltimasLinhasCsv = linhas;
        return new ArquivoExportacaoDto(
            $"sorteio-{ano}-v{sequencia}.csv",
            "text/csv",
            [0xEF, 0xBB, 0xBF]);
    }

    public ArquivoExportacaoDto GerarPdf(ExportacaoRodadaCabecalho cabecalho, IReadOnlyList<LinhaExportRodada> linhas) =>
        new($"sorteio-{cabecalho.Ano}-v{cabecalho.Sequencia}.pdf", "application/pdf", [1, 2, 3]);
}

internal sealed class AdminCredenciaisFake : IAdminCredenciais
{
    private readonly string _usuario;
    private readonly string _senha;

    public AdminCredenciaisFake(string usuario = "admin", string senha = "segredo")
    {
        _usuario = usuario;
        _senha = senha;
    }

    public bool Conferir(string usuario, string senha) => usuario == _usuario && senha == _senha;
}
