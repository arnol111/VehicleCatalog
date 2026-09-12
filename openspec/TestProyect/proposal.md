# Proposal: TestProyect — VehicleCatalog Test Suite

**Change:** TestProyect  
**Date:** 2026-09-11  
**Status:** ready-for-spec

---

## Intent

The VehicleCatalog API has no test coverage. This change introduces a structured test project at `test/testAPI` covering the `src/API` layer with unit tests (handler-level) and integration tests (HTTP-level via Testcontainers SQL Server). It also fixes a code smell in `GetByIdQueryHandler`, which throws a generic `Exception` instead of the domain-specific `NotFoundException`.

---

## Scope

### In Scope
- Create `test/testAPI/testAPI.csproj` and register it in the solution
- 7 unit tests across 3 query handlers (mocked `IUnitOfWork` via NSubstitute)
- 9 integration tests across 3 controllers (HTTP via `WebApplicationFactory<Program>` + Testcontainers SQL Server)
- Fix `GetByIdQueryHandler` to throw `NotFoundException` instead of plain `Exception`
- Update `README.md` in Spanish describing the test project and how to run tests

### Out of Scope
- Tests for `API.Application`, `API.Domain`, `API.Infrastructure` layers (suggested below)
- `CarModelController` (empty shell — no actions to test)
- Write/mutation endpoint tests (none exist in the current API)
- CI/CD pipeline configuration

---

## Capabilities

### New Capabilities
- `test-api-unit`: Unit tests for query handlers (`GetAllBrandsQueryHandler`, `GetByIdQueryHandler`, `GetAllModelsQueryHandler`) using NSubstitute mocks
- `test-api-integration`: HTTP-level integration tests using `WebApplicationFactory<Program>` + Testcontainers SQL Server

### Modified Capabilities
- `get-brand-by-id-handler`: `GetByIdQueryHandler` throws `NotFoundException` instead of `Exception` — behavioral contract change at handler level

---

## Approach

### Test Pyramid
- **Unit layer**: instantiate handlers directly, mock `IUnitOfWork` with NSubstitute; no HTTP, no DB, no DI container
- **Integration layer**: spin up a real SQL Server via Testcontainers, boot the full app with `WebApplicationFactory<Program>`, override the DbContext registration to point at the container

### DB Strategy for Integration Tests
Use **Testcontainers.MsSql** — keeps the existing `UseSqlServer` EF provider and avoids any provider swap. `UseIdentityColumn()` in EF configurations is SQL Server-specific and will work without modification.

- `EnsureCreated()` on test DB start — applies EF model + seed data without running migrations
- Seed data cleanup via explicit DELETE before tests that require an empty state
- Single shared `SqlServerContainer` per test collection via `ICollectionFixture` to avoid per-test container startup overhead

### Handler Fix
`GetByIdQueryHandler` will be updated to throw `NotFoundException` (already defined in `API.Domain`). The `CarBrandController` will be updated to catch `NotFoundException` specifically for 404, keeping general `Exception` for 500. This aligns controller behavior with the domain intent.

---

## Work Units

| # | Unit | Scope | Key Details |
|---|------|-------|-------------|
| 1 | Project scaffold | `test/testAPI/` | Create `.csproj`, add NuGet packages, register in `.slnx` |
| 2 | Handler fix | `src/API.Application/CarBrand/Query/GetById/GetByIdQueryHandler.cs` | Throw `NotFoundException` instead of `Exception` |
| 3 | Unit tests — CarBrand | `test/testAPI/Unit/CarBrand/` | `GetAllBrandsQueryHandlerTests.cs`, `GetByIdQueryHandlerTests.cs` |
| 4 | Unit tests — CarModel | `test/testAPI/Unit/CarModel/` | `GetAllModelsQueryHandlerTests.cs` |
| 5 | Integration fixture | `test/testAPI/Integration/` | `CustomWebApplicationFactory.cs`, `DatabaseFixture.cs` |
| 6 | Integration tests | `test/testAPI/Integration/` | `CarBrandsControllerTests.cs`, `CarBrandControllerTests.cs`, `CarModelsControllerTests.cs` |
| 7 | README update | `README.md` | Spanish — describe test project, packages, how to run |

---

## NuGet Packages

```xml
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="NSubstitute" Version="5.*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.*" />
<PackageReference Include="Testcontainers.MsSql" Version="4.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="coverlet.collector" Version="6.*" />
```

---

## Test Cases

### Unit Tests

| Test | Handler | Mock Setup |
|------|---------|------------|
| `getAllBrands_ShouldReturnList` | `GetAllBrandsQueryHandler` | `CarBrands.GetAllAsync()` → 2 brands |
| `getBrandById_WithValidId_ShouldReturnBrand` | `GetByIdQueryHandler` | `CarBrands.GetByIdAsync(1)` → brand |
| `getBrandById_WithInvalidId_ShouldThrowNotFoundException` | `GetByIdQueryHandler` | `CarBrands.GetByIdAsync(999)` → null → expect `NotFoundException` |
| `getAllModels_ShouldReturnModelsWithDetails` | `GetAllModelsQueryHandler` | `CarModels.GetAllAsync()` → models with `CarBrand` nav prop populated |
| `getModelsByBrand_WithExactName_ShouldFilterCorrectly` | `GetAllModelsQueryHandler` | `CarBrands.GetAllAsync()` → Toyota; `CarModels.GetAllByBrandIdAsync(1)` → models |
| `getModelsByBrand_CaseInsensitive_ShouldReturnFilteredModels` | `GetAllModelsQueryHandler` | Same as above, query `"tOyOtA"` |
| `getModelsByBrand_NonExistentBrand_ShouldReturnEmptyList` | `GetAllModelsQueryHandler` | `CarBrands.GetAllAsync()` → no match → expect `NotFoundException` |

> Note: The last test name is kept for readability but the assertion is `NotFoundException` — the handler throws, it does not return an empty list.

### Integration Tests

| Endpoint | Scenario | Expected |
|----------|----------|----------|
| `GET /api/carBrands` | Seeded data | 200 OK, JSON array |
| `GET /api/carBrands` | Empty DB | 200 OK, `[]` |
| `GET /api/carModels` | Seeded data with relations | 200 OK, nested brand/year |
| `GET /api/carModels?brand=Toyota` | Exact brand match | 200 OK, filtered |
| `GET /api/carModels?brand=tOyOtA` | Case-insensitive match | 200 OK, same filtered result |
| `GET /api/carModels?brand=MarcaInexistente` | No match | 200 OK, `[]` |
| `GET /carBrand?id=1` | Valid ID | 200 OK |
| `GET /carBrand?id=9999` | Not found | 404 Not Found |
| `GET /carBrand?id=abc` | Invalid int | 400 Bad Request |

---

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `test/testAPI/` | New | Full test project — unit + integration |
| `src/API.Application/CarBrand/Query/GetById/GetByIdQueryHandler.cs` | Modified | Throw `NotFoundException` instead of `Exception` |
| `VehicleCatalog.slnx` | Modified | Add test project reference |
| `README.md` | Modified | Add Spanish test documentation section |

---

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Testcontainers SQL Server container slow startup | Med | Use `ICollectionFixture` for shared container; single startup per test run |
| `CarBrand` nav property null in unit tests | Med | Document mock requirement: always populate `CarBrand` on `CarModel` instances |
| Seed data conflicts with empty-DB integration tests | High | Explicit DELETE in test setup for empty-state scenarios |
| Handler fix (`NotFoundException`) changes controller behavior | Low | `CarBrandController` already returns 404 for any exception; behavior is preserved |
| EF Core 10 + Testcontainers.MsSql compatibility | Low | Testcontainers.MsSql is provider-agnostic (starts a container); EF Core 10 SQL Server provider is first-party |

---

## Rollback Plan

- The handler fix is isolated to one file. Revert `GetByIdQueryHandler.cs` to restore original `Exception` throw.
- The test project is additive. Removing `test/` and reverting the `.slnx` entry leaves the production code unchanged.
- README change is documentation-only; revert independently.

---

## Out-of-Scope Suggestions (Other Layers)

These are NOT part of this change but are recommended as future work:

### API.Domain
- `CarModel` constructor: validate `ArgumentNullException` on null name/brand
- `NotFoundException` message format tests

### API.Application
- `Dispatcher.Send` resolves the correct handler from DI (dispatcher unit test)
- Handler registration: verify all handlers are registered in DI container

### API.Infrastructure
- Repository query tests: `GetAllByBrandIdAsync` returns correct models for a brand ID (requires real DB or SQLite)
- `CarBrandRepository.GetByIdAsync` returns null for missing ID

---

## Success Criteria

- [ ] `dotnet test test/testAPI` runs without errors
- [ ] All 7 unit tests pass
- [ ] All 9 integration tests pass (requires Docker)
- [ ] `GET /carBrand?id=9999` returns 404 (validates handler fix)
- [ ] `GET /carBrand?id=abc` returns 400 (model binding, no handler change needed)
- [ ] README updated with test instructions in Spanish
- [ ] Test project is discoverable via `dotnet test` at solution level

---

## Dependencies

- Docker must be running for integration tests (Testcontainers SQL Server)
- `src/API.Application` and `src/API.Domain` referenced from test project

---

## Next Phase

→ **sdd-spec** — write delta specs for `test-api-unit`, `test-api-integration`, and `get-brand-by-id-handler`
