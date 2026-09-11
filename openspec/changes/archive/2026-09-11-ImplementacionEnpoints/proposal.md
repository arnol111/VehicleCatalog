# Proposal: ImplementacionEnpoints

## Intent

The API has no working list/filter endpoints and two DI bugs that will crash the app at runtime. Users cannot query car brands or models. The `IDispatcher` singleton causes captive dependency failures; handlers are not registered in DI at all. This change delivers the three required query endpoints, fixes both DI defects, wires Scalar UI, and adds a project README.

## Scope

### In Scope
- `GET /api/carBrands` — list all brands
- `GET /api/carModels` — list all models with brand ID and year
- `GET /api/carModels?Brand={name}` — filter models by brand name (DB-side)
- Fix `IDispatcher` Singleton → Scoped (captive dependency bug)
- Register all handlers in DI (runtime crash bug)
- Add `Scalar.AspNetCore` for Swagger UI
- Remove duplicate DI registrations in `Program.cs`
- Add `README.md` at solution root

### Out of Scope
- Test project creation (deferred to next session)
- Database schema changes beyond adding `GetAllByBrandIdAsync` repository method
- Write/mutation endpoints (POST, PUT, DELETE)

## Capabilities

### New Capabilities
- `car-brand-list`: GET /api/carBrands — returns all brands as a list
- `car-model-list`: GET /api/carModels with optional `?Brand` filter — returns models with brand reference
- `scalar-ui`: Scalar-based interactive API documentation UI
- `di-configuration`: Correct DI lifetimes, handler registrations, and deduplication

### Modified Capabilities
- `ef-core-persistence`: `ICarModelRepository` gains `GetAllByBrandIdAsync(int brandId)` — spec-level interface contract changes

## Approach

**Navigation / filter strategy**: Add `GetAllByBrandIdAsync(int brandId)` to `ICarModelRepository` and implement with EF Core `Where()`. The `GetAllQueryHandler` for CarModel resolves the brand ID from `IUnitOfWork.CarBrands` when `Brand` param is provided, then calls the scoped repository method. DB-side filter, no in-memory loading.

**DI fix**: Change `IDispatcher` registration from `Singleton` to `Scoped` in `AddInfrastructure()`. Register all four handlers (`GetByIdQueryHandler`, `GetAllQueryHandler` ×2) as `Transient`.

**Scalar**: Add `Scalar.AspNetCore` NuGet to `src/API`. Call `app.MapScalarApiReference()` after `MapOpenApi()`. No schema duplication — Scalar reads the existing `/openapi/v1.json`.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `src/API.Application/CarBrand/Query/GetAll/` | New | `GetAllQuery.cs` + `GetAllQueryHandler.cs` |
| `src/API.Application/CarModel/Query/GetAll/` | New | `GetAllQuery.cs` + `GetAllQueryHandler.cs` (optional Brand filter) |
| `src/API.Application/DTOs/CarModelDTO.cs` | New | DTO for CarModel responses |
| `src/API.Domain/Interfaces/ICarModelRepository.cs` | Modified | Add `GetAllByBrandIdAsync(int brandId)` |
| `src/API.Infrastructure/Repositories/CarModelRepository.cs` | Modified | Implement `GetAllByBrandIdAsync` with EF Where() |
| `src/API/Controllers/CarBrandController.cs` | Modified | Add `GET /api/carBrands` action |
| `src/API/Controllers/CarModelController.cs` | Modified | Add `GET /api/carModels` action with optional `?Brand` param |
| `src/API/Program.cs` | Modified | Fix DI lifetimes, register handlers, add Scalar, remove duplicates |
| `src/API/API.csproj` | Modified | Add `Scalar.AspNetCore` NuGet reference |
| `README.md` | New | Project overview at solution root |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Handler not registered → runtime `InvalidOperationException` | High | Register all handlers explicitly; verified by hitting endpoints manually |
| Scoped service resolved from wrong lifetime after fix | Low | Dispatcher becomes Scoped — no longer a singleton; verify with startup smoke test |
| `GetAllByBrandIdAsync` with non-existent brand name returns empty list silently | Med | Handler returns 404 if brand lookup returns null before querying models |
| Scalar NuGet not yet stable for net10.0 | Low | Check NuGet version compatibility; fallback is Swashbuckle |

## Rollback Plan

Revert `Program.cs` to previous DI wiring (re-add Singleton for Dispatcher, remove handler registrations). Delete new Application/Domain/Infrastructure files. Remove `Scalar.AspNetCore` from `API.csproj`. The existing `GET /carBrand?id=` endpoint is unaffected — it will still fail at runtime as before (handler unregistered), which is the pre-change state.

## Dependencies

- `Scalar.AspNetCore` NuGet package (net10.0 compatible version)
- Existing `/openapi/v1.json` endpoint already wired via `AddOpenApi()` / `MapOpenApi()`

## Success Criteria

- [ ] `GET /api/carBrands` returns HTTP 200 with all 10 brands
- [ ] `GET /api/carModels` returns HTTP 200 with all 50 models
- [ ] `GET /api/carModels?Brand=Toyota` returns only models for that brand
- [ ] `GET /api/carModels?Brand=NonExistent` returns HTTP 404
- [ ] Scalar UI accessible at `/scalar/v1` (or configured route)
- [ ] `dotnet build` succeeds with no errors
- [ ] No `InvalidOperationException` on DI resolution at startup
- [ ] `README.md` exists at solution root
