namespace SorteioEstacionamento.Domain;

public sealed class Inscricao
{
    public Guid UnidadeId { get; private set; }

    private Inscricao()
    {
    }

    public Inscricao(Guid unidadeId) => UnidadeId = unidadeId;
}
