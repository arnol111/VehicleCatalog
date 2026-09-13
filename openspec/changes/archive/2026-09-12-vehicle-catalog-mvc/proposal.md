# Proposal: Vehicle Catalog MVC Frontend

> Change: `vehicle-catalog-mvc`
> Date: 2026-09-12

## Intent

The solution is a REST API-only backend with no user-facing interface. This change adds a read-only ASP.NET Core MVC web application that consumes the existing API and presents a filterable vehicle model catalog to end users — without modifying any existing project.

## Scope

### In Scope
- New `VehicleCatalog.Web` MVC project (`src/VehicleCatalog.Web/`, `net10.0`) added to `VehicleCatalog.slnx`
- Scaffold via `dotnet new mvc`, remove boilerplate, add required files
- `ICarCatalogService` / `CarCatalogService` using `IHttpClientFactory` (typed client)
- `VehiculosController` with `Index(string? marca)` action
- ViewModels: `CarBrandViewModel`, `CarModelViewModel`, `VehiculosIndexViewModel`
- Single Razor view: brand filter dropdown + Bootstrap 5 table with empty-state message
- `ApiSettings:BaseUrl` config key in `appsettings.json`
- Dev SSL bypass / `dotnet dev-certs` documentation
- Ports distinct from API (`5023`/`7294` already taken)

### Out of Scope
- `GET /CarBrand?id=` endpoint — not used by this page
- Authentication / authorization
- Database access from the MVC layer
- Edit, create, or delete operations
- Pagination or client-side search

## Capabilities

### New Capabilities
- `vehicle-catalog-web`: MVC frontend that displays and filters vehicle models via the existing REST API

### Modified Capabilities
- None

## Approach

Scaffold with `dotnet new mvc -n VehicleCatalog.Web -f net10.0` under `src/` to get Bootstrap 5, `_Layout.cshtml`, `wwwroot/`, and Razor infrastructure for free. Remove generated `HomeController` boilerplate. Add the service layer, viewmodels, controller, and view. Register a typed `HttpClient` bound to `ICarCatalogService`. Read `ApiSettings:BaseUrl` from config. Handle SSL via dev-cert trust (documented) and/or `DangerousAcceptAnyServerCertificateValidator` for local dev only.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `VehicleCatalog.slnx` | Modified | Add `src/VehicleCatalog.Web/` project reference |
| `src/VehicleCatalog.Web/` | New | Full MVC project (controller, services, viewmodels, views) |
| `openspec/config.yaml` | Modified | Register new project after creation |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| SSL validation failure calling `https://localhost:7294` | Med | Document `dotnet dev-certs https --trust`; add dev-only bypass in `Program.cs` |
| Port collision when running both projects | Low | Assign ports `5025`/`7296` (or any free pair) to the MVC project |
| API unreachable — `BaseUrl` misconfigured | Low | Validate config at startup; controller try/catch surfaces friendly error |

## Rollback Plan

Remove `src/VehicleCatalog.Web/` directory and delete the corresponding `<Project>` entry from `VehicleCatalog.slnx`. No existing projects are modified; rollback is non-destructive.

## Dependencies

- API running at `https://localhost:7294/` (existing, no changes required)
- .NET SDK 10.0.401 (confirmed present)
- `dotnet dev-certs https --trust` executed on the dev machine

## Success Criteria

- [ ] Full vehicle model list displays on landing (`/Vehiculos`)
- [ ] Brand filter dropdown reloads the table with filtered models
- [ ] "Todas las marcas" option resets to the full list
- [ ] Empty state message shown when no models match the selected brand
- [ ] Bootstrap 5 responsive layout renders correctly on mobile viewport
- [ ] Solution builds with `dotnet build` without errors or warnings
