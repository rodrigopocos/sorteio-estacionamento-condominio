"use client";

import { useRouter } from "next/navigation";
import { useMutation } from "@tanstack/react-query";
import { FormEvent, useState } from "react";
import { api } from "@/lib/api";
import { Alerta, Botao, Campo, Input } from "@/components/ui";

export default function LoginPage() {
  const router = useRouter();
  const [usuario, setUsuario] = useState("admin");
  const [senha, setSenha] = useState("");
  const login = useMutation({
    mutationFn: () =>
      api("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ usuario, senha }),
      }),
    onSuccess: () => router.replace("/"),
  });

  function enviar(e: FormEvent) {
    e.preventDefault();
    login.mutate();
  }

  return (
    <div className="grid min-h-screen lg:grid-cols-[1.1fr_0.9fr]">
      <section className="relative hidden overflow-hidden bg-sidebar text-cream lg:flex lg:flex-col lg:justify-between lg:p-12">
        <div className="pointer-events-none absolute inset-0 opacity-40 [background:radial-gradient(circle_at_20%_20%,#c4843a33,transparent_35%),radial-gradient(circle_at_80%_80%,#3f7a5844,transparent_40%)]" />
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-copper">Painel do síndico</p>
          <h1 className="mt-4 max-w-md font-display text-5xl leading-[1.05] font-semibold">
            Rodada anual das vagas de estacionamento.
          </h1>
        </div>
        <ol className="relative max-w-sm space-y-4 text-sm text-cream/75">
          <li>1. Cadastre unidades e o inventário de vagas.</li>
          <li>2. Abra a rodada do ano e faça reservas manuais (PCD).</li>
          <li>3. Sorteie, revise o resultado e exporte CSV ou PDF.</li>
        </ol>
      </section>
      <section className="flex items-center justify-center p-6 sm:p-10">
        <form onSubmit={enviar} className="w-full max-w-md rounded-3xl border border-line bg-cream p-8 shadow-sm">
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-copper">Acesso</p>
          <h2 className="mt-2 font-display text-3xl">Entrar</h2>
          <p className="mt-2 text-sm text-muted">Um administrador. Sessão em cookie HTTP-only.</p>
          <div className="mt-6 space-y-4">
            <Campo label="Usuário">
              <Input value={usuario} onChange={(e) => setUsuario(e.target.value)} autoComplete="username" required />
            </Campo>
            <Campo label="Senha">
              <Input
                type="password"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                autoComplete="current-password"
                required
              />
            </Campo>
          </div>
          {login.isError ? (
            <div className="mt-4">
              <Alerta tom="erro">{login.error.message}</Alerta>
            </div>
          ) : null}
          <Botao type="submit" className="mt-6 w-full py-2.5" disabled={login.isPending}>
            {login.isPending ? "Entrando…" : "Entrar no painel"}
          </Botao>
        </form>
      </section>
    </div>
  );
}
