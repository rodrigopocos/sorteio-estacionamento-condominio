"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormEvent, useState } from "react";
import { ConfirmDialog } from "@/components/confirm-dialog";
import { Alerta, Badge, Botao, Campo, Card, Carregando, ErroPagina, Input, PageHeader, Tabela, Td, Vazio } from "@/components/ui";
import { api } from "@/lib/api";
import { chaves, formatarData, rotuloEstado } from "@/lib/labels";
import type { RodadaDetalhe, RodadaResumo } from "@/lib/types";

function tomEstado(estado: RodadaResumo["estado"]) {
  if (estado === "Sorteada") return "ok" as const;
  if (estado === "Ativa") return "olive" as const;
  return "copper" as const;
}

export default function RodadasPage() {
  const router = useRouter();
  const qc = useQueryClient();
  const anoAtual = new Date().getFullYear();
  const [ano, setAno] = useState(String(anoAtual));
  const [erro, setErro] = useState<string | null>(null);
  const [excluir, setExcluir] = useState<RodadaResumo | null>(null);
  const lista = useQuery({
    queryKey: chaves.rodadas,
    queryFn: () => api<RodadaResumo[]>("/api/rodadas"),
  });

  const criar = useMutation({
    mutationFn: () =>
      api<RodadaDetalhe>("/api/rodadas", {
        method: "POST",
        body: JSON.stringify({ ano: Number(ano) }),
      }),
    onSuccess: async (rodada) => {
      setErro(null);
      await qc.invalidateQueries({ queryKey: chaves.rodadas });
      router.push(`/rodadas/${rodada.id}`);
    },
    onError: (e: Error) => setErro(e.message),
  });

  const remover = useMutation({
    mutationFn: () => api(`/api/rodadas/${excluir!.id}`, { method: "DELETE" }),
    onSuccess: async () => {
      setExcluir(null);
      setErro(null);
      await qc.invalidateQueries({ queryKey: chaves.rodadas });
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
        kicker="Ciclo anual"
        titulo="Rodadas"
        descricao="Um ano, uma rodada. Ative para inscrever todas as unidades, faça manuais e sorteie."
      />

      <Card className="mb-5">
        <form onSubmit={onCriar} className="flex flex-wrap items-end gap-3">
          <Campo label="Ano" className="w-32">
            <Input type="number" min={2000} value={ano} onChange={(e) => setAno(e.target.value)} required />
          </Campo>
          <Botao type="submit" disabled={criar.isPending}>
            {criar.isPending ? "Criando…" : "Criar rodada"}
          </Botao>
        </form>
      </Card>

      {erro ? <div className="mb-4"><Alerta tom="erro">{erro}</Alerta></div> : null}

      {lista.isLoading ? <Carregando /> : null}
      {lista.isError ? <ErroPagina mensagem={lista.error.message} /> : null}
      {lista.data && lista.data.length === 0 ? (
        <Vazio titulo="Nenhuma rodada" descricao="Crie o ano corrente para começar o fluxo anual." />
      ) : null}
      {lista.data && lista.data.length > 0 ? (
        <Card>
          <Tabela colunas={["Ano", "Estado", "Criada", "Ativada", "Sorteada", ""]}>
            {lista.data.map((r) => (
              <tr key={r.id}>
                <Td className="font-display text-lg">{r.ano}</Td>
                <Td>
                  <Badge tom={tomEstado(r.estado)}>{rotuloEstado(r.estado)}</Badge>
                </Td>
                <Td>{formatarData(r.criadaEm)}</Td>
                <Td>{formatarData(r.ativadaEm)}</Td>
                <Td>{formatarData(r.sorteadaEm)}</Td>
                <Td className="text-right">
                  <Link href={`/rodadas/${r.id}`} className="mr-2 text-sm font-semibold text-olive hover:underline">
                    Abrir
                  </Link>
                  <Botao variante="ghost" type="button" onClick={() => setExcluir(r)}>
                    Excluir
                  </Botao>
                </Td>
              </tr>
            ))}
          </Tabela>
        </Card>
      ) : null}

      <ConfirmDialog
        aberto={!!excluir}
        titulo={`Excluir rodada ${excluir?.ano}?`}
        descricao={
          excluir?.estado === "Sorteada"
            ? "O resultado vigente, inscrições e atribuições desta rodada serão apagados. Unidades e vagas do cadastro permanecem."
            : "Inscrições e atribuições desta rodada serão apagadas. Unidades e vagas do cadastro permanecem."
        }
        confirmar="Excluir"
        perigo
        ocupado={remover.isPending}
        onCancelar={() => setExcluir(null)}
        onConfirmar={() => remover.mutate()}
      />
    </>
  );
}
