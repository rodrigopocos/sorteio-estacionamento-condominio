namespace SorteioEstacionamento.Application;

public sealed record LinhaExportRodada(
    int AnoRodada,
    string Unidade,
    string StatusContemplacao,
    string VagaCodigo,
    string TipoVaga,
    string Cobertura,
    int LugaresFisicos,
    string Origem);

public sealed record ExportacaoRodadaCabecalho(
    int Ano,
    int Sequencia,
    DateTimeOffset ExecutadoEm,
    int UnidadesCompletas,
    int UnidadesParciais,
    int UnidadesNaoContempladas);

public sealed record ArquivoExportacaoDto(string NomeArquivo, string TipoConteudo, byte[] Conteudo);

public interface IExportacaoRodada
{
    ArquivoExportacaoDto GerarCsv(IReadOnlyList<LinhaExportRodada> linhas, int ano, int sequencia);
    ArquivoExportacaoDto GerarPdf(ExportacaoRodadaCabecalho cabecalho, IReadOnlyList<LinhaExportRodada> linhas);
}
