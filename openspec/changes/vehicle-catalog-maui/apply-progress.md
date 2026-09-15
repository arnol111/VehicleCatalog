# Apply Progress: vehicle-catalog-maui

> Change: `vehicle-catalog-maui`
> Work unit: PR 2 — MainPage UI + MauiProgram + slnx registration + build verification
> Date: 2026-09-14
> Mode: Standard (strict_tdd: false)

## Status

18/18 tasks complete (PR 1: tasks 1.1–3.4 ✅; PR 2: tasks 4.1–4.6, 5.1–5.2, 6.1–6.2 ✅; manual smoke tests 6.3–6.5 ✅ confirmed by user on 2026-09-14).

---

## Completed Tasks (PR 1 — scaffold + models + service)

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

## Completed Tasks (PR 2 — UI + bootstrap + slnx + build)

- [x] 4.1 `MainPage.xaml` replaced: `VerticalStackLayout` with `Picker`, `ActivityIndicator`, empty-state `Label`, `CollectionView` with `DataTemplate x:DataType="models:CarModel"` (Name/BrandName/Year); `xmlns:models` namespace added
- [x] 4.2 `MainPage.xaml.cs`: `ObservableCollection<CarModel> Modelos`; `modelsCollection.ItemsSource = Modelos` in constructor; `CarCatalogService _service = new()`
- [x] 4.3 `OnAppearing()`: `SetLoading(true/false)` in finally; brands loaded with "Todas las marcas" prepended; `_isLoadingBrands` guard set before/after Picker population; `GetModelsAsync()` → `RefreshModelos()`; try/catch → `DisplayAlert`
- [x] 4.4 `BrandPicker_SelectedIndexChanged`: guard on `_isLoadingBrands`; index 0 → no brand param; index >0 → brand name; `RefreshModelos()` + `emptyLabel` toggle; try/catch → `DisplayAlert`; `SetLoading` in finally
- [x] 4.5 `SetLoading(bool loading)`: toggles `activityIndicator.IsRunning` and `activityIndicator.IsVisible`
- [x] 4.6 `RefreshModelos(List<CarModel>)`: `Modelos.Clear()`; foreach Add; `emptyLabel.IsVisible = Modelos.Count == 0`
- [x] 5.1 `MauiProgram.cs`: confirmed existing scaffold satisfies design — `.UseMauiApp<App>()` + font config; no DI registration; `App.xaml.cs` boots to `new MainPage()` (already patched in PR 1). No changes needed.
- [x] 5.2 `VehicleCatalog.slnx`: added `<Project Path="src/VehicleCatalog.Maui/VehicleCatalog.Maui.csproj" />` inside `<Folder Name="/src/">` via manual XML edit
- [x] 6.1 `dotnet build src/VehicleCatalog.Maui/ -f net10.0-windows10.0.19041.0`: **Build succeeded. 6 Warning(s). 0 Error(s). Time: 00:00:10.59**
- [x] 6.2 `dotnet build src/VehicleCatalog.Maui/ -f net10.0-android`: **Build succeeded. 3 Warning(s). 0 Error(s). Time: 00:01:35.57**

## Completed Tasks (manual smoke tests — confirmed by user 2026-09-14)

- [x] 6.3 Manual smoke test (Windows): start API; run app; verify model list, Picker, brand filter, "Todas las marcas" reset — **user confirmed OK**
- [x] 6.4 Manual error-path test: stop API; verify `DisplayAlert`; no crash; `ActivityIndicator` hidden — **user confirmed OK**
- [x] 6.5 Manual empty-state test: select brand with no models; verify empty-state label — **user confirmed OK**

---

## Work Unit Evidence (PR 2)

| Evidence | Value |
|---|---|
| Focused test command (Windows) | `dotnet build src/VehicleCatalog.Maui/ -f net10.0-windows10.0.19041.0` |
| Exact result (Windows) | **Build succeeded. 6 Warning(s). 0 Error(s). Time 00:00:10.59** |
| Focused test command (Android) | `dotnet build src/VehicleCatalog.Maui/ -f net10.0-android` |
| Exact result (Android) | **Build succeeded. 3 Warning(s). 0 Error(s). Time 00:01:35.57** |
| Runtime harness | N/A for automated — launching a MAUI desktop/emulator app is not feasible in this non-interactive agent environment; manual tasks 6.3–6.5 remain pending for user |
| Rollback boundary | Remove `<Project Path="src/VehicleCatalog.Maui/VehicleCatalog.Maui.csproj" />` from `VehicleCatalog.slnx`; revert `MainPage.xaml` and `MainPage.xaml.cs` to stubs. Does not affect PR 1 work (models, service, scaffold). |

### Build Warnings (non-blocking, informational)

- `CS0618` (×4 Windows, ×2 Android): `DisplayAlert(string, string, string)` is obsolete in .NET MAUI 10 — `DisplayAlertAsync` is preferred. The spec explicitly requires `DisplayAlert`; this warning is expected and does not affect functionality.
- `CS8622` (×2 Windows, ×1 Android): Nullability mismatch on `EventHandler` delegate generated by MAUI XAML source generator for `BrandPicker_SelectedIndexChanged`. This is a MAUI source-gen artefact, not a code-behind issue; signature matches the standard event handler pattern.

---

## Deviations (PR 2)

- **`MauiProgram.cs` not rewritten**: Existing scaffold already satisfies NFR-03/NFR-04 and design intent (`.UseMauiApp<App>()`, no DI). App bootstrap routes through `App.xaml.cs → CreateWindow → new MainPage()` (patched in PR 1). Rewriting would add noise without value.
- **`DisplayAlert` vs `DisplayAlertAsync`**: Spec REQ-5 mandates `DisplayAlert("Error", "No se pudo conectar con la API", "OK")` verbatim. Used as specified. CS0618 warning is accepted.

## PR 1 Deviations (carried forward)

- **Branch name**: `feature/vehicle-catalog-maui/pr-1` (not `feat/`). Git ref collision prevented `feat/` prefix.
- **`App.xaml.cs`**: Patched from `AppShell` to `new MainPage()` (required for compilation after AppShell removal).
- **Unused import removed** from `App.xaml.cs`: `using Microsoft.Extensions.DependencyInjection`.
