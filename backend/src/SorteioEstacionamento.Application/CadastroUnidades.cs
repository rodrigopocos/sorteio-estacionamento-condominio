using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class ConsultarUnidades
{
    private readonly IUnidadeRepositorio _unidades;

    public ConsultarUnidades(IUnidadeRepositorio unidades) => _unidades = unidades;

    public async Task<IReadOnlyList<UnidadeDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var itens = await _unidades.ListarTodasAsync(cancellationToken);
        return itens.Select(UnidadeDto.De).ToList();
    }

    public async Task<UnidadeDto> ObterAsync(Guid id, CancellationToken cancellationToken = default) =>
        UnidadeDto.De(await ObterEntidadeAsync(id, cancellationToken));

    internal async Task<Unidade> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken) =>
        await _unidades.ObterPorIdAsync(id, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Unidade não encontrada.");
}

public sealed class CadastrarUnidade
{
    private readonly IUnidadeRepositorio _unidades;

    public CadastrarUnidade(IUnidadeRepositorio unidades) => _unidades = unidades;

    public async Task<UnidadeDto> ExecutarAsync(
        CadastrarUnidadeCommand comando,
        CancellationToken cancellationToken = default)
    {
        var numero = ValidarNumero(comando.Numero);
        await GarantirNumeroLivreAsync(numero, idAtual: null, cancellationToken);
        var unidade = Unidade.Criar(numero);
        await _unidades.AdicionarAsync(unidade, cancellationToken);
        return UnidadeDto.De(unidade);
    }

    internal static string ValidarNumero(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new FalhaRegraNegocioException(CodigosErro.CadastroInvalido, "Número da unidade é obrigatório.");

        return numero.Trim();
    }

    internal async Task GarantirNumeroLivreAsync(
        string numero,
        Guid? idAtual,
        CancellationToken cancellationToken)
    {
        var existente = await _unidades.ObterPorNumeroAsync(numero, cancellationToken);
        if (existente is not null && existente.Id != idAtual)
            throw new FalhaRegraNegocioException(CodigosErro.CadastroDuplicado, "Já existe unidade com este número.");
    }
}

public sealed class AtualizarUnidade
{
    private readonly IUnidadeRepositorio _unidades;
    private readonly CadastrarUnidade _cadastro;

    public AtualizarUnidade(IUnidadeRepositorio unidades)
    {
        _unidades = unidades;
        _cadastro = new CadastrarUnidade(unidades);
    }

    public async Task<UnidadeDto> ExecutarAsync(
        AtualizarUnidadeCommand comando,
        CancellationToken cancellationToken = default)
    {
        var numero = CadastrarUnidade.ValidarNumero(comando.Numero);
        var unidade = await _unidades.ObterPorIdAsync(comando.Id, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Unidade não encontrada.");

        await _cadastro.GarantirNumeroLivreAsync(numero, comando.Id, cancellationToken);
        unidade.AlterarNumero(numero);
        await _unidades.AtualizarAsync(unidade, cancellationToken);
        return UnidadeDto.De(unidade);
    }
}

public sealed class RemoverUnidade
{
    private readonly IUnidadeRepositorio _unidades;

    public RemoverUnidade(IUnidadeRepositorio unidades) => _unidades = unidades;

    public async Task ExecutarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var unidade = await _unidades.ObterPorIdAsync(id, cancellationToken)
            ?? throw new FalhaRegraNegocioException(CodigosErro.NaoEncontrado, "Unidade não encontrada.");

        await _unidades.RemoverAsync(unidade, cancellationToken);
    }
}
