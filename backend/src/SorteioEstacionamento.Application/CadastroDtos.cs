using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed record UnidadeDto(Guid Id, string Numero)
{
    public static UnidadeDto De(Unidade unidade) => new(unidade.Id, unidade.Numero);
}

public sealed record VagaDto(
    Guid Id,
    string Codigo,
    TipoVaga Tipo,
    CoberturaVaga Cobertura,
    bool EhPcd,
    int LugaresFisicos)
{
    public static VagaDto De(Vaga vaga) =>
        new(vaga.Id, vaga.Codigo, vaga.Tipo, vaga.Cobertura, vaga.EhPcd, CalculadorLugaresFisicos.Calcular(vaga.Tipo));
}

public sealed record AvisoDto(string Codigo, string Mensagem);

public sealed record ResultadoVaga(VagaDto Vaga, IReadOnlyList<AvisoDto> Avisos);

public sealed record ResumoInventarioDto(int TotalUnidades, int TotalVagas, int LugaresFisicos, int VagasPcd);

public sealed record CadastrarUnidadeCommand(string Numero);

public sealed record AtualizarUnidadeCommand(Guid Id, string Numero);

public sealed record CadastrarVagaCommand(string Codigo, TipoVaga Tipo, CoberturaVaga Cobertura, bool EhPcd);

public sealed record AtualizarVagaCommand(Guid Id, string Codigo, TipoVaga Tipo, CoberturaVaga Cobertura, bool EhPcd);

public sealed record LoginCommand(string Usuario, string Senha);
