# Apply Progress: vehicle-catalog-mvc

## PR 1 — Scaffold + Configuration + ViewModels + Services + Program.cs Wiring

**Status**: Completed  
**Build**: ✅ `dotnet build` — 0 errors, 0 warnings  
**Mode**: Standard  
**Date**: 2026-09-12

---

## Completed Tasks (PR 1)

### Phase 1: Scaffold & Configuration
- [x] 1.1 Scaffolded `VehicleCatalog.Web` with `dotnet new mvc -n VehicleCatalog.Web -f net10.0` under `src/`
- [x] 1.2 Added to solution: `dotnet sln VehicleCatalog.slnx add src/VehicleCatalog.Web/VehicleCatalog.Web.csproj`
- [x] 1.3 Updated `launchSettings.json` — HTTP `5025`, HTTPS `7296`
- [x] 1.4 Verified `wwwroot/lib/bootstrap/` exists — Bootstrap 5 delivered locally by scaffold template (no CDN fallback needed)
- [x] 1.5 Added `ApiSettings.BaseUrl` to `appsettings.json`

### Phase 2: ViewModels
- [x] 2.1 Created `Models/CarBrandViewModel.cs`
- [x] 2.2 Created `Models/CarModelViewModel.cs`
- [x] 2.3 Created `Models/VehiculosIndexViewModel.cs`

### Phase 3: Service Layer (pulled into PR 1 to make Program.cs compile)
- [x] 3.1 Created `Services/ICarCatalogService.cs`
- [x] 3.2 Created `Services/CarCatalogService.cs` — full typed HttpClient implementation

### Phase 4: Program.cs Wiring
- [x] 4.1 Registered `AddHttpClient<ICarCatalogService, CarCatalogService>` with `BaseAddress` from config
- [x] 4.2 Dev-only SSL bypass behind `IsDevelopment()` using `DangerousAcceptAnyServerCertificateValidator`
- [x] 4.3 Default route set to `{controller=Vehiculos}/{action=Index}/{id?}`

---

## Work Unit Evidence (PR 1)

| Evidence | Value |
|---|---|
| Focused test command | `dotnet build` from `src/VehicleCatalog.Web/` |
| Build result | ✅ Compilación correcta — 0 Errores, 0 Advertencias — elapsed 00:00:03.84 |
| Runtime harness | N/A at this stage — no controller/views yet; app would 404 at `/Vehiculos` until PR 2 |
| Rollback boundary | Delete `src/VehicleCatalog.Web/`; remove `<Project>` entry from `VehicleCatalog.slnx` |

---

## PR 2 — Controller + Views + Unit Tests + Cleanup

**Status**: Completed  
**Build**: ✅ `dotnet build` — 0 errors, 0 warnings  
**Tests**: ✅ `dotnet test` — 6/6 passed  
**Mode**: Standard  
**Date**: 2026-09-12

---

## Completed Tasks (PR 2)

### Phase 5: Controller
- [x] 5.1 Created `Controllers/VehiculosController.cs` — injects `ICarCatalogService`
- [x] 5.2 Implemented `[HttpGet] Index(string? marca)`: normalizes null/"Todas" → null; calls service; builds `VehiculosIndexViewModel`; try/catch → `ViewBag.ErrorMessage` + `return View("Error")`

### Phase 6: Views
- [x] 6.1 Created `Views/Vehiculos/Index.cshtml` — brand filter `<select>` with auto-submit; Bootstrap 5 table (Modelo | Marca | Año); empty-state `alert-info`
- [x] 6.2 Updated `Views/Shared/_Layout.cshtml` — navbar brand "Catálogo de Vehículos"; removed Home/Privacy nav links; footer updated
- [x] 6.3 Updated `Views/Shared/Error.cshtml` — Bootstrap `alert-danger`; renders `ViewBag.ErrorMessage` or generic fallback; no stack trace

### Phase 7: Unit Tests
- [x] 7.1 Created `tests/VehicleCatalog.Web.Tests/VehiculosTests.cs` — `CarCatalogServiceTests`: 3 tests covering null brand (no query), brand with value, brand with spaces (URL encoded)
- [x] 7.2 `VehiculosControllerTests`: 3 tests — populated ViewModel returned, error path returns `"Error"` view with `ViewBag.ErrorMessage`, "Todas" normalized to null

### Phase 8: Cleanup
- [x] 8.1 Deleted `Controllers/HomeController.cs` and `Views/Home/` directory (scaffold boilerplate)
- [x] 8.2 Updated `openspec/config.yaml` — registered `VehicleCatalog.Web` and `tests/VehicleCatalog.Web.Tests/`
- [x] 8.3 Final `dotnet build` ✅ — 0 errors; `dotnet test` ✅ — 6/6 passed

---

## Work Unit Evidence (PR 2)

| Evidence | Value |
|---|---|
| Focused test command | `dotnet test tests/VehicleCatalog.Web.Tests/` |
| Test result | ✅ Passed: 6, Failed: 0, Skipped: 0 — elapsed 161 ms |
| Runtime harness | `https://localhost:5025/Vehiculos` — manual smoke test after API is running; controller + views are wired and build clean |
| Rollback boundary | Revert PR 2: delete `Controllers/VehiculosController.cs`, `Views/Vehiculos/`, restore `Views/Shared/_Layout.cshtml` and `Error.cshtml`, delete `tests/VehicleCatalog.Web.Tests/` |

---

## Files Created / Modified (PR 2)

| File | Action |
|------|--------|
| `src/VehicleCatalog.Web/Controllers/VehiculosController.cs` | Created |
| `src/VehicleCatalog.Web/Controllers/HomeController.cs` | Deleted |
| `src/VehicleCatalog.Web/Views/Vehiculos/Index.cshtml` | Created |
| `src/VehicleCatalog.Web/Views/Shared/_Layout.cshtml` | Modified — brand text, nav links, footer |
| `src/VehicleCatalog.Web/Views/Shared/Error.cshtml` | Modified — Bootstrap alert-danger, no stack trace |
| `src/VehicleCatalog.Web/Views/Home/` | Deleted (entire directory) |
| `tests/VehicleCatalog.Web.Tests/VehiculosTests.cs` | Created — 6 unit tests |
| `tests/VehicleCatalog.Web.Tests/VehicleCatalog.Web.Tests.csproj` | Created |
| `openspec/config.yaml` | Modified — added VehicleCatalog.Web entry |

---

## Deviations from Design

- **RZ1031 Razor tag helper constraint**: The `<option selected="@(...)">` pattern used in the spec's view snippet is rejected by Razor's tag helper parser (error RZ1031). Replaced with explicit `@if`/`else` blocks rendering `<option selected>` or `<option>` directly. Functionally identical behavior, compliant with Razor's tag helper rules.

---

## PR Boundary

- **Mode**: Chained PR slice (stacked-to-main)
- **PR 1**: Scaffold + config + ViewModels + Services + Program.cs — ✅ Complete
- **PR 2**: Controller + Views + Tests + Cleanup — ✅ Complete
- **Implementation complete**: All 22 tasks done (Phases 1–8)
- **Estimated review budget PR 2**: ~180 authored lines — within 400-line budget
