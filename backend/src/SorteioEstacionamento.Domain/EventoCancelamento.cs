namespace SorteioEstacionamento.Domain;

public sealed class EventoCancelamento
{
    public int SequenciaCancelada { get; private set; }
    public DateTimeOffset CanceladoEm { get; private set; }

    private EventoCancelamento()
    {
    }

    public EventoCancelamento(int sequenciaCancelada, DateTimeOffset canceladoEm)
    {
        SequenciaCancelada = sequenciaCancelada;
        CanceladoEm = canceladoEm;
    }
}
