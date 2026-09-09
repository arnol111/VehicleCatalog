# Exploration: VehicleCatalog-API-Persistence

> **Change**: VehicleCatalog-API-Persistence
> **Date**: 2026-09-09
> **Artifact store**: openspec

---

## Current State

### Layer: API.Domain (`src/API.Domain`)

| File | Status | Notes |
|------|--------|-------|
| `Entities/CarBrand.cs` | ✅ Implemented | `IdCarBrand` (int PK), `Brand` (string). Private parameterless ctor (EF-compatible). |
| `Entities/CarModel.cs` | ✅ Implemented | `IdCarModel`, `IdCarBrand` (FK), `Model`, `Year`. Guard clauses in ctor. Private parameterless ctor present. |
| `Interfaces/IUnitOfWork.cs` | ✅ Defined | Exposes `ICarBrandRepository`, `ICarModelRepository`, `Task<int> SaveChangesAsync()`. |
| `Interfaces/ICarBrandRepository.cs` | ⚠️ Empty stub | Interface declared but has **no members**. |
| `Interfaces/ICarModelRepository.cs` | ⚠️ Empty stub | Interface declared but has **no members**. |

**No NuGet packages** referenced in `API.Domain.csproj` — correct, domain must stay framework-free.

---

### Layer: API.Infrastructure (`src/API.Infrastructure`)

| File | Status | Notes |
|------|--------|-------|
| `Class1.cs` | ❌ Placeholder | Default generated stub — the entire layer is empty. |
| `API.Infrastructure.csproj` | ❌ No packages, no references | Neither EF Core packages nor a `ProjectReference` to `API.Domain` exist. |

**EF Core is NOT referenced anywhere in the solution.**

---

### Layer: API.Application (`API.Application`)

| File | Status | Notes |
|------|--------|-------|
| `Class1.cs` | ❌ Placeholder | Default generated stub — layer is empty. |
| `API.Application.csproj` | ❌ No packages, no project references | No dependency on Domain or Infrastructure declared. |

---

### Layer: API (`src/API`)

| File | Status | Notes |
|------|--------|-------|
| `Program.cs` | ⚠️ Minimal | Default template — no DI registrations for DbContext, repositories, or UoW. |
| `Controllers/CarBrandController.cs` | ⚠️ MVC stub | Inherits `Controller` (MVC), returns `View()` — wrong base class for a Web API. |
| `Controllers/CarModelController.cs` | ⚠️ MVC stub | Same issue as above. |
| `appsettings.json` | ❌ No connection string | Only Logging and AllowedHosts present. |
| `appsettings.Development.json` | ❌ No connection string | Same. |
| `API.csproj` | Only `Microsoft.AspNetCore.OpenApi 10.0.12` | No project references to Domain, Application, or Infrastructure. |

---

## Gaps

- **EF Core packages missing** — `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, and `Microsoft.EntityFrameworkCore.Tools` must be added to `API.Infrastructure.csproj`; `Microsoft.EntityFrameworkCore.Design` must be added to `src/API.csproj` (required for `dotnet ef` CLI).
- **DbContext absent** — No `VehicleCatalogDbContext` (or equivalent) exists anywhere in the solution.
- **Entity type configurations missing** — No `IEntityTypeConfiguration<CarBrand>` or `IEntityTypeConfiguration<CarModel>` exist; EF Core column mappings, PK conventions, FK navigation, and table names are not declared.
- **Repository interfaces have no members** — `ICarBrandRepository` and `ICarModelRepository` are empty; CRUD method signatures (`GetAllAsync`, `GetByIdAsync`, `AddAsync`, `Update`, `Delete`) are not defined.
- **No repository implementations** — No `CarBrandRepository` or `CarModelRepository` classes exist in Infrastructure.
- **UnitOfWork implementation absent** — `IUnitOfWork` is defined in Domain but there is no `UnitOfWork` class in Infrastructure that wraps the DbContext.
- **No project references wired** — `API.Infrastructure.csproj` does not reference `API.Domain`; `src/API.csproj` does not reference Infrastructure or Application; `API.Application.csproj` has no references.
- **Connection string not configured** — `appsettings.json` and `appsettings.Development.json` have no `ConnectionStrings` section.
- **DI registrations missing** — `Program.cs` does not register `DbContext`, repositories, or `IUnitOfWork`.
- **Controllers use wrong base class** — Both controllers inherit `Controller` (MVC) instead of `ControllerBase` (API); they return `View()` instead of `IActionResult` with HTTP results. *(Out of scope for persistence layer, but a blocker for end-to-end wiring.)*
- **No migrations** — No `Migrations/` folder exists anywhere.
- **`Class1.cs` stubs** — Infrastructure and Application layers both contain only the default placeholder class.

---

## Risks

- **`CarModel` uses `int IdCarBrand` without a navigation property** — EF Core can map a shadow FK, but without a `CarBrand` navigation property there is no referential integrity enforcement at the ORM level. Cascade delete behavior must be configured explicitly.
- **`CarModel` validation strings are in Spanish** — `ArgumentNullException` messages are in Spanish (`"El modelo no puede ser vacio"`). Not a blocker, but inconsistent with the English artifact convention established for this project.
- **`CarBrand.Brand` is not `required` / has no max-length** — EF Core will map it as `nvarchar(max)` without an explicit constraint. Should add `[MaxLength]` or Fluent API configuration.
- **net10.0 target** — .NET 10 is a preview/RC at time of writing. EF Core 10 packages may have a different release cadence than the ASP.NET Core host. Package version alignment is mandatory.
- **`API.Application` is outside `src/`** — Inconsistent folder placement (`API.Application` lives at repo root, not under `src/`). This is a structural inconsistency that should be addressed before the solution scales.
- **No test projects** — `strict_tdd: false` confirmed in `openspec/config.yaml`. Repository implementations will be untested unless test projects are added separately.

---

## Approaches

### Option A — Full Infrastructure build-out (Recommended)

Add all EF Core packages to `API.Infrastructure`, create `VehicleCatalogDbContext` with Fluent API configurations, implement generic or specific repository classes, implement `UnitOfWork`, wire project references, add `ConnectionStrings` to appsettings, and register everything in `Program.cs`.

| Dimension | Detail |
|-----------|--------|
| **Pros** | Clean separation of concerns; Domain stays pure; migrations are fully supported; DI-friendly |
| **Cons** | More files to create from scratch |
| **Effort** | Medium |

### Option B — DbContext in API project (quick hack)

Place `DbContext` directly in `src/API`, skip Infrastructure entirely for now.

| Dimension | Detail |
|-----------|--------|
| **Pros** | Fewer files, faster to scaffold |
| **Cons** | Violates Clean Architecture; coupling API to persistence; unmaintainable at scale |
| **Effort** | Low (but technical debt is High) |

**Option A is the only acceptable path** given the Clean Architecture already established in `openspec/config.yaml`.

---

## Recommended Approach

Implement Option A in the following order:

1. **Define repository interface members** in `API.Domain` (`ICarBrandRepository`, `ICarModelRepository`) — CRUD async signatures only.
2. **Add EF Core NuGet packages** to `API.Infrastructure.csproj` and `Microsoft.EntityFrameworkCore.Design` to `src/API.csproj`.
3. **Add project references**: `API.Infrastructure` → `API.Domain`; `src/API` → `API.Infrastructure` (transitively pulls Domain).
4. **Create `VehicleCatalogDbContext`** in `API.Infrastructure/Persistence/` with `DbSet<CarBrand>` and `DbSet<CarModel>`, plus Fluent API entity configurations.
5. **Implement repository classes** (`CarBrandRepository`, `CarModelRepository`) in `API.Infrastructure/Repositories/`.
6. **Implement `UnitOfWork`** in `API.Infrastructure/Persistence/` wrapping the DbContext.
7. **Add connection string** to `appsettings.json` and `appsettings.Development.json`.
8. **Register services** in `Program.cs` (`AddDbContext`, `AddScoped<IUnitOfWork, UnitOfWork>`, etc.).
9. **Run `dotnet ef migrations add InitialCreate`** and verify `dotnet ef database update`.
10. **Delete `Class1.cs` stubs** from Infrastructure and Application.

---

## Artifacts

| Path | Description |
|------|-------------|
| `src/API.Domain/Interfaces/ICarBrandRepository.cs` | Needs CRUD members added |
| `src/API.Domain/Interfaces/ICarModelRepository.cs` | Needs CRUD members added |
| `src/API.Infrastructure/API.Infrastructure.csproj` | Add EF Core packages + Domain project reference |
| `src/API.Infrastructure/Persistence/VehicleCatalogDbContext.cs` | New — EF Core DbContext |
| `src/API.Infrastructure/Persistence/UnitOfWork.cs` | New — IUnitOfWork implementation |
| `src/API.Infrastructure/Repositories/CarBrandRepository.cs` | New — ICarBrandRepository implementation |
| `src/API.Infrastructure/Repositories/CarModelRepository.cs` | New — ICarModelRepository implementation |
| `src/API/API.csproj` | Add EF Design package + project references |
| `src/API/appsettings.json` | Add `ConnectionStrings.DefaultConnection` |
| `src/API/appsettings.Development.json` | Add dev connection string |
| `src/API/Program.cs` | Register DbContext, UoW, repositories |
| `src/API.Infrastructure/Class1.cs` | Delete (placeholder) |
| `API.Application/Class1.cs` | Delete (placeholder) |

---

### Ready for Proposal

Yes — the codebase state is fully understood. All gaps and implementation steps are clear. The orchestrator can proceed directly to the `propose` phase.
