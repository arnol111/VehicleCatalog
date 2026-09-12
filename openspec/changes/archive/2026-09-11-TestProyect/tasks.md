# Tasks: TestProyect — VehicleCatalog Test Suite

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | 520–620 (new files dominate) |
| 400-line budget risk | High |
| Chained PRs recommended | Yes |
| Suggested split | PR 1 (WU-01 + WU-02) → PR 2 (WU-03 + WU-04) → PR 3 (WU-05 + WU-06) |
| Delivery strategy | ask-on-risk |
| Chain strategy | pending |

Decision needed before apply: Yes
Chained PRs recommended: Yes
Chain strategy: pending
400-line budget risk: High

### Suggested Work Units

| Unit | Goal | Likely PR | Focused test command | Runtime harness | Rollback boundary |
|------|------|-----------|----------------------|-----------------|-------------------|
| WU-01 + WU-02 | Scaffold + handler fix | PR 1 | `dotnet build test/testAPI` | `dotnet build` — no tests yet | Delete `test/testAPI/`, revert `GetByIdQueryHandler.cs`, revert `.slnx` and `CarBrandController.cs` |
| WU-03 + WU-04 | All unit tests | PR 2 | `dotnet test --filter "Category=Unit"` | N/A — no Docker needed; pure in-process | Delete unit test files; no production impact |
| WU-05 + WU-06 | Integration tests + README | PR 3 | `dotnet test --filter "Category=Integration"` | Docker running, `dotnet test --filter "Category=Integration"` | Delete integration test files and README section |

---

## Phase 1: Scaffold

- [x] T-WU01-01 **Create `test/testAPI/` directory tree** — create folders `test/testAPI/Unit/CarBrand/`, `test/testAPI/Unit/CarModel/`, `test/testAPI/Integration/Fixtures/`
  - *Files*: directory structure only
  - *Notes*: Must exist before any `.cs` or `.csproj` is written
  - *Acceptance*: `Test-Path test/testAPI/Integration/Fixtures` returns true
  - *Depends on*: nothing

- [x] T-WU01-02 **Create `test/testAPI/testAPI.csproj`** — net10.0, `IsPackable=false`, all 8 NuGet refs (xunit 2.x, xunit.runner.visualstudio 2.x, Microsoft.NET.Test.Sdk 17.x, NSubstitute 5.x, Microsoft.AspNetCore.Mvc.Testing 10.0.x, Testcontainers.MsSql 4.x, FluentAssertions 6.x, coverlet.collector 6.x), 4 ProjectReferences (`src/API`, `src/API.Application`, `src/API.Domain`, `src/API.Infrastructure`)
  - *Files*: `test/testAPI/testAPI.csproj`
  - *Notes*: Use relative `..\..\src\` paths for ProjectReference. See design for full XML template.
  - *Acceptance*: `dotnet restore test/testAPI` succeeds with no version conflict errors
  - *Depends on*: T-WU01-01

- [x] T-WU01-03 **Register test project in `VehicleCatalog.slnx`** — add `<Project Path="test/testAPI/testAPI.csproj" />` inside a `<Folder Name="/test/">` entry
  - *Files*: `VehicleCatalog.slnx`
  - *Notes*: Read the current `.slnx` first to match existing XML style; do not alter `/src/` folder entries
  - *Acceptance*: `dotnet test` at solution root discovers tests from `test/testAPI/`
  - *Depends on*: T-WU01-02

---

## Phase 2: Handler Fix

- [x] T-WU02-01 **[RED] Write failing test for `NotFoundException` throw** — add `SCEN-U-03` to `GetByIdQueryHandlerTests.cs` asserting `NotFoundException` is thrown for null; confirm it fails (compilation or assertion) before the fix
  - *Files*: `test/testAPI/Unit/CarBrand/GetByIdQueryHandlerTests.cs`
  - *Notes*: Test file may be a stub at this point; only SCEN-U-03 needs to exist and fail
  - *Acceptance*: `dotnet test --filter "getBrandById_WithInvalidId"` reports failure or compilation error
  - *Depends on*: T-WU01-02

- [x] T-WU02-02 **Fix `GetByIdQueryHandler.cs`** — replace `throw new Exception("La marca de carro no existe")` with `throw new NotFoundException("La marca de carro no existe")`
  - *Files*: `src/API.Application/CarBrand/Query/GetById/GetByIdQueryHandler.cs`
  - *Notes*: One-line diff. `NotFoundException` is in `API.Domain/Exceptions/NotFoundException.cs` — verify the `using` directive is present.
  - *Acceptance*: `dotnet test --filter "getBrandById_WithInvalidId"` passes; `dotnet build src/API.Application` succeeds
  - *Depends on*: T-WU02-01

- [x] T-WU02-03 **Fix `CarBrandController.cs` catch blocks** — split the generic `catch (Exception e)` into two: `catch (NotFoundException e)` → `NotFound(e.Message)` first, then `catch (Exception e)` → `StatusCode(500, e.Message)`. `NotFoundException` catch MUST come before the generic one.
  - *Files*: `src/API/Controllers/CarBrandController.cs`
  - *Notes*: Confirmed decision OI-02. This is needed for SCEN-F-04 (non-NotFoundException must not return 404). Verify `NotFoundException` is accessible — add `using` if needed.
  - *Acceptance*: Handler throwing `NotFoundException` → controller returns 404; handler throwing `InvalidOperationException` → controller returns 500
  - *Depends on*: T-WU02-02

---

## Phase 3: Unit Tests

- [x] T-WU03-01 **Create `Unit/CarBrand/GetAllBrandsQueryHandlerTests.cs`** — 1 test: SCEN-U-01
  - *Files*: `test/testAPI/Unit/CarBrand/GetAllBrandsQueryHandlerTests.cs`
  - *Notes*: Mock `IUnitOfWork.CarBrands.GetAllAsync()` → returns `[CarBrand(1,"Toyota"), CarBrand(2,"Ford")]`. Assert result is `IReadOnlyList<CarBrandResponse>` with 2 items, correct Id/Name values. `[Trait("Category","Unit")]` on the method.
  - *Acceptance*: `dotnet test --filter "getAllBrands"` passes
  - *Depends on*: T-WU01-02

- [x] T-WU03-02 **Complete `Unit/CarBrand/GetByIdQueryHandlerTests.cs`** — add SCEN-U-02 alongside the RED test from T-WU02-01
  - *Files*: `test/testAPI/Unit/CarBrand/GetByIdQueryHandlerTests.cs`
  - *Notes*: SCEN-U-02: mock `GetByIdAsync(1)` → `CarBrand{IdCarBrand=1,Brand="Toyota"}`; assert `CarBrandDTO.IdCarBrand==1` and `Brand=="Toyota"`. `[Trait("Category","Unit")]` on both methods.
  - *Acceptance*: `dotnet test --filter "Category=Unit" --filter "CarBrandHandler"` — 2 tests pass
  - *Depends on*: T-WU02-02

- [x] T-WU03-03 **Create `Unit/CarModel/GetAllModelsQueryHandlerTests.cs`** — 4 tests: SCEN-U-04, SCEN-U-05, SCEN-U-06, SCEN-U-07
  - *Files*: `test/testAPI/Unit/CarModel/GetAllModelsQueryHandlerTests.cs`
  - *Notes*: **CRITICAL** — every `CarModel` mock instance MUST have `CarBrand` nav property populated (e.g. `CarBrand = new CarBrand { IdCarBrand=1, Brand="Toyota" }`). Omitting it causes NullReferenceException in the handler's mapping. SCEN-U-07 asserts `NotFoundException` is thrown when brand name does not match. Setup: mock both `IUnitOfWork.CarBrands` and `IUnitOfWork.CarModels`. `[Trait("Category","Unit")]` on all methods.
  - *Acceptance*: `dotnet test --filter "Category=Unit" --filter "CarModel"` — 4 tests pass
  - *Depends on*: T-WU01-02

---

## Phase 4: Integration Fixture

- [x] T-WU04-01 **Create `Integration/Fixtures/SqlServerFixture.cs`** — `IAsyncLifetime` + `ICollectionFixture` wiring
  - *Files*: `test/testAPI/Integration/Fixtures/SqlServerFixture.cs`
  - *Notes*: Follow design contract exactly: `MsSqlBuilder().Build()`, `ConfigureTestServices` removes existing `DbContextOptions<VehicleCatalogDbContext>` and re-registers with `_container.GetConnectionString()`, `EnsureCreatedAsync()` called once in `InitializeAsync`. Expose `HttpClient Client`, `ClearDataAsync(db)` (DELETE CarModels first, then CarBrands to respect FK), `SeedDataAsync(db)` (explicit inserts, not `HasData`). Also declare `[CollectionDefinition("SqlServer")]` + `SqlServerCollection : ICollectionFixture<SqlServerFixture>` in the same file or a separate `SqlServerCollection.cs` in the same folder.
  - *Acceptance*: `dotnet build test/testAPI` succeeds; fixture compiles without error
  - *Depends on*: T-WU01-02

---

## Phase 5: Integration Tests

- [x] T-WU05-01 **Create `Integration/CarBrandsControllerTests.cs`** — SCEN-I-01, SCEN-I-02
  - *Files*: `test/testAPI/Integration/CarBrandsControllerTests.cs`
  - *Notes*: `[Collection("SqlServer")]`, inject `SqlServerFixture`. SCEN-I-01: seed data present → `GET /api/carBrands` → 200, array with `id`+`name` fields. SCEN-I-02: `ClearDataAsync` first → `GET /api/carBrands` → 200, `[]`. After SCEN-I-02 cleanup, restore seed or ensure test isolation order. `[Trait("Category","Integration")]`.
  - *Acceptance*: `dotnet test --filter "CarBrandsController"` — 2 tests pass (Docker required)
  - *Depends on*: T-WU04-01

- [x] T-WU05-02 **Create `Integration/CarModelsControllerTests.cs`** — SCEN-I-03, SCEN-I-04, SCEN-I-05, SCEN-I-06
  - *Files*: `test/testAPI/Integration/CarModelsControllerTests.cs`
  - *Notes*: SCEN-I-06: **confirmed decision OI-01** — `GET /api/carModels?brand=MarcaInexistente` MUST assert `404 Not Found` (not 200+[]). SCEN-I-04 and SCEN-I-05: seed Toyota and Ford with ≥1 model each; assert filtered result contains only Toyota models and no Ford models. SCEN-I-05: send `"tOyOtA"` and compare response body to `"Toyota"` query. `[Trait("Category","Integration")]`.
  - *Acceptance*: `dotnet test --filter "CarModelsController"` — 4 tests pass
  - *Depends on*: T-WU04-01

- [x] T-WU05-03 **Create `Integration/CarBrandControllerTests.cs`** — SCEN-I-07, SCEN-I-08, SCEN-I-09
  - *Files*: `test/testAPI/Integration/CarBrandControllerTests.cs`
  - *Notes*: SCEN-I-07: `GET /carBrand?id=1` (note: no `/api/` prefix per design) → 200, body contains `idCarBrand==1`. SCEN-I-08: `GET /carBrand?id=9999` → 404 (requires handler fix from T-WU02-02 and controller fix from T-WU02-03). SCEN-I-09: `GET /carBrand?id=abc` → 400 (model binding, no handler involved). `[Trait("Category","Integration")]`.
  - *Acceptance*: `dotnet test --filter "CarBrandController"` — 3 tests pass
  - *Depends on*: T-WU04-01, T-WU02-03

---

## Phase 6: Documentation

- [x] T-WU06-01 **Update `README.md` in Spanish** — add section at the bottom describing the test project
  - *Files*: `README.md`
  - *Notes*: Section content (in Spanish): brief description of `test/testAPI`, list of NuGet packages used, how to run unit tests (`dotnet test --filter "Category=Unit"`), how to run integration tests (`dotnet test --filter "Category=Integration"`), Docker requirement for integration tests, command to run all tests (`dotnet test test/testAPI`).
  - *Acceptance*: README contains the new section; content is in Spanish; commands are correct
  - *Depends on*: nothing (can be done in parallel)
