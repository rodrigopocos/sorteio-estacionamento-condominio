namespace SorteioEstacionamento.Domain;

public enum TipoVaga
{
    Simples = 0,
    Gaveta = 1
}

public enum CoberturaVaga
{
    Coberta = 0,
    Descoberta = 1
}

public enum OrigemAtribuicao
{
    Manual = 0,
    Sorteio = 1
}

public enum StatusContemplacao
{
    Completa = 0,
    Parcial = 1,
    NaoContemplada = 2,
    CompletaSemCobertura = 3
}

public enum EstadoRodada
{
    EmPreparacao = 0,
    Ativa = 1,
    Sorteada = 2
}
