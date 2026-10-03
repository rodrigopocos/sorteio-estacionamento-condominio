using Microsoft.EntityFrameworkCore;
using SorteioEstacionamento.Application;
using SorteioEstacionamento.Domain;
using SorteioEstacionamento.Infrastructure.Persistence;

namespace SorteioEstacionamento.Infrastructure;

public sealed class UnidadeRepositorio : IUnidadeRepositorio
{
    private readonly SorteioDbContext _db;

    public UnidadeRepositorio(SorteioDbContext db) => _db = db;

    public async Task<Unidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Unidades.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<Unidade?> ObterPorNumeroAsync(string numero, CancellationToken cancellationToken = default) =>
        await _db.Unidades.AsNoTracking().FirstOrDefaultAsync(u => u.Numero == numero, cancellationToken);

    public async Task<IReadOnlyList<Unidade>> ListarTodasAsync(CancellationToken cancellationToken = default) =>
        await _db.Unidades.AsNoTracking().OrderBy(u => u.Numero).ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Unidade unidade, CancellationToken cancellationToken = default)
    {
        _db.Unidades.Add(unidade);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Unidade unidade, CancellationToken cancellationToken = default) =>
        await _db.SaveChangesAsync(cancellationToken);

    public async Task RemoverAsync(Unidade unidade, CancellationToken cancellationToken = default)
    {
        _db.Unidades.Remove(unidade);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new FalhaRegraNegocioException(
                CodigosErro.CadastroEmUso,
                "Unidade não pode ser removida porque está em uso em uma rodada.");
        }
    }
}

public sealed class VagaRepositorio : IVagaRepositorio
{
    private readonly SorteioDbContext _db;

    public VagaRepositorio(SorteioDbContext db) => _db = db;

    public async Task<Vaga?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Vagas.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<Vaga?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default) =>
        await _db.Vagas.AsNoTracking().FirstOrDefaultAsync(v => v.Codigo == codigo, cancellationToken);

    public async Task<IReadOnlyList<Vaga>> ListarTodasAsync(CancellationToken cancellationToken = default) =>
        await _db.Vagas.AsNoTracking().OrderBy(v => v.Codigo).ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Vaga vaga, CancellationToken cancellationToken = default)
    {
        _db.Vagas.Add(vaga);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Vaga vaga, CancellationToken cancellationToken = default) =>
        await _db.SaveChangesAsync(cancellationToken);

    public async Task RemoverAsync(Vaga vaga, CancellationToken cancellationToken = default)
    {
        _db.Vagas.Remove(vaga);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new FalhaRegraNegocioException(
                CodigosErro.CadastroEmUso,
                "Vaga não pode ser removida porque está atribuída em uma rodada.");
        }
    }
}

public sealed class RodadaRepositorio : IRodadaRepositorio
{
    private readonly SorteioDbContext _db;

    public RodadaRepositorio(SorteioDbContext db) => _db = db;

    public async Task<Rodada?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await ConsultaCompleta().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<Rodada?> ObterPorAnoAsync(int ano, CancellationToken cancellationToken = default) =>
        await _db.Rodadas.AsNoTracking().FirstOrDefaultAsync(r => r.Ano == ano, cancellationToken);

    public async Task<IReadOnlyList<Rodada>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _db.Rodadas.AsNoTracking().OrderByDescending(r => r.Ano).ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Rodada rodada, CancellationToken cancellationToken = default)
    {
        _db.Rodadas.Add(rodada);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new FalhaRegraNegocioException(
                CodigosErro.CadastroDuplicado,
                "Já existe rodada para este ano.");
        }
    }

    public async Task AtualizarAsync(Rodada rodada, CancellationToken cancellationToken = default) =>
        await _db.SaveChangesAsync(cancellationToken);

    public async Task RemoverAsync(Rodada rodada, CancellationToken cancellationToken = default)
    {
        _db.Rodadas.Remove(rodada);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Rodada> ConsultaCompleta() =>
        _db.Rodadas
            .Include(r => r.Inscricoes)
            .Include(r => r.Atribuicoes)
            .Include(r => r.Execucoes)
            .Include(r => r.Cancelamentos);
}
