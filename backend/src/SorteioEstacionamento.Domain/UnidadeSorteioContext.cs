namespace SorteioEstacionamento.Domain;

public sealed record UnidadeSorteioContext(
    string UnidadeId,
    IReadOnlyList<AtribuicaoSnapshot> AtribuicoesManuais)
{
    public int PendenciaLugares => CalculadorPendencia.Calcular(AtribuicoesManuais);
}
