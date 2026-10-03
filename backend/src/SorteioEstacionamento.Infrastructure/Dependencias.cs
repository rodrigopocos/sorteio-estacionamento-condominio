using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SorteioEstacionamento.Application;
using SorteioEstacionamento.Infrastructure.Persistence;

namespace SorteioEstacionamento.Infrastructure;

public static class Dependencias
{
    public static IServiceCollection AddSorteioEstacionamento(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<SorteioDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<IUnidadeRepositorio, UnidadeRepositorio>();
        services.AddScoped<IVagaRepositorio, VagaRepositorio>();
        services.AddScoped<IRodadaRepositorio, RodadaRepositorio>();
        services.AddScoped<ConsultarUnidades>();
        services.AddScoped<CadastrarUnidade>();
        services.AddScoped<AtualizarUnidade>();
        services.AddScoped<RemoverUnidade>();
        services.AddScoped<ConsultarVagas>();
        services.AddScoped<CadastrarVaga>();
        services.AddScoped<AtualizarVaga>();
        services.AddScoped<RemoverVaga>();
        services.AddScoped<ObterResumoInventario>();
        services.AddScoped<AutenticarAdmin>();
        services.AddScoped<CriarRodada>();
        services.AddScoped<ListarRodadas>();
        services.AddScoped<RemoverRodada>();
        services.AddScoped<AtivarRodadaAnual>();
        services.AddScoped<ObterDetalheRodada>();
        services.AddScoped<AtribuirVagaManual>();
        services.AddScoped<RemoverAtribuicaoManual>();
        services.AddScoped<AvaliarPreSorteio>();
        services.AddSingleton<IGeradorSeedSorteio, GeradorSeedCsprng>();
        services.AddScoped<IExportacaoRodada, ExportacaoRodadaServico>();
        services.AddScoped<ExecutarSorteio>();
        services.AddScoped<CancelarSorteio>();
        services.AddScoped<ExportarRodadaCsv>();
        services.AddScoped<ExportarRodadaPdf>();
        return services;
    }
}
