# Proposal: .NET MAUI Vehicle Catalog Client

> Change: `vehicle-catalog-maui`
> Date: 2026-09-14

## Intent

The solution exposes a REST API with no mobile client. This change adds a single-screen .NET MAUI application that consumes the existing API and presents a filterable vehicle model catalog — without modifying any existing project.

## Scope

### In Scope
- New `VehicleCatalog.Maui` project (`src/VehicleCatalog.Maui/`, TFM: `net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0`) added to `VehicleCatalog.slnx`
- Models: `CarBrand.cs`, `CarModel.cs` (match API response shapes, camelCase-insensitive deserialization)
- `CarCatalogService.cs` — plain class, single `HttpClient`, compile-time `BaseUrl` constant with `#if ANDROID` guard
- `MainPage.xaml` — `Picker` (brands + "Todas las marcas"), `ActivityIndicator`, `CollectionView` (name/brand/year), empty-state `Label`
- `MainPage.xaml.cs` — code-behind, `ObservableCollection<CarModel> Modelos`, `OnAppearing()`, `SelectedIndexChanged` handler, try/catch + `DisplayAlert` on HTTP failure
- `MauiProgram.cs` — minimal `CreateMauiApp()` bootstrap; `CarCatalogService` directly instantiated in code-behind
- Solution registration: XML edit to `VehicleCatalog.slnx` inside the `/src/` folder element

### Out of Scope
- `GET /CarBrand?id=` legacy endpoint — not used
- Authentication / authorization
- Navigation (multi-page, Shell)
- DI container or third-party MVVM framework
- Pagination, search, or edit/create/delete operations
- iOS build on this machine (requires macOS)

## Capabilities

### New Capabilities
- `vehicle-catalog-maui-app`: MAUI single-screen client that displays and filters vehicle models via the existing REST API

### Modified Capabilities
- None

## Approach

Scaffold with `dotnet new maui -n VehicleCatalog.Maui -f net10.0` under `src/`. Remove generated boilerplate pages. Add `Models/`, `Services/`, and replace `MainPage.xaml(.cs)` with the single-screen UI. `CarCatalogService` holds one `HttpClient` instance and exposes two async methods (`GetBrandsAsync`, `GetModelsAsync`). Code-behind binds `ObservableCollection<CarModel>` directly to `CollectionView.ItemsSource`. `Picker` loads brands on `OnAppearing()`; `SelectedIndexChanged` calls `GetModelsAsync` with the selected brand name (or no param for "Todas las marcas"). All HTTP calls wrapped in try/catch; non-2xx and network errors surface via `DisplayAlert`.

Platform URL strategy: compile-time `#if ANDROID` constant — `http://10.0.2.2:<port>/` on Android, `https://localhost:7294/` elsewhere. See **Confirmable Decision CD-1** below.

## Key Decisions

| # | Decision | Rationale |
|---|----------|-----------|
| D-1 | TFM: `net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0` | Only SDK 10.0.401 + MAUI workloads on `10.0.100` base installed; "net8" in requirement is a typo |
| D-2 | Project at `src/VehicleCatalog.Maui/` | Matches solution naming convention (`VehicleCatalog.Web`); lives under `src/` |
| D-3 | Code-behind + `ObservableCollection`, no MVVM framework | Explicit user requirement; acceptable for single-page scope |
| D-4 | `CarCatalogService` instantiated directly in code-behind | No DI container per requirement; single instance sufficient |
| D-5 | JSON deserialized with `PropertyNameCaseInsensitive = true` | API returns camelCase; MAUI models use PascalCase |
| D-6 | Empty-brand guard: never send `?brand=` param | API returns 400 on empty string; "Todas las marcas" → call without param |
| D-7 | Non-2xx / network errors → `DisplayAlert("Error", "No se pudo conectar con la API", "OK")` | Matches stated requirement |
| D-8 | iOS target included in TFM but excluded from local build/verify | Build on this machine: Windows + Android only; iOS requires macOS (acceptable per requirement) |

## Confirmable Decisions

### CD-1 — Android HTTP Base URL (⚠️ requires user confirmation)

The Android emulator cannot reach `localhost`. The app must use `http://10.0.2.2:<port>/`.

The API's HTTPS port is `7294`. However, the `.http` dev profile in `launchSettings.json` also exposes an **HTTP** endpoint — its port needs to be confirmed.

**Options:**
- **Option A (recommended):** Use `http://10.0.2.2:5174/` if the API's HTTP profile runs on port `5174` (common default for .NET 10 Web API). Avoids all cert issues on Android.
- **Option B:** Use `https://10.0.2.2:7294/` and add `ServerCertificateCustomValidationCallback = (_, _, _, _) => true` on the Android `HttpClientHandler`. Keeps HTTPS but adds platform-specific handler code.
- **Option C:** Expose the API explicitly on HTTP port `5000` or another fixed port and hardcode that.

**Action needed:** Confirm which HTTP port the API is actually exposed on (run `dotnet run --project src/API` and check the console output for `Now listening on: http://...`). The spec phase will hardcode the confirmed port.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `VehicleCatalog.slnx` | Modified | Add `src/VehicleCatalog.Maui/` project reference |
| `src/VehicleCatalog.Maui/` | New | Full MAUI project (models, service, main page, program) |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Android HTTP port unknown until API is run | Med | Flag as CD-1 above; spec phase blocked until confirmed |
| `CarModelsController` returns 404 on unknown brand | Med | Catch `HttpRequestException` on non-2xx; show `DisplayAlert` |
| Self-signed HTTPS cert rejected on Android | Med | Use HTTP on Android (CD-1 Option A) to avoid entirely |
| iOS target untestable on this machine | Low | Accept: requirement says "at least one emulator/simulator" |
| `.slnx` + MAUI project: untested combination | Low | Manual XML edit — format is straightforward |
| SDK `10.0.401` vs workload `10.0.100` minor gap | Low | Same major; .NET forward-compatible within major |

## Rollback Plan

Remove `src/VehicleCatalog.Maui/` directory and delete the corresponding `<Project>` entry from `VehicleCatalog.slnx`. No existing projects are modified; rollback is non-destructive.

## Dependencies

- API running at `https://localhost:7294/` (existing, no changes required)
- .NET SDK 10.0.401 + MAUI workloads (confirmed present)
- Android emulator or Windows machine for verification
- **CD-1 resolved** before spec phase begins

## Success Criteria

- [ ] Full vehicle model list (name, brand, year) displays on app open
- [ ] Brand `Picker` loads all brands from `/api/carBrands`
- [ ] Selecting a brand filters the list via `/api/carModels?brand=<name>`
- [ ] "Todas las marcas" restores the full unfiltered list
- [ ] Empty-state label shown when no models match selected brand
- [ ] `ActivityIndicator` visible during HTTP calls
- [ ] Network/HTTP errors surface via `DisplayAlert` without crashing
- [ ] Solution builds with `dotnet build -f net10.0-windows10.0.19041.0` and/or `-f net10.0-android`
