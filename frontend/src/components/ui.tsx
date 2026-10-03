import { type ButtonHTMLAttributes, type InputHTMLAttributes, type ReactNode, type SelectHTMLAttributes } from "react";

export function cn(...classes: Array<string | false | null | undefined>) {
  return classes.filter(Boolean).join(" ");
}

type BotaoVariante = "primario" | "secundario" | "ghost" | "perigo";

export function Botao({
  variante = "primario",
  className,
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variante?: BotaoVariante }) {
  const base =
    "inline-flex items-center justify-center gap-2 rounded-lg px-4 py-2 text-sm font-semibold tracking-wide transition disabled:cursor-not-allowed disabled:opacity-50";
  const mapa: Record<BotaoVariante, string> = {
    primario: "bg-olive text-cream shadow-sm hover:bg-moss",
    secundario: "border border-line bg-cream text-ink hover:bg-paper",
    ghost: "text-muted hover:bg-paper hover:text-ink",
    perigo: "bg-danger text-cream hover:bg-[#7f1d1d]",
  };
  return <button className={cn(base, mapa[variante], className)} {...props} />;
}

export function Campo({
  label,
  children,
  className,
}: {
  label: string;
  children: ReactNode;
  className?: string;
}) {
  return (
    <label className={cn("flex min-w-0 flex-col gap-1.5 text-sm", className)}>
      <span className="font-medium text-muted">{label}</span>
      {children}
    </label>
  );
}

const campoBase =
  "w-full rounded-lg border border-line bg-cream px-3 py-2 text-ink outline-none ring-copper/40 transition placeholder:text-muted/60 focus:border-copper focus:ring-2";

export function Input(props: InputHTMLAttributes<HTMLInputElement>) {
  return <input className={cn(campoBase, props.className)} {...props} />;
}

export function Select(props: SelectHTMLAttributes<HTMLSelectElement>) {
  return <select className={cn(campoBase, props.className)} {...props} />;
}

export function Card({
  children,
  className,
}: {
  children: ReactNode;
  className?: string;
}) {
  return (
    <section className={cn("rounded-2xl border border-line bg-cream p-5 shadow-[0_1px_0_rgba(22,33,28,0.04)]", className)}>
      {children}
    </section>
  );
}

export function PageHeader({
  kicker,
  titulo,
  descricao,
  acoes,
}: {
  kicker?: string;
  titulo: string;
  descricao?: string;
  acoes?: ReactNode;
}) {
  return (
    <div className="mb-6 flex flex-wrap items-end justify-between gap-4">
      <div>
        {kicker ? (
          <p className="mb-1 text-xs font-semibold uppercase tracking-[0.18em] text-copper">{kicker}</p>
        ) : null}
        <h1 className="font-display text-3xl font-semibold tracking-tight text-ink">{titulo}</h1>
        {descricao ? <p className="mt-1 max-w-2xl text-muted">{descricao}</p> : null}
      </div>
      {acoes ? <div className="flex flex-wrap gap-2">{acoes}</div> : null}
    </div>
  );
}

export function Alerta({
  tom = "info",
  titulo,
  itens,
  children,
}: {
  tom?: "info" | "aviso" | "erro" | "ok";
  titulo?: string;
  itens?: string[];
  children?: ReactNode;
}) {
  const mapa = {
    info: "border-olive/25 bg-olive/8 text-ink",
    aviso: "border-copper/40 bg-[#f6e7cc] text-warn",
    erro: "border-danger/30 bg-[#f8e4e0] text-danger",
    ok: "border-olive/30 bg-[#e6f0e8] text-olive",
  };
  return (
    <div className={cn("rounded-xl border px-4 py-3 text-sm", mapa[tom])} role="status">
      {titulo ? <p className="font-semibold">{titulo}</p> : null}
      {children}
      {itens && itens.length > 0 ? (
        <ul className="mt-1 list-disc space-y-0.5 pl-5">
          {itens.map((item) => (
            <li key={item}>{item}</li>
          ))}
        </ul>
      ) : null}
    </div>
  );
}

export function Badge({
  children,
  tom = "neutro",
}: {
  children: ReactNode;
  tom?: "neutro" | "olive" | "copper" | "ok" | "aviso" | "erro";
}) {
  const mapa = {
    neutro: "bg-paper text-muted",
    olive: "bg-olive/12 text-olive",
    copper: "bg-copper/15 text-[#7a4e14]",
    ok: "bg-[#dcefe1] text-olive",
    aviso: "bg-[#f6e7cc] text-warn",
    erro: "bg-[#f8e4e0] text-danger",
  };
  return (
    <span className={cn("inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold", mapa[tom])}>
      {children}
    </span>
  );
}

export function Vazio({ titulo, descricao }: { titulo: string; descricao: string }) {
  return (
    <div className="rounded-xl border border-dashed border-line px-4 py-10 text-center">
      <p className="font-semibold text-ink">{titulo}</p>
      <p className="mt-1 text-sm text-muted">{descricao}</p>
    </div>
  );
}

export function Tabela({
  colunas,
  children,
}: {
  colunas: string[];
  children: ReactNode;
}) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[36rem] border-separate border-spacing-0 text-sm">
        <thead>
          <tr>
            {colunas.map((coluna) => (
              <th
                key={coluna}
                className="border-b border-line px-3 py-2 text-left text-xs font-semibold uppercase tracking-wider text-muted"
              >
                {coluna}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>{children}</tbody>
      </table>
    </div>
  );
}

export function Td({ children, className }: { children: ReactNode; className?: string }) {
  return <td className={cn("border-b border-line/70 px-3 py-2.5 align-middle", className)}>{children}</td>;
}

export function ErroPagina({ mensagem }: { mensagem: string }) {
  return <Alerta tom="erro" titulo="Não foi possível carregar">{mensagem}</Alerta>;
}

export function Carregando() {
  return <p className="text-sm text-muted">Carregando…</p>;
}
