# Exploration: ImplementacionEnpoints

**Change**: ImplementacionEnpoints  
**Date**: 2026-09-10  
**Artifact store**: openspec  

---

## Current State

The solution is a Clean Architecture .NET 10 Web API with four projects and a custom Mediator implementation (no MediatR library dependency). One endpoint is fully working as a reference pattern: `GET /carBrand?id={id}`. A SQL Server local database exists with seed data (10 brands, 50 models).

---

## Architecture Overview

| Project | Role |
|---|---|
| `src/API` | Presentation — Controllers, Program.cs, host configuration |
| `src/API.Application` | Application layer — Queries, Handlers, DTOs, Dispatcher contracts |
| `src/API.Domain` | Domain — Entities (`CarBrand`, `CarModel`), repository interfaces, `IUnitOfWork` |
| `src/API.Infrastructure` | Infrastructure — EF Core DbContext, repositories, migrations, Dispatcher implementation, DI extensions |

**Dependency direction**: API → Application → Domain ← Infrastructure

---

## Mediator Pattern Implementation

The project uses a **hand-rolled Mediator** (not MediatR). The contracts live in `API.Application.Dispatcher`:

- `IRequest<TResult>` — marker interface on query/command objects
- `IRequestHandler<TRequest, TResult>` — handler contract
- `IDispatcher` — `Send<TRequest, TResult>(request)` method
- `Dispatcher` (Infrastructure) — resolves handler from DI container via `GetRequiredService<IRequestHandler<TRequest, TResult>>()`

**Handlers must be manually registered in DI.** There is no assembly-scan auto-registration.

### Existing pattern — `GetById` for CarBrand

```
API.Application/
  CarBrand/
    Query/
      GetById/
        GetByIdQuery.cs          ← implements IRequest<CarBrandDTO>
        GetByIdQueryHandler.cs   ← implements IRequestHandler<GetByIdQuery, CarBrandDTO>
```

The handler receives `IUnitOfWork` via constructor injection and calls `_unitOfWork.CarBrands.GetByIdAsync(id)`. Manual mapping from entity to DTO inside the handler.

---

## Entity and Domain Models

### `CarBrand`
- `IdCarBrand` (int, PK, identity)
- `Brand` (string, maxLength 200)
- No navigation property to `CarModel` (navigation is one-way from `CarModel` side via FK)

### `CarModel`
- `IdCarModel` (int, PK, identity)
- `IdCarBrand` (int, FK → CarBrands.IdCarBrand, Restrict on delete)
- `Model` (string, maxLength 200)
- `Year` (int)
- No explicit navigation property to `CarBrand` in the entity class

**Key finding**: `CarModel` holds the `IdCarBrand` FK as a scalar but there is **no `CarBrand` navigation property** on `CarModel`. Filtering by brand name (`?Brand=""`) requires either a JOIN in the repository or resolving the brand ID first via `ICarBrandRepository`.

---

## EF Core Configuration

- `VehicleCatalogDbContext` loads configurations from assembly via `ApplyConfigurationsFromAssembly`.
- FK `CarModels.IdCarBrand → CarBrands.IdCarBrand` with `Restrict` delete.
- Index on `CarModels.IdCarBrand`.
- Seed data baked into `InitialCreate` migration (10 brands, 50 models spread across all brands).

---

## Current Program.cs / DI Configuration

- `AddInfrastructure()` registers: `VehicleCatalogDbContext`, `ICarBrandRepository`, `ICarModelRepository`, `IUnitOfWork`.
- `Program.cs` also **manually re-registers** `VehicleCatalogDbContext`, `IUnitOfWork` and `IDispatcher` (duplication with `AddInfrastructure` — `DbContext` and `IUnitOfWork` are registered twice; this is a minor defect but non-blocking for the task).
- **`IDispatcher` is registered as `Singleton`** — this is a risk because it resolves `Scoped` handlers from a singleton scope (captive dependency problem). Handlers are currently not registered at all in DI, which means any `Send()` call will throw `InvalidOperationException` at runtime.
- Swagger/OpenAPI: `AddOpenApi()` and `MapOpenApi()` are called (ASP.NET Core 10 built-in minimal OpenAPI support). No Swashbuckle/NSwag configured. The built-in endpoint serves at `/openapi/v1.json` — no Swagger UI out of the box.

---

## Affected Areas

- `src/API.Application/CarBrand/Query/GetById/GetByIdQueryHandler.cs` — reference pattern to follow
- `src/API.Application/DTOs/CarBrandDTO.cs` — existing DTO; a `CarModelDTO` is missing
- `src/API.Application/` — needs new `CarBrand/Query/GetAll/` and `CarModel/Query/GetAll/` folders
- `src/API.Infrastructure/Repositories/CarModelRepository.cs` — `GetAllAsync` exists but no filter-by-brand method
- `src/API.Domain/Interfaces/ICarModelRepository.cs` — may need `GetAllByBrandAsync` or the filter can live in the handler
- `src/API/Controllers/CarBrandController.cs` — needs a second action method `GET /carBrand` (list all)
- `src/API/Controllers/CarModelController.cs` — currently empty; needs `GET /carModel` with optional `?Brand=""` filter
- `src/API/Program.cs` — handler registrations missing; DI duplication to clean up; Swagger UI decision

---

## Gaps — What Needs to Be Created

### Application Layer (`API.Application`)

| Artifact | Status |
|---|---|
| `CarBrand/Query/GetAll/GetAllQuery.cs` | ❌ Missing |
| `CarBrand/Query/GetAll/GetAllQueryHandler.cs` | ❌ Missing |
| `CarModel/Query/GetAll/GetAllQuery.cs` | ❌ Missing |
| `CarModel/Query/GetAll/GetAllQueryHandler.cs` | ❌ Missing (supports optional brand filter) |
| `DTOs/CarModelDTO.cs` | ❌ Missing |

### Infrastructure / Domain

| Artifact | Status |
|---|---|
| Filter-by-brand in `ICarModelRepository` / `CarModelRepository` | ⚠️ Needs decision (see Approaches) |

### Presentation Layer (`API`)

| Artifact | Status |
|---|---|
| `CarBrandController` — `GetAll` action | ❌ Missing |
| `CarModelController` — `GetAll` action with optional `?Brand` param | ❌ Missing |
| Handler registrations in `Program.cs` | ❌ Missing (will cause runtime crash) |
| Swagger UI | ⚠️ Decision needed |

---

## Approaches

### Approach 1 — Filter-by-brand: Resolve in Handler (no repository change)

Handler calls `IUnitOfWork.CarBrands` to resolve brand ID from name, then filters models client-side or calls `GetAllAsync` and filters in-memory.

- **Pros**: No domain interface change needed; simple; consistent with existing handler pattern
- **Cons**: Loads all models into memory before filtering; not scalable for large datasets
- **Effort**: Low

### Approach 2 — Filter-by-brand: Add method to repository interface

Add `GetAllByBrandIdAsync(int brandId)` to `ICarModelRepository` and implement with EF Core `Where()`.

- **Pros**: DB-side filter; scales properly; clean separation
- **Cons**: Requires touching domain interface + repository implementation + handler
- **Effort**: Low-Medium

### Approach 3 — Swagger UI: Add Swashbuckle

Add `Swashbuckle.AspNetCore` NuGet package and configure `AddSwaggerGen` + `UseSwaggerUI`.

- **Pros**: Interactive UI at `/swagger`; widely known; easy to use
- **Cons**: Extra package dependency; .NET 10 built-in OpenAPI already provides the JSON spec
- **Effort**: Low

### Approach 4 — Swagger UI: Use Scalar (built-in .NET 10 alternative)

Add `Scalar.AspNetCore` to render the existing `/openapi/v1.json` with a modern UI.

- **Pros**: Lightweight; designed for .NET 10 minimal OpenAPI; no schema duplication
- **Cons**: Less familiar than Swagger UI for some teams
- **Effort**: Low

---

## Recommendation

1. **Filter strategy**: Use **Approach 2** — add `GetAllByBrandIdAsync` to the repository. The filter is a natural query concern and should live at the data access layer, not in memory. The effort is minimal.

2. **Swagger UI**: Use **Approach 3 (Swashbuckle)** if the team needs the classic `/swagger` UI experience. If the project is greenfield .NET 10, **Approach 4 (Scalar)** is the cleaner choice. This is a team preference decision — both work identically for the goal.

3. **IDispatcher singleton**: Fix the captive dependency by registering `IDispatcher` as `Scoped` (or using `IServiceScopeFactory` internally). This is a correctness issue, not just style.

4. **Program.cs duplication**: Remove the manual re-registration of `DbContext` and `IUnitOfWork` since `AddInfrastructure()` already handles them.

---

## Risks

- **Handler DI registrations missing**: The existing `GetByIdQueryHandler` is not registered in DI. This means `GET /carBrand?id=1` currently throws at runtime. All new handlers must be registered, and the existing one needs to be fixed.
- **IDispatcher as Singleton**: Resolving Scoped services (handlers, repositories, DbContext) from a Singleton dispatcher will cause `InvalidOperationException` in production. Must change to Scoped or use `IServiceScopeFactory`.
- **No `CarBrand` navigation on `CarModel`**: Filtering models by brand name requires an extra lookup or a JOIN. Without a navigation property, EF cannot eager-load the brand automatically.
- **No tests**: The project has no test projects. Changes cannot be verified automatically. Manual testing via HTTP file or Swagger is the only option.
- **CarModel.IdCarBrand has no domain validation**: The entity constructor validates `idCarBrand > 0` but not that the brand actually exists — referential integrity is only enforced at DB level.

---

## Ready for Proposal

**Yes.** The architecture is clear, the reference pattern is fully readable, and the gaps are well-defined. The only decision needed before proposing is:

> **Swagger UI preference**: Swashbuckle (classic `/swagger`) or Scalar (modern .NET 10 UI)?

The orchestrator should ask the user this question before moving to the proposal phase.
