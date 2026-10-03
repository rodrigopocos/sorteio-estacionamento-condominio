"use client";

import { Botao } from "./ui";

export function ConfirmDialog({
  aberto,
  titulo,
  descricao,
  confirmar = "Confirmar",
  perigo = false,
  ocupado = false,
  onConfirmar,
  onCancelar,
}: {
  aberto: boolean;
  titulo: string;
  descricao: string;
  confirmar?: string;
  perigo?: boolean;
  ocupado?: boolean;
  onConfirmar: () => void;
  onCancelar: () => void;
}) {
  if (!aberto) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-ink/45 p-4" role="dialog" aria-modal="true">
      <div className="w-full max-w-md rounded-2xl border border-line bg-cream p-5 shadow-xl">
        <h2 className="font-display text-xl font-semibold">{titulo}</h2>
        <p className="mt-2 text-sm text-muted">{descricao}</p>
        <div className="mt-5 flex justify-end gap-2">
          <Botao type="button" variante="secundario" onClick={onCancelar} disabled={ocupado}>
            Voltar
          </Botao>
          <Botao
            type="button"
            variante={perigo ? "perigo" : "primario"}
            onClick={onConfirmar}
            disabled={ocupado}
          >
            {ocupado ? "Aguarde…" : confirmar}
          </Botao>
        </div>
      </div>
    </div>
  );
}
