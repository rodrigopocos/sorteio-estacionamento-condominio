namespace SorteioEstacionamento.Domain;

public static class CalculadorPendencia
{
    public const int DireitoLugares = 2;

    public static int Calcular(IEnumerable<AtribuicaoSnapshot> atribuicoes)
    {
        var lugares = atribuicoes.Sum(a => CalculadorLugaresFisicos.Calcular(a.Vaga));
        var pendencia = DireitoLugares - lugares;
        return pendencia < 0 ? 0 : pendencia;
    }
}
