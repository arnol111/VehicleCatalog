# Proposal: Mejora de Interfaz — App .NET MAUI (Paleta de colores + Navegación)

## Intent

The MAUI app ships with stock template colors (Magenta, MidnightBlue) and no Shell navigation — a single page with no way to browse brands. This change replaces the template palette with a clean automotive-neutral design system and introduces a two-tab Shell so users can navigate between vehicle models and brands.

## Scope

### In Scope
- Define semantic color tokens in `Colors.xaml` with light/dark variants (AppThemeBinding)
- Apply tokens in `Styles.xaml`: Shell bar, TabBar, Picker, CollectionView items, Label hierarchy
- Introduce `AppShell.xaml` with 2-tab TabBar (Modelos / Marcas); wire `App.xaml.cs` to use it
- Rename `MainPage` → `ModelosPage` (file + class + XAML)
- New `MarcasPage`: CollectionView of brands via `CarCatalogService.GetBrandsAsync()`
- Cherry-pick/merge `feature/vehicle-catalog-maui/pr-2` as prerequisite before any edits

### Out of Scope
- MVVM frameworks, DI registration, third-party libraries
- Flyout navigation, more than 2 tabs, custom animations
- Adding MAUI project to `.slnx` or CI pipeline
- Changes to `CarCatalogService`, API, or backend

## Capabilities

### New Capabilities
- `maui-shell-navigation`: Two-tab Shell entry point replacing single-window MainPage wiring
- `maui-marcas-page`: Brand list page using existing service, code-behind pattern

### Modified Capabilities
- None

## Approach

**Color palette — automotive clean neutral + steel-blue accent:**

| Token | Light | Dark |
|-------|-------|------|
| Primary | `#1A73E8` (steel blue) | `#4D9FFF` |
| PrimaryDark | `#1557B0` | `#2979CC` |
| PrimaryLight | `#E8F0FE` | `#1E3A5F` |
| Secondary | `#5F6368` (cool gray) | `#9AA0A6` |
| Accent | `#34A853` (green) | `#57BB75` |
| Background | `#FFFFFF` | `#121212` |
| Surface | `#F8F9FA` | `#1E1E1E` |
| TextPrimary | `#202124` | `#E8EAED` |
| TextSecondary | `#5F6368` | `#9AA0A6` |
| Danger | `#EA4335` | `#FF6B6B` |
| Warning | `#FBBC04` | `#FDD663` |
| Success | `#34A853` | `#57BB75` |

Light/dark pairs defined as flat keys (`PrimaryLight` / `PrimaryDark` for variants; `BackgroundLight`/`BackgroundDark` etc.) with `AppThemeBinding` applied in `Styles.xaml` setters — never as `Color` entries that themselves carry `AppThemeBinding`.

**Navigation:** `App.xaml.cs` sets `MainPage = new AppShell()`, removes `CreateWindow` override. `AppShell.xaml` declares `TabBar` → `Tab[Modelos]` + `Tab[Marcas]` with `ShellContent` pointing to `ModelosPage` / `MarcasPage`.

**MarcasPage:** code-behind + `ObservableCollection<CarBrand>`; `OnAppearing` loads brands; same `try/catch` + `DisplayAlert` pattern; instantiates `new CarCatalogService()`.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `src/VehicleCatalog.Maui/Resources/Styles/Colors.xaml` | Modified | Replace template tokens; add semantic palette |
| `src/VehicleCatalog.Maui/Resources/Styles/Styles.xaml` | Modified | Apply semantic tokens to Shell, TabBar, Picker, CollectionView, Labels |
| `src/VehicleCatalog.Maui/App.xaml.cs` | Modified | `MainPage = new AppShell()`; remove `CreateWindow` |
| `src/VehicleCatalog.Maui/AppShell.xaml` + `.cs` | New | Two-tab TabBar Shell |
| `src/VehicleCatalog.Maui/MainPage.xaml` + `.cs` | Modified | Rename class + file → `ModelosPage` |
| `src/VehicleCatalog.Maui/MarcasPage.xaml` + `.cs` | New | Brand list page |
| `src/VehicleCatalog.Maui/MauiProgram.cs` | Modified | Register `ModelosPage` route if needed |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Source not on working branch — pr-2 must be merged first | High | Merge/cherry-pick pr-2 as first apply task; block all edits until done |
| `AppThemeBinding` in `Color` entries crashes at runtime | Med | Keep flat keys in Colors.xaml; use `AppThemeBinding` only in Styles.xaml setters |
| `OnAppearing` fires on every tab switch — double loads | Low | Guard with `_loaded` flag or check `ObservableCollection.Count` |
| Android hardcoded URL `10.0.2.2:5023` — not a change risk | Low | No change needed; document in MarcasPage |
| No MAUI build in CI (slnx exclusion) — regressions undetected | Med | Manual smoke-test on Android emulator before merge |

## Rollback Plan

`git revert` the feature branch or reset to pre-merge commit. No database migrations or API changes involved — rollback is safe at any point. If only Shell wiring fails, revert `App.xaml.cs` to `CreateWindow` override; app recovers to single-page mode.

## Dependencies

- `feature/vehicle-catalog-maui/pr-2` must be merged/cherry-picked into `feature/vehicle-catalog-maui/interfaz-paleta-navegacion` before any file edits

## Success Criteria

- [ ] App opens showing top nav bar with title "Catálogo de Vehículos" and custom non-template colors
- [ ] Two tabs visible: "Modelos" and "Marcas"; tapping each navigates correctly
- [ ] Brand filter Picker on Modelos tab works exactly as before the change
- [ ] Marcas tab shows full brand list loaded from API
- [ ] Zero hardcoded color values in page XAML — all reference `StaticResource` keys from `Colors.xaml`
- [ ] Light/dark mode toggling applies correct palette variants
