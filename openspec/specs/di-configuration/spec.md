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
