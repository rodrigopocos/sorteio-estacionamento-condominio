export const API_URL =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080";

export class ApiError extends Error {
  constructor(
    public status: number,
    public codigo?: string,
    mensagem?: string,
  ) {
    super(mensagem ?? "Não foi possível concluir a operação.");
    this.name = "ApiError";
  }
}

type Falha = { codigo?: string; mensagem?: string };

async function lerFalha(res: Response): Promise<Falha> {
  try {
    return (await res.json()) as Falha;
  } catch {
    return {};
  }
}

function redirecionarLoginSe401(path: string, status: number) {
  if (status !== 401) return;
  if (typeof window === "undefined") return;
  if (path === "/api/auth/login" || path === "/api/auth/logout") return;
  if (window.location.pathname === "/login") return;
  window.location.assign("/login");
}

export async function api<T>(path: string, init?: RequestInit): Promise<T> {
  let res: Response;
  try {
    res = await fetch(`${API_URL}${path}`, {
      ...init,
      credentials: "include",
      headers: {
        ...(init?.body ? { "Content-Type": "application/json" } : {}),
        ...init?.headers,
      },
    });
  } catch {
    throw new ApiError(
      0,
      "API_INDISPONIVEL",
      "Não foi possível conectar à API. Confira se ela está em execução em http://localhost:5080.",
    );
  }

  if (res.status === 204) return undefined as T;

  if (!res.ok) {
    redirecionarLoginSe401(path, res.status);
    const falha = await lerFalha(res);
    throw new ApiError(res.status, falha.codigo, falha.mensagem);
  }

  const tipo = res.headers.get("content-type") ?? "";
  if (!tipo.includes("application/json")) return undefined as T;
  return (await res.json()) as T;
}

export async function baixarArquivo(path: string, nomePadrao: string) {
  let res: Response;
  try {
    res = await fetch(`${API_URL}${path}`, { credentials: "include" });
  } catch {
    throw new ApiError(
      0,
      "API_INDISPONIVEL",
      "Não foi possível conectar à API. Confira se ela está em execução em http://localhost:5080.",
    );
  }

  if (!res.ok) {
    redirecionarLoginSe401(path, res.status);
    const falha = await lerFalha(res);
    throw new ApiError(res.status, falha.codigo, falha.mensagem);
  }

  const blob = await res.blob();
  const disp = res.headers.get("content-disposition") ?? "";
  const match = /filename\*?=(?:UTF-8''|"?)([^";]+)/i.exec(disp);
  const nome = match ? decodeURIComponent(match[1].replaceAll('"', "")) : nomePadrao;
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = nome;
  document.body.append(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}
