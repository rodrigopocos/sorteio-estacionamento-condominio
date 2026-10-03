# Sorteio de estacionamento — constituição

## Objetivo
MVP admin: cadastrar unidades, vagas, rodadas, inscrever participantes, sortear uma vez por rodada, exportar CSV/PDF.

## Stack
- Backend: .NET 8, DDD (Domain / Application / Infrastructure / Api), EF Core, SQLite em dev
- Frontend: Next.js (App Router), Tailwind CSS
- Testes: xUnit + FluentAssertions (Domain e Application)

## Regras para o agente
- Idioma da UI: pt-BR
- Domain não referencia EF, API ou Next
- Regra de sorteio só muda se `docs/spec/` for atualizado antes
- Implementar em slices; preferir um slice por conversa
- TDD obrigatório para sorteio e invariantes de rodada

## Fora do MVP
Login de morador, portal público, regras PCD configuráveis