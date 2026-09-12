# Apply Progress: TestProyect

## PR 1 — WU-01 + WU-02 (Scaffold + Handler/Controller Fix)

**Status**: completed  
**Date**: 2026-09-11

---

### WU-01: Project Scaffold — DONE

| Task | Status | Notes |
|------|--------|-------|
| T-WU01-01: Create directory tree | ✅ done | `test/testAPI/Unit/CarBrand/`, `test/testAPI/Unit/CarModel/`, `test/testAPI/Integration/Fixtures/` + `.gitkeep` files |
| T-WU01-02: Create `test/testAPI/testAPI.csproj` | ✅ done | net10.0, all 8 NuGet refs, 4 ProjectReferences; xunit.runner.visualstudio resolved to 3.0.0 (NuGet NU1603 — non-blocking) |
| T-WU01-03: Register in `VehicleCatalog.slnx` | ✅ done | Added `<Folder Name="/test/">` with `<Project Path="test/testAPI/testAPI.csproj" />` |

### WU-02: Handler + Controller Fix — DONE

| Task | Status | Notes |
|------|--------|-------|
| T-WU02-02: Fix `GetByIdQueryHandler.cs` | ✅ done | `throw new Exception(...)` → `throw new NotFoundException(...)` + added `using API.Domain.Exceptions;` |
| T-WU02-03: Fix `CarBrandController.cs` catch blocks | ✅ done | Split into `catch (NotFoundException ex)` → 404 and `catch (Exception)` → 500 |

---

### Work Unit Evidence

| Evidence | Result |
|----------|--------|
| Focused build command | `dotnet build VehicleCatalog.slnx` → **0 errors**, 2 warnings (NU1603 — non-blocking) |
| Runtime harness | `dotnet test test/testAPI/testAPI.csproj --no-build` → "No test available" — 0 tests discovered, 0 errors — expected for empty scaffold |
| Rollback boundary | Delete `test/testAPI/`; revert `GetByIdQueryHandler.cs` (remove `using API.Domain.Exceptions;` + restore `throw new Exception(...)`); revert `CarBrandController.cs` (restore single `catch (Exception e)` → 404); revert `VehicleCatalog.slnx` (remove `/test/` folder entry) |

---

### Notes / Deviations

- **`xunit.runner.visualstudio` NU1603**: Version `2.9.3` was not published to NuGet; NuGet resolved `3.0.0` instead. This is a NuGet floating-version resolution behavior, not an error. Build and test discovery work correctly.
- **Controller catch split**: The spec PR scope (WU-02) explicitly required splitting `catch (Exception)` → `catch (NotFoundException)` + `catch (Exception)` per SCEN-F-04. Implemented as instructed. Note: the `design.md` originally said "leave generic catch as-is" but the tasks.md (T-WU02-03) and the prompt scope override this, which is the correct decision per the spec's OI-02 resolution.

---

---

## PR 2 — WU-03 + WU-04 (Unit tests + SqlServerFixture)

**Status**: completed  
**Date**: 2026-09-11

---

### WU-03: Unit Tests — DONE

| Task | Status | Notes |
|------|--------|-------|
| T-WU02-01 (RED): `GetByIdQueryHandlerTests.cs` — SCEN-U-02 + SCEN-U-03 | ✅ done | Both tests in file; SCEN-U-03 asserts `NotFoundException` |
| T-WU03-01: `GetAllBrandsQueryHandlerTests.cs` — SCEN-U-01 | ✅ done | 1 test: getAllBrands_ShouldReturnList |
| T-WU03-02: `GetByIdQueryHandlerTests.cs` — SCEN-U-02 + SCEN-U-03 | ✅ done | Merged into single file with T-WU02-01 |
| T-WU03-03: `GetAllModelsQueryHandlerTests.cs` — SCEN-U-04..U-07 | ✅ done | 4 tests: getAllModels, filterExact, caseInsensitive, notFound |

### WU-04: SqlServerFixture — DONE

| Task | Status | Notes |
|------|--------|-------|
| T-WU04-01: `Integration/Fixtures/SqlServerFixture.cs` + `SqlServerCollection` | ✅ done | `IAsyncLifetime`, `MsSqlBuilder` with explicit image, `ConfigureTestServices` replaces DbContext, `EnsureCreatedAsync`, `SeedDataAsync`, `ClearDataAsync` |

---

### Deviations / Notes

- `Handle()` interface takes only 1 arg (no `CancellationToken`) — removed CT from all test call sites to match the custom `IRequestHandler<TRequest,TResult>` contract.
- `using Xunit;` required in all test files — `ImplicitUsings` does not include xunit namespace.
- `MsSqlBuilder("")` uses explicit image `mcr.microsoft.com/mssql/server:2022-latest` to suppress the obsolete parameterless-constructor warning in Testcontainers 4.x.

---

### Work Unit Evidence

| Evidence | Result |
|----------|--------|
| Focused test command | `dotnet test test/testAPI/testAPI.csproj --filter "Category=Unit" --no-build` → **Passed! Failed: 0, Passed: 7, Skipped: 0, Total: 7** |
| Runtime harness | N/A — unit tests are pure in-process; no Docker or HTTP stack involved |
| Rollback boundary | Delete `test/testAPI/Unit/CarBrand/GetAllBrandsQueryHandlerTests.cs`, `test/testAPI/Unit/CarBrand/GetByIdQueryHandlerTests.cs`, `test/testAPI/Unit/CarModel/GetAllModelsQueryHandlerTests.cs`, `test/testAPI/Integration/Fixtures/SqlServerFixture.cs`; no production code changes in this PR |

---

## Remaining Work

| Work Unit | PR | Status |
|-----------|----|--------|
| WU-05 + WU-06: Integration tests + README | PR 3 | ✅ completed |

---

## PR 3 — WU-05 + WU-06 (Integration Tests + README)

**Status**: completed  
**Date**: 2026-09-11

---

### WU-05: Integration Tests — DONE

| Task | Status | Notes |
|------|--------|-------|
| T-WU05-01: `Integration/CarBrandsControllerTests.cs` — SCEN-I-01, SCEN-I-02 | ✅ done | `IAsyncLifetime`: ClearData+SeedData in init, restore in dispose. SCEN-I-02 clears then verifies empty, then re-seeds. |
| T-WU05-02: `Integration/CarModelsControllerTests.cs` — SCEN-I-03..I-06 | ✅ done | SCEN-I-06 asserts 404 (OI-01 confirmed — controller catches NotFoundException → NotFound). |
| T-WU05-03: `Integration/CarBrandByIdControllerTests.cs` — SCEN-I-07..I-09 | ✅ done | SCEN-I-07 discovers brand ID dynamically via `/api/carBrands` first (no hardcoded id=1). |

### WU-06: README Update — DONE

| Task | Status | Notes |
|------|--------|-------|
| T-WU06-01: `README.md` — Spanish "Proyecto de Pruebas" section | ✅ done | Added at bottom; includes package table, test class descriptions, run commands, Docker requirement note. |

---

### Deviations / Notes

- **IDENTITY_INSERT fix**: `SeedDataAsync` in the fixture originally used explicit `IdCarBrand` values (1, 2). SQL Server IDENTITY columns reject explicit inserts unless `SET IDENTITY_INSERT ON` is active. Fixed by removing explicit IDs from `SeedDataAsync` — EF lets SQL Server assign identity values. `EnsureCreatedAsync` handles the initial `HasData` seed; subsequent `SeedDataAsync` calls use auto-generated IDs.
- **Dynamic ID lookup in SCEN-I-07**: Because identity IDs are not predictable after clear+re-seed cycles, `CarBrandByIdControllerTests` discovers the first available brand ID from `GET /api/carBrands` before testing `GET /carBrand?id={id}`.
- **SqlServerFixture.Services exposed**: Added `public IServiceProvider Services => _factory.Services;` to allow test classes to create scoped `VehicleCatalogDbContext` instances for seed/clear operations.

---

### Work Unit Evidence

| Evidence | Result |
|----------|--------|
| Focused build command | `dotnet build VehicleCatalog.slnx` → **0 errors**, 2 warnings (NU1603 — non-blocking) |
| Focused test command (unit) | `dotnet test --filter "Category=Unit" --no-build` → **Passed! Failed: 0, Passed: 7, Skipped: 0, Total: 7** |
| Focused test command (integration) | `dotnet test --filter "Category=Integration" --no-build` → **Passed! Failed: 0, Passed: 9, Skipped: 0, Total: 9** |
| Runtime harness | Docker Desktop running; Testcontainers pulled `mcr.microsoft.com/mssql/server:2022-latest` and started container automatically |
| Rollback boundary | Delete `test/testAPI/Integration/CarBrandsControllerTests.cs`, `CarModelsControllerTests.cs`, `CarBrandByIdControllerTests.cs`; revert `SqlServerFixture.cs` (remove `Services` property and revert `SeedDataAsync`); remove "Proyecto de Pruebas" section from `README.md` |
