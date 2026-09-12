# Spec: TestProyect — VehicleCatalog Test Suite

**Change:** TestProyect
**Version:** 1.0
**Date:** 2026-09-11
**Status:** ready-for-design

---

## Overview

Delta spec covering four domains:
1. `test-api-unit` — Unit tests for CQRS query handlers
2. `test-api-integration` — HTTP integration tests via WebApplicationFactory + Testcontainers SQL Server
3. `get-brand-by-id-handler-fix` — Fix `GetByIdQueryHandler` to throw `NotFoundException`
4. `test-project-scaffold` — Project infrastructure and NuGet packages

---

## Domain 1: test-api-unit

**New domain — full spec (no existing spec to delta against)**

### Purpose

Unit tests for CQRS query handlers in `src/API.Application`. Handlers are instantiated directly; `IUnitOfWork` is mocked with NSubstitute. No HTTP stack, no DI container, no database.

---

### REQ-U-01: GetAllBrandsQueryHandler returns full list

The handler MUST return all brands provided by `IUnitOfWork.CarBrands.GetAllAsync()`, mapped to `CarBrandResponse(Id, Name)`.

#### SCEN-U-01: Returns mapped list when repo returns brands

- GIVEN `IUnitOfWork.CarBrands.GetAllAsync()` is mocked to return `[CarBrand(1,"Toyota"), CarBrand(2,"Ford")]`
- WHEN `handler.Handle(new GetAllBrandsQuery(), CancellationToken.None)` is called
- THEN the result MUST be an `IReadOnlyList<CarBrandResponse>` with 2 items
- AND item 0 MUST be `CarBrandResponse(1, "Toyota")` and item 1 MUST be `CarBrandResponse(2, "Ford")`

---

### REQ-U-02: GetByIdQueryHandler returns correct brand for valid ID

The handler MUST return a `CarBrandDTO` with matching `IdCarBrand` and `Brand` when `GetByIdAsync(id)` returns a non-null `CarBrand`.

#### SCEN-U-02: Returns brand DTO for existing ID

- GIVEN `IUnitOfWork.CarBrands.GetByIdAsync(1)` is mocked to return `CarBrand { IdCarBrand=1, Brand="Toyota" }`
- WHEN `handler.Handle(new GetByIdQuery(1), CancellationToken.None)` is called
- THEN the result MUST be a `CarBrandDTO` with `IdCarBrand == 1` and `Brand == "Toyota"`

---

### REQ-U-03: GetByIdQueryHandler throws NotFoundException for missing ID

After the handler fix, the handler MUST throw `NotFoundException` (from `src/API.Domain/Exceptions/NotFoundException.cs`) when `GetByIdAsync` returns null.

#### SCEN-U-03: Throws NotFoundException for non-existent ID

- GIVEN `IUnitOfWork.CarBrands.GetByIdAsync(999)` is mocked to return `null`
- WHEN `handler.Handle(new GetByIdQuery(999), CancellationToken.None)` is called
- THEN a `NotFoundException` MUST be thrown
- AND no `CarBrandDTO` is returned

---

### REQ-U-04: GetAllModelsQueryHandler returns all models with CarBrand populated

When `brandName` is null, the handler MUST return all models via `GetAllAsync()`. Each returned `CarModelResponse` MUST include `BrandName` sourced from `CarBrand.Brand`. The mock MUST populate the `CarBrand` navigation property on each `CarModel` instance.

#### SCEN-U-04: Returns models with brand details when no filter

- GIVEN `IUnitOfWork.CarModels.GetAllAsync()` returns `[CarModel { Id=1, Name="Corolla", Year=2022, CarBrand=CarBrand{Brand="Toyota"} }]`
- WHEN `handler.Handle(new GetAllModelsQuery(null), CancellationToken.None)` is called
- THEN the result MUST contain one `CarModelResponse` with `Id=1`, `Name="Corolla"`, `Year=2022`, `BrandName="Toyota"`

---

### REQ-U-05: GetAllModelsQueryHandler filters by exact brand name

When `brandName` is provided, the handler MUST find the matching brand via `GetAllAsync()` (OrdinalIgnoreCase), then call `GetAllByBrandIdAsync(brandId)` and return the result.

#### SCEN-U-05: Returns filtered models for exact brand name match

- GIVEN `IUnitOfWork.CarBrands.GetAllAsync()` returns `[CarBrand { IdCarBrand=1, Brand="Toyota" }]`
- AND `IUnitOfWork.CarModels.GetAllByBrandIdAsync(1)` returns `[CarModel { Id=1, Name="Corolla", Year=2022, CarBrand=CarBrand{Brand="Toyota"} }]`
- WHEN `handler.Handle(new GetAllModelsQuery("Toyota"), CancellationToken.None)` is called
- THEN the result MUST contain exactly one `CarModelResponse` with `BrandName="Toyota"`

---

### REQ-U-06: GetAllModelsQueryHandler filters case-insensitively

Brand name lookup MUST be case-insensitive using `OrdinalIgnoreCase`. `"toyota"`, `"TOYOTA"`, and `"tOyOtA"` MUST all resolve to the same brand.

#### SCEN-U-06: Filters correctly with mixed-case brand name

- GIVEN `IUnitOfWork.CarBrands.GetAllAsync()` returns `[CarBrand { IdCarBrand=1, Brand="Toyota" }]`
- AND `IUnitOfWork.CarModels.GetAllByBrandIdAsync(1)` returns 1 `CarModel` for Toyota
- WHEN `handler.Handle(new GetAllModelsQuery("tOyOtA"), CancellationToken.None)` is called
- THEN the result MUST be identical to the result of querying with `"Toyota"`

---

### REQ-U-07: GetAllModelsQueryHandler throws NotFoundException for non-existent brand

When `brandName` is provided but no brand matches, the handler MUST throw `NotFoundException`.

#### SCEN-U-07: Throws NotFoundException for unmatched brand

- GIVEN `IUnitOfWork.CarBrands.GetAllAsync()` returns `[CarBrand { Brand="Toyota" }]`
- WHEN `handler.Handle(new GetAllModelsQuery("MarcaInexistente"), CancellationToken.None)` is called
- THEN a `NotFoundException` MUST be thrown

---

## Domain 2: test-api-integration

**New domain — full spec**

### Purpose

HTTP-level integration tests using `WebApplicationFactory<Program>` with a real SQL Server via Testcontainers. Tests verify routing, status codes, response shapes, filter behavior, and error handling end-to-end.

---

### REQ-I-01: GET /api/carBrands with seeded data returns 200 and JSON array

The endpoint MUST return `200 OK` with a non-empty JSON array of brand objects when the database contains seeded brand records.

#### SCEN-I-01: Returns brand list from seeded database

- GIVEN the SQL Server container has been initialized with `EnsureCreated()` and seed data is present
- WHEN `GET /api/carBrands` is sent
- THEN the response status MUST be `200 OK`
- AND the body MUST be a JSON array where each item has `id` (int) and `name` (string) fields

---

### REQ-I-02: GET /api/carBrands with empty DB returns 200 and empty array

The endpoint MUST return `200 OK` with `[]` when no brands exist in the database.

#### SCEN-I-02: Returns empty array when no brands in DB

- GIVEN all rows have been deleted from `CarBrands` and `CarModels` tables
- WHEN `GET /api/carBrands` is sent
- THEN the response status MUST be `200 OK`
- AND the body MUST be `[]`

---

### REQ-I-03: GET /api/carModels with seeded data returns nested brand/year

The endpoint MUST return `200 OK` with a JSON array where each model object includes `id`, `name`, `year`, and `brandName`.

#### SCEN-I-03: Returns model list with nested brand name and year

- GIVEN the database has seeded `CarBrands` and `CarModels` with associated records
- WHEN `GET /api/carModels` is sent
- THEN the response status MUST be `200 OK`
- AND each item in the array MUST contain `id`, `name`, `year`, and `brandName` fields

---

### REQ-I-04: GET /api/carModels?brand=Toyota returns only Toyota models

The endpoint MUST return `200 OK` with only models whose associated brand name matches the query parameter (case-insensitive).

#### SCEN-I-04: Filters models by exact brand name

- GIVEN the database has Toyota and Ford brands each with at least one model
- WHEN `GET /api/carModels?brand=Toyota` is sent
- THEN the response status MUST be `200 OK`
- AND all returned models MUST have `brandName == "Toyota"`
- AND no Ford models MUST appear in the response

---

### REQ-I-05: GET /api/carModels?brand=tOyOtA returns same result as brand=Toyota

Brand filter lookup MUST be case-insensitive at the HTTP level. Mixed-case variants MUST return identical results.

#### SCEN-I-05: Case-insensitive filter returns same models

- GIVEN the same seeded data as SCEN-I-04
- WHEN `GET /api/carModels?brand=tOyOtA` is sent
- THEN the response status MUST be `200 OK`
- AND the response body MUST be identical in content to `GET /api/carModels?brand=Toyota`

---

### REQ-I-06: GET /api/carModels?brand=MarcaInexistente returns 200 and empty array

When the brand query parameter does not match any brand, the endpoint MUST return `200 OK` with `[]`.

#### SCEN-I-06: Returns empty array for unknown brand filter

- GIVEN the database has no brand named "MarcaInexistente"
- WHEN `GET /api/carModels?brand=MarcaInexistente` is sent
- THEN the response status MUST be `200 OK`
- AND the body MUST be `[]`

> Note: `CarModelsController` catches `NotFoundException` and returns 404; however, `GetAllModelsQueryHandler` throws `NotFoundException` when the brand is not found, so this scenario requires verifying the controller maps it to 200+[] OR that the endpoint returns 404. The spec owner MUST confirm expected behavior before implementation. Current proposal documents `200 OK, []` as expected — this conflicts with handler behavior unless the controller is updated.

---

### REQ-I-07: GET /carBrand?id=1 for existing brand returns 200 and correct object

The endpoint MUST return `200 OK` with a JSON object containing `idCarBrand` and `brand` matching the seeded record.

#### SCEN-I-07: Returns brand object for existing numeric ID

- GIVEN brand with `id=1` exists in the database
- WHEN `GET /carBrand?id=1` is sent
- THEN the response status MUST be `200 OK`
- AND the body MUST contain `idCarBrand == 1` and a non-empty `brand` string

---

### REQ-I-08: GET /carBrand?id=9999 for non-existent ID returns 404

After the handler fix, the endpoint MUST return `404 Not Found` when no brand exists for the given ID.

#### SCEN-I-08: Returns 404 for non-existent brand ID

- GIVEN no brand with `id=9999` exists in the database
- WHEN `GET /carBrand?id=9999` is sent
- THEN the response status MUST be `404 Not Found`

---

### REQ-I-09: GET /carBrand?id=abc for invalid format returns 400

Model binding MUST reject non-integer values for the `id` query parameter before the controller or handler executes.

#### SCEN-I-09: Returns 400 for non-integer id parameter

- GIVEN the application is running normally
- WHEN `GET /carBrand?id=abc` is sent
- THEN the response status MUST be `400 Bad Request`
- AND no handler logic is invoked

---

## Domain 3: get-brand-by-id-handler-fix

**New domain (behavioral contract change on existing handler)**

### Purpose

Fix `GetByIdQueryHandler` to throw `NotFoundException` instead of plain `Exception`. Aligns handler behavior with the domain exception hierarchy and makes 404 responses intentional rather than accidental.

---

### REQ-F-01: Handler throws NotFoundException when brand is not found

`GetByIdQueryHandler` MUST throw `NotFoundException` (defined in `src/API.Domain/Exceptions/NotFoundException.cs`) when `ICarBrandRepository.GetByIdAsync(id)` returns null.

#### SCEN-F-01: NotFoundException thrown for null repository result

- GIVEN `GetByIdAsync(id)` returns `null`
- WHEN `GetByIdQueryHandler.Handle` executes
- THEN `NotFoundException` MUST be thrown
- AND the thrown exception type MUST be exactly `NotFoundException`, not a base `Exception`

#### SCEN-F-02: No exception thrown for valid result

- GIVEN `GetByIdAsync(id)` returns a non-null `CarBrand`
- WHEN `GetByIdQueryHandler.Handle` executes
- THEN no exception MUST be thrown
- AND a populated `CarBrandDTO` MUST be returned

---

### REQ-F-02: CarBrandController maps NotFoundException to 404

`CarBrandController` MUST return `404 Not Found` when `NotFoundException` is thrown by the handler. The existing catch block MUST handle `NotFoundException` explicitly or as a subclass of `Exception`.

#### SCEN-F-03: Controller returns 404 for NotFoundException

- GIVEN `GetByIdQueryHandler` throws `NotFoundException`
- WHEN the controller processes the request
- THEN the HTTP response status MUST be `404 Not Found`

#### SCEN-F-04: Controller does not return 404 for unrelated exceptions

- GIVEN an unexpected `InvalidOperationException` is thrown inside the handler
- WHEN the controller processes the request
- THEN the HTTP response status MUST NOT be `404 Not Found`

> Note: Current controller catches all `Exception` and returns 404. After fix, the catch block SHOULD differentiate `NotFoundException` (→ 404) from other exceptions (→ 500). This is required for SCEN-F-04 to pass.

---

## Domain 4: test-project-scaffold

**New domain — project infrastructure**

### Purpose

Infrastructure requirements for the test project. All items are prerequisites before any tests can run.

---

### REQ-S-01: Test project targets net10.0

The test project at `test/testAPI/testAPI.csproj` MUST target `net10.0` to match the production API.

#### SCEN-S-01: Project builds on net10.0

- GIVEN `testAPI.csproj` declares `<TargetFramework>net10.0</TargetFramework>`
- WHEN `dotnet build test/testAPI` is run
- THEN the build MUST succeed without TFM mismatch errors

---

### REQ-S-02: Test project is registered in solution

`VehicleCatalog.slnx` MUST include a reference to `test/testAPI/testAPI.csproj` so `dotnet test` at solution root discovers the project.

#### SCEN-S-02: dotnet test discovers test project from solution root

- GIVEN `testAPI.csproj` is listed in `VehicleCatalog.slnx`
- WHEN `dotnet test` is run at the solution root
- THEN test discovery MUST include tests from `test/testAPI/`

---

### REQ-S-03: Required NuGet packages are referenced

The test project MUST reference: `xunit` 2.x, `xunit.runner.visualstudio` 2.x, `NSubstitute` 5.x, `Microsoft.AspNetCore.Mvc.Testing` 10.x, `Testcontainers.MsSql` 4.x, `FluentAssertions` 6.x, `coverlet.collector` 6.x.

#### SCEN-S-03: All required packages restore without conflict

- GIVEN the `.csproj` lists all required `PackageReference` entries
- WHEN `dotnet restore test/testAPI` is run
- THEN all packages MUST restore successfully with no version conflict errors

---

### REQ-S-04: Test project references production projects

The test project MUST have `ProjectReference` entries for: `src/API`, `src/API.Application`, `src/API.Domain`, `src/API.Infrastructure`.

#### SCEN-S-04: Handler and domain types are accessible from test project

- GIVEN project references are declared in `testAPI.csproj`
- WHEN test code references `GetAllBrandsQueryHandler`, `IUnitOfWork`, `NotFoundException`
- THEN the build MUST resolve these types without errors

---

### REQ-S-05: SqlServerContainer is shared across integration test classes via ICollectionFixture

A single `SqlServerContainer` instance MUST be shared across all integration test classes using xUnit's `ICollectionFixture<DatabaseFixture>`. Container startup MUST occur once per test run, not once per test class.

#### SCEN-S-05: Container starts once for all integration tests

- GIVEN `DatabaseFixture` implements `IAsyncLifetime` and starts the `MsSqlContainer`
- AND all integration test classes are decorated with `[Collection("IntegrationTests")]`
- WHEN the integration test suite runs
- THEN the SQL Server container MUST start exactly once across all test classes

---

### REQ-S-06: EnsureCreated() is used in test fixture (not migrations)

The test fixture MUST call `dbContext.Database.EnsureCreated()` to initialize the schema. `dotnet ef database update` or migration bundles MUST NOT be used in the test setup.

#### SCEN-S-06: Schema is created via EnsureCreated

- GIVEN `DatabaseFixture.InitializeAsync()` calls `EnsureCreated()` on the test `DbContext`
- WHEN integration tests run
- THEN the database schema MUST be available including all tables and seed data applied via `HasData()`

---

### REQ-S-07: Explicit DELETE clears tables before empty-DB test scenarios

Test methods that require an empty database MUST explicitly delete all rows from `CarModels` and `CarBrands` (in that order, respecting FK) before executing the request. `EnsureDeleted()` MUST NOT be used.

#### SCEN-S-07: Empty-DB test runs after explicit table cleanup

- GIVEN the fixture has populated the database via `EnsureCreated()` with seed data
- WHEN a test deletes all rows via `DELETE FROM CarModels; DELETE FROM CarBrands;`
- THEN subsequent queries to those tables MUST return zero rows
- AND other tests sharing the container MUST not be affected (cleanup is scoped to the specific test)

---

## Acceptance Criteria

| ID | Criterion | Verified By |
|----|-----------|-------------|
| AC-01 | `dotnet test test/testAPI` runs without errors | CI / local run |
| AC-02 | All 7 unit tests pass | xUnit output |
| AC-03 | All 9 integration tests pass (Docker required) | xUnit output |
| AC-04 | `GET /carBrand?id=9999` returns `404 Not Found` | SCEN-I-08 |
| AC-05 | `GET /carBrand?id=abc` returns `400 Bad Request` | SCEN-I-09 |
| AC-06 | `GetByIdQueryHandler` throws `NotFoundException`, not `Exception` | SCEN-U-03, SCEN-F-01 |
| AC-07 | `dotnet test` at solution root discovers `testAPI` project | SCEN-S-02 |
| AC-08 | SQL Server container starts once per test run | SCEN-S-05 |
| AC-09 | README updated with test instructions in Spanish | Manual review |

---

## Open Issues

| # | Issue | Impact |
|---|-------|--------|
| OI-01 | REQ-I-06 / SCEN-I-06 conflict: `GetAllModelsQueryHandler` throws `NotFoundException` for unknown brand, but proposal expects `200 OK, []`. Controller currently returns `404` for `NotFoundException`. Behavior must be resolved before implementation. | Blocks SCEN-I-06 |
| OI-02 | REQ-F-02 / SCEN-F-04: Current `CarBrandController` returns `404` for ALL exceptions. Fix scope must include differentiating `NotFoundException` (404) from other exceptions (500), or SCEN-F-04 will not pass. | Blocks SCEN-F-04 |
