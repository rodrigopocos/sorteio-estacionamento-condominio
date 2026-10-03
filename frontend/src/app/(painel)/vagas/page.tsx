"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormEvent, useState } from "react";
import { ConfirmDialog } from "@/components/confirm-dialog";
import { Alerta, Badge, Botao, Campo, Card, Carregando, ErroPagina, Input, PageHeader, Select, Tabela, Td, Vazio } from "@/components/ui";
import { api } from "@/lib/api";
import { chaves, rotuloCobertura, rotuloTipo } from "@/lib/labels";
import type { CoberturaVaga, ResultadoVaga, TipoVaga, Vaga } from "@/lib/types";

const vagaVazia = {
  codigo: "",
  tipo: "Simples" as TipoVaga,
  cobertura: "Coberta" as CoberturaVaga,
  ehPcd: false,
};

export default function VagasPage() {
  const qc = useQueryClient();
  const lista = useQuery({
    queryKey: chaves.vagas,
    queryFn: () => api<Vaga[]>("/api/vagas"),
  });
  const [form, setForm] = useState(vagaVazia);
  const [editando, setEditando] = useState<Vaga | null>(null);
  const [remover, setRemover] = useState<Vaga | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const [avisos, setAvisos] = useState<string[]>([]);

  async function invalidar() {
    await qc.invalidateQueries({ queryKey: chaves.vagas });
    await qc.invalidateQueries({ queryKey: chaves.inventario });
  }

  const criar = useMutation({
    mutationFn: () =>
      api<ResultadoVaga>("/api/vagas", { method: "POST", body: JSON.stringify(form) }),
    onSuccess: async (r) => {
      setForm(vagaVazia);
      setErro(null);
      setAvisos(r.avisos.map((a) => a.mensagem));
      await invalidar();
    },
    onError: (e: Error) => setErro(e.message),
  });

  const salvar = useMutation({
    mutationFn: () =>
      api<ResultadoVaga>(`/api/vagas/${editando!.id}`, {
        method: "PUT",
        body: JSON.stringify({
          codigo: editando!.codigo,
          tipo: editando!.tipo,
          cobertura: editando!.cobertura,
          ehPcd: editando!.ehPcd,
        }),
      }),
    onSuccess: async (r) => {
      setEditando(null);
      setErro(null);
      setAvisos(r.avisos.map((a) => a.mensagem));
      await invalidar();
    },
    onError: (e: Error) => setErro(e.message),
  });

  const excluir = useMutation({
    mutationFn: () => api(`/api/vagas/${remover!.id}`, { method: "DELETE" }),
    onSuccess: async () => {
      setRemover(null);
      setErro(null);
      await invalidar();
    },
    onError: (e: Error) => setErro(e.message),
  });

  function onCriar(e: FormEvent) {
    e.preventDefault();
    criar.mutate();
  }

  const lugares = lista.data?.reduce((s, v) => s + v.lugaresFisicos, 0) ?? 0;

  return (
    <>
      <PageHeader
        kicker="Cadastro"
        titulo="Vagas"
        descricao="Gaveta conta 2 lugares físicos. Marque PCD só nas vagas reservadas manualmente antes do sorteio."
        acoes={<Badge tom="olive">{lugares} lugares físicos</Badge>}
      />

      <Card className="mb-5">
        <form onSubmit={onCriar} className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5 lg:items-end">
          <Campo label="Código">
            <Input value={form.codigo} onChange={(e) => setForm({ ...form, codigo: e.target.value })} placeholder="C-12" required />
          </Campo>
          <Campo label="Tipo">
            <Select value={form.tipo} onChange={(e) => setForm({ ...form, tipo: e.target.value as TipoVaga })}>
              <option value="Simples">Simples (1 lugar)</option>
              <option value="Gaveta">Gaveta (2 lugares)</option>
            </Select>
          </Campo>
          <Campo label="Cobertura">
            <Select value={form.cobertura} onChange={(e) => setForm({ ...form, cobertura: e.target.value as CoberturaVaga })}>
              <option value="Coberta">Coberta</option>
              <option value="Descoberta">Descoberta</option>
            </Select>
          </Campo>
          <label className="flex items-center gap-2 pb-2 text-sm">
            <input
              type="checkbox"
              checked={form.ehPcd}
              onChange={(e) => setForm({ ...form, ehPcd: e.target.checked })}
            />
            Vaga PCD
          </label>
          <Botao type="submit" disabled={criar.isPending}>
            {criar.isPending ? "Salvando…" : "Adicionar vaga"}
          </Botao>
        </form>
      </Card>

      {erro ? <div className="mb-4"><Alerta tom="erro">{erro}</Alerta></div> : null}
      {avisos.length > 0 ? <div className="mb-4"><Alerta tom="aviso" titulo="Atenção" itens={avisos} /></div> : null}

      {lista.isLoading ? <Carregando /> : null}
      {lista.isError ? <ErroPagina mensagem={lista.error.message} /> : null}
      {lista.data && lista.data.length === 0 ? (
        <Vazio titulo="Nenhuma vaga ainda" descricao="Cadastre o inventário. O sorteio exige lugares ≥ 2 × unidades." />
      ) : null}
      {lista.data && lista.data.length > 0 ? (
        <Card>
          <Tabela colunas={["Código", "Tipo", "Cobertura", "Lugares", "PCD", ""]}>
            {lista.data.map((v) => (
              <tr key={v.id}>
                <Td className="font-semibold">{v.codigo}</Td>
                <Td>{rotuloTipo(v.tipo)}</Td>
                <Td>{rotuloCobertura(v.cobertura)}</Td>
                <Td>{v.lugaresFisicos}</Td>
                <Td>{v.ehPcd ? <Badge tom="copper">PCD</Badge> : "—"}</Td>
                <Td className="text-right">
                  <Botao variante="ghost" type="button" onClick={() => setEditando({ ...v })}>
                    Editar
                  </Botao>
                  <Botao variante="ghost" type="button" onClick={() => setRemover(v)}>
                    Excluir
                  </Botao>
                </Td>
              </tr>
            ))}
          </Tabela>
        </Card>
      ) : null}

      {editando ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-ink/45 p-4">
          <form
            className="w-full max-w-md space-y-3 rounded-2xl border border-line bg-cream p-5"
            onSubmit={(e) => {
              e.preventDefault();
              salvar.mutate();
            }}
          >
            <h2 className="font-display text-xl">Editar vaga</h2>
            <Campo label="Código">
              <Input value={editando.codigo} onChange={(e) => setEditando({ ...editando, codigo: e.target.value })} />
            </Campo>
            <Campo label="Tipo">
              <Select value={editando.tipo} onChange={(e) => setEditando({ ...editando, tipo: e.target.value as TipoVaga })}>
                <option value="Simples">Simples</option>
                <option value="Gaveta">Gaveta</option>
              </Select>
            </Campo>
            <Campo label="Cobertura">
              <Select value={editando.cobertura} onChange={(e) => setEditando({ ...editando, cobertura: e.target.value as CoberturaVaga })}>
                <option value="Coberta">Coberta</option>
                <option value="Descoberta">Descoberta</option>
              </Select>
            </Campo>
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={editando.ehPcd}
                onChange={(e) => setEditando({ ...editando, ehPcd: e.target.checked })}
              />
              Vaga PCD
            </label>
            <div className="flex justify-end gap-2 pt-2">
              <Botao type="button" variante="secundario" onClick={() => setEditando(null)}>
                Cancelar
              </Botao>
              <Botao type="submit" disabled={salvar.isPending}>
                Salvar
              </Botao>
            </div>
          </form>
        </div>
      ) : null}

      <ConfirmDialog
        aberto={!!remover}
        titulo="Excluir vaga?"
        descricao={`A vaga ${remover?.codigo} será removida. Não é possível excluir se já estiver atribuída em uma rodada.`}
        confirmar="Excluir"
        perigo
        ocupado={excluir.isPending}
        onCancelar={() => setRemover(null)}
        onConfirmar={() => excluir.mutate()}
      />
    </>
  );
}
