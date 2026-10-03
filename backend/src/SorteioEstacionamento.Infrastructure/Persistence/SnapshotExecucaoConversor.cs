using System.Text.Json;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Infrastructure.Persistence;

internal static class SnapshotExecucaoConversor
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Serializar(ResultadoExecucaoSorteio resultado) =>
        JsonSerializer.Serialize(ParaDocumento(resultado), Json);

    public static ResultadoExecucaoSorteio Desserializar(string json)
    {
        var documento = JsonSerializer.Deserialize<SnapshotExecucaoDocumento>(json, Json)
            ?? throw new InvalidOperationException("Snapshot de execução inválido.");
        return DeDocumento(documento);
    }

    private static SnapshotExecucaoDocumento ParaDocumento(ResultadoExecucaoSorteio resultado) =>
        new()
        {
            Seed = Convert.ToBase64String(resultado.Seed),
            OrdemUnidades = [.. resultado.OrdemUnidades],
            Unidades = resultado.ResultadosPorUnidade.Select(u => new SnapshotExecucaoUnidade
            {
                UnidadeId = u.UnidadeId,
                VagaIds = u.AtribuicoesSorteio.Select(a => a.Vaga.Codigo).ToArray(),
                Origem = OrigemAtribuicao.Sorteio.ToString(),
                Status = u.Status.ToString(),
                Vagas = u.AtribuicoesSorteio.Select(a => new SnapshotVaga
                {
                    Codigo = a.Vaga.Codigo,
                    Tipo = a.Vaga.Tipo.ToString(),
                    Cobertura = a.Vaga.Cobertura.ToString(),
                    EhPcd = a.Vaga.EhPcd
                }).ToArray()
            }).ToArray()
        };

    private static ResultadoExecucaoSorteio DeDocumento(SnapshotExecucaoDocumento documento)
    {
        var seed = documento.Seed.Length == 0 ? [] : Convert.FromBase64String(documento.Seed);
        var resultados = documento.Unidades.Select(u =>
        {
            var status = Enum.Parse<StatusContemplacao>(u.Status);
            var atribuicoes = u.Vagas.Select(v =>
                new AtribuicaoSnapshot(
                    u.UnidadeId,
                    new VagaSnapshot(
                        v.Codigo,
                        Enum.Parse<TipoVaga>(v.Tipo),
                        Enum.Parse<CoberturaVaga>(v.Cobertura),
                        v.EhPcd),
                    Enum.Parse<OrigemAtribuicao>(u.Origem))).ToList();
            return new ResultadoSorteioUnidade(u.UnidadeId, atribuicoes, status);
        }).ToList();

        return new ResultadoExecucaoSorteio(seed, documento.OrdemUnidades, resultados);
    }

    private sealed class SnapshotExecucaoDocumento
    {
        public string Seed { get; set; } = "";
        public string[] OrdemUnidades { get; set; } = [];
        public SnapshotExecucaoUnidade[] Unidades { get; set; } = [];
    }

    private sealed class SnapshotExecucaoUnidade
    {
        public string UnidadeId { get; set; } = "";
        public string[] VagaIds { get; set; } = [];
        public string Origem { get; set; } = "";
        public string Status { get; set; } = "";
        public SnapshotVaga[] Vagas { get; set; } = [];
    }

    private sealed class SnapshotVaga
    {
        public string Codigo { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Cobertura { get; set; } = "";
        public bool EhPcd { get; set; }
    }
}
