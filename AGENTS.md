# Sorteio de estacionamento

## Objetivo (MVP em curso)

Painel **admin** (login único): cadastro de unidades e vagas, rodada anual, atribuições manuais (PCD/reservas), sorteio com regras de gaveta/cobertura, export CSV/PDF. Detalhes de produto: `docs/001-mvp/1-pre-spec.md`.

## Fora do MVP

Portal do morador, auto-inscrição, regras PCD configuráveis na UI, multi-admin, multi-condomínio.

## Stack

- Backend: .NET 8, DDD (Domain / Application / Infrastructure / Api), EF Core, SQLite em dev
- Frontend: Next.js (App Router), Tailwind CSS
- Testes: xUnit + FluentAssertions (Domain e Application)

## Regras para o agente

- Idioma da UI: pt-BR
- Domain não referencia EF, API ou Next
- Regra de negócio / sorteio: alterar `1-pre-spec.md` antes de `2-spec.md`; implementação segue `2-spec.md` aprovada
- Implementar em **slices** conforme `3-plan.md`; preferir um slice por conversa
- TDD obrigatório para sorteio, status de contemplação e invariantes de rodada
- Sorteio: **uma execução vigente por vez**; re-sorteio só após **cancelamento** (remove atribuições de sorteio, mantém manuais) — ver `2-spec.md` §7.5

## Documentação (SDD)

Cada funcionalidade vive em `docs/<nome-funcionalidade>/` (kebab-case; prefixo numérico quando houver ordem, ex. `001-mvp`).

**Funcionalidade atual:** `docs/001-mvp/`

| Arquivo | Dono | Conteúdo |
|---------|------|----------|
| `1-pre-spec.md` | Humano (PO) | Decisões, escopo IN/OUT, pendências — fonte da verdade de produto |
| `2-spec.md` | Humano aprova; agente pode rascunhar | Regras testáveis, EARS, falhas, stories, checklist |
| `3-plan.md` | Humano aprova; agente pode rascunhar | Plano técnico **após** spec aprovada — slices, API, telas |

**Fluxo:** `1-pre-spec.md` → `2-spec.md` (aprovada) → `3-plan.md` (se usar) → código em slices.

**Regras:**

- `2-spec.md` não contradiz `1-pre-spec.md`
- Mudou decisão de negócio → atualizar `1-pre-spec.md` primeiro, depois `2-spec.md`
- Não implementar escopo que não esteja na spec aprovada da pasta da funcionalidade
