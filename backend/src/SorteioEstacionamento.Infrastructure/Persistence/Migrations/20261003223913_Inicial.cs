using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SorteioEstacionamento.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rodadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ano = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    CriadaEm = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AtivadaEm = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    SorteadaEm = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rodadas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Unidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vagas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Cobertura = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    EhPcd = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vagas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventosCancelamento",
                columns: table => new
                {
                    SequenciaCancelada = table.Column<int>(type: "INTEGER", nullable: false),
                    RodadaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CanceladoEm = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosCancelamento", x => new { x.RodadaId, x.SequenciaCancelada });
                    table.ForeignKey(
                        name: "FK_EventosCancelamento_Rodadas_RodadaId",
                        column: x => x.RodadaId,
                        principalTable: "Rodadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExecucoesSorteio",
                columns: table => new
                {
                    Sequencia = table.Column<int>(type: "INTEGER", nullable: false),
                    RodadaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Seed = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ExecutadoEm = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SnapshotJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecucoesSorteio", x => new { x.RodadaId, x.Sequencia });
                    table.ForeignKey(
                        name: "FK_ExecucoesSorteio_Rodadas_RodadaId",
                        column: x => x.RodadaId,
                        principalTable: "Rodadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inscricoes",
                columns: table => new
                {
                    UnidadeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RodadaId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscricoes", x => new { x.RodadaId, x.UnidadeId });
                    table.ForeignKey(
                        name: "FK_Inscricoes_Rodadas_RodadaId",
                        column: x => x.RodadaId,
                        principalTable: "Rodadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inscricoes_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Atribuicoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UnidadeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VagaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Origem = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    RodadaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VagaCobertura = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    VagaCodigo = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    VagaEhPcd = table.Column<bool>(type: "INTEGER", nullable: false),
                    VagaTipo = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atribuicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Atribuicoes_Rodadas_RodadaId",
                        column: x => x.RodadaId,
                        principalTable: "Rodadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Atribuicoes_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Atribuicoes_Vagas_VagaId",
                        column: x => x.VagaId,
                        principalTable: "Vagas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atribuicoes_RodadaId_VagaId",
                table: "Atribuicoes",
                columns: new[] { "RodadaId", "VagaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Atribuicoes_UnidadeId",
                table: "Atribuicoes",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Atribuicoes_VagaId",
                table: "Atribuicoes",
                column: "VagaId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_UnidadeId",
                table: "Inscricoes",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rodadas_Ano",
                table: "Rodadas",
                column: "Ano",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_Numero",
                table: "Unidades",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vagas_Codigo",
                table: "Vagas",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Atribuicoes");

            migrationBuilder.DropTable(
                name: "EventosCancelamento");

            migrationBuilder.DropTable(
                name: "ExecucoesSorteio");

            migrationBuilder.DropTable(
                name: "Inscricoes");

            migrationBuilder.DropTable(
                name: "Vagas");

            migrationBuilder.DropTable(
                name: "Rodadas");

            migrationBuilder.DropTable(
                name: "Unidades");
        }
    }
}
