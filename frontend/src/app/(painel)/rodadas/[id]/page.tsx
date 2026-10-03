"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormEvent, useState } from "react";
import { ConfirmDialog } from "@/components/confirm-dialog";
import { Alerta, Badge, Botao, Campo, Card, Carregando, ErroPagina, PageHeader, Select, Tabela, Td, Vazio } from "@/components/ui";
import { api, baixarArquivo } from "@/lib/api";
import { chaves, formatarData, lugaresDe, rotuloCobertura, rotuloEstado, rotuloOrigem, rotuloStatus, rotuloTipo } from "@/lib/labels";
import type { RodadaDetalhe, StatusContemplacao, Vaga } from "@/lib/types";

function tomEstado(estado: RodadaDetalhe["estado"]) {
  if (estado === "Sorteada") return "ok" as const;
  if (estado === "Ativa") return "olive" as const;
  return "copper" as const;
}

function tomStatus(status: StatusContemplacao) {
  if (status === "Completa") return "ok" as const;
  if (status === "Parcial") return "aviso" as const;
  if (status === "CompletaSemCobertura") return "erro" as const;
  return "neutro" as const;
}

export default function RodadaDetalhePage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const qc = useQueryClient();
  const [confirmar, setConfirmar] = useState<"ativar" | "sortear" | "cancelar" | "excluir" | null>(null);
  const [unidadeId, setUnidadeId] = useState("");
  const [vagaId, setVagaId] = useState("");
  const [erro, setErro] = useState<string | null>(null);
  const [exportando, setExportando] = useState<"csv" | "pdf" | null>(null);

  const detalhe = useQuery({
    queryKey: chaves.rodada(id),
    queryFn: () => api<RodadaDetalhe>(`/api/rodadas/${id}`),
  });
  const vagas = useQuery({
    queryKey: chaves.vagas,
    queryFn: () => api<Vaga[]>("/api/vagas"),
  });

  async function recarregar() {
    await qc.invalidateQueries({ queryKey: chaves.rodada(id) });
    await qc.invalidateQueries({ queryKey: chaves.rodadas });
  }

  const ativar = useMutation({
    mutationFn: () => api<RodadaDetalhe>(`/api/rodadas/${id}/ativar`, { method: "POST" }),
    onSuccess: async () => {
      setConfirmar(null);
      setErro(null);
      await recarregar();
    },
    onError: (e: Error) => {
      setConfirmar(null);
      setErro(e.message);
    },
  });

  const sortear = useMutation({
    mutationFn: () => api(`/api/rodadas/${id}/sorteio/executar`, { method: "POST" }),
    onSuccess: async () => {
      setConfirmar(null);
      setErro(null);
      await recarregar();
    },
    onError: (e: Error) => {
      setConfirmar(null);
      setErro(e.message);
    },
  });

  const cancelar = useMutation({
    mutationFn: () => api<RodadaDetalhe>(`/api/rodadas/${id}/sorteio/cancelar`, { method: "POST" }),
    onSuccess: async () => {
      setConfirmar(null);
      setErro(null);
      await recarregar();
    },
    onError: (e: Error) => {
      setConfirmar(null);
      setErro(e.message);
    },
  });

  const excluir = useMutation({
    mutationFn: () => api(`/api/rodadas/${id}`, { method: "DELETE" }),
    onSuccess: async () => {
      setConfirmar(null);
      await qc.invalidateQueries({ queryKey: chaves.rodadas });
      router.replace("/rodadas");
    },
    onError: (e: Error) => {
      setConfirmar(null);
      setErro(e.message);
    },
  });

  const atribuir = useMutation({
    mutationFn: () =>
      api(`/api/rodadas/${id}/atribuicoes-manuais`, {
        method: "POST",
        body: JSON.stringify({ unidadeId, vagaId }),
      }),
    onSuccess: async () => {
      setUnidadeId("");
      setVagaId("");
      setErro(null);
      await recarregar();
    },
    onError: (e: Error) => setErro(e.message),
  });

  const removerManual = useMutation({
    mutationFn: (atribuicaoId: string) =>
      api(`/api/rodadas/${id}/atribuicoes-manuais/${atribuicaoId}`, { method: "DELETE" }),
    onSuccess: async () => {
      setErro(null);
      await recarregar();
    },
    onError: (e: Error) => setErro(e.message),
  });

  async function exportar(tipo: "csv" | "pdf") {
    setExportando(tipo);
    setErro(null);
    try {
      const ano = detalhe.data?.ano ?? "rodada";
      const seq = detalhe.data?.sequenciaVigente ?? 1;
      await baixarArquivo(
        `/api/rodadas/${id}/export/${tipo}`,
        `sorteio-${ano}-v${seq}.${tipo}`,
      );
    } catch (e) {
      setErro(e instanceof Error ? e.message : "Falha na exportação.");
    } finally {
      setExportando(null);
    }
  }

  if (detalhe.isLoading) return <Carregando />;
  if (detalhe.isError) return <ErroPagina mensagem={detalhe.error.message} />;

  const r = detalhe.data!;
  const manuais = r.atribuicoes.filter((a) => a.origem === "Manual");
  const ocupadas = new Set(r.atribuicoes.map((a) => a.vagaId));
  const vagasLivres = (vagas.data ?? []).filter((v) => !ocupadas.has(v.id));
  const podeManual = r.estado === "Ativa";
  const podeSortear = r.estado === "Ativa" && r.bloqueios.length === 0;
  const ocupado = ativar.isPending || sortear.isPending || cancelar.isPending || excluir.isPending;

  function onManual(e: FormEvent) {
    e.preventDefault();
    if (!unidadeId || !vagaId) return;
    atribuir.mutate();
  }

  return (
    <>
      <PageHeader
        kicker="Rodada anual"
        titulo={`Rodada ${r.ano}`}
        descricao={
          r.sequenciaVigente
            ? `Sorteio #${r.sequenciaVigente} · criada em ${formatarData(r.criadaEm)}`
            : `Criada em ${formatarData(r.criadaEm)}`
        }
        acoes={
          <>
            <Badge tom={tomEstado(r.estado)}>{rotuloEstado(r.estado)}</Badge>
            <Link href="/rodadas" className="text-sm font-semibold text-olive hover:underline">
              Todas as rodadas
            </Link>
          </>
        }
      />

      <Stepper estado={r.estado} />

      <div className="mt-4 flex flex-wrap gap-2">
        {r.estado === "EmPreparacao" ? (
          <Botao type="button" onClick={() => setConfirmar("ativar")}>
            Ativar rodada
          </Botao>
        ) : null}
        {r.estado === "Ativa" ? (
          <Botao type="button" onClick={() => setConfirmar("sortear")} disabled={!podeSortear}>
            Executar sorteio
          </Botao>
        ) : null}
        {r.estado === "Sorteada" ? (
          <>
            <Botao type="button" variante="secundario" onClick={() => exportar("csv")} disabled={!!exportando}>
              {exportando === "csv" ? "Gerando…" : "Exportar CSV"}
            </Botao>
            <Botao type="button" variante="secundario" onClick={() => exportar("pdf")} disabled={!!exportando}>
              {exportando === "pdf" ? "Gerando…" : "Exportar PDF"}
            </Botao>
            <Botao type="button" variante="perigo" onClick={() => setConfirmar("cancelar")}>
              Cancelar sorteio
            </Botao>
          </>
        ) : null}
        <Botao type="button" variante="ghost" onClick={() => setConfirmar("excluir")}>
          Excluir rodada
        </Botao>
      </div>

      {erro ? <div className="mt-4"><Alerta tom="erro">{erro}</Alerta></div> : null}

      {r.bloqueios.length > 0 ? (
        <div className="mt-4">
          <Alerta tom="erro" titulo="Sorteio bloqueado" itens={r.bloqueios.map((b) => b.mensagem)} />
        </div>
      ) : null}
      {r.avisos.length > 0 ? (
        <div className="mt-4">
          <Alerta tom="aviso" titulo="Avisos (não impedem o sorteio)" itens={r.avisos.map((a) => a.mensagem)} />
        </div>
      ) : null}

      <div className="mt-6 grid gap-5 xl:grid-cols-2">
        <Card>
          <h2 className="font-display text-xl">Inscrições</h2>
          <p className="mb-3 text-sm text-muted">
            {r.estado === "EmPreparacao"
              ? "As unidades entram automaticamente ao ativar a rodada anual."
              : `${r.inscricoes.length} unidade(s) inscritas.`}
          </p>
          {r.inscricoes.length === 0 ? (
            <Vazio titulo="Ainda sem inscrições" descricao="Ative a rodada para incluir todas as unidades cadastradas." />
          ) : (
            <Tabela colunas={["Unidade"]}>
              {r.inscricoes.map((i) => (
                <tr key={i.unidadeId}>
                  <Td className="font-semibold">{i.numero}</Td>
                </tr>
              ))}
            </Tabela>
          )}
        </Card>

        <Card>
          <h2 className="font-display text-xl">Atribuições manuais</h2>
          <p className="mb-3 text-sm text-muted">PCD e reservas acordadas. A vaga sai do pool do sorteio.</p>
          {podeManual ? (
            <form onSubmit={onManual} className="mb-4 grid gap-3 sm:grid-cols-[1fr_1fr_auto] sm:items-end">
              <Campo label="Unidade">
                <Select value={unidadeId} onChange={(e) => setUnidadeId(e.target.value)} required>
                  <option value="">Selecione</option>
                  {r.inscricoes.map((i) => (
                    <option key={i.unidadeId} value={i.unidadeId}>
                      {i.numero}
                    </option>
                  ))}
                </Select>
              </Campo>
              <Campo label="Vaga">
                <Select value={vagaId} onChange={(e) => setVagaId(e.target.value)} required>
                  <option value="">Selecione</option>
                  {vagasLivres.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.codigo} · {rotuloTipo(v.tipo)} · {rotuloCobertura(v.cobertura)}
                      {v.ehPcd ? " · PCD" : ""}
                    </option>
                  ))}
                </Select>
              </Campo>
              <Botao type="submit" disabled={atribuir.isPending}>
                Atribuir
              </Botao>
            </form>
          ) : (
            <p className="mb-3 text-sm text-muted">
              {r.estado === "Sorteada"
                ? "Cancele o sorteio para alterar manuais."
                : "Ative a rodada para registrar manuais."}
            </p>
          )}
          {manuais.length === 0 ? (
            <Vazio titulo="Nenhuma reserva manual" descricao="Opcional no fluxo feliz — o sorteio cobre o restante." />
          ) : (
            <Tabela colunas={["Unidade", "Vaga", "Tipo", ""]}>
              {manuais.map((a) => (
                <tr key={a.id}>
                  <Td className="font-semibold">{a.unidadeNumero}</Td>
                  <Td>
                    {a.vagaCodigo}
                    {a.ehPcd ? " · PCD" : ""}
                  </Td>
                  <Td>
                    {rotuloTipo(a.tipo)} · {rotuloCobertura(a.cobertura)} · {lugaresDe(a.tipo) === 1 ? "1 lugar" : "2 lugares"}
                  </Td>
                  <Td className="text-right">
                    {podeManual ? (
                      <Botao
                        type="button"
                        variante="ghost"
                        onClick={() => removerManual.mutate(a.id)}
                        disabled={removerManual.isPending}
                      >
                        Remover
                      </Botao>
                    ) : null}
                  </Td>
                </tr>
              ))}
            </Tabela>
          )}
        </Card>
      </div>

      {r.estado === "Sorteada" ? (
        <Card className="mt-5">
          <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
            <div>
              <h2 className="font-display text-xl">Resultado</h2>
              <p className="text-sm text-muted">
                Status por unidade após o sorteio
                {r.sequenciaVigente ? ` #${r.sequenciaVigente}` : ""}.
                {r.sorteadaEm ? ` Executado em ${formatarData(r.sorteadaEm)}.` : ""}
              </p>
            </div>
          </div>
          {r.contemplacoes.length === 0 ? (
            <Vazio titulo="Sem resultado" descricao="Não há inscrições para exibir contemplação." />
          ) : (
            <Tabela colunas={["Unidade", "Status", "Vagas"]}>
              {r.contemplacoes.map((c) => {
                const attrs = r.atribuicoes.filter((a) => a.unidadeId === c.unidadeId);
                return (
                  <tr key={c.unidadeId}>
                    <Td className="font-semibold">{c.numero}</Td>
                    <Td>
                      <Badge tom={tomStatus(c.status)}>{rotuloStatus(c.status)}</Badge>
                    </Td>
                    <Td>
                      {attrs.length === 0
                        ? "—"
                        : attrs
                            .map(
                              (a) =>
                                `${a.vagaCodigo} (${rotuloTipo(a.tipo)}, ${rotuloCobertura(a.cobertura)}, ${rotuloOrigem(a.origem)})`,
                            )
                            .join(" · ")}
                    </Td>
                  </tr>
                );
              })}
            </Tabela>
          )}
        </Card>
      ) : null}

      <ConfirmDialog
        aberto={confirmar === "ativar"}
        titulo={`Ativar rodada ${r.ano}?`}
        descricao="Todas as unidades cadastradas serão inscritas. Depois disso você poderá fazer atribuições manuais e sortear."
        confirmar="Ativar"
        ocupado={ocupado}
        onCancelar={() => setConfirmar(null)}
        onConfirmar={() => ativar.mutate()}
      />
      <ConfirmDialog
        aberto={confirmar === "sortear"}
        titulo="Executar o sorteio agora?"
        descricao="A ordem das unidades e as vagas seguem a semente gerada nesta execução. Só é possível sortear de novo depois de cancelar."
        confirmar="Sortear"
        ocupado={ocupado}
        onCancelar={() => setConfirmar(null)}
        onConfirmar={() => sortear.mutate()}
      />
      <ConfirmDialog
        aberto={confirmar === "cancelar"}
        titulo="Cancelar o sorteio vigente?"
        descricao="As atribuições de sorteio serão removidas. As manuais permanecem e a rodada volta para Ativa."
        confirmar="Cancelar sorteio"
        perigo
        ocupado={ocupado}
        onCancelar={() => setConfirmar(null)}
        onConfirmar={() => cancelar.mutate()}
      />
      <ConfirmDialog
        aberto={confirmar === "excluir"}
        titulo={`Excluir rodada ${r.ano}?`}
        descricao={
          r.estado === "Sorteada"
            ? "O resultado vigente, inscrições e atribuições desta rodada serão apagados. Unidades e vagas do cadastro permanecem. O ano poderá ser criado de novo."
            : "Inscrições e atribuições desta rodada serão apagadas. Unidades e vagas do cadastro permanecem. O ano poderá ser criado de novo."
        }
        confirmar="Excluir"
        perigo
        ocupado={ocupado}
        onCancelar={() => setConfirmar(null)}
        onConfirmar={() => excluir.mutate()}
      />
    </>
  );
}

function Stepper({ estado }: { estado: RodadaDetalhe["estado"] }) {
  const passos = [
    { id: "prep", label: "Preparação" },
    { id: "ativa", label: "Ativa · manuais" },
    { id: "sorteio", label: "Sorteada · exportar" },
  ];
  const idx = estado === "EmPreparacao" ? 0 : estado === "Ativa" ? 1 : 2;
  return (
    <ol className="grid gap-2 sm:grid-cols-3">
      {passos.map((p, i) => (
        <li
          key={p.id}
          className={`rounded-xl border px-4 py-3 text-sm ${
            i === idx ? "border-olive bg-olive/10 font-semibold" : i < idx ? "border-line bg-cream text-muted" : "border-dashed border-line text-muted"
          }`}
        >
          <span className="mr-2 font-display text-copper">{String(i + 1).padStart(2, "0")}</span>
          {p.label}
        </li>
      ))}
    </ol>
  );
}
