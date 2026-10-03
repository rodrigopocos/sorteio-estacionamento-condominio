-- Schema inicial (migration 20261003223913_Inicial)
-- Use no DBeaver: conecte em sorteio-dev.db e execute este script (ou crie o arquivo vazio e rode).

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS "Rodadas" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Rodadas" PRIMARY KEY,
    "Ano" INTEGER NOT NULL,
    "Estado" TEXT NOT NULL,
    "CriadaEm" TEXT NOT NULL,
    "AtivadaEm" TEXT NULL,
    "SorteadaEm" TEXT NULL
);

CREATE TABLE IF NOT EXISTS "Unidades" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Unidades" PRIMARY KEY,
    "Numero" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "Vagas" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Vagas" PRIMARY KEY,
    "Codigo" TEXT NOT NULL,
    "Tipo" TEXT NOT NULL,
    "Cobertura" TEXT NOT NULL,
    "EhPcd" INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS "EventosCancelamento" (
    "SequenciaCancelada" INTEGER NOT NULL,
    "RodadaId" TEXT NOT NULL,
    "CanceladoEm" TEXT NOT NULL,
    CONSTRAINT "PK_EventosCancelamento" PRIMARY KEY ("RodadaId", "SequenciaCancelada"),
    CONSTRAINT "FK_EventosCancelamento_Rodadas_RodadaId" FOREIGN KEY ("RodadaId") REFERENCES "Rodadas" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "ExecucoesSorteio" (
    "Sequencia" INTEGER NOT NULL,
    "RodadaId" TEXT NOT NULL,
    "Seed" BLOB NOT NULL,
    "ExecutadoEm" TEXT NOT NULL,
    "SnapshotJson" TEXT NOT NULL,
    CONSTRAINT "PK_ExecucoesSorteio" PRIMARY KEY ("RodadaId", "Sequencia"),
    CONSTRAINT "FK_ExecucoesSorteio_Rodadas_RodadaId" FOREIGN KEY ("RodadaId") REFERENCES "Rodadas" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "Inscricoes" (
    "UnidadeId" TEXT NOT NULL,
    "RodadaId" TEXT NOT NULL,
    CONSTRAINT "PK_Inscricoes" PRIMARY KEY ("RodadaId", "UnidadeId"),
    CONSTRAINT "FK_Inscricoes_Rodadas_RodadaId" FOREIGN KEY ("RodadaId") REFERENCES "Rodadas" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Inscricoes_Unidades_UnidadeId" FOREIGN KEY ("UnidadeId") REFERENCES "Unidades" ("Id") ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS "Atribuicoes" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Atribuicoes" PRIMARY KEY,
    "UnidadeId" TEXT NOT NULL,
    "VagaId" TEXT NOT NULL,
    "Origem" TEXT NOT NULL,
    "RodadaId" TEXT NOT NULL,
    "VagaCobertura" TEXT NOT NULL,
    "VagaCodigo" TEXT NOT NULL,
    "VagaEhPcd" INTEGER NOT NULL,
    "VagaTipo" TEXT NOT NULL,
    CONSTRAINT "FK_Atribuicoes_Rodadas_RodadaId" FOREIGN KEY ("RodadaId") REFERENCES "Rodadas" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Atribuicoes_Unidades_UnidadeId" FOREIGN KEY ("UnidadeId") REFERENCES "Unidades" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Atribuicoes_Vagas_VagaId" FOREIGN KEY ("VagaId") REFERENCES "Vagas" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Atribuicoes_RodadaId_VagaId" ON "Atribuicoes" ("RodadaId", "VagaId");
CREATE INDEX IF NOT EXISTS "IX_Atribuicoes_UnidadeId" ON "Atribuicoes" ("UnidadeId");
CREATE INDEX IF NOT EXISTS "IX_Atribuicoes_VagaId" ON "Atribuicoes" ("VagaId");
CREATE INDEX IF NOT EXISTS "IX_Inscricoes_UnidadeId" ON "Inscricoes" ("UnidadeId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Rodadas_Ano" ON "Rodadas" ("Ano");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Unidades_Numero" ON "Unidades" ("Numero");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Vagas_Codigo" ON "Vagas" ("Codigo");

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003223913_Inicial', '8.0.21');
