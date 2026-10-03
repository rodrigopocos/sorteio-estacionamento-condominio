"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useEffect, type ReactNode } from "react";
import { api } from "@/lib/api";
import { chaves } from "@/lib/labels";
import type { Sessao } from "@/lib/types";
import { Botao } from "./ui";

const NAV = [
  { href: "/", label: "Painel", dica: "Resumo do inventário" },
  { href: "/unidades", label: "Unidades", dica: "Apartamentos" },
  { href: "/vagas", label: "Vagas", dica: "Inventário físico" },
  { href: "/rodadas", label: "Rodadas", dica: "Ciclo anual" },
];

function ativo(pathname: string, href: string) {
  if (href === "/") return pathname === "/";
  return pathname === href || pathname.startsWith(`${href}/`);
}

export function AdminShell({ children }: { children: ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const me = useQuery({
    queryKey: chaves.me,
    queryFn: () => api<Sessao>("/api/auth/me"),
  });
  const sair = useMutation({
    mutationFn: () => api("/api/auth/logout", { method: "POST" }),
    onSuccess: () => router.replace("/login"),
  });

  useEffect(() => {
    if (me.isError) router.replace("/login");
  }, [me.isError, router]);

  if (me.isLoading || me.isError) {
    return (
      <div className="grid min-h-screen place-items-center bg-paper text-muted">
        Abrindo o painel…
      </div>
    );
  }

  return (
    <div className="min-h-screen lg:grid lg:grid-cols-[17rem_1fr]">
      <aside className="bg-sidebar text-cream">
        <div className="flex h-full flex-col px-5 py-6">
          <Link href="/" className="mb-8 block">
            <p className="text-[11px] font-semibold uppercase tracking-[0.22em] text-copper">Condomínio</p>
            <p className="font-display text-2xl leading-tight">Sorteio de vagas</p>
          </Link>
          <nav className="flex flex-1 flex-col gap-1">
            {NAV.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className={`rounded-xl px-3 py-2.5 transition ${
                  ativo(pathname, item.href)
                    ? "bg-cream/10 text-cream"
                    : "text-cream/65 hover:bg-cream/5 hover:text-cream"
                }`}
              >
                <span className="block text-sm font-semibold">{item.label}</span>
                <span className="block text-xs text-cream/45">{item.dica}</span>
              </Link>
            ))}
          </nav>
          <div className="mt-6 border-t border-cream/10 pt-4">
            <p className="text-xs uppercase tracking-wider text-cream/40">Sessão</p>
            <p className="mt-1 font-medium">{me.data?.usuario}</p>
            <Botao
              className="mt-3 w-full border border-cream/15 bg-transparent text-cream hover:bg-cream/10"
              variante="secundario"
              type="button"
              onClick={() => sair.mutate()}
              disabled={sair.isPending}
            >
              Sair
            </Botao>
          </div>
        </div>
      </aside>
      <div className="min-w-0">
        <main className="mx-auto max-w-6xl px-4 py-8 sm:px-8">{children}</main>
      </div>
    </div>
  );
}
