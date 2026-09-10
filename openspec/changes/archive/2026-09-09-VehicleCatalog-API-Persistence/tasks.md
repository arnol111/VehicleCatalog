# Tasks: VehicleCatalog-API-Persistence

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | 320–380 |
| 400-line budget risk | Medium |
| Chained PRs recommended | No |
| Suggested split | Single PR |
| Delivery strategy | ask-on-risk |
| Chain strategy | N/A |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: N/A
400-line budget risk: Medium

### Suggested Work Units

| Unit | Goal | Likely PR | Focused test command | Runtime harness | Rollback boundary |
|------|------|-----------|----------------------|-----------------|-------------------|
| 1 | All persistence code + DI wiring | PR 1 | `dotnet build VehicleCatalog.slnx` | `dotnet run --project API` then verify app starts | Delete all new files; revert .csproj and Program.cs edits |

---

## Phase 1: Cleanup & Foundation

- [x] TASK-001 — Delete `API.Infrastructure/Class1.cs` and `API.Application/Class1.cs` (SCEN-019)
- [x] TASK-002 — Add EF Core packages to `API.Infrastructure/API.Infrastructure.csproj`: `Microsoft.EntityFrameworkCore` 10.0.x and `Microsoft.EntityFrameworkCore.SqlServer` 10.0.x (REQ-001)
- [x] TASK-003 — Add `Microsoft.EntityFrameworkCore.Design` 10.0.x (PrivateAssets=All) to `API/API.csproj` (REQ-001)
- [x] TASK-004 — Add project reference `API.Infrastructure → API.Domain` in `API.Infrastructure/API.Infrastructure.csproj` (REQ-008)
- [x] TASK-005 — Add project references `API → API.Infrastructure` and `API → API.Application` in `API/API.csproj` (REQ-008)

## Phase 2: Domain Interface Updates

- [x] TASK-006 — Update `API.Domain/Interfaces/ICarBrandRepository.cs`: add `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update`, `Delete` per design signatures (REQ-005)
- [x] TASK-007 — Update `API.Domain/Interfaces/ICarModelRepository.cs`: add same five CRUD members (REQ-005)
- [x] TASK-008 — Verify `API.Domain/Interfaces/IUnitOfWork.cs` exposes `CarBrands`, `CarModels` properties and `SaveChangesAsync` — add missing members if needed (REQ-007)

## Phase 3: Infrastructure — Persistence Core

- [x] TASK-009 — Create `API.Infrastructure/Persistence/Configurations/CarBrandConfiguration.cs` implementing `IEntityTypeConfiguration<CarBrand>` with `ToTable`, `HasKey`, `UseIdentityColumn`, `IsRequired`, `HasMaxLength(200)` (REQ-003)
- [x] TASK-010 — Create `API.Infrastructure/Persistence/Configurations/CarModelConfiguration.cs` implementing `IEntityTypeConfiguration<CarModel>` with explicit FK `HasOne<CarBrand>().WithMany().HasForeignKey(e => e.CarBrandId).OnDelete(Restrict)` (REQ-004)
- [x] TASK-011 — Create `API.Infrastructure/Persistence/VehicleCatalogDbContext.cs` inheriting `DbContext`, exposing `DbSet<CarBrand> CarBrands` and `DbSet<CarModel> CarModels`, calling `ApplyConfigurationsFromAssembly` in `OnModelCreating` (REQ-002)
- [x] TASK-012 — Create `API.Infrastructure/Persistence/UnitOfWork.cs` implementing `IUnitOfWork`, accepting `VehicleCatalogDbContext` via constructor, delegating `SaveChangesAsync` to DbContext (REQ-007)

## Phase 4: Infrastructure — Repositories

- [x] TASK-013 — Create `API.Infrastructure/Repositories/CarBrandRepository.cs` implementing `ICarBrandRepository`, injecting `VehicleCatalogDbContext`, implementing all five CRUD methods without calling `SaveChangesAsync` (REQ-006)
- [x] TASK-014 — Create `API.Infrastructure/Repositories/CarModelRepository.cs` implementing `ICarModelRepository`, same pattern as TASK-013 (REQ-006)

## Phase 5: Host Wiring

- [x] TASK-015 — Create `API.Infrastructure/ServiceCollectionExtensions.cs` with `AddInfrastructure(IConfiguration)` registering DbContext (Scoped, SqlServer), repositories, and UnitOfWork (REQ-009)
- [x] TASK-016 — Update `API/appsettings.json`: add `ConnectionStrings.DefaultConnection` with SQL Server connection string (REQ-010)
- [x] TASK-017 — Update `API/appsettings.Development.json`: override `ConnectionStrings.DefaultConnection` with localdb connection string (REQ-010)
- [x] TASK-018 — Update `API/Program.cs`: call `builder.Services.AddInfrastructure(builder.Configuration)`; remove any MVC leftover (REQ-009)
- [x] TASK-019 — Update `API/Controllers/*.cs`: change base class from `Controller` to `ControllerBase` (REQ-009)

## Phase 6: Migration Generation

- [x] TASK-020 — Run `dotnet build VehicleCatalog.slnx` and confirm exit code 0 before generating migration (SCEN-013)
- [x] TASK-021 — Run `dotnet ef migrations add InitialCreate --project API.Infrastructure --startup-project API`; verify migration files created in `API.Infrastructure/Migrations/` with `Up`/`Down` for `CarBrands` and `CarModels` tables and explicit FK (REQ-011, SCEN-017, SCEN-008)

## Dependency Graph

```
TASK-001
TASK-002 → TASK-004 → TASK-009, TASK-010, TASK-011, TASK-012, TASK-013, TASK-014
TASK-003 → TASK-005
TASK-004 → TASK-006, TASK-007, TASK-008
TASK-006, TASK-007, TASK-008 → TASK-009 ... TASK-014
TASK-009, TASK-010 → TASK-011
TASK-011 → TASK-012, TASK-013, TASK-014
TASK-012, TASK-013, TASK-014 → TASK-015
TASK-015, TASK-016, TASK-017, TASK-018, TASK-019 → TASK-020 → TASK-021
```
