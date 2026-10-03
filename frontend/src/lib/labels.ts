import type {
  CoberturaVaga,
  EstadoRodada,
  OrigemAtribuicao,
  StatusContemplacao,
  TipoVaga,
} from "./types";

export const chaves = {
  me: ["me"] as const,
  inventario: ["inventario"] as const,
  unidades: ["unidades"] as const,
  vagas: ["vagas"] as const,
  rodadas: ["rodadas"] as const,
  rodada: (id: string) => ["rodadas", id] as const,
};

export function rotuloEstado(estado: EstadoRodada) {
  switch (estado) {
    case "EmPreparacao":
      return "Em preparação";
    case "Ativa":
      return "Ativa";
    case "Sorteada":
      return "Sorteada";
  }
}

export function rotuloStatus(status: StatusContemplacao) {
  switch (status) {
    case "Completa":
      return "Completa";
    case "Parcial":
      return "Parcial";
    case "NaoContemplada":
      return "Não contemplada";
    case "CompletaSemCobertura":
      return "Completa sem cobertura";
  }
}

export function rotuloTipo(tipo: TipoVaga) {
  return tipo === "Gaveta" ? "Gaveta" : "Simples";
}

export function rotuloCobertura(cobertura: CoberturaVaga) {
  return cobertura === "Coberta" ? "Coberta" : "Descoberta";
}

export function rotuloOrigem(origem: OrigemAtribuicao) {
  return origem === "Manual" ? "Manual" : "Sorteio";
}

export function formatarData(iso: string | null | undefined) {
  if (!iso) return "—";
  return new Date(iso).toLocaleString("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  });
}

export function lugaresDe(tipo: TipoVaga) {
  return tipo === "Gaveta" ? 2 : 1;
}
