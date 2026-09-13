# Design: Vehicle Catalog MVC Frontend

> Change: `vehicle-catalog-mvc`
> Date: 2026-09-12

---

## Technical Approach

Scaffold `VehicleCatalog.Web` with `dotnet new mvc -n VehicleCatalog.Web -f net10.0` inside `src/`, remove boilerplate, then add service layer, viewmodels, controller, and view. Register a typed `HttpClient` bound to `ICarCatalogService`. All API calls are read-only; no existing project is modified.

---

## Architecture Decisions

| Decision | Choice | Alternatives | Rationale |
|----------|--------|--------------|-----------|
| Scaffold strategy | `dotnet new mvc` + trim | Hand-craft from scratch | Template ships Bootstrap 5, `_Layout.cshtml`, `wwwroot/` — avoids manual wiring risk |
| HTTP client pattern | Typed `HttpClient` via `IHttpClientFactory` | `IHttpClientFactory` named client; plain `HttpClient` singleton | Typed client binds DI lifetime correctly; avoids socket exhaustion; testable interface |
| View filter interaction | GET form + `onchange="this.form.submit()"` | AJAX partial update | Simpler; no JS dependency; bookmarkable URL with `?marca=` |
| Error handling | Controller try/catch → `Error.cshtml` + `ViewBag.ErrorMessage` | Global middleware | Scoped to the one action; keeps the pattern local and explicit |
| SSL in development | `dotnet dev-certs https --trust` (primary); dev-only bypass behind `IsDevelopment()` flag | CDN proxy; ignore SSL globally | Keeps production safe; bypass is clearly gated |

---

## Data Flow

```
Browser GET /Vehiculos?marca=Toyota
        │
        ▼
VehiculosController.Index(marca)
        │
        ├── ICarCatalogService.GetModelsAsync(marca) ──► GET https://localhost:7294/api/carModels?brand=Toyota
        │                                                 ◄── List<CarModelViewModel>
        │
        └── ICarCatalogService.GetBrandsAsync()      ──► GET https://localhost:7294/api/carBrands
                                                          ◄── List<CarBrandViewModel>
        │
        ▼
VehiculosIndexViewModel { Modelos, Marcas, MarcaSeleccionada }
        │
        ▼
Views/Vehiculos/Index.cshtml  ──►  HTML (Bootstrap 5 table + filter form)
```

---

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `VehicleCatalog.slnx` | Modify | Add `src/VehicleCatalog.Web/VehicleCatalog.Web.csproj` reference |
| `src/VehicleCatalog.Web/VehicleCatalog.Web.csproj` | Create | MVC project targeting `net10.0`; ports `5025`/`7296` |
| `src/VehicleCatalog.Web/Program.cs` | Create | DI: MVC, typed HttpClient, default route to `Vehiculos/Index` |
| `src/VehicleCatalog.Web/appsettings.json` | Create | `ApiSettings:BaseUrl` → `https://localhost:7294/` |
| `src/VehicleCatalog.Web/Controllers/VehiculosController.cs` | Create | `GET Index(string? marca)` — calls service, builds VM, handles errors |
| `src/VehicleCatalog.Web/Services/ICarCatalogService.cs` | Create | Interface: `GetBrandsAsync`, `GetModelsAsync` |
| `src/VehicleCatalog.Web/Services/CarCatalogService.cs` | Create | Typed HttpClient implementation |
| `src/VehicleCatalog.Web/Models/CarBrandViewModel.cs` | Create | `{ int Id, string Name }` |
| `src/VehicleCatalog.Web/Models/CarModelViewModel.cs` | Create | `{ int Id, string Name, int Year, string BrandName }` |
| `src/VehicleCatalog.Web/Models/VehiculosIndexViewModel.cs` | Create | `{ List<CarModelViewModel> Modelos, List<CarBrandViewModel> Marcas, string? MarcaSeleccionada }` |
| `src/VehicleCatalog.Web/Views/Vehiculos/Index.cshtml` | Create | Filter form + Bootstrap 5 table + empty-state message |
| `src/VehicleCatalog.Web/Views/Shared/_Layout.cshtml` | Create | Base layout: title "Catálogo de Vehículos", Bootstrap 5 nav |
| `src/VehicleCatalog.Web/Views/Shared/Error.cshtml` | Create | Renders `ViewBag.ErrorMessage`; no stack traces |
| `openspec/config.yaml` | Modify | Register `VehicleCatalog.Web` project after creation |

---

## Interfaces / Contracts

```csharp
// ICarCatalogService.cs
public interface ICarCatalogService
{
    Task<List<CarBrandViewModel>> GetBrandsAsync();
    Task<List<CarModelViewModel>> GetModelsAsync(string? brand = null);
}
```

```csharp
// Program.cs — DI registration
builder.Services.AddHttpClient<ICarCatalogService, CarCatalogService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
});

// Dev-only SSL bypass
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient<ICarCatalogService, CarCatalogService>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Vehiculos}/{action=Index}/{id?}");
```

---

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | `CarCatalogService` — correct URL construction with/without `brand`; null-safe return on empty response | Mock `HttpMessageHandler`; assert `RequestUri` |
| Unit | `VehiculosController.Index` — VM populated correctly; error path returns `"Error"` view | Mock `ICarCatalogService`; assert `ViewResult` and `ViewBag` |
| Integration (manual) | Full page loads at `https://localhost:5025/Vehiculos` with API running | Browser + DevTools network tab |
| Manual | Brand filter narrows table; "Todas" resets; empty-state renders | Test against live API |

---

## Threat Matrix

N/A — no routing changes to existing API, no shell commands in application code, no subprocess integration, no VCS/PR automation, no executable-file classification.

---

## Migration / Rollout

No migration required. The MVC project is purely additive. To roll back: delete `src/VehicleCatalog.Web/` and remove the `<Project>` entry from `VehicleCatalog.slnx`.

---

## Open Questions

- [x] ~~Project path `src/VehicleCatalog.Web/` vs `src/Web/`~~ — resolved: `src/VehicleCatalog.Web/` (consistent with solution naming)
- [x] ~~`GET /CarBrand?id=` in scope?~~ — resolved: out of scope (proposal confirms it)
- [ ] Bootstrap delivery: CDN (from template) or local `libman`? Template default is CDN; confirm acceptable for target deployment.
