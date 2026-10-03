namespace SorteioEstacionamento.Domain;

public sealed class ExecucaoSorteio
{
    public int Sequencia { get; private set; }
    public byte[] Seed { get; private set; } = [];
    public DateTimeOffset ExecutadoEm { get; private set; }
    public ResultadoExecucaoSorteio Resultado { get; private set; } = null!;

    private ExecucaoSorteio()
    {
    }

    public ExecucaoSorteio(
        int sequencia,
        byte[] seed,
        DateTimeOffset executadoEm,
        ResultadoExecucaoSorteio resultado)
    {
        Sequencia = sequencia;
        Seed = seed;
        ExecutadoEm = executadoEm;
        Resultado = resultado;
    }
}
