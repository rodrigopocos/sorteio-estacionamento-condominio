namespace SorteioEstacionamento.Domain;

public sealed class Unidade
{
    public Guid Id { get; private set; }
    public string Numero { get; private set; } = "";

    private Unidade()
    {
    }

    public Unidade(Guid id, string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Número da unidade é obrigatório.", nameof(numero));

        Id = id;
        Numero = numero.Trim();
    }

    public static Unidade Criar(string numero) => new(Guid.NewGuid(), numero);

    public void AlterarNumero(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Número da unidade é obrigatório.", nameof(numero));

        Numero = numero.Trim();
    }
}
