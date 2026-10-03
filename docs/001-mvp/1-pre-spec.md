# Pre-spec — MVP sorteio de vagas de estacionamento

**Status:** rascunho para revisão do PO  
**Versão:** 0.1  
**Data:** 2026-03-10  

> Fonte da verdade de **produto** para o MVP. Próximo passo SDD: aprovar esta pre-spec → redigir `spec.md` (critérios EARS, falhas, checklist).  
> **Nota de organização:** este arquivo está em `docs/01-mvp-pre-spec.md`. A convenção do repositório permite migrar para `docs/001-mvp/pre-spec.md` quando a funcionalidade for formalizada na pasta SDD.

---

## 1. Visão e problema

Condomínio com **36 apartamentos** e **72 vagas** de estacionamento. A convenção do condomínio prevê **sorteio anual** para definir quais vagas cada unidade utiliza no período, respeitando regras como **mínimo de uma vaga coberta** por apartamento, existência de **vagas gaveta** e **vagas PCD** atribuídas manualmente.

Hoje o processo tende a ser manual (planilha), com risco de erro, duplicidade de vaga e dificuldade de explicar o resultado aos moradores.

**Solução MVP:** painel **administrativo** operado pelo **síndico/administradora** para cadastrar unidades e vagas, conduzir a **rodada anual**, executar o **sorteio** com as regras acordadas e **exportar** o resultado (CSV/PDF).

---

## 2. Objetivos do MVP

1. Cadastrar **unidades** (apartamentos) e **vagas** (simples e gaveta; coberta/descoberta).
2. Gerenciar **rodada anual** de sorteio (ciclo principal do produto).
3. Incluir **as 36 unidades** na rodada (inscrição automática/obrigatória no fluxo anual).
4. Permitir **atribuição manual** pré-sorteado (PCD e reservas acordadas).
5. Executar **sorteio** respeitando tipos de vaga, gaveta e regra de cobertura.
6. **Cancelar e sortear de novo** uma rodada, quando necessário.
7. **Exportar** lista simples de resultado (CSV/PDF).
8. Proteger o painel com **login simples** (usuário administrador único).

---

## 3. Personas

| Persona | Papel no MVP |
|--------|----------------|
| **Síndico / administradora** | Usuário principal: cadastro, rodada, atribuições manuais, sorteio, exportação. |
| **Morador** | Sem acesso ao sistema no MVP (não há portal nem auto-inscrição). |

---

## 4. Glossário

| Termo | Definição |
|-------|-----------|
| **Unidade** | Apartamento do condomínio, identificado pelo **número do apartamento** (ex.: 101). |
| **Vaga** | Lugar de estacionamento com identificador único no cadastro. |
| **Vaga simples** | Vaga que ocupa **1** lugar físico no total de 72. |
| **Vaga gaveta** | **1 registro** no sistema (ex.: "Gaveta 15") que representa **2 lugares físicos** no total de 72. Sorteada uma vez, **encerra** a contemplação das 2 vagas da unidade. |
| **Coberta / descoberta** | Atributo de cada vaga (simples ou gaveta). |
| **Rodada** | Ciclo de sorteio (no MVP, foco em **rodada anual**). |
| **Inscrição** | Participação da unidade na rodada (no anual: **sempre as 36 unidades**). |
| **Pool elegível** | Conjunto de vagas disponíveis para o sorteio na rodada (exclui vagas já atribuídas manualmente e vagas já consumidas). |
| **Atribuição manual** | Vínculo unidade↔vaga feito pelo admin **antes** do sorteio (PCD, reservas). |
| **Contemplação** | Unidade com direito às **2 vagas** cumprido na rodada (via gaveta, 2 simples ou manual). |
| **Regra do mínimo 1 coberta** | Cada apartamento contemplado deve ter **pelo menos 1 vaga coberta** entre suas 2 vagas, exceto casos acordados via **atribuição manual PCD** pré-sorteado. |

---

## 5. Contexto do condomínio (referência)

| Dimensão | Valor |
|----------|--------|
| Unidades | 36 apartamentos |
| Vagas físicas | 72 lugares |
| Direito por unidade | 2 vagas por rodada |
| Vagas PCD (hoje) | 2 vagas; unidades PCD definidas **manualmente** antes do sorteio |
| Periodicidade | Rodada **anual** |

---

## 6. Escopo IN (MVP)

### 6.1 Cadastro

- **Unidades:** número do apartamento (único).
- **Vagas:** identificador único; **tipo** `simples` ou `gaveta`; **cobertura** `coberta` ou `descoberta`.
- **Gaveta:** um cadastro = dois lugares físicos contabilizados no total de 72.
- Marcação de vagas **PCD** (as 2 existentes) e demais reservas que exijam atribuição manual.

### 6.2 Autenticação

- **Login simples:** um usuário administrador (credenciais configuráveis no ambiente).
- Morador **fora** do escopo.

### 6.3 Rodada anual

- Criar e conduzir rodada com estados de negócio em alto nível, por exemplo:
  - preparação / inscrições (36 unidades incluídas automaticamente no fluxo anual);
  - atribuições manuais permitidas;
  - sorteio executado;
  - resultado exportável.
- **Cancelar** rodada sorteada e **executar novo sorteio** (substitui resultado anterior; registro mínimo de re-sorteio — detalhe na spec).
- **Excluir** rodada (qualquer estado): remove a rodada e dados vinculados; unidades e vagas do cadastro permanecem. O ano fica livre para criar de novo.

### 6.4 Atribuição manual (pré-sorteado)

- Admin vincula vaga(s) a unidade **antes** do sorteio.
- Vagas atribuídas manualmente **não entram** no pool do sorteio.
- **PCD:** unidades com direito às vagas PCD são definidas manualmente; as 2 vagas PCD saem do pool.
- Gaveta atribuída manualmente **conta como 2 vagas** cumpridas para aquela unidade.
- Unidade já atendida manualmente (ex.: PCD com par completo) **não concorre** ao sorteio do restante — apenas completa o que faltar via pool, se ainda houver direito pendente (detalhe na spec).

### 6.5 Sorteio

**Formas válidas de contemplação por unidade** (via sorteio do pool):

| Forma | Descrição |
|-------|-----------|
| **A — 1 gaveta** | Uma vaga gaveta sorteada encerra a unidade (2 vagas satisfeitas). |
| **B — 2 simples** | Duas vagas simples **distintas** encerram a unidade. |

**Não permitido:** misturar gaveta + simples na mesma unidade na mesma rodada.

**Regra do mínimo 1 coberta** (sorteio):

- Par final deve incluir **≥ 1 vaga coberta**.
- **Gaveta coberta:** válida como contemplação completa.
- **Gaveta descoberta** sozinha: **não** cumpre a regra; não pode ser sorteada como forma A válida.
- **Duas simples:** válidas se **pelo menos uma** for coberta.

**Aleatoriedade:** sorteio **uniforme** entre opções **elegíveis**, respeitando restrições acima (não é “puro” sem regras).

**Inviabilidade / recursos insuficientes:** se não for possível contemplar todas as unidades plenamente (ex.: falta de cobertas no pool após manuais), o sistema **permite sorteio parcial** e deve deixar **explícito** quais unidades ficaram **incompletas** ou fora da regra.

**Integridade:** mesma vaga não pode ser atribuída a duas unidades na mesma rodada; gaveta consome 2 lugares físicos no inventário.

### 6.6 Exportação

- **CSV e PDF** com **lista simples** do resultado: unidade ↔ vaga(s) contempladas.
- **Sem** exigência de ata completa (seed, lista de participantes, etc.) no export do MVP.

### 6.7 Auditoria

- **Mínima:** foco no resultado utilizável pelo síndico/assembleia.
- Re-sorteio após cancelamento: manter evidência interna suficiente para saber que houve substituição (detalhe na spec).

---

## 7. Escopo OUT (não MVP)

- Login ou portal do **morador**.
- Auto-inscrição pelo morador.
- Regras **PCD configuráveis** no produto (no MVP: 2 vagas + manual fixo).
- Prioridade por histórico (“quem não ganhou na rodada anterior”).
- Ata de sorteio com seed/lista completa no export.
- Múltiplos perfis de admin / multi-condomínio.
- Configuração flexível de regras de sorteio pela UI (regras fixas conforme esta pre-spec).

---

## 8. Fluxo principal (síndico)

```mermaid
flowchart LR
  cadastro[CadastroUnidadesVagas]
  rodada[CriarRodadaAnual]
  insc[Incluir36Unidades]
  manual[AtribuirPCD_e_Reservas]
  sorteio[ExecutarSorteio]
  export[ExportarCSV_PDF]
  cadastro --> rodada --> insc --> manual --> sorteio --> export
```

---

## 9. Requisitos não funcionais (leves)

- Interface em **português (pt-BR)**.
- Uso principal em **desktop** pelo síndico.
- Sorteio não deve aceitar como sucesso um resultado que viole regras explícitas (ex.: par sem coberta quando deveria ser completo); em cenário parcial, **sinalizar** incompletude.

---

## 10. Critérios de sucesso (PO)

1. Síndico conduz uma **rodada anual completa** (cadastro → manual PCD → sorteio → export) sem planilha paralela.
2. **Nenhuma vaga** aparece duplicada no resultado da mesma rodada.
3. Quando viável, **100% das unidades** com contemplação válida e **≥ 1 coberta** (meta de negócio).
4. Em **sorteio parcial**, fica claro quais unidades não foram plenamente atendidas.
5. Export CSV/PDF é aceito como **lista oficial simples** para comunicação/assembleia.

---

## 11. Pendências para `spec.md`

Itens deliberadamente deixados para especificação técnica e testes (EARS):

1. **Algoritmo de sorteio** com tipos mistos (gaveta vs 2 simples), ordem de processamento das unidades e aleatoriedade reprodutível internamente.
2. Tratamento de **gaveta descoberta** no pool (exclusão automática da forma A).
3. Unidade com **atribuição manual parcial** (1 simples manual + sorteio do restante?) — definir invariantes.
4. **Validações pré-sorteado** (opcional): avisos quando cobertas ou capacidade do pool parecerem insuficientes.
5. Comportamento detalhado de **contemplação parcial** (0, 1 ou 2 vagas; status por unidade).
6. Formato exato de **CSV/PDF** (colunas; gaveta em uma linha ou duas).
7. **Login admin:** sessão, troca de senha, armazenamento de credencial.
8. Persistência ao **cancelar/refazer** rodada (histórico mínimo).
9. Campos opcionais futuros (bloco, placa, nome morador) — confirmar ausência no MVP.

---

## 12. Rastreabilidade (entrevista PO)

| Decisão | Origem |
|---------|--------|
| Operador síndico/admin | Entrevista rodada 1 |
| 36 aptos / 72 vagas | Entrevista rodada 1 |
| 2 vagas por unidade; anual | Entrevista rodada 1–2 |
| Inscrição admin; 36 sempre na rodada anual | Entrevista rodada 2–3 |
| Aleatório uniforme com restrições | Entrevista rodada 1, refinado com gaveta/coberta |
| PCD manual pré-sorteado (2 vagas) | Entrevista rodada 2–3 |
| Cancelar e sortear de novo | Entrevista rodada 2 |
| Export lista simples; auditoria mínima | Entrevista rodada 1 |
| Login admin único | Entrevista rodada 1 |
| Gaveta = 1 sorteio → 2 vagas | Entrevista iterativa |
| Forma A gaveta OU forma B 2 simples | Entrevista iterativa |
| Coberta/descoberta; mínimo 1 coberta | Entrevista iterativa |
| Gaveta com atributo coberta/descoberta | Entrevista iterativa |
| Gaveta = 1 registro, 2 lugares físicos | Entrevista rodada 3 |
| Sorteio parcial se inviável | Entrevista rodada 3 |

---

## 13. Aprovação

| Papel | Nome | Data | Aprovado? |
|-------|------|------|-----------|
| PO / Síndico | | | ☐ |

Após aprovação, iniciar `spec.md` na pasta SDD acordada (`docs/001-mvp/spec.md` recomendado).
