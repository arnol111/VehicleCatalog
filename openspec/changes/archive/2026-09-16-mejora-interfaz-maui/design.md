# Design: Mejora de Interfaz — App .NET MAUI (Paleta de colores + Navegación)

## Technical Approach

Replace template palette and single-window wiring with a semantic color system + two-tab Shell. All changes are pure XAML/code-behind in the MAUI project — no MVVM, DI, or new packages. Prerequisite: merge `feature/vehicle-catalog-maui/pr-2` before any edits.

## Architecture Decisions

| Decision | Options | Choice | Rationale |
|----------|---------|--------|-----------|
| Color key naming | Flat `XxxLight`/`XxxDark` vs nested `{AppThemeBinding}` in Colors.xaml | Flat suffixed keys — 24 total (12 tokens × 2) | AppThemeBinding inside a Color entry throws `XamlParseException` at runtime; flat keys are the only safe pattern |
| Template keys (Magenta, MidnightBlue, Gray*, Brushes) | Keep all / Keep neutrals / Remove all | **Remove**: Magenta, MidnightBlue, PrimaryDark (old purple), PrimaryDarkText, Secondary (old purple), SecondaryDarkText, Tertiary. **Keep**: White, Black, OffBlack, Gray100–Gray950, and all Brush entries | Magenta/MidnightBlue appear in Shell and Headline implicit styles — must be removed; Gray* kept because Button, Border, Entry, etc. still reference them |
| Shell title location | `Shell.Title` on AppShell element vs `ShellContent.Title` vs `ContentPage.Title` | Set `Title="Catálogo de Vehículos"` on `AppShell` element; tab labels via `Tab.Title` | Shell.Title shows in top nav bar when no page overrides it |
| Shell chrome color application | Implicit `Style TargetType="Shell"` in Styles.xaml vs inline on AppShell element | Implicit style in Styles.xaml (spec requirement) | Keeps XAML declarative; spec explicitly requires Styles.xaml |
| AppShell page reference | `local:ModelosPage` via xmlns:local vs route string | `xmlns:local` clr-namespace reference in ShellContent | Standard MAUI pattern; route strings are for programmatic navigation |
| CollectionView item separator | Alternating background (converter) vs bottom Border separator | **Bottom Border per item** using `Border` with `Stroke="{AppThemeBinding ...}"` inside DataTemplate | Alternating background requires `IValueConverter` with index workaround; a Border bottom line is pure XAML |
| MarcasPage load guard | `_loaded` bool flag vs `Brands.Count > 0` | `_loaded` bool field | Count check fails on empty API response; `_loaded` is explicit and mirrors `_isLoadingBrands` pattern in MainPage |
| MarcasPage exception scope | Catch `HttpRequestException` only vs catch `Exception` broadly | **Catch `HttpRequestException`** — same as existing ModelosPage pattern | Consistency with existing codebase pattern; broad `Exception` catch would swallow bugs silently |
| ModelosPage rename sequence | Rename file first vs update class first | Rename file → update `x:Class` + class name → update `App.xaml.cs` reference | Compiler errors contained to one file at a time |

## Design Constraints

1. **Zero hardcoded colors in page XAML**: All color values in `AppShell.xaml`, `ModelosPage.xaml`, and `MarcasPage.xaml` MUST reference `StaticResource` keys from `Colors.xaml` only. No inline hex literals, named color literals, or per-page `ResourceDictionary` entries that shadow app-level keys are permitted anywhere in page XAML.

2. **Empty-state label styling**: The ModelosPage empty-state label ("No se encontraron modelos") MUST use the `TextSecondary` token via the `LabelSecondary` keyed style (defined in Styles.xaml). Direct color assignment on the label is prohibited.

3. **API URL non-change**: Android hardcoded URL `10.0.2.2:5023` in `CarCatalogService` is an acknowledged pre-existing condition. No change to the URL is made in this change; it is documented here as a known risk per the proposal risk table.

## Data Flow

```
App.xaml.cs
  └─ MainPage = new AppShell()
       └─ TabBar
            ├─ Tab "Modelos" → ModelosPage
            │     OnAppearing → CarCatalogService.GetBrandsAsync()
            │                 → CarCatalogService.GetModelsAsync()
            │                 → ObservableCollection<CarModel>
            └─ Tab "Marcas"  → MarcasPage
                  OnAppearing (_loaded guard)
                  → CarCatalogService.GetBrandsAsync()
                  → ObservableCollection<CarBrand>
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `src/VehicleCatalog.Maui/Resources/Styles/Colors.xaml` | Modify | Remove template tokens. Add 24 flat semantic keys (12 tokens × Light/Dark) |
| `src/VehicleCatalog.Maui/Resources/Styles/Styles.xaml` | Modify | Update Shell implicit style (7 Shell.* properties). Update Headline/SubHeadline. Add `LabelSecondary` style. Update Picker and Page background |
| `src/VehicleCatalog.Maui/App.xaml.cs` | Modify | Replace `CreateWindow` override with `MainPage = new AppShell()` in constructor |
| `src/VehicleCatalog.Maui/AppShell.xaml` | Create | Shell + TabBar + 2 Tabs with ShellContent pointing to ModelosPage / MarcasPage |
| `src/VehicleCatalog.Maui/AppShell.xaml.cs` | Create | Standard `partial class AppShell : Shell` code-behind |
| `src/VehicleCatalog.Maui/MainPage.xaml` | Rename → `ModelosPage.xaml` | Update `x:Class="VehicleCatalog.Maui.ModelosPage"` |
| `src/VehicleCatalog.Maui/MainPage.xaml.cs` | Rename → `ModelosPage.xaml.cs` | Update class name to `ModelosPage` |
| `src/VehicleCatalog.Maui/MarcasPage.xaml` | Create | CollectionView bound to Brands, item template with Border separator; all colors via StaticResource |
| `src/VehicleCatalog.Maui/MarcasPage.xaml.cs` | Create | `ObservableCollection<CarBrand>`, `_loaded` guard, `catch (HttpRequestException)` DisplayAlert, `new CarCatalogService()` |
| `src/VehicleCatalog.Maui/MauiProgram.cs` | No change | Route registration not required for tab-based Shell with direct ShellContent references |

## Interfaces / Contracts

**Colors.xaml — 24 flat entries (all 12 tokens × Light/Dark):**

| Key | Light value | Dark value |
|-----|-------------|------------|
| `PrimaryLight` / `PrimaryDark` | `#1A73E8` | `#4D9FFF` |
| `PrimaryDarkLight` / `PrimaryDarkDark` | `#1557B0` | `#2979CC` |
| `PrimaryLightLight` / `PrimaryLightDark` | `#E8F0FE` | `#1E3A5F` |
| `SecondaryLight` / `SecondaryDark` | `#5F6368` | `#9AA0A6` |
| `AccentLight` / `AccentDark` | `#34A853` | `#57BB75` |
| `BackgroundLight` / `BackgroundDark` | `#FFFFFF` | `#121212` |
| `SurfaceLight` / `SurfaceDark` | `#F8F9FA` | `#1E1E1E` |
| `TextPrimaryLight` / `TextPrimaryDark` | `#202124` | `#E8EAED` |
| `TextSecondaryLight` / `TextSecondaryDark` | `#5F6368` | `#9AA0A6` |
| `DangerLight` / `DangerDark` | `#EA4335` | `#FF6B6B` |
| `WarningLight` / `WarningDark` | `#FBBC04` | `#FDD663` |
| `SuccessLight` / `SuccessDark` | `#34A853` | `#57BB75` |

> **Naming convention**: `{TokenName}Light` = the value used in light mode; `{TokenName}Dark` = the value used in dark mode. The tokens `PrimaryDark` and `PrimaryLight` (semantic variants of Primary) follow the same suffix rule: their flat keys are `PrimaryDarkLight`/`PrimaryDarkDark` and `PrimaryLightLight`/`PrimaryLightDark` respectively.

**Styles.xaml — Shell implicit style (7 properties, explicit mapping):**

```xml
<Style TargetType="Shell" ApplyToDerivedTypes="True">
  <!-- Shell.BackgroundColor → Primary token -->
  <Setter Property="Shell.BackgroundColor"
          Value="{AppThemeBinding Light={StaticResource PrimaryLight}, Dark={StaticResource PrimaryDark}}" />
  <!-- Shell.ForegroundColor → Background token (icon/text on bar) -->
  <Setter Property="Shell.ForegroundColor"
          Value="{AppThemeBinding Light={StaticResource BackgroundLight}, Dark={StaticResource BackgroundDark}}" />
  <!-- Shell.TitleColor → Background token -->
  <Setter Property="Shell.TitleColor"
          Value="{AppThemeBinding Light={StaticResource BackgroundLight}, Dark={StaticResource BackgroundDark}}" />
  <!-- Shell.TabBarBackgroundColor → Surface token -->
  <Setter Property="Shell.TabBarBackgroundColor"
          Value="{AppThemeBinding Light={StaticResource SurfaceLight}, Dark={StaticResource SurfaceDark}}" />
  <!-- Shell.TabBarForegroundColor → Primary token (selected tab icon/text) -->
  <Setter Property="Shell.TabBarForegroundColor"
          Value="{AppThemeBinding Light={StaticResource PrimaryLight}, Dark={StaticResource PrimaryDark}}" />
  <!-- Shell.TabBarTitleColor → Primary token -->
  <Setter Property="Shell.TabBarTitleColor"
          Value="{AppThemeBinding Light={StaticResource PrimaryLight}, Dark={StaticResource PrimaryDark}}" />
  <!-- Shell.TabBarUnselectedColor → Secondary token -->
  <Setter Property="Shell.TabBarUnselectedColor"
          Value="{AppThemeBinding Light={StaticResource SecondaryLight}, Dark={StaticResource SecondaryDark}}" />
</Style>
```

**AppShell.xaml skeleton:**
```xml
<Shell xmlns:local="clr-namespace:VehicleCatalog.Maui"
       Title="Catálogo de Vehículos">
  <TabBar>
    <Tab Title="Modelos">
      <ShellContent ContentTemplate="{DataTemplate local:ModelosPage}" />
    </Tab>
    <Tab Title="Marcas">
      <ShellContent ContentTemplate="{DataTemplate local:MarcasPage}" />
    </Tab>
  </TabBar>
</Shell>
```

**MarcasPage code-behind pattern:**
```csharp
public partial class MarcasPage : ContentPage
{
    private readonly CarCatalogService _service = new();
    public ObservableCollection<CarBrand> Brands { get; } = new();
    private bool _loaded;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded) return;
        _loaded = true;
        try
        {
            var brands = await _service.GetBrandsAsync();
            foreach (var b in brands) Brands.Add(b);
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Error", "No se pudo conectar con la API", "OK");
        }
    }
}
```

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Manual smoke | App launches, top bar shows "Catálogo de Vehículos", non-template colors visible | Android emulator — debug build |
| Manual smoke | Modelos tab active by default; tapping Marcas loads brand list | Visual verification |
| Manual smoke | Light/dark toggle in emulator updates all palette colors | System settings toggle |
| Manual smoke | No Magenta or MidnightBlue visible after palette replacement | Visual scan |
| Manual smoke | Empty-state label ("No se encontraron modelos") renders in secondary text color | Disconnect API, verify label color matches TextSecondary token |
| Build | Zero compilation errors after rename + new files | `dotnet build src/VehicleCatalog.Maui/VehicleCatalog.Maui.csproj -f net9.0-android` |

## Threat Matrix

N/A — no routing, shell commands, subprocess execution, VCS/PR automation, executable-file classification, or process-integration boundary. MAUI Shell navigation is declarative UI, not a security boundary.

## Migration / Rollout

No migration required. No data schema changes. Rollback: `git revert` feature branch. If only Shell wiring breaks, revert `App.xaml.cs` to restore `CreateWindow` override — app reverts to single-page mode without data loss.

**Prerequisite merge (must execute before any file edit):**
```bash
git merge feature/vehicle-catalog-maui/pr-2
```
Run on `feature/vehicle-catalog-maui/interfaz-paleta-navegacion`. This brings all MAUI source (pr-2 includes pr-1 commits). Do NOT touch `.slnx` — CI runs on Ubuntu and the MAUI project is intentionally excluded.

Merged lines from pr-2 are not authored by this change and must not count against the 400-line PR budget.

## Open Questions

- [ ] None blocking. CollectionView item separator chosen as bottom Border — confirm visual result is acceptable during smoke test.
