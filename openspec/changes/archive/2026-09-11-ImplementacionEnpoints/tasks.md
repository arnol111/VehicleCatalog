# Tasks: ImplementacionEnpoints

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | 450–550 |
| 400-line budget risk | High |
| Chained PRs recommended | Yes |
| Suggested split | WU-01 → WU-02 → WU-03 → WU-04 → WU-05 → WU-06 → WU-07 |
| Delivery strategy | ask-on-risk |
| Chain strategy | pending |

Decision needed before apply: Yes
Chained PRs recommended: Yes
Chain strategy: pending
400-line budget risk: High

> **⚠️ Decision required**: The estimated diff (~450–550 lines across 15+ files) exceeds the 400-line review budget. Choose a chain strategy before sdd-apply starts:
> - **stacked-to-main** — each WU merges to main in order; best for speed.
> - **feature-branch-chain** — one tracker branch accumulates all WUs; only it merges to main; best for rollback control.
> - **size:exception** — single PR, maintainer approval required.

### Suggested Work Units

| Unit | Goal | Likely PR | Focused test command | Runtime harness | Rollback boundary |
|------|------|-----------|----------------------|-----------------|-------------------|
| WU-01 | Fix DI bugs (Dispatcher lifetime, handler registrations, deduplication) | PR 1 | `dotnet build src/API` — zero errors; app starts without `InvalidOperationException` | `dotnet run --project src/API` + `GET /api/carBrand?id=1` returns 200 | Revert `ServiceCollectionExtensions.cs` and `Program.cs` to pre-change state |
| WU-02 | EF domain change: CarModel nav property + migration | PR 2 | `dotnet ef migrations add AddCarBrandNavProp --dry-run` (inspect output) | `dotnet ef database update` applies cleanly; schema unchanged | Delete migration file; revert `CarModel.cs` and `CarModelConfiguration.cs` |
| WU-03 | CarBrand layer: DTO + query + handler | PR 3 | `dotnet build src/API.Application` | `GET /api/carBrands` returns 200 with 10 brands (Scalar or .http file) | Delete `CarBrandResponse.cs`, `GetAllBrandsQuery.cs`, `GetAllBrandsQueryHandler.cs` |
| WU-04 | CarModel layer: interface delta + repo + queries + handlers + DTO | PR 4 | `dotnet build src/API.Application src/API.Infrastructure` | `GET /api/carModels`, `GET /api/carModels?Brand=toyota` return 200; `?Brand=NonExistent` returns 404 | Delete `CarModelResponse.cs`, `GetAllModels*`, revert `ICarModelRepository.cs`, `CarModelRepository.cs` |
| WU-05 | Controllers: CarBrandsController + CarModelsController | PR 5 | `dotnet build src/API` | All five endpoint scenarios match spec (Scalar UI or `.http` file) | Delete controller files; routes are removed with no other side effects |
| WU-06 | Scalar/OpenAPI setup | PR 6 | `dotnet build src/API` | Browser `GET /scalar` renders Scalar UI; `GET /openapi/v1.json` returns valid JSON | Remove `Scalar.AspNetCore` from `API.csproj`; remove wiring from `Program.cs` |
| WU-07 | README | PR 7 | N/A — doc only | Visual review of `README.md` in browser/IDE | Delete `README.md` |

---

## Phase 1: DI Infrastructure Fix (WU-01)

- [x] 1.1 In `src/API.Infrastructure/ServiceCollectionExtensions.cs`: change `IDispatcher` registration from `AddSingleton` to `AddScoped<IDispatcher, Dispatcher>()`.
- [x] 1.2 In `src/API.Infrastructure/ServiceCollectionExtensions.cs`: add `services.AddTransient<IRequestHandler<GetByIdQuery, CarBrandDTO>, GetByIdQueryHandler>()` (fixes existing unregistered handler).
- [x] 1.3 In `src/API/Program.cs`: remove manual duplicate registrations of `VehicleCatalogDbContext`, `IUnitOfWork`, and `IDispatcher` that shadow `AddInfrastructure()`.
- [x] 1.4 Verify `dotnet build src/API` succeeds and app starts without `InvalidOperationException` on `GET /carBrand?id=1`.

## Phase 2: EF Domain Change (WU-02)

- [x] 2.1 In `src/API.Domain/Entities/CarModel.cs`: add navigation property `public CarBrand? CarBrand { get; set; }`.
- [x] 2.2 In `src/API.Infrastructure/Persistence/Configurations/CarModelConfiguration.cs`: add explicit fluent config `HasOne(m => m.CarBrand).WithMany().HasForeignKey(m => m.IdCarBrand).OnDelete(DeleteBehavior.Restrict)`.
- [x] 2.3 Run `dotnet ef migrations add AddCarBrandNavProp --project src/API.Infrastructure --startup-project src/API`; inspect generated migration — confirm no destructive schema delta.
- [x] 2.4 Run `dotnet ef database update --project src/API.Infrastructure --startup-project src/API`; verify DB applies without error.

## Phase 3: CarBrand Application Layer (WU-03)

- [x] 3.1 Create `src/API.Application/DTOs/CarBrandResponse.cs`: `public record CarBrandResponse(int Id, string Name);`
- [x] 3.2 Create `src/API.Application/CarBrand/Query/GetAll/GetAllBrandsQuery.cs`: `IRequest<IReadOnlyList<CarBrandResponse>>`, no properties.
- [x] 3.3 Create `src/API.Application/CarBrand/Query/GetAll/GetAllBrandsQueryHandler.cs`: call `_unitOfWork.CarBrands.GetAllAsync(ct)`; map each `CarBrand` → `CarBrandResponse(IdCarBrand, Brand)`.
- [x] 3.4 In `src/API.Infrastructure/ServiceCollectionExtensions.cs`: register `AddTransient<IRequestHandler<GetAllBrandsQuery, IReadOnlyList<CarBrandResponse>>, GetAllBrandsQueryHandler>()`.

## Phase 4: CarModel Application + Infrastructure Layer (WU-04)

- [x] 4.1 In `src/API.Domain/Interfaces/ICarModelRepository.cs`: add `Task<IReadOnlyList<CarModel>> GetAllByBrandIdAsync(int brandId, CancellationToken ct = default);`.
- [x] 4.2 In `src/API.Infrastructure/Repositories/CarModelRepository.cs`: implement `GetAllByBrandIdAsync` — `_context.CarModels.Include(m => m.CarBrand).Where(m => m.IdCarBrand == brandId).AsNoTracking().ToListAsync(ct)`.
- [x] 4.3 In `src/API.Infrastructure/Repositories/CarModelRepository.cs`: update `GetAllAsync` to add `.Include(m => m.CarBrand)` so `BrandName` is populated on unfiltered results.
- [x] 4.4 Create `src/API.Application/DTOs/CarModelResponse.cs`: `public record CarModelResponse(int Id, string Name, int Year, string BrandName);`
- [x] 4.5 Create `src/API.Application/CarModel/Query/GetAll/GetAllModelsQuery.cs`: `IRequest<IReadOnlyList<CarModelResponse>>` with `string? BrandName` property.
- [x] 4.6 Create `src/API.Application/CarModel/Query/GetAll/GetAllModelsQueryHandler.cs`: branch on `BrandName == null` → `GetAllAsync`; else resolve brand by name (case-insensitive) → 404 if not found → `GetAllByBrandIdAsync(brand.IdCarBrand)`. Map to `CarModelResponse`.
- [x] 4.7 In `src/API.Infrastructure/ServiceCollectionExtensions.cs`: register `AddTransient<IRequestHandler<GetAllModelsQuery, IReadOnlyList<CarModelResponse>>, GetAllModelsQueryHandler>()`.

## Phase 5: Controllers (WU-05)

- [x] 5.1 Create `src/API/Controllers/CarBrandsController.cs`: `[Route("api/carBrands")]`, single `[HttpGet]` action dispatches `GetAllBrandsQuery`, returns `Ok(result)`.
- [x] 5.2 Create `src/API/Controllers/CarModelsController.cs`: `[Route("api/carModels")]`, single `[HttpGet]` action with `[FromQuery] string? brand`; guard `brand == ""` → `BadRequest()`; dispatch `GetAllModelsQuery(brand)`; map `NotFoundException` → `NotFound()`.
- [x] 5.3 Keep `src/API/Controllers/CarBrandController.cs` (read-only) unchanged — existing `GET /carBrand?id=` route must remain functional.
- [x] 5.4 Manually verify all five spec scenarios: 200 brands list, 200 models list, 200 filtered models, 400 empty brand, 404 non-existent brand.

## Phase 6: Scalar UI Setup (WU-06)

- [x] 6.1 In `src/API/API.csproj`: add `<PackageReference Include="Scalar.AspNetCore" Version="x.y.z" />` (confirm net10.0-compatible version on NuGet before adding).
- [x] 6.2 In `src/API/Program.cs`: add `using Scalar.AspNetCore;`; after `app.MapOpenApi()` add `if (app.Environment.IsDevelopment()) { app.MapScalarApiReference(); }`.
- [x] 6.3 Verify `GET /scalar` (or `/scalar/v1`) renders Scalar UI in Development; confirm `GET /openapi/v1.json` returns valid OpenAPI JSON.

## Phase 7: README (WU-07)

- [x] 7.1 Create `README.md` at solution root with: project overview, Clean Architecture layer map, prerequisites, `dotnet run` setup instructions, migration commands, complete endpoint reference (routes, params, response shapes), and known constraints (no test project yet).
