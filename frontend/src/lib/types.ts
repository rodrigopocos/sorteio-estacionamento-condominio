export type TipoVaga = "Simples" | "Gaveta";
export type CoberturaVaga = "Coberta" | "Descoberta";
export type OrigemAtribuicao = "Manual" | "Sorteio";
export type EstadoRodada = "EmPreparacao" | "Ativa" | "Sorteada";
export type StatusContemplacao =
  | "Completa"
  | "Parcial"
  | "NaoContemplada"
  | "CompletaSemCobertura";

export type Unidade = { id: string; numero: string };

export type Vaga = {
  id: string;
  codigo: string;
  tipo: TipoVaga;
  cobertura: CoberturaVaga;
  ehPcd: boolean;
  lugaresFisicos: number;
};

export type Aviso = { codigo: string; mensagem: string };

export type ResultadoVaga = { vaga: Vaga; avisos: Aviso[] };

export type ResumoInventario = {
  totalUnidades: number;
  totalVagas: number;
  lugaresFisicos: number;
  vagasPcd: number;
};

export type Sessao = { usuario: string };

export type RodadaResumo = {
  id: string;
  ano: number;
  estado: EstadoRodada;
  criadaEm: string;
  ativadaEm: string | null;
  sorteadaEm: string | null;
};

export type Inscricao = { unidadeId: string; numero: string };

export type Atribuicao = {
  id: string;
  unidadeId: string;
  unidadeNumero: string;
  vagaId: string;
  vagaCodigo: string;
  tipo: TipoVaga;
  cobertura: CoberturaVaga;
  ehPcd: boolean;
  origem: OrigemAtribuicao;
};

export type Contemplacao = {
  unidadeId: string;
  numero: string;
  status: StatusContemplacao;
};

export type RodadaDetalhe = {
  id: string;
  ano: number;
  estado: EstadoRodada;
  criadaEm: string;
  ativadaEm: string | null;
  sorteadaEm: string | null;
  inscricoes: Inscricao[];
  atribuicoes: Atribuicao[];
  avisos: Aviso[];
  bloqueios: Aviso[];
  sequenciaVigente: number | null;
  contemplacoes: Contemplacao[];
};

export type ResultadoUnidadeSorteio = {
  unidadeId: string;
  status: StatusContemplacao;
  atribuicoesSorteio: Atribuicao[];
};

export type ResultadoSorteio = {
  rodadaId: string;
  sequencia: number;
  seedHex: string;
  executadoEm: string;
  estado: EstadoRodada;
  resultadosPorUnidade: ResultadoUnidadeSorteio[];
  avisos: Aviso[];
};
