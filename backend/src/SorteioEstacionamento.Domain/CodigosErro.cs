namespace SorteioEstacionamento.Domain;

public static class CodigosErro
{
    public const string RodadaEstadoInvalido = "RODADA_ESTADO_INVALIDO";
    public const string VagaIndisponivel = "VAGA_INDISPONIVEL";
    public const string ManualRegraViolada = "MANUAL_REGRA_VIOLADA";
    public const string SorteioBloqueado = "SORTEIO_BLOQUEADO";
    public const string CapacidadeInsuficiente = "CAPACIDADE_INSUFICIENTE";
    public const string NaoEncontrado = "NAO_ENCONTRADO";
    public const string AuthInvalido = "AUTH_INVALID";
    public const string AuthRequired = "AUTH_REQUIRED";
    public const string CadastroDuplicado = "CADASTRO_DUPLICADO";
    public const string CadastroInvalido = "CADASTRO_INVALIDO";
    public const string CadastroEmUso = "CADASTRO_EM_USO";
}

public static class CodigosAviso
{
    public const string CobertasInsuficientes = "COBERTAS_INSUFICIENTES";
    public const string OpcoesPendencia2Insuficientes = "OPCOES_PENDENCIA2_INSUFICIENTES";
    public const string PcdAcimaDoReferencial = "PCD_ACIMA_DO_REFERENCIAL";
}
