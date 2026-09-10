# Proposal: VehicleCatalog API Persistence Layer

## Intent

The VehicleCatalog solution has domain entities (`CarBrand`, `CarModel`) and repository interfaces defined, but the persistence layer is entirely absent — no EF Core packages, no `DbContext`, no repository implementations, no project wiring, and no DI registrations. The application cannot connect to a database or execute any data operation.

This change establishes the full EF Core persistence infrastructure so that:
- The app connects to a configured SQL Server database on startup.
- `CarBrand` and `CarModel` records can be created, read, updated, and deleted through the repository abstraction.
- Migrations can be generated and applied with `dotnet ef migrations add` / `dotnet ef database update`.

Endpoints are explicitly **not** part of this change — focus is the persistence layer only.

---

## Scope

### In Scope
- Add EF Core 10 NuGet packages to `API.Infrastructure` and `API` (host)
- Create `VehicleCatalogDbContext` with `DbSet<CarBrand>` and `DbSet<CarModel>`
- Fluent API entity configurations for both entities (constraints, FK, max-lengths)
- Define CRUD members on `ICarBrandRepository` and `ICarModelRepository` in `API.Domain`
- Implement `CarBrandRepository`, `CarModelRepository`, and `UnitOfWork` in `API.Infrastructure`
- Wire project references: `API.Infrastructure → API.Domain`, `API → API.Infrastructure`, `API → API.Application`
- Register `DbContext`, repositories, and `UnitOfWork` in `Program.cs` DI
- Add `ConnectionStrings:DefaultConnection` to `appsettings.json` (local SQL Server placeholder)
- Support override via environment variable (`ConnectionStrings__DefaultConnection`)
- Generate initial EF migration (`InitialCreate`)
- Delete `Class1.cs` stubs from `API.Infrastructure` and `API.Application`
- Change `API` controllers base class from `Controller` to `ControllerBase`

### Out of Scope
- REST endpoint implementation (routes, DTOs, controller actions)
- `API.Application` service/use-case layer implementation
- Test projects (unit or integration)
- Authentication, authorization, pagination, or caching
- Production database provisioning or CI/CD pipeline changes

---

## Capabilities

### New Capabilities
- `ef-core-persistence`: EF Core DbContext, Fluent API configurations, repository implementations, UnitOfWork, and migration support for CarBrand and CarModel

### Modified Capabilities
- None

---

## Approach

Use EF Core 10 with SQL Server provider, aligning package versions to `net10.0`. Define `VehicleCatalogDbContext` in `API.Infrastructure`, apply Fluent API configurations via separate `IEntityTypeConfiguration<T>` classes (one per entity) to keep the context clean. Repository interfaces in `API.Domain` gain standard CRUD members (`GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update`, `Delete`). Implementations in `API.Infrastructure` wrap `DbContext` directly and are registered as scoped. `UnitOfWork` wraps `DbContext.SaveChangesAsync`. Connection string is read from `appsettings.json` with environment variable override — no hard-coded strings. The initial migration is generated after wiring is confirmed.

---

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `API.Domain/Repositories/ICarBrandRepository.cs` | Modified | Add CRUD method signatures |
| `API.Domain/Repositories/ICarModelRepository.cs` | Modified | Add CRUD method signatures |
| `API.Infrastructure/API.Infrastructure.csproj` | Modified | Add EF Core + SQL Server NuGet refs, project ref to Domain |
| `API.Infrastructure/Class1.cs` | Removed | Delete stub |
| `API.Infrastructure/Persistence/VehicleCatalogDbContext.cs` | New | DbContext with DbSets |
| `API.Infrastructure/Persistence/Configurations/CarBrandConfiguration.cs` | New | Fluent API for CarBrand |
| `API.Infrastructure/Persistence/Configurations/CarModelConfiguration.cs` | New | Fluent API for CarModel (incl. FK) |
| `API.Infrastructure/Repositories/CarBrandRepository.cs` | New | CRUD implementation |
| `API.Infrastructure/Repositories/CarModelRepository.cs` | New | CRUD implementation |
| `API.Infrastructure/Persistence/UnitOfWork.cs` | New | IUnitOfWork implementation |
| `API.Application/Class1.cs` | Removed | Delete stub |
| `API.Application/API.Application.csproj` | Modified | Add project ref to Domain |
| `API/API.csproj` | Modified | Add project refs to Infrastructure + Application; add EF Design package |
| `API/Program.cs` | Modified | Register DbContext, repos, UoW; fix controller base class |
| `API/appsettings.json` | Modified | Add ConnectionStrings:DefaultConnection |
| `API/Migrations/` | New | InitialCreate migration (generated) |

---

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| EF Core 10 package version mismatch on `net10.0` preview | Medium | Pin all `Microsoft.EntityFrameworkCore.*` packages to the same `10.0.x` version |
| `CarBrand.Brand` maps to `nvarchar(max)` without MaxLength | High | Enforce `HasMaxLength(200)` (or chosen limit) in Fluent API config |
| `CarModel` FK to `CarBrand` not inferred — no navigation property | High | Explicitly configure `HasOne` / `WithMany` + `HasForeignKey` in `CarModelConfiguration` |
| `API.Application` lives outside `src/` — structural inconsistency | Low | Noted; out of scope to restructure, but document for future cleanup |
| No test projects — persistence behavior unverified at unit level | Medium | Accepted for this change; integration tests deferred to a follow-up change |

---

## Rollback Plan

All changes are additive except `Class1.cs` deletions and `csproj` modifications. To revert:
1. `git revert` the change set or `git checkout` the affected files.
2. Remove the `Migrations/` folder if partially applied.
3. No database schema changes are destructive until `dotnet ef database update` is explicitly run.

---

## Dependencies

- SQL Server instance accessible at the configured connection string (local or Docker) — required only to run `dotnet ef database update` or the app; not required to compile or generate migrations.
- .NET 10 SDK with EF Core CLI tools (`dotnet tool install --global dotnet-ef`).

---

## Success Criteria

- [ ] `dotnet build` succeeds across all four projects with no warnings about missing references
- [ ] `dotnet ef migrations add InitialCreate` generates a valid migration in `API/Migrations/`
- [ ] `dotnet ef database update` applies the migration and creates the `CarBrands` and `CarModels` tables
- [ ] App starts without exceptions when `ConnectionStrings:DefaultConnection` points to a reachable SQL Server
- [ ] `ICarBrandRepository` and `ICarModelRepository` each expose at minimum: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update`, `Delete`
- [ ] No `Class1.cs` files remain in any project
