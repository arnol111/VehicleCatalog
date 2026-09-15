# Exploration: .NET MAUI Vehicle Catalog Client

**Change:** `vehicle-catalog-maui`  
**Date:** 2026-09-14  
**Status:** Ready for Proposal

---

## Current State

The solution (`VehicleCatalog.slnx`) is a .NET 10 Clean Architecture project with:

| Project | Role | Framework |
|---|---|---|
| `src/API` | ASP.NET Core Web API host | `net10.0` |
| `src/API.Application` | Use-case layer, DTOs | `net10.0` |
| `src/API.Domain` | Entities + repo interfaces | `net10.0` |
| `src/API.Infrastructure` | EF Core, repositories | `net10.0` |
| `src/VehicleCatalog.Web` | MVC frontend (reference) | `net10.0` |

A fully-working MVC web frontend was previously built and archived under  
`openspec/changes/archive/2026-09-12-vehicle-catalog-mvc/`. It serves as the  
primary reference pattern for the MAUI client.

No MAUI project exists yet. The solution has no iOS/macOS targets at all.

---

## API Response Shapes — Verified

Verified directly from DTOs and controllers in `src/API.Application/DTOs/` and `src/API/Controllers/`:

### `GET /api/carBrands` → `CarBrandResponse[]`
```csharp
public record CarBrandResponse(int Id, string Name);
```
JSON: `[{ "id": 1, "name": "Toyota" }, ...]`

### `GET /api/carModels?brand={name}` → `CarModelResponse[]`
```csharp
public record CarModelResponse(int Id, string Name, int Year, string BrandName);
```
JSON: `[{ "id": 1, "name": "Corolla", "year": 2022, "brandName": "Toyota" }, ...]`

Filter note: `brand` param is the **brand name** (string), not an id. Sending an empty string
returns HTTP 400. Sending a non-existent brand name returns HTTP 404. Both must be caught.

### `GET /CarBrand?id={int}` → `CarBrandDTO`
```csharp
public class CarBrandDTO { public int IdCarBrand; public string Brand; }
```
This is a legacy controller (`CarBrandController.cs`) — the MAUI client does NOT need it;
use `/api/carBrands` (plural) instead.

---

## Affected Areas

| Path | Impact |
|---|---|
| `VehicleCatalog.slnx` | New MAUI project must be added to the solution |
| `src/API/Program.cs` | No change needed; CORS not required for Android loopback |
| `src/VehicleCatalog.Web/Services/CarCatalogService.cs` | Reference pattern for service design |
| `src/VehicleCatalog.Web/Models/CarBrandViewModel.cs` | Reference for MAUI model shape |
| `src/VehicleCatalog.Web/Models/CarModelViewModel.cs` | Reference for MAUI model shape |
| `src/VehicleCatalog.Web/Program.cs` | Reference for dev SSL bypass pattern |
| *(new)* `src/VehicleCatalog.Maui/` | New MAUI app project |

---

## Open Decisions

### OD-1: Target Framework — net10.0-* vs net8.0-*

**Evidence:**
- `dotnet --list-sdks` → single SDK installed: **10.0.401**
- `dotnet workload list` → `android 36.1.69/10.0.100`, `ios 26.5.10301/10.0.100`, `maui-windows 10.0.20/10.0.100`
- All workload manifests are on `10.0.100` base → no .NET 8 MAUI workload present.
- The rest of the solution targets `net10.0`.

**Conclusion:** Use **`net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0`** (or the
full MAUI TFM set). The "net8" mention in the requirement is a typo/copy-paste error.
Using net8 would require a separate SDK and workloads that are NOT installed.

**Decision to flag in proposal:** Target `net10.0` across all MAUI TFMs.

---

### OD-2: Project Location and Naming

**Recommendation:** `src/VehicleCatalog.Maui/VehicleCatalog.Maui.csproj`

Rationale:
- Follows existing naming convention (`VehicleCatalog.Web` for the MVC app).
- Lives under `src/` consistent with the solution layout.
- MAUI projects are self-contained; no reference to `API.Domain`/`API.Application` needed.

**Solution registration:** Add inside the existing `/src/` folder element of `VehicleCatalog.slnx`:
```xml
<Project Path="src/VehicleCatalog.Maui/VehicleCatalog.Maui.csproj" />
```
Feasibility: `.slnx` format supports this natively — no issues expected.

---

### OD-3: Base URL / Platform Handling

**MVC reference pattern** (`src/VehicleCatalog.Web/Program.cs`):
- Base URL from `appsettings.json` → `ApiSettings:BaseUrl`
- Dev SSL bypass via `DangerousAcceptAnyServerCertificateValidator`

**MAUI constraints differ:**
- Android emulator cannot reach `localhost`; must use `http://10.0.2.2:<port>/`
- Android emulator may reject self-signed HTTPS certs without extra config
- iOS Simulator and Windows can use `https://localhost:7294/`

**Approach A — Simple constant (recommended for scope):**
```csharp
// CarCatalogService.cs
#if ANDROID
private const string BaseUrl = "http://10.0.2.2:5179/";
#else
private const string BaseUrl = "https://localhost:7294/";
#endif
```
- Pros: Zero config, minimal code, matches acceptance criteria
- Cons: Hardcoded; Android uses HTTP (acceptable for dev emulator)

**Approach B — Platform-conditional with HTTPS + cert bypass on Android:**
```csharp
// Also bypass SSL validation on Android via HttpClientHandler
#if ANDROID
handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
```
- Pros: Keeps HTTPS everywhere
- Cons: Slightly more code; HttpClient configuration in MAUI requires platform-specific setup

**Recommendation:** Approach A + Approach B combined — use `http://10.0.2.2` on Android  
(simpler, avoids cert issues) and `https://localhost:7294/` on other platforms. The cert  
bypass is needed on Android only if HTTPS is used; with HTTP it is moot.

---

### OD-4: Build Constraints

**Finding:** `maui-windows` workload is installed (`10.0.20/10.0.100`). Android workload present.
iOS/macOS workloads present but building iOS requires a macOS host (not available on Windows).

**For verification:**
- `dotnet build -f net10.0-windows10.0.19041.0` → should work on this machine
- `dotnet build -f net10.0-android` → should work (Android workload present)
- `dotnet build -f net10.0-ios` → will fail (needs macOS); exclude from CI

---

## Approaches

### Approach 1 — Code-behind with ObservableCollection (recommended)

Single `MainPage.xaml` + `MainPage.xaml.cs`. `CarCatalogService` injected or instantiated
directly in code-behind. `ObservableCollection<CarModel>` bound to `CollectionView`.
`Picker` bound to brands list. No MVVM framework, no DI container.

| Aspect | Detail |
|---|---|
| Pros | Minimal complexity, matches stated requirement, easy to verify, aligns with MVC service pattern already in codebase |
| Cons | Code-behind gets heavier if the app grows — acceptable for single-page scope |
| Effort | Low |
| Risk | Low |

### Approach 2 — Minimal MVVM (ViewModel class, no framework)

Hand-rolled `MainPageViewModel : INotifyPropertyChanged`. Keeps XAML binding clean.
Still no DI, no third-party libraries.

| Aspect | Detail |
|---|---|
| Pros | Cleaner separation if the app grows; XAML binding is more idiomatic |
| Cons | More boilerplate; user explicitly asked for code-behind |
| Effort | Medium |
| Risk | Low |

**Selected:** Approach 1 per explicit user requirement.

---

## Folder Structure (inside `src/VehicleCatalog.Maui/`)

```
VehicleCatalog.Maui/
├── Models/
│   ├── CarBrand.cs          — { int Id, string Name }
│   └── CarModel.cs          — { int Id, string Name, int Year, string BrandName }
├── Services/
│   └── CarCatalogService.cs — HttpClient wrapper, BaseUrl constant
├── MainPage.xaml            — CollectionView + Picker + loading state
├── MainPage.xaml.cs         — code-behind, ObservableCollection, event handlers
├── MauiProgram.cs           — app bootstrap, HttpClient registration
└── VehicleCatalog.Maui.csproj
```

Model property names match the JSON response (camelCase deserialized to PascalCase via
`PropertyNameCaseInsensitive = true`), consistent with the MVC `CarBrandViewModel` /
`CarModelViewModel` pattern.

---

## Risks

| Risk | Severity | Mitigation |
|---|---|---|
| MAUI build on this machine targets Windows/Android only; iOS untestable | Low | Accept: requirement says "at least one emulator/simulator" |
| `CarModelsController` returns 404 on unknown brand — MAUI must handle it gracefully | Medium | Catch `HttpRequestException` on non-2xx; show `DisplayAlert` |
| `CarModelsController` returns 400 on empty `brand` query string — must not send empty string | Low | Send `brand` param only when non-null/non-empty |
| Dev API runs on HTTPS with self-signed cert; Android emulator may reject it | Medium | Use `http://10.0.2.2` on Android (avoids cert issue entirely) |
| `.slnx` format for MAUI project: untested combination; `dotnet sln` may not support `.slnx` natively | Low | Add entry manually via XML edit — format is straightforward |
| `net10.0` MAUI workload is `10.0.100`-based; SDK is `10.0.401` — minor version gap | Low | Same major; .NET is forward-compatible within major |

---

## Ready for Proposal

**Yes.** All open decisions are resolved or flagged with a clear recommendation:
- Target framework: **net10.0** (only installed SDK/workloads)
- Project location: **`src/VehicleCatalog.Maui/`**
- Architecture: **code-behind + ObservableCollection** (per requirement)
- Base URL: **compile-time constant with `#if ANDROID`**
- API shapes: **verified** — match the stated requirement exactly
- Build: **Windows + Android** buildable on this machine; iOS requires macOS
