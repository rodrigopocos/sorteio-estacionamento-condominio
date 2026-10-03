"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormEvent, useState } from "react";
import { ConfirmDialog } from "@/components/confirm-dialog";
import { Alerta, Botao, Campo, Card, Carregando, ErroPagina, Input, PageHeader, Tabela, Td, Vazio } from "@/components/ui";
import { api } from "@/lib/api";
import { chaves } from "@/lib/labels";
import type { Unidade } from "@/lib/types";

export default function UnidadesPage() {
  const qc = useQueryClient();
  const lista = useQuery({
    queryKey: chaves.unidades,
    queryFn: () => api<Unidade[]>("/api/unidades"),
  });
  const [numero, setNumero] = useState("");
  const [editando, setEditando] = useState<Unidade | null>(null);
  const [remover, setRemover] = useState<Unidade | null>(null);
  const [erro, setErro] = useState<string | null>(null);

  const criar = useMutation({
    mutationFn: () => api<Unidade>("/api/unidades", { method: "POST", body: JSON.stringify({ numero }) }),
    onSuccess: async () => {
      setNumero("");
      setErro(null);
      await qc.invalidateQueries({ queryKey: chaves.unidades });
      await qc.invalidateQueries({ queryKey: chaves.inventario });
    },
    onError: (e: Error) => setErro(e.message),
  });

  const salvar = useMutation({
    mutationFn: () =>
      api<Unidade>(`/api/unidades/${editando!.id}`, {
        method: "PUT",
        body: JSON.stringify({ numero: editando!.numero }),
      }),
    onSuccess: async () => {
      setEditando(null);
      setErro(null);
      await qc.invalidateQueries({ queryKey: chaves.unidades });
    },
    onError: (e: Error) => setErro(e.message),
  });

  const excluir = useMutation({
    mutationFn: () => api(`/api/unidades/${remover!.id}`, { method: "DELETE" }),
    onSuccess: async () => {
      setRemover(null);
      setErro(null);
      await qc.invalidateQueries({ queryKey: chaves.unidades });
      await qc.invalidateQueries({ queryKey: chaves.inventario });
    },
    onError: (e: Error) => setErro(e.message),
  });

  function onCriar(e: FormEvent) {
    e.preventDefault();
    criar.mutate();
  }

  return (
    <>
      <PageHeader
        kicker="Cadastro"
        titulo="Unidades"
        descricao="Número do apartamento, único. No anual, todas as unidades cadastradas entram na rodada ao ativar."
      />

      <Card className="mb-5">
        <form onSubmit={onCriar} className="flex flex-wrap items-end gap-3">
          <Campo label="Número do apartamento" className="w-48">
            <Input value={numero} onChange={(e) => setNumero(e.target.value)} placeholder="101" required />
          </Campo>
          <Botao type="submit" disabled={criar.isPending}>
            {criar.isPending ? "Salvando…" : "Adicionar unidade"}
          </Botao>
        </form>
      </Card>

      {erro ? <div className="mb-4"><Alerta tom="erro">{erro}</Alerta></div> : null}

      {lista.isLoading ? <Carregando /> : null}
      {lista.isError ? <ErroPagina mensagem={lista.error.message} /> : null}
      {lista.data && lista.data.length === 0 ? (
        <Vazio titulo="Nenhuma unidade ainda" descricao="Cadastre o primeiro apartamento para começar a rodada." />
      ) : null}
      {lista.data && lista.data.length > 0 ? (
        <Card>
          <Tabela colunas={["Número", ""]}>
            {lista.data.map((u) => (
              <tr key={u.id}>
                <Td className="font-semibold">{u.numero}</Td>
                <Td className="text-right">
                  <Botao variante="ghost" type="button" onClick={() => setEditando({ ...u })}>
                    Editar
                  </Botao>
                  <Botao variante="ghost" type="button" onClick={() => setRemover(u)}>
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
            className="w-full max-w-md rounded-2xl border border-line bg-cream p-5"
            onSubmit={(e) => {
              e.preventDefault();
              salvar.mutate();
            }}
          >
            <h2 className="font-display text-xl">Editar unidade</h2>
            <Campo label="Número" className="mt-4">
              <Input value={editando.numero} onChange={(e) => setEditando({ ...editando, numero: e.target.value })} />
            </Campo>
            <div className="mt-5 flex justify-end gap-2">
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
        titulo="Excluir unidade?"
        descricao={`A unidade ${remover?.numero} será removida do cadastro. Não é possível excluir se já estiver em uma rodada.`}
        confirmar="Excluir"
        perigo
        ocupado={excluir.isPending}
        onCancelar={() => setRemover(null)}
        onConfirmar={() => excluir.mutate()}
      />
    </>
  );
}
