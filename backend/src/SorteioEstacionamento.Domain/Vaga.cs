namespace SorteioEstacionamento.Domain;

public sealed class Vaga
{
    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = "";
    public TipoVaga Tipo { get; private set; }
    public CoberturaVaga Cobertura { get; private set; }
    public bool EhPcd { get; private set; }

    private Vaga()
    {
    }

    public Vaga(Guid id, string codigo, TipoVaga tipo, CoberturaVaga cobertura, bool ehPcd = false)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Código da vaga é obrigatório.", nameof(codigo));

        Id = id;
        Codigo = codigo.Trim();
        Tipo = tipo;
        Cobertura = cobertura;
        EhPcd = ehPcd;
    }

    public static Vaga Criar(string codigo, TipoVaga tipo, CoberturaVaga cobertura, bool ehPcd = false) =>
        new(Guid.NewGuid(), codigo, tipo, cobertura, ehPcd);

    public void Alterar(string codigo, TipoVaga tipo, CoberturaVaga cobertura, bool ehPcd)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Código da vaga é obrigatório.", nameof(codigo));

        Codigo = codigo.Trim();
        Tipo = tipo;
        Cobertura = cobertura;
        EhPcd = ehPcd;
    }

    public VagaSnapshot ParaSnapshot() => new(Codigo, Tipo, Cobertura, EhPcd);
}
