# Exploration: vehicle-catalog-mvc

> Generated: 2026-09-12  
> Change: `vehicle-catalog-mvc`  
> Artifact store: openspec

---

## Current State

The repository is a Clean Architecture solution (`VehicleCatalog.slnx`) with four existing .NET 10 projects:

| Project | SDK | Role |
|---|---|---|
| `src/API` | `Microsoft.NET.Sdk.Web` | ASP.NET Core Web API host (controllers, program, config) |
| `src/API.Application` | `Microsoft.NET.Sdk` | Use-case layer (queries, dispatcher, DTOs) |
| `src/API.Domain` | `Microsoft.NET.Sdk` | Entities, repository interfaces, exceptions |
| `src/API.Infrastructure` | `Microsoft.NET.Sdk` | EF Core, data access |

**There is NO existing MVC project.** The solution is purely a REST API backend. The MVC frontend must be created from scratch as a new project added to the solution.

The API is confirmed running at `https://localhost:7294` (HTTPS profile in `launchSettings.json`). All three required endpoints exist and are implemented:

- `GET /api/carBrands` → `CarBrandResponse(int Id, string Name)` — `CarBrandsController`
- `GET /api/carModels?brand=` → `CarModelResponse(int Id, string Name, int Year, string BrandName)` — `CarModelsController`
- `GET /CarBrand?id=` → `CarBrandDTO { IdCarBrand, Brand }` — `CarBrandController`

---

## Affected Areas

- `VehicleCatalog.slnx` — must add the new MVC project reference
- `src/` — new subfolder `src/Web.MVC/` (or `src/VehicleCatalog.Web/`) will be created here
- `openspec/config.yaml` — should be updated to register the new project after creation

**Nothing in the existing API is modified.** The MVC project is purely additive.

---

## Key Findings

### What EXISTS (no action needed)
- ✅ .NET SDK 10.0.401 — matches target framework `net10.0`
- ✅ API project running at `https://localhost:7294` with all required endpoints
- ✅ API response shapes confirmed: `CarBrandResponse`, `CarModelResponse`, `CarBrandDTO` match the spec exactly
- ✅ `openspec/` directory and `config.yaml` already initialized
- ✅ Solution file (`VehicleCatalog.slnx`) uses the modern `.slnx` format

### What DOES NOT EXIST (must be created)
- 🔲 MVC project (`src/Web.MVC/` — `Microsoft.NET.Sdk.Web`, `net10.0`)
- 🔲 `Controllers/VehiculosController.cs`
- 🔲 `Services/ICarCatalogService.cs` + `Services/CarCatalogService.cs`
- 🔲 `Models/CarBrandViewModel.cs`, `Models/CarModelViewModel.cs`, `Models/VehiculosIndexViewModel.cs`
- 🔲 `Views/Vehiculos/Index.cshtml`
- 🔲 `Views/Shared/_Layout.cshtml`, `Views/Shared/Error.cshtml`
- 🔲 `appsettings.json` (with `ApiBaseUrl` key for `https://localhost:7294`)
- 🔲 `Program.cs` (registers `IHttpClientFactory`, `ICarCatalogService`, MVC services)
- 🔲 Bootstrap 5 (via `libman.json` or bundled from MVC template `wwwroot/`)
- 🔲 Project reference added to `VehicleCatalog.slnx`

---

## Approaches

### 1. Scaffold from `dotnet new mvc` — recommended

Use `dotnet new mvc -n VehicleCatalog.Web -f net10.0` inside `src/`, then surgically add the required files.

- **Pros:** Gets Bootstrap 5, `_Layout.cshtml`, `Error.cshtml`, `wwwroot/`, `libman.json`, and `bundleconfig.json` for free from the template. No manual wiring of static assets.
- **Cons:** Template generates `HomeController` and `WeatherForecastController` boilerplate that must be removed.
- **Effort:** Low

### 2. Hand-craft the project from scratch

Create `.csproj`, all folders, and all files manually.

- **Pros:** No cleanup of template noise; full control over what is created.
- **Cons:** Must manually wire Bootstrap 5 CDN or `libman`, create all Razor infrastructure, higher risk of missing a required file.
- **Effort:** Medium

---

## Recommendation

**Use Approach 1** (scaffold + trim). The `dotnet new mvc` template for .NET 10 ships with Bootstrap 5 and the correct Razor layout infrastructure. The only post-scaffold work is removing `HomeController` boilerplate, then adding `VehiculosController`, the service layer, and the viewmodels. This minimises risk and effort.

Suggested project name: `VehicleCatalog.Web` → placed at `src/VehicleCatalog.Web/`.

---

## Risks

1. **Self-signed HTTPS certificate** — `HttpClient` calling `https://localhost:7294` in development will fail SSL validation unless `HttpClientHandler.ServerCertificateCustomValidationCallback` is set to bypass in development, or the dev cert is trusted (`dotnet dev-certs https --trust`). Must be addressed in `Program.cs`.
2. **Port collision** — if the MVC project and the API project are launched simultaneously, they need different ports. The API uses `5023`/`7294`; the MVC project must use a different pair (e.g. `5024`/`7295`).
3. **API base URL configuration** — `appsettings.json` in the existing API project has no `ApiBaseUrl` key. The new MVC project must introduce its own `appsettings.json` with `"ApiBaseUrl": "https://localhost:7294"` and read it via `IConfiguration` in `Program.cs`.
4. **`CarBrand?id=` endpoint** — the proposed spec only calls `/api/carBrands` and `/api/carModels`, but lists `GET /CarBrand?id=` as an available endpoint. The `Index` action does not appear to use it; confirm it is out of scope for the initial page.
5. **Solution file format** — `VehicleCatalog.slnx` is the new XML-based solution format. `dotnet sln` commands still work; `dotnet sln add` correctly updates `.slnx`.

---

## Open Questions

- Should the MVC project live at `src/VehicleCatalog.Web/` (consistent with existing naming) or `src/Web/`?
- Is Bootstrap via CDN acceptable, or must it be bundled locally (libman)?
- Is the `GET /CarBrand?id=` endpoint intentionally out of scope for this page?

---

## Ready for Proposal

**Yes.** All API contracts are verified, the SDK is confirmed at 10.0.401, and there are no blocking conflicts with existing code. The proposal phase can proceed with the scaffold-and-trim approach.
