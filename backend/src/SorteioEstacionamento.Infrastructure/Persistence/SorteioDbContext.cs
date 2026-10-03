using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Infrastructure.Persistence;

public sealed class SorteioDbContext : DbContext
{
    public SorteioDbContext(DbContextOptions<SorteioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Unidade> Unidades => Set<Unidade>();
    public DbSet<Vaga> Vagas => Set<Vaga>();
    public DbSet<Rodada> Rodadas => Set<Rodada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurarUnidade(modelBuilder);
        ConfigurarVaga(modelBuilder);
        ConfigurarRodada(modelBuilder);
        ConfigurarInscricao(modelBuilder);
        ConfigurarAtribuicao(modelBuilder);
        ConfigurarExecucao(modelBuilder);
        ConfigurarCancelamento(modelBuilder);
    }

    private static void ConfigurarUnidade(ModelBuilder modelBuilder)
    {
        var b = modelBuilder.Entity<Unidade>();
        b.ToTable("Unidades");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).ValueGeneratedNever();
        b.Property(u => u.Numero).HasMaxLength(32).IsRequired();
        b.HasIndex(u => u.Numero).IsUnique();
    }

    private static void ConfigurarVaga(ModelBuilder modelBuilder)
    {
        var b = modelBuilder.Entity<Vaga>();
        b.ToTable("Vagas");
        b.HasKey(v => v.Id);
        b.Property(v => v.Id).ValueGeneratedNever();
        b.Property(v => v.Codigo).HasMaxLength(64).IsRequired();
        b.HasIndex(v => v.Codigo).IsUnique();
        b.Property(v => v.Tipo).HasConversion<string>().HasMaxLength(16);
        b.Property(v => v.Cobertura).HasConversion<string>().HasMaxLength(16);
    }

    private static void ConfigurarRodada(ModelBuilder modelBuilder)
    {
        var b = modelBuilder.Entity<Rodada>();
        b.ToTable("Rodadas");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedNever();
        b.HasIndex(r => r.Ano).IsUnique();
        b.Property(r => r.Estado).HasConversion<string>().HasMaxLength(24);

        b.HasMany(r => r.Inscricoes)
            .WithOne()
            .HasForeignKey("RodadaId")
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(r => r.Inscricoes).HasField("_inscricoes").UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(r => r.Atribuicoes)
            .WithOne()
            .HasForeignKey("RodadaId")
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(r => r.Atribuicoes).HasField("_atribuicoes").UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(r => r.Execucoes)
            .WithOne()
            .HasForeignKey("RodadaId")
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(r => r.Execucoes).HasField("_execucoes").UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(r => r.Cancelamentos)
            .WithOne()
            .HasForeignKey("RodadaId")
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(r => r.Cancelamentos).HasField("_cancelamentos").UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigurarInscricao(ModelBuilder modelBuilder)
    {
        var b = modelBuilder.Entity<Inscricao>();
        b.ToTable("Inscricoes");
        b.Property<Guid>("RodadaId");
        b.HasKey("RodadaId", nameof(Inscricao.UnidadeId));
        b.HasOne<Unidade>()
            .WithMany()
            .HasForeignKey(i => i.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarAtribuicao(ModelBuilder modelBuilder)
    {
        var b = modelBuilder.Entity<Atribuicao>();
        b.ToTable("Atribuicoes");
        b.HasKey(a => a.Id);
        b.Property(a => a.Id).ValueGeneratedNever();
        b.Property<Guid>("RodadaId");
        b.HasIndex("RodadaId", nameof(Atribuicao.VagaId)).IsUnique();
        b.Property(a => a.Origem).HasConversion<string>().HasMaxLength(16);
        b.HasOne<Unidade>()
            .WithMany()
            .HasForeignKey(a => a.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Vaga>()
            .WithMany()
            .HasForeignKey(a => a.VagaId)
            .OnDelete(DeleteBehavior.Restrict);

        b.ComplexProperty(a => a.DadosVaga, v =>
        {
            v.Property(x => x.Codigo).HasColumnName("VagaCodigo").HasMaxLength(64).IsRequired();
            v.Property(x => x.Tipo).HasColumnName("VagaTipo").HasConversion<string>().HasMaxLength(16);
            v.Property(x => x.Cobertura).HasColumnName("VagaCobertura").HasConversion<string>().HasMaxLength(16);
            v.Property(x => x.EhPcd).HasColumnName("VagaEhPcd");
        });
    }

    private static void ConfigurarExecucao(ModelBuilder modelBuilder)
    {
        var b = modelBuilder.Entity<ExecucaoSorteio>();
        b.ToTable("ExecucoesSorteio");
        b.Property<Guid>("RodadaId");
        b.HasKey("RodadaId", nameof(ExecucaoSorteio.Sequencia));
        b.Property(e => e.Seed).IsRequired();

        var conversor = new ValueConverter<ResultadoExecucaoSorteio, string>(
            v => SnapshotExecucaoConversor.Serializar(v),
            v => SnapshotExecucaoConversor.Desserializar(v));

        b.Property(e => e.Resultado)
            .HasColumnName("SnapshotJson")
            .HasConversion(conversor)
            .IsRequired();
    }

    private static void ConfigurarCancelamento(ModelBuilder modelBuilder)
    {
        var b = modelBuilder.Entity<EventoCancelamento>();
        b.ToTable("EventosCancelamento");
        b.Property<Guid>("RodadaId");
        b.HasKey("RodadaId", nameof(EventoCancelamento.SequenciaCancelada));
    }
}
