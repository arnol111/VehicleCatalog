# Apply Progress: vehicle-catalog-maui

> Change: `vehicle-catalog-maui`
> Work unit: PR 1 — scaffold + models + service
> Date: 2026-09-14
> Mode: Standard (strict_tdd: false)

## Status

10/15 tasks complete (PR 1 scope: tasks 1.1–3.4 — all complete; PR 2 scope: 4.1–6.5 — pending).

## Completed Tasks (PR 1)

- [x] 1.1 Scaffolded with `dotnet new maui -n VehicleCatalog.Maui -f net10.0` under `src/`
- [x] 1.2 TFM set to `net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0`; `<UseMaui>true</UseMaui>` confirmed present
- [x] 1.3 Deleted `AppShell.xaml` and `AppShell.xaml.cs`; fixed `App.xaml.cs` to use `new MainPage()` instead of `new AppShell()`
- [x] 1.4 Cleared `MainPage.xaml` and `MainPage.xaml.cs` to minimal compilable stubs (UI implementation deferred to PR 2)
- [x] 2.1 Created `src/VehicleCatalog.Maui/Models/CarBrand.cs` — `{ int Id, string Name }`
- [x] 2.2 Created `src/VehicleCatalog.Maui/Models/CarModel.cs` — `{ int Id, string Name, int Year, string BrandName }`
- [x] 3.1 Created `src/VehicleCatalog.Maui/Services/CarCatalogService.cs` with `private readonly HttpClient _client`
- [x] 3.2 `#if ANDROID` BaseUrl: Android → `http://10.0.2.2:5023/`; else → `https://localhost:7294/`
- [x] 3.3 `GetBrandsAsync()` → `GET /api/carBrands`; `EnsureSuccessStatusCode()`; `PropertyNameCaseInsensitive = true`
- [x] 3.4 `GetModelsAsync(string? brand = null)` → `GET /api/carModels`; `?brand=` only when non-null/non-empty; same options

## Pending Tasks (PR 2)

- [ ] 4.1–4.6 MainPage UI (XAML + code-behind)
- [ ] 5.1 MauiProgram.cs
- [ ] 5.2 VehicleCatalog.slnx registration
- [ ] 6.1–6.5 Build verification + manual smoke tests

## Work Unit Evidence

| Evidence | Value |
|---|---|
| Focused test command | `dotnet build src/VehicleCatalog.Maui/ -f net10.0-windows10.0.19041.0` |
| Exact result | **Build succeeded. 0 Warning(s). 0 Error(s).** Time: 00:00:17.51 |
| Runtime harness | N/A — no UI boundary in this work unit; service verifiable by compilation only |
| Rollback boundary | Delete `src/VehicleCatalog.Maui/`; no `VehicleCatalog.slnx` change in this slice |

## Deviations

- **Branch name**: Used `feature/vehicle-catalog-maui/pr-1` instead of `feat/vehicle-catalog-maui/pr-1`. Git rejects creation of `feat/vehicle-catalog-maui/pr-1` because `refs/heads/feat/vehicle-catalog-maui` already exists as a branch; Git treats `/` as a namespace separator and the ref would collide. The `feature/` prefix is semantically identical.
- **`App.xaml.cs`**: Scaffold referenced `AppShell` in `CreateWindow()`; patched to `new MainPage()` to eliminate the broken reference after AppShell deletion. This is required for compilation and consistent with design (design.md: "set `MainPage = new MainPage()` in `MauiProgram.cs`" / App bootstrap).
- **`using Microsoft.Extensions.DependencyInjection`** removed from `App.xaml.cs` (was unused after AppShell removal).
