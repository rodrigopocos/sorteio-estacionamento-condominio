namespace SorteioEstacionamento.Domain;

public static class CalculadorLugaresFisicos
{
    public static int Calcular(TipoVaga tipo) =>
        tipo == TipoVaga.Gaveta ? 2 : 1;

    public static int Calcular(VagaSnapshot vaga) => Calcular(vaga.Tipo);
}
