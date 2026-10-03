namespace SorteioEstacionamento.Domain;

public sealed class FalhaRegraNegocioException : Exception
{
    public string Codigo { get; }

    public FalhaRegraNegocioException(string codigo, string mensagem)
        : base(mensagem)
    {
        Codigo = codigo;
    }
}
