# Specs: ImplementacionEnpoints

**Change**: ImplementacionEnpoints
**Date**: 2026-09-10
**Artifact store**: openspec

---

# car-brand-list Specification

## Purpose

Defines requirements for the `GET /api/carBrands` endpoint that returns all car brands.

## Requirements

### Requirement: List All Car Brands

The system MUST expose `GET /api/carBrands` and return all car brands persisted in the database.

#### Scenario: Brands exist

- GIVEN the database contains one or more car brands
- WHEN a client sends `GET /api/carBrands`
- THEN the response status is 200 OK
- AND the body is a JSON array of objects with `id` and `name` fields

#### Scenario: No brands in database

- GIVEN the database has no car brand records
- WHEN a client sends `GET /api/carBrands`
- THEN the response status is 200 OK
- AND the body is an empty JSON array `[]`

#### Scenario: Database error

- GIVEN an unexpected database failure occurs
- WHEN a client sends `GET /api/carBrands`
- THEN the response status is 500 Internal Server Error

---

# car-model-list Specification

## Purpose

Defines requirements for the `GET /api/carModels` endpoint with optional brand-name filtering.

## Requirements

### Requirement: List All Car Models

The system MUST expose `GET /api/carModels` and return all models with their brand name and year.

#### Scenario: Models exist, no filter

- GIVEN the database contains car models
- WHEN a client sends `GET /api/carModels` with no query parameters
- THEN the response status is 200 OK
- AND the body is a JSON array of `{ id, name, year, brandName }`

#### Scenario: No models in database

- GIVEN the database has no car model records
- WHEN a client sends `GET /api/carModels`
- THEN the response status is 200 OK
- AND the body is an empty JSON array `[]`

### Requirement: Filter Car Models by Brand Name

The system MUST support filtering models via `?Brand={name}`. The match MUST be case-insensitive.

#### Scenario: Filter returns matching models

- GIVEN car models exist for brand "Toyota"
- WHEN a client sends `GET /api/carModels?Brand=toyota`
- THEN the response status is 200 OK
- AND the body contains only models whose brand name matches "Toyota" (case-insensitive)

#### Scenario: Filter returns no matches for valid brand with no models

- GIVEN brand "EmptyBrand" exists but has no associated models
- WHEN a client sends `GET /api/carModels?Brand=EmptyBrand`
- THEN the response status is 200 OK
- AND the body is an empty JSON array `[]`

#### Scenario: Brand param is empty string

- GIVEN the query parameter `Brand` is present but empty (`?Brand=`)
- WHEN a client sends `GET /api/carModels?Brand=`
- THEN the response status is 400 Bad Request

#### Scenario: Brand name does not exist

- GIVEN no brand matching the provided name exists in the database
- WHEN a client sends `GET /api/carModels?Brand=NonExistent`
- THEN the response status is 404 Not Found

#### Scenario: Database error

- GIVEN an unexpected database failure occurs
- WHEN a client sends `GET /api/carModels`
- THEN the response status is 500 Internal Server Error

---

# scalar-ui Specification

## Purpose

Defines requirements for interactive API documentation via Scalar UI.

## Requirements

### Requirement: Scalar UI Accessibility

The system MUST serve Scalar UI at a configured route (e.g. `/scalar`) when running in the Development environment.

#### Scenario: Scalar UI loads in Development

- GIVEN the application runs in the Development environment
- WHEN a browser navigates to `/scalar` (or `/docs`)
- THEN an interactive Scalar UI page renders successfully

#### Scenario: OpenAPI JSON is accessible

- GIVEN the application is running
- WHEN a client requests `GET /openapi/v1.json`
- THEN the response is a valid OpenAPI JSON document

#### Scenario: Scalar UI not exposed in Production

- GIVEN the application runs in the Production environment
- WHEN a client requests the Scalar UI route
- THEN the response is 404 Not Found or the route is not registered

---

# di-configuration Specification

## Purpose

Defines requirements for correct DI lifetimes, handler registrations, and deduplication in Program.cs.

## Requirements

### Requirement: IDispatcher Registered as Scoped

The system MUST register `IDispatcher` with Scoped lifetime. It MUST NOT be registered as Singleton.

#### Scenario: IDispatcher resolves without captive-dependency error

- GIVEN the application starts with `IDispatcher` registered as Scoped
- WHEN a request is handled that invokes the dispatcher
- THEN no `InvalidOperationException` related to lifetime mismatch is thrown

### Requirement: All Handlers Registered in DI

The system MUST explicitly register all query handlers in DI before the application starts.

| Handler | Lifetime |
|---|---|
| `GetByIdQueryHandler` (CarBrand) | Transient |
| `GetAllQueryHandler` (CarBrand) | Transient |
| `GetAllQueryHandler` (CarModel) | Transient |

#### Scenario: Handler resolves at runtime

- GIVEN all handlers are registered in DI
- WHEN any registered endpoint is called
- THEN the dispatcher resolves the correct handler without throwing

#### Scenario: Missing handler throws at call time

- GIVEN a handler is NOT registered in DI
- WHEN an endpoint that requires that handler is called
- THEN an `InvalidOperationException` is thrown at dispatch time

### Requirement: No Duplicate DI Registrations

The system MUST NOT register `VehicleCatalogDbContext` or `IUnitOfWork` more than once in `Program.cs`.

#### Scenario: No duplicate registrations on startup

- GIVEN `AddInfrastructure()` registers DbContext and UnitOfWork
- WHEN `Program.cs` is inspected and the app starts
- THEN no service type appears registered twice in the DI container

---

# Delta for ef-core-persistence

## MODIFIED Requirements

### Requirement: Repository Interface CRUD Contracts

`ICarBrandRepository` and `ICarModelRepository` in `API.Domain` MUST each declare:

| Method | Signature |
|---|---|
| GetByIdAsync | `Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)` |
| GetAllAsync | `Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)` |
| AddAsync | `Task AddAsync(TEntity entity, CancellationToken ct = default)` |
| Update | `void Update(TEntity entity)` |
| Delete | `void Delete(TEntity entity)` |

`ICarModelRepository` MUST additionally declare:

| Method | Signature |
|---|---|
| GetAllByBrandIdAsync | `Task<IReadOnlyList<CarModel>> GetAllByBrandIdAsync(int brandId, CancellationToken ct = default)` |

Interfaces MUST NOT expose `SaveChangesAsync` — persistence is the UnitOfWork's concern.

(Previously: `ICarModelRepository` had no `GetAllByBrandIdAsync` method)

#### Scenario: Interface contract is complete

- GIVEN `ICarBrandRepository` is inspected
- WHEN checking declared members
- THEN all five base methods are present with matching signatures

#### Scenario: ICarModelRepository exposes brand filter method

- GIVEN `ICarModelRepository` is inspected
- WHEN checking declared members
- THEN `GetAllByBrandIdAsync(int brandId, CancellationToken ct)` is present

#### Scenario: GetAllByBrandIdAsync filters at DB level

- GIVEN 50 models exist across 10 brands and a valid brand ID is provided
- WHEN `GetAllByBrandIdAsync(brandId)` is called
- THEN only models belonging to that brand ID are returned

### Requirement: CarModel Fluent API Configuration

`CarModelConfiguration` MUST implement `IEntityTypeConfiguration<CarModel>` and apply:

| Rule | Constraint |
|---|---|
| Primary key | `Id` |
| FK to `CarBrand` | Explicit `HasOne` / `WithMany` / `HasForeignKey("CarBrandId")` |
| `Model` column | `HasMaxLength(200)`, `IsRequired` |

Because `CarModel` has no navigation property to `CarBrand`, the relationship
MUST be configured explicitly — convention-based inference MUST NOT be relied upon.

(Previously: No change to config rules — this block preserved from REQ-004 for archive fidelity)

#### Scenario: CarModel FK constraint is enforced

- GIVEN a `CarModel` referencing a non-existent `CarBrandId`
- WHEN `SaveChangesAsync` is called
- THEN the operation fails with a foreign-key constraint violation

#### Scenario: Migration schema reflects explicit FK

- GIVEN `CarModelConfiguration` is applied
- WHEN a migration is generated
- THEN the migration contains an explicit `AddForeignKey` for `CarBrandId → CarBrands.Id`

#### Scenario: Migration can be generated and applied

- GIVEN all project references and DbContext are correctly wired
- WHEN `dotnet ef migrations add` and `dotnet ef database update` are executed
- THEN the migration applies without errors and FK constraint exists in the database
