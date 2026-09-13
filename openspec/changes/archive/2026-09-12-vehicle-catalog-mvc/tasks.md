# Tasks: Vehicle Catalog MVC Frontend

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | 350–450 |
| 400-line budget risk | Medium |
| Chained PRs recommended | Yes |
| Suggested split | PR 1 (scaffold + config + models) → PR 2 (service + controller + views) |
| Delivery strategy | ask-on-risk |
| Chain strategy | pending |

Decision needed before apply: Yes
Chained PRs recommended: Yes
Chain strategy: pending
400-line budget risk: Medium

### Suggested Work Units

| Unit | Goal | Likely PR | Focused test command | Runtime harness | Rollback boundary |
|------|------|-----------|----------------------|-----------------|-------------------|
| 1 | Scaffold + config + ViewModels | PR 1 | `dotnet build src/VehicleCatalog.Web/` | `dotnet run --project src/VehicleCatalog.Web/` (startup check) | Delete `src/VehicleCatalog.Web/`; remove slnx entry |
| 2 | Service + Controller + Views | PR 2 | `dotnet test` (unit tests for service + controller) | `https://localhost:5025/Vehiculos` with API running | Revert PR 2 only; PR 1 remains clean |

---

## Phase 1: Scaffold & Configuration

- [x] 1.1 Run `dotnet new mvc -n VehicleCatalog.Web -f net10.0` under `src/`
- [x] 1.2 Run `dotnet sln VehicleCatalog.slnx add src/VehicleCatalog.Web/VehicleCatalog.Web.csproj`
- [x] 1.3 Update `src/VehicleCatalog.Web/Properties/launchSettings.json` — set HTTP port `5025`, HTTPS port `7296`
- [x] 1.4 Verify `wwwroot/lib/bootstrap/` exists from libman scaffold; confirm no CDN fallback needed
- [x] 1.5 Add `"ApiSettings": { "BaseUrl": "https://localhost:7294/" }` to `src/VehicleCatalog.Web/appsettings.json`

## Phase 2: ViewModels

- [x] 2.1 Create `src/VehicleCatalog.Web/Models/CarBrandViewModel.cs` — `{ int Id, string Name }`
- [x] 2.2 Create `src/VehicleCatalog.Web/Models/CarModelViewModel.cs` — `{ int Id, string Name, int Year, string BrandName }`
- [x] 2.3 Create `src/VehicleCatalog.Web/Models/VehiculosIndexViewModel.cs` — `{ List<CarModelViewModel> Modelos, List<CarBrandViewModel> Marcas, string? MarcaSeleccionada }`

## Phase 3: Service Layer

- [x] 3.1 Create `src/VehicleCatalog.Web/Services/ICarCatalogService.cs` — interface with `GetBrandsAsync()` and `GetModelsAsync(string? brand)`
- [x] 3.2 Create `src/VehicleCatalog.Web/Services/CarCatalogService.cs` — typed `HttpClient` implementation; `GetBrandsAsync()` → `GET api/carBrands`; `GetModelsAsync` → `GET api/carModels` or `api/carModels?brand={Uri.EscapeDataString(brand)}`; use `PropertyNameCaseInsensitive = true`

## Phase 4: Program.cs Wiring

- [x] 4.1 Update `src/VehicleCatalog.Web/Program.cs` — register `AddHttpClient<ICarCatalogService, CarCatalogService>` with `BaseAddress` from config
- [x] 4.2 Add dev-only SSL bypass block behind `IsDevelopment()` using `DangerousAcceptAnyServerCertificateValidator`
- [x] 4.3 Set default route to `{controller=Vehiculos}/{action=Index}/{id?}`

## Phase 5: Controller

- [x] 5.1 Create `src/VehicleCatalog.Web/Controllers/VehiculosController.cs` — inject `ICarCatalogService`
- [x] 5.2 Implement `[HttpGet] Index(string? marca)`: normalize null/"Todas" → null; call service; build `VehiculosIndexViewModel`; wrap in try/catch → `ViewBag.ErrorMessage` + `return View("Error")`

## Phase 6: Views

- [x] 6.1 Create `src/VehicleCatalog.Web/Views/Vehiculos/Index.cshtml` — `@model VehiculosIndexViewModel`; GET form with `<select name="marca" onchange="this.form.submit()">`; "Todas las marcas" option; Bootstrap 5 table (Modelo | Marca | Año); empty-state `alert` when no results; `ViewData["Title"] = "Catálogo de Vehículos"`
- [x] 6.2 Update `src/VehicleCatalog.Web/Views/Shared/_Layout.cshtml` — nav title "Catálogo de Vehículos"; ensure Bootstrap 5 local lib reference
- [x] 6.3 Update `src/VehicleCatalog.Web/Views/Shared/Error.cshtml` — render `ViewBag.ErrorMessage` inside Bootstrap `alert-danger`; remove any stack trace output

## Phase 7: Tests

- [x] 7.1 Create unit test: `CarCatalogService` — assert correct `RequestUri` when `brand` is null (no query string) and when `brand` has a value (encoded query string); mock `HttpMessageHandler`
- [x] 7.2 Create unit test: `VehiculosController.Index` — assert `VehiculosIndexViewModel` is populated; assert error path returns view named `"Error"` with `ViewBag.ErrorMessage` set; mock `ICarCatalogService`

## Phase 8: Cleanup

- [x] 8.1 Remove unused boilerplate controllers/views from scaffold (e.g. `HomeController`, `Privacy.cshtml`) if not needed
- [x] 8.2 Update `openspec/config.yaml` — register `VehicleCatalog.Web` project
- [x] 8.3 Do a final `dotnet build` + manual smoke test at `https://localhost:5025/Vehiculos` with API running
