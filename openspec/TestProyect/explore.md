# Exploration: TestProyect — VehicleCatalog Test Suite

**Status:** completed  
**Change:** TestProyect  
**Date:** 2026-09-11

---

## Executive Summary

The VehicleCatalog API is a .NET 10 Clean Architecture project with a custom hand-rolled dispatcher (no MediatR). Controllers depend solely on `IDispatcher`, and handlers depend solely on `IUnitOfWork`, making both layers trivially mockable. Docker 29 is available on the machine, so Testcontainers with a PostgreSQL container is viable for integration tests — but **the production DB is SQL Server**, which must be swapped to either `npgsql` or `Microsoft.EntityFrameworkCore.InMemory` / SQLite for the integration test environment.

---

## Findings

### 1. Solution Structure

```
VehicleCatalog/
├── src/
│   ├── API/                     ← Entry point, controllers, Program.cs
│   ├── API.Application/         ← Queries, handlers, DTOs, IDispatcher
│   ├── API.Domain/              ← Entities, interfaces, exceptions
│   └── API.Infrastructure/      ← EF Core, repositories, UoW, Dispatcher impl
└── openspec/
```

No `test/` folder exists yet — must be created.

### 2. Controllers

| File | Route | Method | Dispatcher Call |
|------|-------|--------|-----------------|
| `CarBrandsController.cs` | `GET /api/carBrands` | `GetAll()` | `IDispatcher.Send<GetAllBrandsQuery, IReadOnlyList<CarBrandResponse>>` |
| `CarBrandController.cs` | `GET /carBrand?id={id}` | `GetById([FromQuery] int id)` | `IDispatcher.Send<GetByIdQuery, CarBrandDTO>` |
| `CarModelsController.cs` | `GET /api/carModels[?brand=]` | `GetAll([FromQuery] string? brand)` | `IDispatcher.Send<GetAllModelsQuery, IReadOnlyList<CarModelResponse>>` |
| `CarModelController.cs` | `GET /CarModel` | *(empty — no actions)* | — |

**Notable behaviors:**
- `CarBrandsController` catches all exceptions and returns `500`.
- `CarBrandController` catches all exceptions and returns `404` (indiscriminate — even non-"not found" errors become 404).
- `CarModelsController` explicitly catches `NotFoundException` → 404, and catches general `Exception` → 500. Also returns 400 if `brand` key is present but whitespace.
- `CarModelController` is an empty shell — no tests needed.

### 3. Application Handlers

| Handler | Input | Behavior |
|---------|-------|----------|
| `GetAllBrandsQueryHandler` | `GetAllBrandsQuery` | Calls `IUnitOfWork.CarBrands.GetAllAsync()`, maps to `CarBrandResponse(id, name)` |
| `GetByIdQueryHandler` | `GetByIdQuery(int id)` | Calls `IUnitOfWork.CarBrands.GetByIdAsync(id)`, throws `Exception` (not `NotFoundException`) if null |
| `GetAllModelsQueryHandler` | `GetAllModelsQuery(string? brandName)` | If `brandName == null` → `GetAllAsync()`; else → find brand (OrdinalIgnoreCase) → `GetAllByBrandIdAsync()`. Throws `NotFoundException` if brand not found |

**Critical discovery:** `GetByIdQueryHandler` throws a plain `Exception("La marca de carro no existe")`, **not** `NotFoundException`. The controller catches `Exception` and returns 404. This means `GET /carBrand?id=abc` (invalid int) will result in model binding failure (400 Bad Request from ASP.NET Core) before the controller even runs — correct behavior by accident.

### 4. Domain Interfaces

```csharp
// ICarBrandRepository
Task<IEnumerable<CarBrand>> GetAllAsync(CancellationToken = default);
Task<CarBrand?> GetByIdAsync(int id, CancellationToken = default);
Task AddAsync(CarBrand entity, CancellationToken = default);
void Update(CarBrand entity);
void Delete(CarBrand entity);

// ICarModelRepository
Task<IEnumerable<CarModel>> GetAllAsync(CancellationToken = default);
Task<IReadOnlyList<CarModel>> GetAllByBrandIdAsync(int brandId, CancellationToken = default);
Task<CarModel?> GetByIdAsync(int id, CancellationToken = default);
...

// IUnitOfWork
ICarBrandRepository CarBrands { get; }
ICarModelRepository CarModels { get; }
Task<int> SaveChangesAsync(CancellationToken = default);
```

### 5. DTOs / Response Types

```csharp
record CarBrandResponse(int Id, string Name);
record CarModelResponse(int Id, string Name, int Year, string BrandName);
class CarBrandDTO { int IdCarBrand; string Brand; }
```

Note: `CarBrandResponse` uses `Id`/`Name` while `CarBrandDTO` uses `IdCarBrand`/`Brand` — different shapes for the two brand endpoints.

### 6. Infrastructure / DI Wiring

- `Program.cs` calls `builder.Services.AddInfrastructure(builder.Configuration)`.
- `ServiceCollectionExtensions.AddInfrastructure` registers: `DbContext` (SQL Server), repositories, `IUnitOfWork`, `IDispatcher`, and all three handlers as `Transient`.
- `Program` class is declared `public partial` — **WebApplicationFactory is directly usable** without any adapter.

### 7. EF Core / Database

- `VehicleCatalogDbContext` uses `DbContextOptions<VehicleCatalogDbContext>` — standard constructor, works with `UseInMemoryDatabase` or `UseNpgsql` override.
- Seed data is defined in `IEntityTypeConfiguration` via `HasData()` — will apply automatically on `EnsureCreated()` or migrations.
- Currently wired to SQL Server (`UseSqlServer`).

### 8. Dispatcher Implementation

Custom, reflection-free dispatcher that resolves `IRequestHandler<TRequest, TResult>` from `IServiceProvider`. Fully mockable via `IDispatcher` interface for controller-level unit tests.

### 9. Docker Availability

Docker 29.7.2 is installed and available. Testcontainers can spin up containers.

---

## Test Strategy

### Unit Tests (Handler Level)

**Framework:** xUnit (standard for .NET; integrates with `dotnet test`)  
**Mock library:** NSubstitute (cleaner syntax than Moq for `async` + interfaces; no `.Object` noise)  
**Alternative:** Moq — also valid, slightly more verbose  

**What to mock:** `IUnitOfWork` (and its properties `CarBrands`, `CarModels`)  
**Do NOT mock:** `IDispatcher` at unit test level — test handlers directly, bypassing the dispatcher entirely.

**Unit test structure:**
```
test/testAPI/
├── Unit/
│   ├── CarBrand/
│   │   ├── GetAllBrandsQueryHandlerTests.cs
│   │   └── GetByIdQueryHandlerTests.cs
│   └── CarModel/
│       └── GetAllModelsQueryHandlerTests.cs
└── Integration/
    └── ...
```

**Test cases mapped to handlers:**

| Test name | Handler | Mock setup |
|-----------|---------|------------|
| `getAllBrands_ShouldReturnList` | `GetAllBrandsQueryHandler` | `CarBrands.GetAllAsync()` returns 2 brands |
| `getBrandById_WithValidId_ShouldReturnBrand` | `GetByIdQueryHandler` | `CarBrands.GetByIdAsync(1)` returns brand |
| `getBrandById_WithInvalidId_ShouldThrowException` | `GetByIdQueryHandler` | `CarBrands.GetByIdAsync(999)` returns null → expect `Exception` thrown |
| `getAllModels_ShouldReturnModelsWithDetails` | `GetAllModelsQueryHandler` | `CarModels.GetAllAsync()` returns models with `CarBrand` nav prop populated |
| `getModelsByBrand_WithExactName_ShouldFilterCorrectly` | `GetAllModelsQueryHandler` | `CarBrands.GetAllAsync()` returns Toyota, `CarModels.GetAllByBrandIdAsync(1)` returns models |
| `getModelsByBrand_CaseInsensitive_ShouldReturnFilteredModels` | `GetAllModelsQueryHandler` | Same as above, query with `"tOyOtA"` |
| `getModelsByBrand_NonExistentBrand_ShouldReturnEmptyList` | `GetAllModelsQueryHandler` | `CarBrands.GetAllAsync()` returns list without match → `NotFoundException` thrown |

> **Note:** The last test should assert `NotFoundException` is thrown, not empty list. The handler throws when brand is not found — behavior is exception, not empty result.

### Integration Tests (HTTP Level)

**Approach:** `WebApplicationFactory<Program>` + Testcontainers  
**DB for integration:** Two options:

| Option | Pros | Cons |
|--------|------|------|
| **Testcontainers + PostgreSQL** | Real SQL dialect, closest to prod, exercises EF Core SQL generation | Requires `Npgsql.EntityFrameworkCore.PostgreSQL` added to Infrastructure or test project override; slower startup |
| **EF Core InMemory** | Fast, no Docker dependency at CI | Does NOT enforce FK constraints; `HasData` seed applies via `EnsureCreated()` |
| **SQLite** | Fast, real SQL, FK support optional | `UseIdentityColumn()` in configs is SQL Server-specific — needs config swap |

**Recommended:** Testcontainers + PostgreSQL via `WebApplicationFactory` with `ConfigureTestServices` override to replace `DbContext` registration. Docker is confirmed available.

**Integration test structure:**
```
test/testAPI/
└── Integration/
    ├── CarBrandsControllerTests.cs
    ├── CarBrandControllerTests.cs
    └── CarModelsControllerTests.cs
```

**Required packages for test project:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.*" />
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="NSubstitute" Version="5.*" />
<PackageReference Include="Testcontainers.PostgreSql" Version="4.*" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />  <!-- or 10 when available -->
<PackageReference Include="FluentAssertions" Version="6.*" />  <!-- optional but recommended -->
```

**WebApplicationFactory override pattern:**
```csharp
// CustomWebApplicationFactory.cs
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.ConfigureTestServices(services =>
    {
        // Remove SQL Server DbContext registration
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<VehicleCatalogDbContext>));
        if (descriptor != null) services.Remove(descriptor);

        // Add PostgreSQL via Testcontainers connection string
        services.AddDbContext<VehicleCatalogDbContext>(options =>
            options.UseNpgsql(_postgresContainer.GetConnectionString()));
    });
}
```

---

## Risks

1. **`UseIdentityColumn()` is SQL Server-specific.** If using PostgreSQL for integration tests, EF Core will fail on migration unless `UseIdentityColumn()` is replaced with `UseGeneratedAlwaysAsIdentity()` or `UseIdentityByDefaultColumn()` for Npgsql. Workaround: use `EnsureCreated()` in test setup (skips migration, uses EF model directly) OR override configurations in the test project.

2. **`GetByIdQueryHandler` throws `Exception`, not `NotFoundException`.** This is a code smell and a testability risk. The controller returns 404 for any exception — a handler crashing for another reason would look like "not found". Tests must reflect actual behavior (expect `Exception`, not `NotFoundException`).

3. **Seed data via `HasData`.** Seed data is embedded in EF configurations. In integration tests, calling `EnsureCreated()` will apply the seed. Tests that need an "empty DB" scenario must either delete seeded rows after creation or use a separate empty context without configurations.

4. **`CarModelResponse` needs `CarBrand` navigation property populated.** `GetAllModelsQueryHandler` accesses `m.CarBrand!.Brand` — if the repository doesn't include the nav prop via `.Include()`, it will be null and throw a `NullReferenceException`. Unit tests must populate the `CarBrand` property on mock-returned `CarModel` instances.

5. **No test project in the solution yet.** The `.slnx` must be updated to include the new test project, or tests will not be discovered by `dotnet test` at solution level.

6. **EF Core 10 + Npgsql compatibility.** EF Core 10 is very new (Oct 2025). Npgsql provider may still be at version 9.x for EF Core 9. Verify latest Npgsql version that supports EF Core 10 before selecting packages.

---

## Artifacts Read

| File | Purpose |
|------|---------|
| `VehicleCatalog.slnx` | Solution structure |
| `src/API/Program.cs` | DI wiring, middleware, `partial class Program` |
| `src/API/API.csproj` | TFM net10.0, package refs |
| `src/API/Controllers/CarBrandsController.cs` | GET /api/carBrands |
| `src/API/Controllers/CarBrandController.cs` | GET /carBrand?id= |
| `src/API/Controllers/CarModelsController.cs` | GET /api/carModels |
| `src/API/Controllers/CarModelController.cs` | Empty controller |
| `src/API.Application/CarBrand/Query/GetAll/GetAllBrandsQuery.cs` | Query type |
| `src/API.Application/CarBrand/Query/GetAll/GetAllBrandsQueryHandler.cs` | Handler logic |
| `src/API.Application/CarBrand/Query/GetById/GetByIdQuery.cs` | Query type |
| `src/API.Application/CarBrand/Query/GetById/GetByIdQueryHandler.cs` | Handler logic |
| `src/API.Application/CarModel/Query/GetAll/GetAllModelsQuery.cs` | Query type |
| `src/API.Application/CarModel/Query/GetAll/GetAllModelsQueryHandler.cs` | Handler logic |
| `src/API.Application/Dispatcher/IDispatcher.cs` | Custom dispatcher interface |
| `src/API.Application/Dispatcher/IRequestHandler.cs` | Handler interface |
| `src/API.Application/DTOs/CarBrandDTO.cs` | DTO for single brand |
| `src/API.Application/DTOs/CarBrandResponse.cs` | Record for list response |
| `src/API.Application/DTOs/CarModelResponse.cs` | Record for model response |
| `src/API.Domain/Entities/CarBrand.cs` | Domain entity |
| `src/API.Domain/Entities/CarModel.cs` | Domain entity |
| `src/API.Domain/Interfaces/ICarBrandRepository.cs` | Repository contract |
| `src/API.Domain/Interfaces/ICarModelRepository.cs` | Repository contract |
| `src/API.Domain/Interfaces/IUnitOfWork.cs` | UoW contract |
| `src/API.Domain/Exceptions/NotFoundException.cs` | Custom exception |
| `src/API.Infrastructure/ServiceCollectionExtensions.cs` | DI registration |
| `src/API.Infrastructure/Dispatcher/Dispatcher.cs` | IDispatcher implementation |
| `src/API.Infrastructure/Persistence/VehicleCatalogDbContext.cs` | DbContext |
| `src/API.Infrastructure/Persistence/Configurations/CarBrandConfiguration.cs` | EF config + seed |
| `src/API.Infrastructure/Persistence/Configurations/CarModelConfiguration.cs` | EF config + seed |
| `src/API.Infrastructure/API.Infrastructure.csproj` | Infrastructure packages |

---

## Next Recommended Phase

**→ sdd-propose**

The proposal should define:
1. Test project layout (`test/testAPI/testAPI.csproj`)
2. Package selection (xUnit + NSubstitute + Testcontainers.PostgreSql + FluentAssertions)
3. Decision on DB provider for integration tests (recommend PostgreSQL Testcontainers with `EnsureCreated()` to avoid migration SQL Server-isms)
4. Fix/acknowledge the `GetByIdQueryHandler` throwing `Exception` vs `NotFoundException` — test should reflect real behavior
5. Strategy for "empty DB" integration test scenario
6. Solution file update to include test project

**Suggested but out of scope:**
- Domain layer tests: `CarModel` constructor validation (throws `ArgumentNullException` on invalid args)
- Infrastructure layer tests: repository queries against real DB (already covered by integration tests via WebApplicationFactory)
- Dispatcher tests: `Dispatcher.Send` resolves handler from DI

---

## Skill Resolution

`none` — No additional skills were needed for this exploration.
