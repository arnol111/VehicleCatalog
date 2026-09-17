# Proposal: EvidenciaProcesoIA

## Intent

The VehicleCatalog project was built iteratively using SDD and gentle-ai, but no consolidated record of that process exists. Stakeholders, reviewers, and future contributors cannot trace how AI assisted each phase, what decisions the developer made, or why the architecture looks as it does. `AI-LOG.md` closes that gap with a single, evidence-backed log at the repository root.

## Scope

### In Scope
- Create `AI-LOG.md` at repository root on branch `feat/evidencia-proceso-ia`
- Document 8 SDD iterations (one per archived/active change) plus the pre-SDD initial state
- Each record covers the 10 required fields using only verified evidence
- Merge via PR to `main` (single file, ≤ 400 lines)

### Out of Scope
- Modifying any existing spec, design, or source file
- Documenting future SDD changes not yet in the repo
- Reconstructing verbatim AI prompt text that does not exist in evidence

## Capabilities

### New Capabilities
- `ai-process-log`: Single Markdown artifact documenting the iterative AI-assisted development process with evidence-backed per-iteration records.

### Modified Capabilities
None

## Approach

Read all 8 change artifacts under `openspec/changes/` and `openspec/changes/archive/`, then cross-reference the 43-commit Git history. Compose each iteration record from real artifacts only. Apply cognitive-doc-design progressive-disclosure structure: summary table first, then per-iteration detail sections.

**Iteration record model (10 fields):**

| Field | Evidence source | Handling when absent |
|-------|----------------|----------------------|
| 1. Initial specification | `proposal.md` / `spec.md` | Note "spec gap" if missing (e.g. TestProyect, mejora-interfaz-maui) |
| 2. Prompt used | Raw text in repo | Label as **"Objetivo derivado del Intent del proposal"** — never fabricate |
| 3. Prompt objective | `## Intent` of proposal | Always available for all 8 changes |
| 4. AI result / proposal | `proposal.md` content | Always available |
| 5. Implementation | Task list + `apply` artifacts | Available for most changes |
| 6. Validations | `verify` artifact + CI status | Available where archived |
| 7. Problems identified | `design.md` / `verify` notes | Cited directly; none invented |
| 8. Adjustments | Commit messages + design decisions | Git log evidence |
| 9. Developer decisions | "Key Decisions" tables in proposals | Explicitly labelled as developer choices |
| 10. Related commit | Real hash from exploration handoff | Omit field when no commit exists |

**Language:** AI-LOG.md content in Spanish (repository documentation language — README and archived docs are in Spanish). SDD phase artifacts in `openspec/` remain in English.

**Pre-SDD initial state:** commit `43b7af6` (2026-09-09) is documented as "Estado inicial del repositorio — pre-SDD" in a dedicated section, NOT as an AI iteration.

**DocumentacionFinal verbatim instruction:** the single confirmed verbatim user instruction ("Se debe primero actualizar main y luego realizar la documentación.") is quoted and labelled `[verbatim]`.

**Gaps documented honestly:**
- `mejora-interfaz-maui`: no `spec.md` — record status as `sin spec.md`
- `TestProyect`: non-standard path `openspec/TestProyect/`, no `explore.md`
- `DocumentacionFinal`: active change, not yet archived — record status as `cambio activo (sin archivar)`

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `AI-LOG.md` (root) | New | Single new file; no existing files modified |
| `openspec/changes/EvidenciaProcesoIA/` | New | This proposal + future spec/design/tasks |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| No raw prompt text in repo | High | Evidence model pre-defines "derived from Intent" label; fabrication prohibited |
| spec.md gap (mejora-interfaz-maui, TestProyect) | Medium | Gaps are recorded honestly with explicit gap labels |
| DocumentacionFinal not archived | Low | Iteration marked active; status recorded without blocking delivery |
| Fabrication drift during authoring | Medium | Evidence column required per field; any field without backing artifact is omitted |

## Rollback Plan

Delete `AI-LOG.md` from the branch and close the PR. No other file is modified; rollback is a single `git revert` or file deletion with zero downstream impact.

## Dependencies

- Git history accessible on `feat/evidencia-proceso-ia`
- All 8 change artifacts readable under `openspec/changes/` and `openspec/changes/archive/`

## Success Criteria

- [ ] `AI-LOG.md` exists at repository root
- [ ] Documents all 8 SDD changes plus the pre-SDD initial state
- [ ] No prompt text is invented; absent prompts labelled "Objetivo derivado del Intent"
- [ ] Each iteration record includes an evidence citation (artifact path or commit hash)
- [ ] Developer decisions are explicitly distinguished from AI proposals
- [ ] Gaps (spec.md missing, not archived) are documented with honest status labels
- [ ] File stays within 400-line budget
