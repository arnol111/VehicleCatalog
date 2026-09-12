# Archive Report: TestProyect — VehicleCatalog Test Suite

**Change:** TestProyect
**Archive Date:** 2026-09-11
**Verdict:** ✅ PASS
**Archived to:** `openspec/changes/archive/2026-09-11-TestProyect/`

---

## Summary

This change introduced a full test project (`test/testAPI`) for the VehicleCatalog API layer, covering CQRS query handlers with unit tests (NSubstitute mocks) and HTTP-level integration tests (WebApplicationFactory + Testcontainers SQL Server). It also applied a handler-level bugfix and a controller catch-block improvement to align exception semantics with the domain model.

---

## What Was Built

| Area | Action | Description |
|------|--------|-------------|
| `test/testAPI/testAPI.csproj` | Created | net10.0 test project; 8 NuGet packages; 4 ProjectReferences to src/ layers |
| `test/testAPI/Unit/CarBrand/GetAllBrandsQueryHandlerTests.cs` | Created | 1 unit test (SCEN-U-01) |
| `test/testAPI/Unit/CarBrand/GetByIdQueryHandlerTests.cs` | Created | 2 unit tests (SCEN-U-02, SCEN-U-03) |
| `test/testAPI/Unit/CarModel/GetAllModelsQueryHandlerTests.cs` | Created | 4 unit tests (SCEN-U-04..U-07) |
| `test/testAPI/Integration/Fixtures/SqlServerFixture.cs` | Created | IAsyncLifetime + ICollectionFixture; MsSqlContainer; EnsureCreatedAsync; SeedDataAsync; ClearDataAsync |
| `test/testAPI/Integration/CarBrandsControllerTests.cs` | Created | 2 integration tests (SCEN-I-01, SCEN-I-02) |
| `test/testAPI/Integration/CarModelsControllerTests.cs` | Created | 4 integration tests (SCEN-I-03..I-06) |
| `test/testAPI/Integration/CarBrandByIdControllerTests.cs` | Created | 3 integration tests (SCEN-I-07..I-09) |
| `src/API.Application/CarBrand/Query/GetById/GetByIdQueryHandler.cs` | Modified | `throw new Exception(...)` → `throw new NotFoundException(...)` |
| `src/API/Controllers/CarBrandController.cs` | Modified | Split `catch (Exception)` → `catch (NotFoundException ex)` → 404 and `catch (Exception)` → 500 |
| `VehicleCatalog.slnx` | Modified | Added `<Folder Name="/test/">` with test project reference |
| `README.md` | Modified | Added Spanish "## Proyecto de Pruebas" section |

---

## Final Test Results

| Suite | Tests | Passed | Failed | Skipped |
|-------|-------|--------|--------|---------|
| Unit | 7 | 7 | 0 | 0 |
| Integration | 9 | 9 | 0 | 0 |
| **Total** | **16** | **16** | **0** | **0** |

Build: **0 errors**, 2 non-blocking warnings (NU1603).

---

## Spec Scenario Coverage

All 27 scenarios covered and passing at archive:

- **SCEN-U-01..U-07** — Unit tests (GetAllBrands, GetById, GetAllModels with filter/case/notfound)
- **SCEN-I-01..I-09** — Integration tests (carBrands list, empty DB, carModels list+filter+caseinsensitive+notfound, carBrand by ID valid/notfound/invalid)
- **SCEN-F-01..F-04** — Handler fix (NotFoundException thrown, no throw on valid, controller 404/not-404)
- **SCEN-S-01..S-07** — Scaffold (net10.0, solution registration, NuGet packages, project refs, ICollectionFixture, EnsureCreated, explicit DELETE)

---

## Task Completeness

All 16 tasks complete across 6 phases (WU-01 through WU-06). No unchecked tasks at archive time.

| Phase | Tasks | Status |
|-------|-------|--------|
| Phase 1 — Scaffold | T-WU01-01, T-WU01-02, T-WU01-03 | ✅ |
| Phase 2 — Handler Fix | T-WU02-01, T-WU02-02, T-WU02-03 | ✅ |
| Phase 3 — Unit Tests | T-WU03-01, T-WU03-02, T-WU03-03 | ✅ |
| Phase 4 — Integration Fixture | T-WU04-01 | ✅ |
| Phase 5 — Integration Tests | T-WU05-01, T-WU05-02, T-WU05-03 | ✅ |
| Phase 6 — Documentation | T-WU06-01 | ✅ |

---

## Open Issues Resolved

| OI | Resolution |
|----|------------|
| OI-01 — SCEN-I-06 behavior (200+[] vs 404) | **Resolved as 404** — `CarModelsController` catches `NotFoundException` → `NotFound()`. Test asserts 404. |
| OI-02 — SCEN-F-04: generic catch returns 404 for all exceptions | **Resolved** — `CarBrandController` split per T-WU02-03: `catch (NotFoundException)` → 404, `catch (Exception)` → 500. |

---

## Design Deviations Accepted

| Deviation | Reason | Outcome |
|-----------|--------|---------|
| `CarBrandController` generic catch split (design.md said "leave as-is") | tasks.md T-WU02-03 and spec OI-02 explicitly required the split for SCEN-F-04 compliance | Improves correctness; accepted in apply-progress.md and verify-report.md |
| `SeedDataAsync` uses auto-generated identity IDs (no explicit IdCarBrand) | SQL Server IDENTITY columns reject explicit inserts without `SET IDENTITY_INSERT ON` | `CarBrandByIdControllerTests` discovers brand ID dynamically via `GET /api/carBrands` |
| `SqlServerFixture` exposes `public IServiceProvider Services` | Required for test classes to create scoped `VehicleCatalogDbContext` for seed/clear operations | No impact on production code |

---

## Warnings and Suggestions

### WARNING (non-blocking)
- **W-01 — NU1603 xunit.runner.visualstudio**: `testAPI.csproj` requests `>= 2.9.3` but `2.9.3` is not published to NuGet; resolved to `3.0.0`. Build and test discovery work correctly. Consider pinning `3.0.0` explicitly to suppress the warning.

### SUGGESTIONS (no action required)
- **S-01 — ClearDataAsync uses EF RemoveRange**: Loads rows into the change tracker before deleting. Functionally correct; for test fixture scale this is acceptable. Raw `ExecuteSqlRawAsync("DELETE FROM ...")` would be more efficient if seed data grows.
- **S-02 — DisposeAsync re-seeds after clear**: `CarBrandsControllerTests.DisposeAsync` calls `ClearDataAsync` + `SeedDataAsync`. If this class runs last, it seeds the DB unnecessarily. No functional impact.

---

## Out-of-Scope Suggestions (Future Work)

These were documented in `proposal.md` and remain outside this change:

### API.Domain
- `CarModel` constructor: validate `ArgumentNullException` on null name/brand
- `NotFoundException` message format tests

### API.Application
- `Dispatcher.Send` resolves the correct handler from DI (dispatcher unit test)
- Handler registration: verify all handlers are registered in DI container

### API.Infrastructure
- Repository query tests: `GetAllByBrandIdAsync` returns correct models for a brand ID (requires real DB or SQLite)
- `CarBrandRepository.GetByIdAsync` returns null for missing ID

### CI/CD
- Pipeline configuration for automated test execution (Docker-capable runner for integration tests)

---

## Archive Contents

| File | Description |
|------|-------------|
| `proposal.md` | Intent, scope, approach, work units, test cases, affected areas, risks |
| `spec.md` | Delta specs for 4 domains: test-api-unit, test-api-integration, get-brand-by-id-handler-fix, test-project-scaffold |
| `design.md` | Architecture decisions, file tree, file changes, contracts, handler fix decision |
| `tasks.md` | 16 implementation tasks across 6 phases; all checked complete |
| `apply-progress.md` | PR 1/2/3 progress reports; all 3 PRs completed on 2026-09-11 |
| `verify-report.md` | Full verification report; verdict PASS; all 27 scenarios covered |
| `archive-report.md` | This document |

---

## SDD Cycle Complete

Change **TestProyect** has been fully planned, implemented, verified, and archived.
