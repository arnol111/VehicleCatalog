# VehicleCatalog REST API

A REST API built with .NET 10 following Clean Architecture principles. Exposes endpoints to query car brands and car models stored in a SQL Server database.

---

## Architecture Overview

The solution is organized into four projects, each with a clearly bounded responsibility:

| Project | Role |
|---------|------|
| `API` | ASP.NET Core host — controllers, middleware pipeline, DI composition root |
| `API.Application` | Use cases — queries, handlers, DTOs, domain interfaces (no framework dependencies) |
| `API.Domain` | Core entities, repository interfaces, domain exceptions |
| `API.Infrastructure` | EF Core `DbContext`, repository implementations, DI registration (`AddInfrastructure`) |

Requests flow inward: `Controller → IDispatcher → IRequestHandler → IUnitOfWork → Repository → DbContext`.

---

## Tech Stack

- **.NET 10** — target framework
- **Entity Framework Core 10** — ORM and migrations
- **Scalar / OpenAPI** — interactive API documentation (Development only)
- **Custom Mediator** — lightweight `IDispatcher` / `IRequestHandler<TRequest, TResponse>` pattern (no MediatR)

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server or SQL Server LocalDB

---

## Setup & Run

1. **Clone the repository**

   ```bash
   git clone <repo-url>
   cd VehicleCatalog
   ```

2. **Configure the connection string**

   Edit `src/API/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VehicleCatalog;Trusted_Connection=True;"
     }
   }
   ```

3. **Apply database migrations**

   ```bash
   dotnet ef database update --project src/API.Infrastructure --startup-project src/API
   ```

4. **Run the API**

   ```bash
   dotnet run --project src/API
   ```

   The API starts at `https://localhost:7xxx` (port printed on startup).

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/carBrands` | Get all car brands |
| GET | `/api/carModels` | Get all car models with brand and year |
| GET | `/api/carModels?brand={name}` | Get models filtered by brand name (case-insensitive) |
| GET | `/carBrand?id={id}` | Get a single car brand by ID |

### Response shapes

**`GET /api/carBrands`**
```json
[{ "id": 1, "name": "Toyota" }, ...]
```

**`GET /api/carModels`** and **`GET /api/carModels?brand=toyota`**
```json
[{ "id": 1, "name": "Corolla", "year": 2022, "brandName": "Toyota" }, ...]
```

**`GET /carBrand?id=1`**
```json
{ "idCarBrand": 1, "brand": "Toyota" }
```

### Status codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 400 | `brand` query parameter is an empty string |
| 404 | Brand name not found (filtered models query) |
| 500 | Unexpected server error |

---

## API Documentation

When running in **Development** mode, the interactive Scalar UI is available at:

```
https://localhost:<port>/scalar/v1
```

The raw OpenAPI JSON spec is at:

```
https://localhost:<port>/openapi/v1.json
```

---

## Migrations

**Apply existing migrations to the database:**

```bash
dotnet ef database update --project src/API.Infrastructure --startup-project src/API
```

**Add a new migration after modifying entities or configurations:**

```bash
dotnet ef migrations add <MigrationName> --project src/API.Infrastructure --startup-project src/API
```

Inspect the generated migration file before applying to confirm there are no unexpected schema changes.

---

## Known Constraints

- No automated test project exists in this solution yet. Endpoint verification is performed manually via the Scalar UI or `.http` files.
- The custom mediator resolves handlers from the DI container — all `IRequestHandler<TRequest, TResponse>` implementations must be registered as `Transient` in `ServiceCollectionExtensions.cs`.
