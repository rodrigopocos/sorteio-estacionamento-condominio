using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public interface IRodadaRepositorio
{
    Task<Rodada?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Rodada?> ObterPorAnoAsync(int ano, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Rodada>> ListarAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Rodada rodada, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Rodada rodada, CancellationToken cancellationToken = default);
    Task RemoverAsync(Rodada rodada, CancellationToken cancellationToken = default);
}

public interface IUnidadeRepositorio
{
    Task<Unidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Unidade?> ObterPorNumeroAsync(string numero, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Unidade>> ListarTodasAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Unidade unidade, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Unidade unidade, CancellationToken cancellationToken = default);
    Task RemoverAsync(Unidade unidade, CancellationToken cancellationToken = default);
}

public interface IVagaRepositorio
{
    Task<Vaga?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vaga?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vaga>> ListarTodasAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Vaga vaga, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Vaga vaga, CancellationToken cancellationToken = default);
    Task RemoverAsync(Vaga vaga, CancellationToken cancellationToken = default);
}

public interface IAdminCredenciais
{
    bool Conferir(string usuario, string senha);
}
