"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { chaves, rotuloEstado } from "@/lib/labels";
import type { ResumoInventario, RodadaResumo } from "@/lib/types";
import { Alerta, Badge, Card, Carregando, ErroPagina, PageHeader } from "@/components/ui";

const PASSOS = [
  { n: "01", titulo: "Unidades", href: "/unidades", texto: "Cadastre os apartamentos da rodada." },
  { n: "02", titulo: "Vagas", href: "/vagas", texto: "Simples ou gaveta, coberta ou descoberta." },
  { n: "03", titulo: "Rodada", href: "/rodadas", texto: "Crie o ano, ative, reserve PCD e sorteie." },
];

export default function PainelPage() {
  const inventario = useQuery({
    queryKey: chaves.inventario,
    queryFn: () => api<ResumoInventario>("/api/inventario/resumo"),
  });
  const rodadas = useQuery({
    queryKey: chaves.rodadas,
    queryFn: () => api<RodadaResumo[]>("/api/rodadas"),
  });

  if (inventario.isLoading) return <Carregando />;
  if (inventario.isError) return <ErroPagina mensagem={inventario.error.message} />;

  const r = inventario.data!;
  const necessarios = r.totalUnidades * 2;
  const capacidadeOk = r.totalUnidades === 0 || r.lugaresFisicos >= necessarios;
  const vigente = rodadas.data?.[0];

  return (
    <>
      <PageHeader
        kicker="Início"
        titulo="Painel da rodada"
        descricao="Cadastro, atribuições manuais, sorteio e exportação — sem planilha paralela."
      />

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <Stat rotulo="Unidades" valor={r.totalUnidades} detalhe="apartamentos" />
        <Stat rotulo="Vagas" valor={r.totalVagas} detalhe="registros no cadastro" />
        <Stat rotulo="Lugares físicos" valor={r.lugaresFisicos} detalhe={`meta de referência: 72 · necessários agora: ${necessarios}`} />
        <Stat rotulo="Vagas PCD" valor={r.vagasPcd} detalhe="referência do condomínio: 2" />
      </div>

      <div className="mt-4 space-y-3">
        {!capacidadeOk ? (
          <Alerta tom="aviso" titulo="Capacidade abaixo do direito da rodada">
            Há {r.lugaresFisicos} lugares para {r.totalUnidades} unidades (2 por unidade). O sorteio será bloqueado até completar o inventário.
          </Alerta>
        ) : r.totalUnidades > 0 ? (
          <Alerta tom="ok" titulo="Inventário suficiente para contemplar todas as unidades inscritas." />
        ) : (
          <Alerta tom="info" titulo="Comece cadastrando unidades e vagas para abrir a primeira rodada." />
        )}
      </div>

      <div className="mt-6 grid gap-4 lg:grid-cols-3">
        {PASSOS.map((passo) => (
          <Link key={passo.href} href={passo.href}>
            <Card className="h-full transition hover:border-copper/50">
              <p className="font-display text-2xl text-copper">{passo.n}</p>
              <h2 className="mt-2 font-display text-xl">{passo.titulo}</h2>
              <p className="mt-1 text-sm text-muted">{passo.texto}</p>
            </Card>
          </Link>
        ))}
      </div>

      {vigente ? (
        <Card className="mt-6 flex flex-wrap items-center justify-between gap-3">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wider text-muted">Última rodada cadastrada</p>
            <p className="mt-1 font-display text-2xl">{vigente.ano}</p>
          </div>
          <Badge tom={vigente.estado === "Sorteada" ? "ok" : vigente.estado === "Ativa" ? "olive" : "copper"}>
            {rotuloEstado(vigente.estado)}
          </Badge>
          <Link href={`/rodadas/${vigente.id}`} className="text-sm font-semibold text-olive hover:underline">
            Abrir rodada →
          </Link>
        </Card>
      ) : null}
    </>
  );
}

function Stat({ rotulo, valor, detalhe }: { rotulo: string; valor: number; detalhe: string }) {
  return (
    <Card>
      <p className="text-xs font-semibold uppercase tracking-wider text-muted">{rotulo}</p>
      <p className="mt-2 font-display text-4xl">{valor}</p>
      <p className="mt-1 text-xs text-muted">{detalhe}</p>
    </Card>
  );
}
