# ai-process-log Specification

## Purpose

Defines requirements for `AI-LOG.md`, the single Markdown artifact at the repository
root that consolidates evidence-backed records of the 8 SDD iterations plus the pre-SDD
initial state. Content language: Spanish. Structure: progressive disclosure (summary
table first, per-iteration detail sections second).

## Requirements

### Requirement: File Existence and Location

`AI-LOG.md` MUST exist at the repository root on branch `feat/evidencia-proceso-ia`
and MUST be merged to `main` via PR. No existing file MUST be modified as part of
this change.

#### Scenario: File is present after apply

- GIVEN the `feat/evidencia-proceso-ia` branch is checked out
- WHEN the working tree is listed
- THEN `AI-LOG.md` exists at the repository root

#### Scenario: No existing file is modified

- GIVEN the branch diff against `main`
- WHEN `git diff --name-only main..HEAD` is executed
- THEN only `AI-LOG.md` appears as added; no other file shows as modified or deleted

---

### Requirement: Pre-SDD Initial State Section

`AI-LOG.md` MUST contain a dedicated section for commit `43b7af6` (2026-09-09)
labeled **"Estado inicial del repositorio — pre-SDD"**. This section MUST NOT be
numbered as an AI iteration and MUST NOT include the 10-field iteration record
model; it documents only the baseline state before SDD began.

#### Scenario: Pre-SDD section is not labeled as an iteration

- GIVEN `AI-LOG.md` is read in full
- WHEN the pre-SDD section is located
- THEN it carries neither an iteration number nor the 10-field record structure
- AND it references commit `43b7af6` as its evidence anchor

---

### Requirement: Summary Table (Progressive Disclosure)

`AI-LOG.md` MUST open with a summary table listing all 8 SDD iterations before any
per-iteration detail section. The table MUST include at minimum: iteration number,
change name, date, and archive/status column. This satisfies the cognitive-doc-design
progressive-disclosure pattern: answer first, detail second.

#### Scenario: Summary table precedes all detail sections

- GIVEN `AI-LOG.md` is parsed
- WHEN heading positions are extracted
- THEN the summary table appears before the first per-iteration `##` heading
- AND the table contains exactly 8 rows (one per SDD iteration)

---

### Requirement: Per-Iteration Record Model (10 Fields)

Each of the 8 SDD iteration sections MUST document the following fields using only
verified evidence. Fields without backing evidence MUST be omitted or labeled as
gaps — never invented.

| # | Field | Evidence Source |
|---|-------|----------------|
| 1 | Especificación inicial | `proposal.md` / `spec.md` under change folder |
| 2 | Prompt utilizado | Raw text if present; otherwise labeled per the Prompt-Handling Rule |
| 3 | Objetivo del prompt | `## Intent` section of `proposal.md` |
| 4 | Resultado / propuesta de la IA | `proposal.md` content |
| 5 | Implementación | Task list + `apply-progress.md` artifacts |
| 6 | Validaciones | `verify-report.md` + CI status |
| 7 | Problemas identificados | `design.md` notes / `verify-report.md` |
| 8 | Ajustes realizados | Commit messages + design decisions |
| 9 | Decisiones técnicas del desarrollador | "Key Decisions" / "Confirmable Decisions" tables |
| 10 | Commit(s) relacionados | Real commit hash from exploration handoff |

#### Scenario: All 10 fields present in a fully-evidenced iteration

- GIVEN an iteration with all artifacts available (e.g. `VehicleCatalog-API-Persistence`)
- WHEN the corresponding section of `AI-LOG.md` is reviewed
- THEN all 10 fields appear with an evidence citation (artifact path or commit hash)

#### Scenario: Missing-evidence field is labeled, not invented

- GIVEN an iteration where a field has no backing artifact
- WHEN that field is authored
- THEN it carries an explicit gap label (e.g. `sin spec.md`, `no disponible`)
- AND no content is fabricated to fill the gap

---

### Requirement: Prompt-Handling Rule

Because no raw prompt text exists in the repository or Engram for any iteration,
the "Prompt utilizado" field MUST be labeled:
`"Objetivo derivado del Intent del proposal (texto verbatim no disponible)"`.
The source MUST be cited as the `## Intent` section of the corresponding
`proposal.md`. The single confirmed verbatim instruction from `DocumentacionFinal`
("Se debe primero actualizar main y luego realizar la documentación.") MUST be
quoted exactly and labeled `[verbatim]`.

#### Scenario: Derived-intent label is applied to all non-verbatim prompts

- GIVEN any iteration other than `DocumentacionFinal`
- WHEN the "Prompt utilizado" field is read
- THEN it contains the exact label defined in this requirement
- AND no fabricated prompt text is present

#### Scenario: Verbatim instruction is quoted for DocumentacionFinal

- GIVEN the `DocumentacionFinal` iteration section
- WHEN the "Prompt utilizado" field is read
- THEN the verbatim instruction is enclosed in quotes and tagged `[verbatim]`

---

### Requirement: AI Suggestions vs. Developer Decisions Distinction

Each iteration record MUST explicitly distinguish AI-generated proposals from
developer decisions. Developer decisions MUST be cited from "Key Decisions" or
"Confirmable Decisions" tables in the relevant `proposal.md`.

#### Scenario: Developer decisions are labeled as such

- GIVEN any iteration section
- WHEN the "Decisiones técnicas del desarrollador" field is read
- THEN decisions are explicitly labeled as developer choices
- AND each decision cites its source (e.g. `proposal.md § Key Decisions`)

---

### Requirement: Honest Gap Labels

The following known gaps MUST be documented with the exact status labels shown:

| Iteration | Gap | Required Label |
|-----------|-----|----------------|
| `mejora-interfaz-maui` | No `spec.md` in change folder | `sin spec.md` |
| `TestProyect` | Non-standard path `openspec/TestProyect/`; no `explore.md` | `ruta no estándar; sin explore.md` |
| `DocumentacionFinal` | Active change, not yet archived | `cambio activo (sin archivar)` |

#### Scenario: Gap labels are present and match required text

- GIVEN `AI-LOG.md` is read
- WHEN the three gap iterations are located
- THEN each carries its required label exactly as specified in the table above

---

### Requirement: File Size Budget

`AI-LOG.md` MUST NOT exceed 400 lines. Structure MUST remain within budget through
disciplined use of tables over prose and by omitting fields that have no evidence.

#### Scenario: File stays within budget

- GIVEN `AI-LOG.md` is written and committed
- WHEN `(Get-Content AI-LOG.md).Count` is executed
- THEN the result is less than or equal to 400