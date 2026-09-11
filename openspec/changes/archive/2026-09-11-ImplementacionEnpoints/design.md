# Design: ImplementacionEnpoints

## Technical Approach

Extend the existing custom Mediator pattern (no MediatR) to serve two new query endpoints following the exact `GetByIdQuery` → `GetByIdQueryHandler` → `IUnitOfWork` → repository reference pattern. Fix two DI correctness bugs (Singleton Dispatcher, unregistered handlers), wire Scalar UI for Development only, and add a README.

No new architectural patterns are introduced. All work follows conventions already present in the codebase.

---

## Architecture Decisions

| Decision | Choice | Alternatives | Rationale |
|----------|--------|--------------|-----------|
| Brand-filter strategy | `GetAllByBrandIdAsync(int brandId)` in `ICarModelRepository` + EF `Where()` | In-memory filter in handler; JOIN in `GetAllAsync` | DB-side filter; minimal interface change; consistent with repository pattern already used for `GetById` |
| CarBrand resolution for filter | Handler calls `_unitOfWork.CarBrands.GetAllAsync()` + LINQ `FirstOrDefault` (case-insensitive) | Add `GetByNameAsync` to repository | Avoids new repository method; brand lookup is O(10) rows — acceptable cost |
| Navigation property on CarModel | Add `public CarBrand? CarBrand { get; set; }` + EF nav config | Keep no-nav, JOIN in raw SQL | Required to include brand name in response without a second roundtrip; EF eager-load with `Include()` is the established pattern |
| IDispatcher lifetime | Change `Singleton` → `Scoped` in `AddInfrastructure()` | Keep Singleton + use `IServiceScopeFactory` internally | Scoped is simpler; Dispatcher has no state; `IServiceScopeFactory` workaround adds complexity for no gain |
| Handler DI lifetime | `Transient` for all three handlers | Scoped | Handlers are stateless; Transient matches the existing pattern and avoids accidental state leakage |
| Scalar vs Swashbuckle | `Scalar.AspNetCore` | Swashbuckle | Proposal decision; reads existing `/openapi/v1.json`; no schema duplication; net10.0-native |
| Program.cs deduplication | Remove manual `AddDbContext` + `AddScoped<IUnitOfWork>` from `Program.cs` | Move everything to `Program.cs` and delete `AddInfrastructure` | `AddInfrastructure()` is the canonical registration point per Clean Architecture; `Program.cs` should only orchestrate |

---

## Data Flow

### GET /api/carBrands (list all)

```
CarBrandsController.GetAll()
  → IDispatcher.Send<GetAllBrandsQuery, IReadOnlyList<CarBrandResponse>>()
    → GetAllBrandsQueryHandler.Handle()
      → IUnitOfWork.CarBrands.GetAllAsync()
        → CarBrandRepository → _context.CarBrands.AsNoTracking().ToListAsync()
      ← IEnumerable<CarBrand>
    ← map to List<CarBrandResponse>{ Id=IdCarBrand, Name=Brand }
  ← List<CarBrandResponse>
← 200 OK [ { id, name }, ... ]
```

### GET /api/carModels (no filter)

```
CarModelsController.GetAll(brand: null)
  → IDispatcher.Send<GetAllModelsQuery, IReadOnlyList<CarModelResponse>>()
    → GetAllModelsQueryHandler.Handle()
      → IUnitOfWork.CarModels.GetAllAsync()   [includes CarBrand nav via .Include()]
        → CarModelRepository → _context.CarModels.Include(m => m.CarBrand).AsNoTracking().ToListAsync()
      ← IEnumerable<CarModel> (CarBrand populated)
    ← map to List<CarModelResponse>{ Id, Name=Model, Year, BrandName=CarBrand.Brand }
← 200 OK [ { id, name, year, brandName }, ... ]
```

### GET /api/carModels?Brand=toyota (with filter)

```
CarModelsController.GetAll(brand: "toyota")
  → guard: brand == "" → 400 BadRequest
  → IDispatcher.Send<GetAllModelsQuery("toyota"), IReadOnlyList<CarModelResponse>>()
    → GetAllModelsQueryHandler.Handle()
      → IUnitOfWork.CarBrands.GetAllAsync()
        → find brand by name (case-insensitive OrdinalIgnoreCase)
        → brand == null → throw NotFoundException("Brand not found")
      → IUnitOfWork.CarModels.GetAllByBrandIdAsync(brand.IdCarBrand)
        → CarModelRepository → _context.CarModels.Include(m => m.CarBrand)
                                .Where(m => m.IdCarBrand == brandId).AsNoTracking().ToListAsync()
      ← IEnumerable<CarModel>
    ← map to List<CarModelResponse>
← 200 OK [ ... ] or 404 if brand not found
```

---

## File Changes

| File | Action | Class / Interface | Notes |
|------|--------|-------------------|-------|
| `src/API.Domain/Entities/CarModel.cs` | Modify | `CarModel` | Add `public CarBrand? CarBrand { get; set; }` nav prop |
| `src/API.Domain/Interfaces/ICarModelRepository.cs` | Modify | `ICarModelRepository` | Add `GetAllByBrandIdAsync(int brandId, CancellationToken ct = default)` |
| `src/API.Infrastructure/Repositories/CarModelRepository.cs` | Modify | `CarModelRepository` | Implement `GetAllByBrandIdAsync` with `Where` + `Include`; update `GetAllAsync` to also `Include(CarBrand)` |
| `src/API.Infrastructure/Persistence/Configurations/CarModelConfiguration.cs` | Modify | `CarModelConfiguration` | Add explicit nav-property relationship: `HasOne(m => m.CarBrand).WithMany().HasForeignKey(m => m.IdCarBrand)` |
| `src/API.Application/DTOs/CarModelResponse.cs` | Create | `CarModelResponse` | `{ int Id, string Name, int Year, string BrandName }` |
| `src/API.Application/DTOs/CarBrandResponse.cs` | Create | `CarBrandResponse` | `{ int Id, string Name }` — replaces current `CarBrandDTO` for new endpoint (existing DTO kept for backward compat) |
| `src/API.Application/CarBrand/Query/GetAll/GetAllBrandsQuery.cs` | Create | `GetAllBrandsQuery` | `IRequest<IReadOnlyList<CarBrandResponse>>`, no properties |
| `src/API.Application/CarBrand/Query/GetAll/GetAllBrandsQueryHandler.cs` | Create | `GetAllBrandsQueryHandler` | `IRequestHandler<GetAllBrandsQuery, IReadOnlyList<CarBrandResponse>>`, maps from `IUnitOfWork.CarBrands.GetAllAsync()` |
| `src/API.Application/CarModel/Query/GetAll/GetAllModelsQuery.cs` | Create | `GetAllModelsQuery` | `IRequest<IReadOnlyList<CarModelResponse>>`, `string? BrandName` property |
| `src/API.Application/CarModel/Query/GetAll/GetAllModelsQueryHandler.cs` | Create | `GetAllModelsQueryHandler` | Branches on `BrandName == null`; throws on brand not found |
| `src/API/Controllers/CarBrandsController.cs` | Create | `CarBrandsController` | `[Route("api/carBrands")]` — new file, clean separation from existing `CarBrandController` |
| `src/API/Controllers/CarModelsController.cs` | Create | `CarModelsController` | `[Route("api/carModels")]`, `[FromQuery] string? brand` param, guard empty string → 400 |
| `src/API/Controllers/CarBrandController.cs` | Keep | `CarBrandController` | Existing `GET /carBrand?id=` unchanged |
| `src/API.Infrastructure/ServiceCollectionExtensions.cs` | Modify | `AddInfrastructure()` | Change `IDispatcher` to `Scoped`; add all three handler registrations as `Transient`; add `GetByIdQueryHandler` registration |
| `src/API/Program.cs` | Modify | `Program` | Remove duplicate `AddDbContext`, `AddScoped<IUnitOfWork>`, `AddSingleton<IDispatcher>`; add `Scalar.AspNetCore` wiring under `IsDevelopment` |
| `src/API/API.csproj` | Modify | — | Add `<PackageReference Include="Scalar.AspNetCore" />` |
| `README.md` | Create | — | Solution root; overview, architecture, setup, migrations, endpoint list |

> **EF Migration**: A new migration is required because `CarModel` gains a navigation property reconfigured via Fluent API. The FK relationship already exists in the DB — the migration may be empty if EF detects no schema delta, but it must be generated and verified.

---

## Interfaces / Contracts

```csharp
// CarBrandResponse.cs
public record CarBrandResponse(int Id, string Name);

// CarModelResponse.cs
public record CarModelResponse(int Id, string Name, int Year, string BrandName);

// ICarModelRepository addition
Task<IReadOnlyList<CarModel>> GetAllByBrandIdAsync(int brandId, CancellationToken ct = default);

// GetAllModelsQuery.cs
public class GetAllModelsQuery : IRequest<IReadOnlyList<CarModelResponse>>
{
    public string? BrandName { get; init; }
    public GetAllModelsQuery(string? brandName = null) => BrandName = brandName;
}

// DI Registration order in AddInfrastructure()
services.AddDbContext<VehicleCatalogDbContext>(...);
services.AddScoped<ICarBrandRepository, CarBrandRepository>();
services.AddScoped<ICarModelRepository, CarModelRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IDispatcher, Dispatcher>();
services.AddTransient<IRequestHandler<GetByIdQuery, CarBrandDTO>, GetByIdQueryHandler>();
services.AddTransient<IRequestHandler<GetAllBrandsQuery, IReadOnlyList<CarBrandResponse>>, GetAllBrandsQueryHandler>();
services.AddTransient<IRequestHandler<GetAllModelsQuery, IReadOnlyList<CarModelResponse>>, GetAllModelsQueryHandler>();
```

---

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | `GetAllModelsQueryHandler` branching (null brand, valid brand, missing brand) | xUnit + Moq on `IUnitOfWork` |
| Unit | `GetAllBrandsQueryHandler` mapping correctness | xUnit + Moq |
| Integration | DI resolution — all handlers resolve without exception | `WebApplicationFactory` startup smoke test |
| Manual | All 5 endpoints via Scalar UI / `.http` file | Verify HTTP 200/400/404 responses match spec scenarios |

> No test project exists yet — test project creation is out of scope per proposal. Manual testing via Scalar UI is the verification path for this change.

---

## Threat Matrix

N/A — no routing rule changes, shell commands, subprocesses, VCS/PR automation, executable-file classification, or process-integration boundaries introduced. Change is limited to HTTP controller actions, DI wiring, and EF repository methods.

---

## Migration / Rollout

A new EF Core migration must be added after `CarModel` entity and `CarModelConfiguration` are updated:

```bash
dotnet ef migrations add AddCarBrandNavProp --project src/API.Infrastructure --startup-project src/API
dotnet ef database update --project src/API.Infrastructure --startup-project src/API
```

The migration may produce no schema changes (FK already exists). Verify by inspecting the generated `.cs` migration file before applying. If empty, it is safe to apply — it confirms model snapshot alignment.

---

## Open Questions

- [ ] Confirm `Scalar.AspNetCore` NuGet version compatible with `net10.0` before adding to `API.csproj` (risk: package may target `net9.0` only at time of implementation).
- [ ] Decide whether `CarBrandsController` is a new file or `CarBrandController` is renamed. Current design creates a new `CarBrandsController` (`api/carBrands`) to avoid breaking the existing `GET /carBrand?id=` route.
