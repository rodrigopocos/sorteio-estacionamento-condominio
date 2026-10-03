namespace SorteioEstacionamento.Domain;

public sealed class Rodada
{
    private readonly List<Inscricao> _inscricoes = [];
    private readonly List<Atribuicao> _atribuicoes = [];
    private readonly List<ExecucaoSorteio> _execucoes = [];
    private readonly List<EventoCancelamento> _cancelamentos = [];

    public Guid Id { get; private set; }
    public int Ano { get; private set; }
    public EstadoRodada Estado { get; private set; }
    public DateTimeOffset CriadaEm { get; private set; }
    public DateTimeOffset? AtivadaEm { get; private set; }
    public DateTimeOffset? SorteadaEm { get; private set; }

    public IReadOnlyList<Inscricao> Inscricoes => _inscricoes;
    public IReadOnlyList<Atribuicao> Atribuicoes => _atribuicoes;
    public IReadOnlyList<ExecucaoSorteio> Execucoes => _execucoes;
    public IReadOnlyList<EventoCancelamento> Cancelamentos => _cancelamentos;

    private Rodada()
    {
    }

    private Rodada(Guid id, int ano, DateTimeOffset criadaEm)
    {
        Id = id;
        Ano = ano;
        Estado = EstadoRodada.EmPreparacao;
        CriadaEm = criadaEm;
    }

    public static Rodada Criar(int ano, DateTimeOffset criadaEm)
    {
        if (ano < 2000)
            throw new ArgumentOutOfRangeException(nameof(ano), "Ano da rodada inválido.");

        return new Rodada(Guid.NewGuid(), ano, criadaEm);
    }

    public void Ativar(IReadOnlyList<Unidade> unidades, DateTimeOffset agora)
    {
        GarantirEstado(EstadoRodada.EmPreparacao);

        _inscricoes.Clear();
        foreach (var unidade in unidades.DistinctBy(u => u.Id))
            _inscricoes.Add(new Inscricao(unidade.Id));

        Estado = EstadoRodada.Ativa;
        AtivadaEm = agora;
    }

    public Atribuicao AtribuirManual(Guid unidadeId, Vaga vaga)
    {
        GarantirEstado(EstadoRodada.Ativa);
        GarantirInscrita(unidadeId);

        if (_atribuicoes.Any(a => a.VagaId == vaga.Id))
            throw new FalhaRegraNegocioException(
                CodigosErro.VagaIndisponivel,
                "A vaga já está atribuída nesta rodada.");

        var manuaisDaUnidade = ManuaisDaUnidade(unidadeId)
            .Select(a => a.DadosVaga)
            .Append(vaga.ParaSnapshot())
            .ToList();

        var validacao = ValidadorAtribuicaoManual.Validar(manuaisDaUnidade);
        if (!validacao.EhValida)
            throw new FalhaRegraNegocioException(
                validacao.CodigoErro ?? CodigosErro.ManualRegraViolada,
                "A atribuição manual viola as regras da rodada.");

        var atribuicao = new Atribuicao(
            Guid.NewGuid(),
            unidadeId,
            vaga.Id,
            OrigemAtribuicao.Manual,
            vaga.ParaSnapshot());
        _atribuicoes.Add(atribuicao);
        return atribuicao;
    }

    public void RemoverManual(Guid atribuicaoId)
    {
        GarantirEstado(EstadoRodada.Ativa);

        var atribuicao = _atribuicoes.FirstOrDefault(a => a.Id == atribuicaoId)
            ?? throw new FalhaRegraNegocioException(
                CodigosErro.NaoEncontrado,
                "Atribuição manual não encontrada.");

        if (atribuicao.Origem != OrigemAtribuicao.Manual)
            throw new FalhaRegraNegocioException(
                CodigosErro.ManualRegraViolada,
                "Somente atribuições manuais podem ser removidas por esta operação.");

        _atribuicoes.Remove(atribuicao);
    }

    public ResultadoExecucaoRodada ExecutarSorteio(
        IReadOnlyList<Vaga> vagasCadastradas,
        byte[] seed,
        DateTimeOffset agora)
    {
        var validacao = ValidadorPreSorteio.Validar(this, vagasCadastradas);
        if (!validacao.PodeExecutar)
        {
            var bloqueio = validacao.Bloqueios[0];
            throw new FalhaRegraNegocioException(bloqueio.Codigo, bloqueio.Mensagem);
        }

        var contextos = ContextosSorteio();
        var snapshots = vagasCadastradas.Select(v => v.ParaSnapshot()).ToList();
        var resultado = MotorSorteio.Executar(snapshots, contextos, seed);
        var vagasPorCodigo = vagasCadastradas.ToDictionary(v => v.Codigo, StringComparer.Ordinal);

        foreach (var unidade in resultado.ResultadosPorUnidade)
        {
            var unidadeId = Guid.Parse(unidade.UnidadeId);
            foreach (var atribuicaoSorteio in unidade.AtribuicoesSorteio)
            {
                var vaga = vagasPorCodigo[atribuicaoSorteio.Vaga.Codigo];
                _atribuicoes.Add(new Atribuicao(
                    Guid.NewGuid(),
                    unidadeId,
                    vaga.Id,
                    OrigemAtribuicao.Sorteio,
                    atribuicaoSorteio.Vaga));
            }
        }

        var sequencia = (_execucoes.LastOrDefault()?.Sequencia ?? 0) + 1;
        var execucao = new ExecucaoSorteio(sequencia, seed.ToArray(), agora, resultado);
        _execucoes.Add(execucao);
        Estado = EstadoRodada.Sorteada;
        SorteadaEm = agora;

        return new ResultadoExecucaoRodada(execucao, validacao.Avisos);
    }

    public void CancelarSorteio(DateTimeOffset agora)
    {
        GarantirEstado(EstadoRodada.Sorteada);

        var vigente = _execucoes.LastOrDefault()
            ?? throw new FalhaRegraNegocioException(
                CodigosErro.SorteioBloqueado,
                "Não há execução de sorteio vigente para cancelar.");

        _atribuicoes.RemoveAll(a => a.Origem == OrigemAtribuicao.Sorteio);
        _cancelamentos.Add(new EventoCancelamento(vigente.Sequencia, agora));
        Estado = EstadoRodada.Ativa;
        SorteadaEm = null;
    }

    public IReadOnlyList<UnidadeSorteioContext> ContextosSorteio() =>
        _inscricoes.Select(i => new UnidadeSorteioContext(
            i.UnidadeId.ToString(),
            ManuaisDaUnidade(i.UnidadeId).Select(a => a.ParaSnapshot()).ToList()))
        .ToList();

    private IEnumerable<Atribuicao> ManuaisDaUnidade(Guid unidadeId) =>
        _atribuicoes.Where(a => a.UnidadeId == unidadeId && a.Origem == OrigemAtribuicao.Manual);

    private void GarantirInscrita(Guid unidadeId)
    {
        if (_inscricoes.All(i => i.UnidadeId != unidadeId))
            throw new FalhaRegraNegocioException(
                CodigosErro.SorteioBloqueado,
                "A unidade não está inscrita nesta rodada.");
    }

    private void GarantirEstado(EstadoRodada esperado)
    {
        if (Estado != esperado)
            throw new FalhaRegraNegocioException(
                CodigosErro.RodadaEstadoInvalido,
                "A rodada não permite esta ação no estado atual.");
    }
}

public sealed record ResultadoExecucaoRodada(
    ExecucaoSorteio Execucao,
    IReadOnlyList<AvisoPreSorteio> Avisos);
