using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class ConsultarVagas
{
    private readonly IVagaRepositorio _vagas;

    public ConsultarVagas(IVagaRepositorio vagas) => _vagas = vagas;

    public async Task<IReadOnlyList<VagaDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var itens = await _vagas.ListarTodasAsync(cancellationToken);
        return itens.Select(VagaDto.De).ToList();
    }

    public async Task<VagaDto> ObterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vaga = await _vagas.ObterPorIdAsync(id, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Vaga não encontrada.");
        return VagaDto.De(vaga);
    }
}

public sealed class CadastrarVaga
{
    private readonly IVagaRepositorio _vagas;

    public CadastrarVaga(IVagaRepositorio vagas) => _vagas = vagas;

    public async Task<ResultadoVaga> ExecutarAsync(
        CadastrarVagaCommand comando,
        CancellationToken cancellationToken = default)
    {
        var codigo = ValidarCodigo(comando.Codigo);
        await GarantirCodigoLivreAsync(codigo, idAtual: null, cancellationToken);
        var vaga = Vaga.Criar(codigo, comando.Tipo, comando.Cobertura, comando.EhPcd);
        await _vagas.AdicionarAsync(vaga, cancellationToken);
        return new ResultadoVaga(VagaDto.De(vaga), await AvisosPcdAsync(cancellationToken));
    }

    internal static string ValidarCodigo(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new FalhaRegraNegocioException(CodigosErro.CadastroInvalido, "Código da vaga é obrigatório.");

        return codigo.Trim();
    }

    internal async Task GarantirCodigoLivreAsync(
        string codigo,
        Guid? idAtual,
        CancellationToken cancellationToken)
    {
        var existente = await _vagas.ObterPorCodigoAsync(codigo, cancellationToken);
        if (existente is not null && existente.Id != idAtual)
            throw new FalhaRegraNegocioException(CodigosErro.CadastroDuplicado, "Já existe vaga com este código.");
    }

    internal async Task<IReadOnlyList<AvisoDto>> AvisosPcdAsync(CancellationToken cancellationToken)
    {
        var vagas = await _vagas.ListarTodasAsync(cancellationToken);
        if (vagas.Count(v => v.EhPcd) <= 2)
            return [];

        return
        [
            new AvisoDto(
                CodigosAviso.PcdAcimaDoReferencial,
                "Há mais de 2 vagas marcadas como PCD no cadastro (referência do condomínio).")
        ];
    }
}

public sealed class AtualizarVaga
{
    private readonly IVagaRepositorio _vagas;
    private readonly CadastrarVaga _cadastro;

    public AtualizarVaga(IVagaRepositorio vagas)
    {
        _vagas = vagas;
        _cadastro = new CadastrarVaga(vagas);
    }

    public async Task<ResultadoVaga> ExecutarAsync(
        AtualizarVagaCommand comando,
        CancellationToken cancellationToken = default)
    {
        var codigo = CadastrarVaga.ValidarCodigo(comando.Codigo);
        var vaga = await _vagas.ObterPorIdAsync(comando.Id, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Vaga não encontrada.");

        await _cadastro.GarantirCodigoLivreAsync(codigo, comando.Id, cancellationToken);
        vaga.Alterar(codigo, comando.Tipo, comando.Cobertura, comando.EhPcd);
        await _vagas.AtualizarAsync(vaga, cancellationToken);
        return new ResultadoVaga(VagaDto.De(vaga), await _cadastro.AvisosPcdAsync(cancellationToken));
    }
}

public sealed class RemoverVaga
{
    private readonly IVagaRepositorio _vagas;

    public RemoverVaga(IVagaRepositorio vagas) => _vagas = vagas;

    public async Task ExecutarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vaga = await _vagas.ObterPorIdAsync(id, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Vaga não encontrada.");

        await _vagas.RemoverAsync(vaga, cancellationToken);
    }
}
