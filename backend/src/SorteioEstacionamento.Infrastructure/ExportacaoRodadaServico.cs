using System.Globalization;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SorteioEstacionamento.Application;

namespace SorteioEstacionamento.Infrastructure;

public sealed class ExportacaoRodadaServico : IExportacaoRodada
{
    private static readonly string[] ColunasCsv =
    [
        "ano_rodada",
        "unidade",
        "status_contemplacao",
        "vaga_codigo",
        "tipo_vaga",
        "cobertura",
        "lugares_fisicos",
        "origem"
    ];

    static ExportacaoRodadaServico()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ArquivoExportacaoDto GerarCsv(IReadOnlyList<LinhaExportRodada> linhas, int ano, int sequencia)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(';', ColunasCsv));
        foreach (var linha in linhas)
        {
            builder.Append(CampoCsv(linha.AnoRodada.ToString(CultureInfo.InvariantCulture)));
            builder.Append(';');
            builder.Append(CampoCsv(linha.Unidade));
            builder.Append(';');
            builder.Append(CampoCsv(linha.StatusContemplacao));
            builder.Append(';');
            builder.Append(CampoCsv(linha.VagaCodigo));
            builder.Append(';');
            builder.Append(CampoCsv(linha.TipoVaga));
            builder.Append(';');
            builder.Append(CampoCsv(linha.Cobertura));
            builder.Append(';');
            builder.Append(CampoCsv(linha.LugaresFisicos.ToString(CultureInfo.InvariantCulture)));
            builder.Append(';');
            builder.Append(CampoCsv(linha.Origem));
            builder.AppendLine();
        }

        var conteudo = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(builder.ToString());
        return new ArquivoExportacaoDto(
            $"sorteio-{ano}-v{sequencia}.csv",
            "text/csv; charset=utf-8",
            conteudo);
    }

    public ArquivoExportacaoDto GerarPdf(ExportacaoRodadaCabecalho cabecalho, IReadOnlyList<LinhaExportRodada> linhas)
    {
        var executadoLocal = cabecalho.ExecutadoEm.ToLocalTime();
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text($"Sorteio de vagas — {cabecalho.Ano}")
                        .FontSize(16).SemiBold();
                    col.Item().Text(
                        $"Execução em {executadoLocal:dd/MM/yyyy HH:mm} (sequência {cabecalho.Sequencia})")
                        .FontSize(10);
                    col.Item().PaddingTop(6).Text(
                        $"Completas: {cabecalho.UnidadesCompletas} | Parciais: {cabecalho.UnidadesParciais} | Não contempladas: {cabecalho.UnidadesNaoContempladas}")
                        .FontSize(10);
                });

                page.Content().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(42);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.ConstantColumn(52);
                        columns.ConstantColumn(58);
                        columns.ConstantColumn(36);
                        columns.ConstantColumn(52);
                    });

                    CabecalhoCelula(table, "Ano");
                    CabecalhoCelula(table, "Unidade");
                    CabecalhoCelula(table, "Status");
                    CabecalhoCelula(table, "Vaga");
                    CabecalhoCelula(table, "Tipo");
                    CabecalhoCelula(table, "Cobertura");
                    CabecalhoCelula(table, "Lugares");
                    CabecalhoCelula(table, "Origem");

                    foreach (var linha in linhas)
                    {
                        Celula(table, linha.AnoRodada.ToString(CultureInfo.InvariantCulture));
                        Celula(table, linha.Unidade);
                        Celula(table, linha.StatusContemplacao);
                        Celula(table, linha.VagaCodigo);
                        Celula(table, linha.TipoVaga);
                        Celula(table, linha.Cobertura);
                        Celula(table, linha.LugaresFisicos.ToString(CultureInfo.InvariantCulture));
                        Celula(table, linha.Origem);
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Sequência ");
                    text.Span(cabecalho.Sequencia.ToString(CultureInfo.InvariantCulture)).SemiBold();
                    text.Span($" — gerado em {DateTimeOffset.Now.ToLocalTime():dd/MM/yyyy HH:mm}");
                });
            });
        }).GeneratePdf();

        return new ArquivoExportacaoDto(
            $"sorteio-{cabecalho.Ano}-v{cabecalho.Sequencia}.pdf",
            "application/pdf",
            pdf);
    }

    private static string CampoCsv(string valor)
    {
        if (valor.Contains(';', StringComparison.Ordinal) || valor.Contains('"', StringComparison.Ordinal))
            return $"\"{valor.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        return valor;
    }

    private static void CabecalhoCelula(TableDescriptor table, string texto) =>
        table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(texto).SemiBold();

    private static void Celula(TableDescriptor table, string texto) =>
        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(texto);
}
