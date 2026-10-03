namespace SorteioEstacionamento.Domain;

public abstract record OpcaoSorteio
{
    public abstract IReadOnlyList<VagaSnapshot> Vagas { get; }

    public sealed record FormaA(VagaSnapshot Gaveta) : OpcaoSorteio
    {
        public override IReadOnlyList<VagaSnapshot> Vagas => [Gaveta];
    }

    public sealed record FormaB(VagaSnapshot Primeira, VagaSnapshot Segunda) : OpcaoSorteio
    {
        public override IReadOnlyList<VagaSnapshot> Vagas => [Primeira, Segunda];
    }

    public sealed record Complemento(VagaSnapshot Simples) : OpcaoSorteio
    {
        public override IReadOnlyList<VagaSnapshot> Vagas => [Simples];
    }
}
