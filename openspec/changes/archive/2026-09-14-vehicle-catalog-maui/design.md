# Design: Aplicación .NET MAUI — Catálogo de Modelos de Vehículos

> Change: `vehicle-catalog-maui`
> Date: 2026-09-14

---

## Technical Approach

Scaffold `VehicleCatalog.Maui` with `dotnet new maui -n VehicleCatalog.Maui -f net10.0` under `src/`, trim boilerplate pages, then place `Models/`, `Services/`, and a single `MainPage.xaml(.cs)`. `CarCatalogService` owns one `HttpClient` instance with a compile-time `BaseUrl` constant guarded by `#if ANDROID`. Code-behind exposes `ObservableCollection<CarModel> Modelos` directly as `CollectionView.ItemsSource`; no MVVM framework is used. `MainPage` registers in `VehicleCatalog.slnx` via manual XML edit.

---

## Architecture Decisions

| Decision | Choice | Alternatives | Rationale |
|----------|--------|--------------|-----------|
| HTTP client lifecycle | Single `private readonly HttpClient` in `CarCatalogService` constructor | `IHttpClientFactory` / per-request `new` | NFR-02 requirement; factory overkill for single-page app with no DI |
| Base URL strategy | Compile-time `#if ANDROID` constant | Runtime config / `appsettings.json` | NFR-03 + REQ-6; no DI, no config system needed; deterministic per TFM |
| Android port | `http://10.0.2.2:5023/` (HTTP) | HTTPS `7294` + cert bypass | Avoids self-signed cert rejection on Android; port 5023 confirmed in spec |
| Service instantiation | `new CarCatalogService()` in `MainPage` constructor | DI / factory | NFR-03 explicit requirement |
| UI binding strategy | `CollectionView.ItemsSource = Modelos` in code-behind; no `BindingContext` | Full MVVM with `BindingContext` | NFR-05; single-page scope; keeps XAML minimal |
| Empty-state visibility | `emptyLabel.IsVisible = Modelos.Count == 0` after each collection update | `CollectionView.EmptyView` | Explicit and debuggable; matches code-behind pattern |
| Picker guard (double-fire) | `bool _isLoadingBrands` flag; skip handler body when `true` | Unsubscribe/resubscribe event | Least-invasive; flag set before and cleared after Picker population |
| `.slnx` registration | Manual XML `<Project>` element insertion | `dotnet sln add` | `dotnet sln` does not support `.slnx` format |

---

## Data Flow

```
MainPage.OnAppearing()
    │
    ├── _service.GetBrandsAsync()  ──► GET /api/carBrands
    │       ◄── List<CarBrand>
    │       → _isLoadingBrands = true
    │       → Picker.Items: ["Todas las marcas", "BMW", "Toyota", ...]
    │       → _isLoadingBrands = false
    │
    └── _service.GetModelsAsync()  ──► GET /api/carModels
            ◄── List<CarModel>
            → Modelos.Clear() + AddRange
            → emptyLabel.IsVisible = (Modelos.Count == 0)

Picker.SelectedIndexChanged (guard: skip if _isLoadingBrands)
    │
    ├── index == 0  → _service.GetModelsAsync(null)   ──► GET /api/carModels
    └── index > 0   → _service.GetModelsAsync(name)   ──► GET /api/carModels?brand=<name>
            ◄── List<CarModel>
            → Modelos.Clear() + AddRange
            → emptyLabel.IsVisible = (Modelos.Count == 0)

On HttpRequestException / non-2xx:
    → ActivityIndicator hidden
    → DisplayAlert("Error", "No se pudo conectar con la API", "OK")
```

---

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `VehicleCatalog.slnx` | Modify | Add `<Project Path="src/VehicleCatalog.Maui/VehicleCatalog.Maui.csproj" />` inside the `<Folder Name="/src/">` element |
| `src/VehicleCatalog.Maui/VehicleCatalog.Maui.csproj` | Create | MAUI project; TFM `net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0`; `<UseMaui>true</UseMaui>` |
| `src/VehicleCatalog.Maui/MauiProgram.cs` | Create | Minimal `CreateMauiApp()` bootstrap; sets `MainPage = new MainPage()` |
| `src/VehicleCatalog.Maui/MainPage.xaml` | Create | Single-screen UI: `Picker`, `ActivityIndicator`, `CollectionView`, empty-state `Label` |
| `src/VehicleCatalog.Maui/MainPage.xaml.cs` | Create | Code-behind: `Modelos`, `OnAppearing()`, `SelectedIndexChanged`, try/catch, `DisplayAlert` |
| `src/VehicleCatalog.Maui/Models/CarBrand.cs` | Create | `{ int Id, string Name }` |
| `src/VehicleCatalog.Maui/Models/CarModel.cs` | Create | `{ int Id, string Name, int Year, string BrandName }` |
| `src/VehicleCatalog.Maui/Services/CarCatalogService.cs` | Create | `HttpClient` wrapper; `GetBrandsAsync` / `GetModelsAsync`; `#if ANDROID` BaseUrl |
| `src/VehicleCatalog.Maui/AppShell.xaml` | Delete (if scaffolded) | MAUI template generates Shell; remove — app uses `MainPage` directly |
| `src/VehicleCatalog.Maui/AppShell.xaml.cs` | Delete (if scaffolded) | Same reason |

---

## Interfaces / Contracts

```csharp
// Models/CarBrand.cs
public class CarBrand
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Models/CarModel.cs
public class CarModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public string BrandName { get; set; } = string.Empty;
}
```

```csharp
// Services/CarCatalogService.cs
public class CarCatalogService
{
#if ANDROID
    private const string BaseUrl = "http://10.0.2.2:5023/";
#else
    private const string BaseUrl = "https://localhost:7294/";
#endif

    private readonly HttpClient _client;

    public CarCatalogService()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public async Task<List<CarBrand>> GetBrandsAsync() { ... }

    // brand == null or empty → omit query param entirely
    public async Task<List<CarModel>> GetModelsAsync(string? brand = null) { ... }
    // Deserialization: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
    // Both methods call EnsureSuccessStatusCode() — throws HttpRequestException on non-2xx
}
```

```csharp
// MainPage.xaml.cs — key members
public partial class MainPage : ContentPage
{
    private readonly CarCatalogService _service = new();
    public ObservableCollection<CarModel> Modelos { get; } = new();
    private bool _isLoadingBrands;   // Picker double-fire guard

    protected override async void OnAppearing() { ... }
    private async void BrandPicker_SelectedIndexChanged(object sender, EventArgs e) { ... }
    private void SetLoading(bool loading) { /* activityIndicator.IsRunning/IsVisible */ }
    private void RefreshModelos(List<CarModel> models)
    {
        Modelos.Clear();
        foreach (var m in models) Modelos.Add(m);
        emptyLabel.IsVisible = Modelos.Count == 0;
    }
}
```

```xml
<!-- MainPage.xaml — element tree (abbreviated) -->
<ContentPage ...>
  <VerticalStackLayout Padding="16" Spacing="8">
    <Picker x:Name="brandPicker"
            Title="Seleccionar marca"
            SelectedIndexChanged="BrandPicker_SelectedIndexChanged" />
    <ActivityIndicator x:Name="activityIndicator"
                       IsRunning="False" IsVisible="False" />
    <Label x:Name="emptyLabel"
           Text="No se encontraron modelos"
           IsVisible="False" HorizontalOptions="Center" />
    <CollectionView x:Name="modelsCollection">
      <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="models:CarModel">
          <VerticalStackLayout Padding="8">
            <Label Text="{Binding Name}" FontAttributes="Bold" />
            <Label Text="{Binding BrandName}" />
            <Label Text="{Binding Year}" />
          </VerticalStackLayout>
        </DataTemplate>
      </CollectionView.ItemTemplate>
    </CollectionView>
  </VerticalStackLayout>
</ContentPage>
```

`CollectionView.ItemsSource` is set in code-behind constructor: `modelsCollection.ItemsSource = Modelos;` — no `BindingContext` needed on the page.

---

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Manual (Windows) | Full load on `OnAppearing()`; Picker filter; "Todas las marcas" reset; empty-state; `ActivityIndicator` | Run app targeting `net10.0-windows10.0.19041.0`; observe UI |
| Manual (Android) | Same scenarios + verify `http://10.0.2.2:5023/` resolves from emulator | Android emulator with API running; check Logcat on error |
| Build verification | `dotnet build -f net10.0-windows10.0.19041.0` and `dotnet build -f net10.0-android` pass without errors | CI / local terminal |
| Error path (manual) | Stop API; launch app → `DisplayAlert` appears; no crash | Kill API process; reopen app |

---

## Threat Matrix

N/A — no routing changes to existing API, no shell commands in application code, no subprocess integration, no VCS/PR automation, no executable-file classification, no process-integration boundary.

---

## Migration / Rollout

No migration required. The MAUI project is purely additive. Rollback: delete `src/VehicleCatalog.Maui/` and remove the `<Project>` entry from `VehicleCatalog.slnx`.

### Build Gotchas

- `<UseMaui>true</UseMaui>` is required in `.csproj`; without it, MAUI SDK targets are not injected.
- MAUI workloads must be installed: `dotnet workload install maui-android maui-windows` (already confirmed present).
- `AppShell.xaml` generated by template conflicts with direct `MainPage` use; remove it and set `MainPage = new MainPage()` in `MauiProgram.cs`.
- iOS TFM is declared but excluded from local `dotnet build` targets (D-8); CI on Windows must specify `-f net10.0-windows10.0.19041.0` or `-f net10.0-android` explicitly.

---

## Open Questions

- [x] ~~Android HTTP port (CD-1)~~ — resolved: `http://10.0.2.2:5023/` (confirmed in spec REQ-6)
- [ ] `CollectionView` item separator: none (default) or `ItemSeparatorHeight`? Template default; confirm acceptable.
