namespace SorteioEstacionamento.Domain;

public sealed class Atribuicao
{
    public Guid Id { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid VagaId { get; private set; }
    public OrigemAtribuicao Origem { get; private set; }
    public VagaSnapshot DadosVaga { get; private set; } = null!;

    private Atribuicao()
    {
    }

    public Atribuicao(
        Guid id,
        Guid unidadeId,
        Guid vagaId,
        OrigemAtribuicao origem,
        VagaSnapshot dadosVaga)
    {
        Id = id;
        UnidadeId = unidadeId;
        VagaId = vagaId;
        Origem = origem;
        DadosVaga = dadosVaga;
    }

    public AtribuicaoSnapshot ParaSnapshot() =>
        new(UnidadeId.ToString(), DadosVaga, Origem);
}
