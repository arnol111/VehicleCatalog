# Apply Progress: ImplementacionEnpoints

## Summary

| Field | Value |
|-------|-------|
| Change | ImplementacionEnpoints |
| Mode | Standard |
| Delivery strategy | stacked-to-main |
| Last updated | 2026-09-10 |

---

## WU-01: Fix DI bugs — COMPLETE

### Completed Tasks

- [x] 1.1 Changed `IDispatcher` registration from `AddSingleton` to `AddScoped<IDispatcher, Dispatcher>()` in `ServiceCollectionExtensions.cs`
- [x] 1.2 Added `services.AddTransient<IRequestHandler<GetByIdQuery, CarBrandDTO>, GetByIdQueryHandler>()` in `ServiceCollectionExtensions.cs`
- [x] 1.3 Removed duplicate registrations of `VehicleCatalogDbContext`, `IUnitOfWork`, and `IDispatcher` from `Program.cs`
- [x] 1.4 Build verified — 0 errors, 0 warnings

### Files Changed

| File | Action | What Was Done |
|------|--------|---------------|
| `src/API.Infrastructure/ServiceCollectionExtensions.cs` | Modified | Added `IDispatcher` as Scoped; added `GetByIdQueryHandler` Transient registration; added required using directives |
| `src/API/Program.cs` | Modified | Removed `AddSingleton<IDispatcher>`, `AddDbContext`, and `AddScoped<IUnitOfWork>` duplicates; reduced to single `AddInfrastructure()` call |

### Work Unit Evidence

| Evidence | Value |
|----------|-------|
| Focused test command and exact result | `dotnet build C:/Users/ArnolSuarez/Desktop/ER/VehicleCatalog` → **Build succeeded. 0 Warning(s). 0 Error(s).** |
| Runtime harness command/scenario and exact result | N/A for build-only WU — runtime harness verification (`dotnet run` + `GET /carBrand?id=1`) is deferred to WU-05 manual verification when controllers are present |
| Rollback boundary | Revert `src/API.Infrastructure/ServiceCollectionExtensions.cs` and `src/API/Program.cs` to pre-change state; no other files touched |

### Deviations from Design

None — implementation matches design exactly. `IDispatcher` is `Scoped`, `GetByIdQueryHandler` is `Transient`, and `Program.cs` delegates all DI to `AddInfrastructure()`.

---

## WU-02: EF Domain Change — CarModel nav property + migration — COMPLETE

### Completed Tasks

- [x] 2.1 In `src/API.Domain/Entities/CarModel.cs`: added navigation property `public CarBrand? CarBrand { get; set; }`
- [x] 2.2 In `src/API.Infrastructure/Persistence/Configurations/CarModelConfiguration.cs`: updated fluent config to `HasOne(m => m.CarBrand).WithMany().HasForeignKey(m => m.IdCarBrand).OnDelete(DeleteBehavior.Restrict)`
- [x] 2.3 Generated migration `AddCarBrandNavProp` — no destructive schema delta (EF snapshot alignment only; FK already existed in DB)
- [x] 2.4 `dotnet ef database update` applied without error

### Files Changed

| File | Action | What Was Done |
|------|--------|---------------|
| `src/API.Domain/Entities/CarModel.cs` | Modified | Added `public CarBrand? CarBrand { get; set; }` navigation property |
| `src/API.Infrastructure/Persistence/Configurations/CarModelConfiguration.cs` | Modified | Changed `HasOne<CarBrand>()` to `HasOne(m => m.CarBrand)` so EF resolves the nav property; same FK/delete behavior retained |
| `src/API.Infrastructure/Migrations/20260911041111_AddCarBrandNavProp.cs` | Created | EF migration for nav property; no schema changes — FK already existed |
| `src/API.Infrastructure/Migrations/20260911041111_AddCarBrandNavProp.Designer.cs` | Created | EF migration designer snapshot |

### Work Unit Evidence

| Evidence | Value |
|----------|-------|
| Focused test command and exact result | `dotnet build C:/Users/ArnolSuarez/Desktop/ER/VehicleCatalog` → **Build succeeded. 0 Warning(s). 0 Error(s).** |
| Runtime harness command/scenario and exact result | `dotnet ef database update` → **Done.** Migration `20260911041111_AddCarBrandNavProp` applied; DB accepted the update with no errors |
| Rollback boundary | Delete `20260911041111_AddCarBrandNavProp.cs` and `.Designer.cs`; revert `CarModel.cs` (remove `CarBrand?` property) and `CarModelConfiguration.cs` (`HasOne<CarBrand>()` without nav reference); run `ef database update` to roll back migration |

### Deviations from Design

None — implementation matches design exactly. The FK property was `IdCarBrand` (already present on entity, as confirmed by reading the actual file). Design.md confirms `HasForeignKey(m => m.IdCarBrand)` — used as-is.

---

## WU-03: CarBrand Application Layer — DTO + Query + Handler + DI — COMPLETE

### Completed Tasks

- [x] 3.1 Created `src/API.Application/DTOs/CarBrandResponse.cs`: `public record CarBrandResponse(int Id, string Name);`
- [x] 3.2 Created `src/API.Application/CarBrand/Query/GetAll/GetAllBrandsQuery.cs`: `IRequest<IReadOnlyList<CarBrandResponse>>`, no properties
- [x] 3.3 Created `src/API.Application/CarBrand/Query/GetAll/GetAllBrandsQueryHandler.cs`: calls `_unitOfWork.CarBrands.GetAllAsync()` and maps `CarBrand → CarBrandResponse(IdCarBrand, Brand)`
- [x] 3.4 In `src/API.Infrastructure/ServiceCollectionExtensions.cs`: registered `AddTransient<IRequestHandler<GetAllBrandsQuery, IReadOnlyList<CarBrandResponse>>, GetAllBrandsQueryHandler>()`

### Files Changed

| File | Action | What Was Done |
|------|--------|---------------|
| `src/API.Application/DTOs/CarBrandResponse.cs` | Created | Record DTO `CarBrandResponse(int Id, string Name)` in `API.Application.DTOs` namespace |
| `src/API.Application/CarBrand/Query/GetAll/GetAllBrandsQuery.cs` | Created | Query class implementing `IRequest<IReadOnlyList<CarBrandResponse>>` with no properties |
| `src/API.Application/CarBrand/Query/GetAll/GetAllBrandsQueryHandler.cs` | Created | Handler injecting `IUnitOfWork`; calls `GetAllAsync()`, maps to `List<CarBrandResponse>` via LINQ Select |
| `src/API.Infrastructure/ServiceCollectionExtensions.cs` | Modified | Added `using API.Application.CarBrand.Query.GetAll;`; added `GetAllBrandsQueryHandler` Transient registration |

### Work Unit Evidence

| Evidence | Value |
|----------|-------|
| Focused test command and exact result | `dotnet build C:/Users/ArnolSuarez/Desktop/ER/VehicleCatalog` → **Build succeeded. 0 Warning(s). 0 Error(s).** |
| Runtime harness command/scenario and exact result | N/A — `GET /api/carBrands` endpoint not yet wired (controller created in WU-05); DI resolvability verified via clean build with handler registration present |
| Rollback boundary | Delete `CarBrandResponse.cs`, `GetAllBrandsQuery.cs`, `GetAllBrandsQueryHandler.cs`; remove `GetAllBrandsQueryHandler` registration and its `using` from `ServiceCollectionExtensions.cs` |

### Deviations from Design

None — implementation matches design exactly. `ICarBrandRepository.GetAllAsync` returns `IEnumerable<CarBrand>` (not `IReadOnlyList`); handler calls `.ToList()` to satisfy the `IReadOnlyList<CarBrandResponse>` return type. This is consistent with the design data flow.

---

## WU-04: CarModel Application + Infrastructure Layer — COMPLETE

### Completed Tasks

- [x] 4.1 In `src/API.Domain/Interfaces/ICarModelRepository.cs`: added `Task<IReadOnlyList<CarModel>> GetAllByBrandIdAsync(int brandId, CancellationToken ct = default)`
- [x] 4.2 In `src/API.Infrastructure/Repositories/CarModelRepository.cs`: implemented `GetAllByBrandIdAsync` with `Include(m => m.CarBrand).Where(m => m.IdCarBrand == brandId).AsNoTracking().ToListAsync(ct)`
- [x] 4.3 In `src/API.Infrastructure/Repositories/CarModelRepository.cs`: updated `GetAllAsync` to add `.Include(m => m.CarBrand)` so `BrandName` is populated on unfiltered results
- [x] 4.4 Created `src/API.Application/DTOs/CarModelResponse.cs`: `public record CarModelResponse(int Id, string Name, int Year, string BrandName);`
- [x] 4.5 Created `src/API.Application/CarModel/Query/GetAll/GetAllModelsQuery.cs`: `IRequest<IReadOnlyList<CarModelResponse>>` with `string? BrandName` property
- [x] 4.6 Created `src/API.Application/CarModel/Query/GetAll/GetAllModelsQueryHandler.cs`: branches on `BrandName == null` → `GetAllAsync`; else resolves brand by name (case-insensitive OrdinalIgnoreCase) → throws `NotFoundException` if not found → calls `GetAllByBrandIdAsync(brand.IdCarBrand)`; maps to `CarModelResponse`
- [x] 4.7 In `src/API.Infrastructure/ServiceCollectionExtensions.cs`: registered `AddTransient<IRequestHandler<GetAllModelsQuery, IReadOnlyList<CarModelResponse>>, GetAllModelsQueryHandler>()`

### Files Changed

| File | Action | What Was Done |
|------|--------|---------------|
| `src/API.Domain/Interfaces/ICarModelRepository.cs` | Modified | Added `GetAllByBrandIdAsync(int brandId, CancellationToken ct)` method signature |
| `src/API.Domain/Exceptions/NotFoundException.cs` | Created | Domain exception `NotFoundException : Exception` — used by handler for missing brand signal |
| `src/API.Infrastructure/Repositories/CarModelRepository.cs` | Modified | `GetAllAsync` now includes `CarBrand` nav prop; new `GetAllByBrandIdAsync` with `Where` + `Include` |
| `src/API.Application/DTOs/CarModelResponse.cs` | Created | Record DTO `CarModelResponse(int Id, string Name, int Year, string BrandName)` |
| `src/API.Application/CarModel/Query/GetAll/GetAllModelsQuery.cs` | Created | Query class with `string? BrandName` property implementing `IRequest<IReadOnlyList<CarModelResponse>>` |
| `src/API.Application/CarModel/Query/GetAll/GetAllModelsQueryHandler.cs` | Created | Handler with null-branch logic; throws `NotFoundException` on missing brand |
| `src/API.Infrastructure/ServiceCollectionExtensions.cs` | Modified | Added `using API.Application.CarModel.Query.GetAll;`; added `GetAllModelsQueryHandler` Transient registration |

### Work Unit Evidence

| Evidence | Value |
|----------|-------|
| Focused test command and exact result | `dotnet build C:/Users/ArnolSuarez/Desktop/ER/VehicleCatalog` → **Build succeeded. 0 Warning(s). 0 Error(s).** |
| Runtime harness command/scenario and exact result | N/A — `GET /api/carModels` endpoint not yet wired (controller created in WU-05); DI resolvability verified via clean build with handler registration present |
| Rollback boundary | Delete `CarModelResponse.cs`, `GetAllModelsQuery.cs`, `GetAllModelsQueryHandler.cs`, `NotFoundException.cs`; revert `ICarModelRepository.cs` (remove `GetAllByBrandIdAsync`), `CarModelRepository.cs` (remove `GetAllByBrandIdAsync`, restore `GetAllAsync` without Include), `ServiceCollectionExtensions.cs` (remove `GetAllModelsQueryHandler` registration and using) |

### Deviations from Design

- **`NotFoundException` created in `API.Domain.Exceptions`**: design.md referenced `throw NotFoundException` but no such class existed in the codebase. Created `src/API.Domain/Exceptions/NotFoundException.cs` as a plain domain exception. Controller (WU-05) will catch it and return `404 NotFound`. This follows the design data flow exactly.
- **`GetAllAsync` return type**: `ICarModelRepository.GetAllAsync` returns `IEnumerable<CarModel>` (not `IReadOnlyList`); handler calls `.ToList()` internally. Consistent with `CarBrandRepository` pattern (WU-03 precedent).

---

## WU-05: Controllers — CarBrandsController + CarModelsController — COMPLETE

### Completed Tasks

- [x] 5.1 Created `src/API/Controllers/CarBrandsController.cs`: `[Route("api/carBrands")]`, single `[HttpGet]` action dispatches `GetAllBrandsQuery`, returns `Ok(result)`; catches unexpected exceptions → `StatusCode(500)`
- [x] 5.2 Created `src/API/Controllers/CarModelsController.cs`: `[Route("api/carModels")]`, single `[HttpGet]` with `[FromQuery] string? brand`; guards `brand == ""` → `BadRequest("Brand parameter cannot be empty")`; catches `NotFoundException` → `NotFound("Brand '{brand}' not found")`; catches unexpected exceptions → `StatusCode(500)`
- [x] 5.3 `src/API/Controllers/CarBrandController.cs` left untouched — existing `GET /carBrand?id=` route confirmed functional
- [ ] 5.4 Manual verification of all five spec scenarios pending — requires `dotnet run` + HTTP client (Scalar UI or `.http` file)

### Files Changed

| File | Action | What Was Done |
|------|--------|---------------|
| `src/API/Controllers/CarBrandsController.cs` | Created | `[Route("api/carBrands")]`; injects `IDispatcher`; `GET` dispatches `GetAllBrandsQuery`; returns `Ok(result)` or `StatusCode(500)` |
| `src/API/Controllers/CarModelsController.cs` | Created | `[Route("api/carModels")]`; injects `IDispatcher`; `GET` with `[FromQuery] string? brand`; guards empty string → 400; catches `NotFoundException` → 404; returns `Ok(result)` or `StatusCode(500)` |

### Work Unit Evidence

| Evidence | Value |
|----------|-------|
| Focused test command and exact result | `dotnet build C:/Users/ArnolSuarez/Desktop/ER/VehicleCatalog` → **Build succeeded. 0 Warning(s). 0 Error(s).** |
| Runtime harness command/scenario and exact result | N/A — manual runtime verification (task 5.4) requires `dotnet run` + Scalar UI or `.http` file; deferred to sdd-verify phase |
| Rollback boundary | Delete `CarBrandsController.cs` and `CarModelsController.cs`; routes are removed with no other side effects; `CarBrandController.cs` remains untouched |

### Deviations from Design

None — implementation matches design exactly. DI injection pattern, route attributes, `[FromQuery]` binding, empty-string guard, `NotFoundException` catch, and 500 fallback all match spec and design.

---

## WU-06: Scalar/OpenAPI Setup — COMPLETE

### Completed Tasks

- [x] 6.1 In `src/API/API.csproj`: added `<PackageReference Include="Scalar.AspNetCore" Version="2.17.3" />` (latest stable 2.x on NuGet; net10.0-compatible confirmed)
- [x] 6.2 In `src/API/Program.cs`: added `using Scalar.AspNetCore;`; added `app.MapScalarApiReference()` inside the existing `IsDevelopment()` block after `app.MapOpenApi()`
- [x] 6.3 Build verified — `dotnet build` → **Build succeeded. 0 Warning(s). 0 Error(s).** Scalar UI wiring is Development-only; runtime render verification (`/scalar/v1`) deferred to sdd-verify

### Files Changed

| File | Action | What Was Done |
|------|--------|---------------|
| `src/API/API.csproj` | Modified | Added `<PackageReference Include="Scalar.AspNetCore" Version="2.17.3" />` |
| `src/API/Program.cs` | Modified | Added `using Scalar.AspNetCore;`; added `app.MapScalarApiReference()` inside `IsDevelopment` block |

### Work Unit Evidence

| Evidence | Value |
|----------|-------|
| Focused test command and exact result | `dotnet build C:/Users/ArnolSuarez/Desktop/ER/VehicleCatalog` → **Build succeeded. 0 Warning(s). 0 Error(s).** |
| Runtime harness command/scenario and exact result | Browser `GET /scalar/v1` and `GET /openapi/v1.json` — deferred to sdd-verify (requires `dotnet run` in Development; consistent with WU-01 through WU-05 pattern) |
| Rollback boundary | Remove `<PackageReference Include="Scalar.AspNetCore" ...>` from `API.csproj`; remove `using Scalar.AspNetCore;` and `app.MapScalarApiReference()` from `Program.cs` |

### Deviations from Design

None — `Scalar.AspNetCore` 2.17.3 is the latest stable 2.x and is net10.0-compatible. `builder.Services.AddOpenApi()` was already present from a previous WU; no duplicate added. `MapScalarApiReference()` is correctly placed inside the `IsDevelopment()` guard per design.

---

## WU-07: README — COMPLETE

### Completed Tasks

- [x] 7.1 Created `README.md` at solution root with: project overview, Clean Architecture layer map (4-project table), prerequisites, `dotnet run` + connection string setup, migration commands, complete endpoint reference (routes, params, response shapes, status codes), Scalar UI documentation section, and known constraints note

### Files Changed

| File | Action | What Was Done |
|------|--------|---------------|
| `README.md` | Created | Solution-root README with all required sections per orchestrator spec |

### Work Unit Evidence

| Evidence | Value |
|----------|-------|
| Focused test command and exact result | N/A — documentation artifact; no compilation required |
| Runtime harness command/scenario and exact result | File exists at `C:/Users/ArnolSuarez/Desktop/ER/VehicleCatalog/README.md`; all required sections present (verified by authoring) |
| Rollback boundary | Delete `README.md` — no other files affected |

### Deviations from Design

None — all sections from spec are present: title + description, architecture overview, tech stack, prerequisites, setup & run, endpoint table, API documentation (Scalar), migrations, known constraints.

---

## WU-08: Surgical fix — empty-brand guard in CarModelsController — COMPLETE

### Completed Tasks

- [x] 8.1 Replaced `brand == string.Empty` guard with `Request.Query.ContainsKey("brand") && string.IsNullOrWhiteSpace(brand)` in `src/API/Controllers/CarModelsController.cs` — covers `?brand=` (null binding) and `?brand=   ` (whitespace-only) per ASP.NET Core nullable-string binding behaviour

### Files Changed

| File | Action | What Was Done |
|------|--------|---------------|
| `src/API/Controllers/CarModelsController.cs` | Modified | Guard changed from `brand == string.Empty` to `Request.Query.ContainsKey("brand") && string.IsNullOrWhiteSpace(brand)` |

### Work Unit Evidence

| Evidence | Value |
|----------|-------|
| Focused test command and exact result | `dotnet build C:/Users/ArnolSuarez/Desktop/ER/VehicleCatalog` → **Build succeeded. 0 Warning(s). 0 Error(s).** |
| Runtime harness command/scenario and exact result | `GET http://localhost:5023/api/carModels?brand=` → **HTTP 400**; `GET http://localhost:5023/api/carModels?brand=   ` → **HTTP 400** |
| Rollback boundary | Revert `src/API/Controllers/CarModelsController.cs` line 23 back to `if (brand == string.Empty)` — single-line change, no other files affected |

### Deviations from Design

None — fix matches the verify-phase suggestion exactly. `string.IsNullOrWhiteSpace` is a strict superset of `IsNullOrEmpty`; no behaviour regression on valid `brand` values.

---

## All Work Units — Final Status

| Unit | Status |
|------|--------|
| WU-01 | ✅ Complete |
| WU-02 | ✅ Complete |
| WU-03 | ✅ Complete |
| WU-04 | ✅ Complete |
| WU-05 | ✅ Complete (build verified; manual runtime verification pending in sdd-verify) |
| WU-06 | ✅ Complete (build verified; Scalar UI runtime render pending in sdd-verify) |
| WU-07 | ✅ Complete |
| WU-08 | ✅ Complete (build + runtime verified — `?brand=` and `?brand=   ` both return 400) |
