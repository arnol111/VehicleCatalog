# Design: EvidenciaProcesoIA

## Technical Approach

Author `AI-LOG.md` at the repository root by reading all 8 change artifact folders plus
supplemental evidence sources, then composing a progressive-disclosure Markdown document
(summary table → pre-SDD section → 8 per-iteration detail sections). Every field is
backed by a cited artifact path or real commit hash; no content is invented.

Threat matrix: N/A — no routing, shell, subprocess, VCS/PR automation,
executable-file classification, or process-integration boundary.

---

## Architecture Decisions

| Option | Tradeoff | Decision |
|--------|----------|----------|
| Prose paragraphs per iteration | Readable but consumes 80–100 lines/iteration → budget breach | ❌ Rejected |
| 10-field table per iteration (field \| content \| evidence) | Compact: ~35–40 lines/iteration; verifiable; fits budget | ✅ Chosen |
| One monolithic table for all iterations | Hard to navigate; obscures per-iteration detail | ❌ Rejected |
| Summary table + per-iteration subsections | Progressive disclosure: scan first, read detail on demand | ✅ Chosen |

---

## Evidence Access Plan

Read sources in this order to maximize field coverage before authoring each section:

| Order | Source | Path | Provides |
|-------|--------|------|----------|
| 1 | SDD config | `openspec/config.yaml` | Phase file conventions, stack context |
| 2 | Canonical specs | `openspec/specs/` (9 delta specs) | Baseline capability definitions |
| 3 | Archive: Persistence | `openspec/changes/archive/2026-09-09-VehicleCatalog-API-Persistence/` | Fields 1–10 (fully evidenced) |
| 4 | Archive: Endpoints | `openspec/changes/archive/2026-09-11-ImplementacionEnpoints/` | Fields 1–10 |
| 5 | Archive: TestProyect | `openspec/TestProyect/` ← non-standard path | Fields 1–10 (gap: no explore.md) |
| 6 | Archive: CI | `openspec/changes/archive/2026-09-12-GithubWorkflowCreacion/` | Fields 1–10 |
| 7 | Archive: MVC | `openspec/changes/archive/2026-09-12-vehicle-catalog-mvc/` | Fields 1–10 |
| 8 | Archive: MAUI | `openspec/changes/archive/2026-09-14-vehicle-catalog-maui/` | Fields 1–10 |
| 9 | Archive: MAUI Interface | `openspec/changes/archive/2026-09-16-mejora-interfaz-maui/` | Fields 1–10 (gap: no spec.md) |
| 10 | Active change | `openspec/changes/DocumentacionFinal/` | Fields 1–10 (gap: not archived) |
| 11 | Git log | `git log --oneline` on `feat/evidencia-proceso-ia` | Field 10 (real commit hashes) |

**Commit hash mapping** (pre-resolved from exploration handoff):

| Iteration | Commit hashes | PR |
|-----------|---------------|----|
| Initial state (pre-SDD) | `43b7af6` | — |
| VehicleCatalog-API-Persistence | `718a08d` | PR #1 `2be5d69` |
| ImplementacionEnpoints | `79cbf36`, `66c67d8`, `3609293`, `57d8378` | — |
| TestProyect | `496ae54`, `9525c40` | — |
| GithubWorkflowCreacion | `a01141f` | PR #2 `989dfdf` |
| vehicle-catalog-mvc | `4d2fde6`, `d9ca16b` | PR #3 `1170f4b` |
| vehicle-catalog-maui (PR1) | `19953f9`–`f6c1e48` | PR #4 `d56a6f2` |
| vehicle-catalog-maui (PR2) | `0ea262c`–`ba80c73` | PR #5 `045693f` |
| mejora-interfaz-maui | `9a12b75`, `df423ab`, `dbcab89`, `cd6c692`, `d9dc66b`, `5f3cc14` | PR #6 `567f65c` |
| DocumentacionFinal | `1bfc4fd` | PR #7 `535bcf5` |

---

## AI-LOG.md Structure Design

```
AI-LOG.md (≤ 400 lines)
├── H1 Title + intro paragraph                    [~8 lines]
├── ## Resumen — Summary table (8 rows)            [~15 lines]
├── ## Estado inicial — pre-SDD (43b7af6)          [~15 lines]
├── ## Iteración 1 — VehicleCatalog-API-Persistence [~40 lines]
├── ## Iteración 2 — ImplementacionEnpoints         [~38 lines]
├── ## Iteración 3 — TestProyect                    [~38 lines]
├── ## Iteración 4 — GithubWorkflowCreacion         [~38 lines]
├── ## Iteración 5 — vehicle-catalog-mvc            [~38 lines]
├── ## Iteración 6 — vehicle-catalog-maui           [~40 lines]
├── ## Iteración 7 — mejora-interfaz-maui           [~38 lines]
└── ## Iteración 8 — DocumentacionFinal             [~38 lines]
```

### Line Budget Arithmetic

| Section | Lines |
|---------|-------|
| H1 + intro (title, blank, 3-sentence intro, blank) | 8 |
| Summary table (header 3 + 8 data rows + blanks) | 15 |
| Pre-SDD section (heading + commit anchor + 3 bullets + blanks) | 15 |
| 8 × per-iteration sections @ ~40 lines each | 320 |
| Inter-section blank lines + buffer | 30 |
| **Total** | **388** |

Buffer headroom: **12 lines** before hitting the 400-line cap (REQ-008).

---

## Per-Iteration Format

Each iteration section uses a single 3-column table plus an optional prose note (≤ 3 lines):

```markdown
## Iteración N — {change-name}

| Campo | Contenido | Evidencia |
|-------|-----------|-----------|
| 1. Especificación inicial | … | `openspec/changes/archive/{folder}/proposal.md` |
| 2. Prompt utilizado | Objetivo derivado del Intent del proposal (texto verbatim no disponible) | `proposal.md § Intent` |
| 3. Objetivo del prompt | … | `proposal.md § Intent` |
| 4. Resultado / propuesta IA | … | `proposal.md` |
| 5. Implementación | … | `tasks.md` / `apply-progress.md` |
| 6. Validaciones | … | `verify-report.md` / CI badge |
| 7. Problemas identificados | … | `design.md` / `verify-report.md` |
| 8. Ajustes realizados | … | commit messages |
| 9. Decisiones técnicas del desarrollador | … | `proposal.md § Key Decisions` |
| 10. Commit(s) relacionados | `{hash}` | `git log` |
```

> Optional brief note (≤ 3 lines) for context not capturable in a cell.
```

### Field Authoring Rules

| Field | Rule |
|-------|------|
| Field 2 (Prompt utilizado) | ALL iterations except DocumentacionFinal: label `"Objetivo derivado del Intent del proposal (texto verbatim no disponible)"`. DocumentacionFinal: `"Se debe primero actualizar main y luego realizar la documentación." [verbatim]` (REQ-005) |
| Field 9 (Decisiones técnicas) | Cite "Key Decisions" or "Confirmable Decisions" table from each `proposal.md`; label explicitly as developer choice, not AI output (REQ-006) |
| Field 10 (Commits) | Real hashes only from pre-resolved mapping above; omit field when no commit assigned (REQ-004) |
| Gap labels | Apply exact text from REQ-007: `sin spec.md` (mejora-interfaz-maui), `ruta no estándar; sin explore.md` (TestProyect), `cambio activo (sin archivar)` (DocumentacionFinal) |
| Missing evidence | Omit cell content or write `no disponible` — never invent content (REQ-004 SCEN-006) |

### Summary Table Schema

```markdown
| # | Cambio | Fecha | Estado | Commit(s) |
|---|--------|-------|--------|-----------|
| 1 | VehicleCatalog-API-Persistence | 2026-09-09 | Archivado | `718a08d` / PR #1 |
…
| 8 | DocumentacionFinal | 2026-09-16 | Activo | `1bfc4fd` / PR #7 |
```

---

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `AI-LOG.md` | Create | Evidence-backed AI process log at repository root |

No existing file is modified or deleted (REQ-001).

---

## Data Flow

```
openspec/changes/archive/{7 folders}/  ──┐
openspec/changes/DocumentacionFinal/  ──┤
openspec/TestProyect/                 ──┤─→ [Author] ──→ AI-LOG.md (≤400 lines)
openspec/specs/ (9 delta specs)       ──┤
openspec/config.yaml                  ──┤
git log (43 commits, resolved hashes) ──┘
```

---

## Verification Plan

| Check | Command / Assertion |
|-------|---------------------|
| Line budget | `(Get-Content AI-LOG.md).Count` ≤ 400 |
| Section count | 8 `## Iteración` headings + 1 pre-SDD heading present |
| Only AI-LOG.md added | `git diff --name-only main..HEAD` returns only `AI-LOG.md` |
| No fabricated prompts | All Field-2 cells contain derived label or `[verbatim]` quote; no free-form prompt text |
| Gap labels exact match | `mejora-interfaz-maui` → `sin spec.md`; `TestProyect` → `ruta no estándar; sin explore.md`; `DocumentacionFinal` → `cambio activo (sin archivar)` |
| Evidence citations resolve | All `openspec/…` paths exist on disk; all commit hashes present in `git log` |
| Developer decisions labeled | Field-9 cells cite `proposal.md § Key Decisions` and carry developer-choice label |

---

## Threat Matrix

N/A — no routing, shell, subprocess, VCS/PR automation, executable-file classification,
or process-integration boundary.

---

## Migration / Rollout

No migration required. Single new file `AI-LOG.md` added to `feat/evidencia-proceso-ia`,
merged to `main` via PR. Rollback: `git revert` or delete file — zero downstream impact.

---

## Open Questions

None. All design choices are resolved by the proposal, spec requirements, and
pre-resolved commit mapping from the exploration handoff.
