# EF Core Persistence Specification
# Change: VehicleCatalog-API-Persistence

## Purpose

Defines requirements for the complete EF Core 10 persistence layer:
NuGet packages, DbContext, Fluent API configurations, repository interfaces,
repository implementations, UnitOfWork, project wiring, DI registration,
connection-string configuration, and migration generation.

---

## Requirements

### REQ-001 — NuGet Package Alignment

All EF Core packages added to `API.Infrastructure` and `API` MUST target the same
`10.0.x` version to match `net10.0`. Required packages:

| Package | Project |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` 10.0.x | API.Infrastructure |
| `Microsoft.EntityFrameworkCore` 10.0.x | API.Infrastructure |
| `Microsoft.EntityFrameworkCore.Design` 10.0.x | API (host, PrivateAssets=All) |

No package version MUST differ from the others within the same EF Core family.

#### SCEN-001 — Packages resolve without conflicts

- GIVEN the solution targets `net10.0`
- WHEN `dotnet restore` is executed
- THEN all EF Core packages resolve to the same `10.0.x` patch version
- AND there are no `NU1605` downgrade warnings

#### SCEN-002 — Version mismatch is caught at restore

- GIVEN one EF Core package is set to a different minor version
- WHEN `dotnet restore` is executed
- THEN the build fails or warns with a version conflict diagnostic

---

### REQ-002 — VehicleCatalogDbContext

`VehicleCatalogDbContext` MUST reside in `API.Infrastructure/Persistence/` and
inherit `DbContext`. It MUST expose `DbSet<CarBrand> CarBrands` and
`DbSet<CarModel> CarModels`. Entity configurations MUST be applied via
`modelBuilder.ApplyConfigurationsFromAssembly(...)` in `OnModelCreating`.

#### SCEN-003 — DbSets are accessible

- GIVEN the DbContext is registered in DI
- WHEN a service resolves `VehicleCatalogDbContext`
- THEN `context.CarBrands` and `context.CarModels` are non-null and queryable

#### SCEN-004 — Configurations are applied automatically

- GIVEN `IEntityTypeConfiguration<T>` classes exist in the same assembly
- WHEN `OnModelCreating` runs
- THEN all configurations are applied without manual registration per entity

---

### REQ-003 — CarBrand Fluent API Configuration

`CarBrandConfiguration` MUST implement `IEntityTypeConfiguration<CarBrand>` and apply:

| Rule | Constraint |
|---|---|
| Primary key | `Id` |
| `Brand` column | `HasMaxLength(200)`, `IsRequired` |
| Table name | MAY be explicit or convention-based |

#### SCEN-005 — Brand column enforces max length

- GIVEN a `CarBrand` with `Brand` longer than 200 characters
- WHEN `SaveChangesAsync` is called
- THEN EF Core throws a `DbUpdateException` due to column length violation

#### SCEN-006 — Brand column is required

- GIVEN a `CarBrand` with `Brand` set to null
- WHEN `SaveChangesAsync` is called
- THEN the operation fails with a validation or DB constraint error

---

### REQ-004 — CarModel Fluent API Configuration

`CarModelConfiguration` MUST implement `IEntityTypeConfiguration<CarModel>` and apply:

| Rule | Constraint |
|---|---|
| Primary key | `Id` |
| FK to `CarBrand` | Explicit `HasOne` / `WithMany` / `HasForeignKey("CarBrandId")` |
| `Model` column | `HasMaxLength(200)`, `IsRequired` |

Because `CarModel` has no navigation property to `CarBrand`, the relationship
MUST be configured explicitly — convention-based inference MUST NOT be relied upon.

#### SCEN-007 — CarModel FK constraint is enforced

- GIVEN a `CarModel` referencing a non-existent `CarBrandId`
- WHEN `SaveChangesAsync` is called
- THEN the operation fails with a foreign-key constraint violation

#### SCEN-008 — Migration schema reflects explicit FK

- GIVEN `CarModelConfiguration` is applied
- WHEN a migration is generated
- THEN the migration contains an explicit `AddForeignKey` for `CarBrandId → CarBrands.Id`

---

### REQ-005 — Repository Interface CRUD Contracts

`ICarBrandRepository` and `ICarModelRepository` in `API.Domain` MUST each declare:

| Method | Signature |
|---|---|
| GetByIdAsync | `Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)` |
| GetAllAsync | `Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)` |
| AddAsync | `Task AddAsync(TEntity entity, CancellationToken ct = default)` |
| Update | `void Update(TEntity entity)` |
| Delete | `void Delete(TEntity entity)` |

Interfaces MUST NOT expose `SaveChangesAsync` — persistence is the UnitOfWork's concern.

#### SCEN-009 — Interface contract is complete

- GIVEN `ICarBrandRepository` is inspected via reflection
- WHEN checking declared members
- THEN all five methods above are present with matching signatures

---

### REQ-006 — Repository Implementations

`CarBrandRepository` and `CarModelRepository` in `API.Infrastructure/Repositories/`
MUST implement their respective interfaces. Each MUST:

- Accept `VehicleCatalogDbContext` via constructor injection
- Implement all five CRUD methods against the injected context
- NOT call `SaveChangesAsync` internally

#### SCEN-010 — GetByIdAsync returns null for missing record

- GIVEN an empty database
- WHEN `GetByIdAsync(999)` is called
- THEN the method returns `null` without throwing

#### SCEN-011 — AddAsync stages entity for insert

- GIVEN a valid entity instance
- WHEN `AddAsync` is called followed by `UnitOfWork.SaveAsync`
- THEN the entity is persisted and receives a non-zero `Id`

---

### REQ-007 — UnitOfWork

`UnitOfWork` in `API.Infrastructure/Persistence/` MUST implement `IUnitOfWork`
(defined in `API.Domain`). It MUST expose `Task<int> SaveAsync(CancellationToken ct = default)`
that delegates to `DbContext.SaveChangesAsync`. `IUnitOfWork` MUST be the only
path through which repositories trigger persistence.

#### SCEN-012 — Save commits staged changes

- GIVEN one or more entities staged via repository methods
- WHEN `UnitOfWork.SaveAsync()` is called
- THEN all staged changes are written to the database in a single transaction

---

### REQ-008 — Project References

| Reference | Direction |
|---|---|
| `API.Infrastructure → API.Domain` | Infrastructure implements Domain contracts |
| `API.Application → API.Domain` | Application depends on Domain |
| `API → API.Infrastructure` | Host wires Infrastructure |
| `API → API.Application` | Host wires Application |

No circular references are permitted. `API.Domain` MUST NOT reference any other project.

#### SCEN-013 — Solution builds after wiring

- GIVEN all project references above are added
- WHEN `dotnet build VehicleCatalog.slnx` is executed
- THEN the build succeeds with exit code 0 and no missing-reference errors

---

### REQ-009 — DI Registration

`Program.cs` MUST register:

| Service | Lifetime |
|---|---|
| `VehicleCatalogDbContext` via `AddDbContext<>` | Scoped |
| `ICarBrandRepository` → `CarBrandRepository` | Scoped |
| `ICarModelRepository` → `CarModelRepository` | Scoped |
| `IUnitOfWork` → `UnitOfWork` | Scoped |

The connection string MUST be read from `IConfiguration["ConnectionStrings:DefaultConnection"]`.
Controllers MUST inherit `ControllerBase`, not `Controller`.

#### SCEN-014 — App starts with valid connection string

- GIVEN `appsettings.json` contains a reachable SQL Server connection string
- WHEN the application starts
- THEN no DI resolution exceptions are thrown and the app reaches the request pipeline

#### SCEN-015 — Missing connection string throws on startup

- GIVEN `ConnectionStrings:DefaultConnection` is absent from configuration
- WHEN `AddDbContext` is called during startup
- THEN an `InvalidOperationException` or equivalent is raised before the app accepts requests

---

### REQ-010 — Connection String Configuration

`appsettings.json` MUST contain a `ConnectionStrings` section with a
`DefaultConnection` key. The value MUST be a valid SQL Server connection string
pointing to a local or Docker instance. Environment variable
`ConnectionStrings__DefaultConnection` MUST override the file value at runtime.

#### SCEN-016 — Environment variable overrides appsettings

- GIVEN `appsettings.json` has a placeholder connection string
- AND the environment variable `ConnectionStrings__DefaultConnection` is set to a valid connection string
- WHEN the app starts
- THEN the environment variable value is used for DbContext configuration

---

### REQ-011 — InitialCreate Migration

An EF Core migration named `InitialCreate` MUST be generated in `API/Migrations/`.
It MUST produce `Up`/`Down` methods that create and drop the `CarBrands` and
`CarModels` tables with all constraints defined in REQ-003 and REQ-004.

#### SCEN-017 — Migration generates successfully

- GIVEN all project references and DbContext are correctly wired
- WHEN `dotnet ef migrations add InitialCreate --project API.Infrastructure --startup-project API` is executed
- THEN migration files are created in `API/Migrations/` with no errors

#### SCEN-018 — Migration applies cleanly

- GIVEN a reachable SQL Server and the `InitialCreate` migration
- WHEN `dotnet ef database update` is executed
- THEN tables `CarBrands` and `CarModels` are created with the expected schema

---

### REQ-012 — Stub Cleanup

All `Class1.cs` files MUST be deleted from `API.Infrastructure` and `API.Application`
before the change is complete. No stub or placeholder types MUST remain.

#### SCEN-019 — No Class1.cs files remain

- GIVEN the change is applied
- WHEN the repository is searched for `Class1.cs`
- THEN no matches are found in any project directory
