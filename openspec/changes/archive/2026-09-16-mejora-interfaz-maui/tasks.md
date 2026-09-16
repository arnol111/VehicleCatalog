# Tasks: Mejora de Interfaz — App .NET MAUI (Paleta de colores + Navegación)

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines (authored) | ~220–270 lines |
| 400-line budget risk | Low |
| Chained PRs recommended | No |
| Suggested split | Single PR |
| Delivery strategy | ask-on-risk |
| Chain strategy | pending |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: pending
400-line budget risk: Low

### Suggested Work Units

| Unit | Goal | Likely PR | Focused test command | Runtime harness | Rollback boundary |
|------|------|-----------|----------------------|-----------------|-------------------|
| 1 | Single PR — all authored changes fit under budget | PR 1 | `dotnet build src/VehicleCatalog.Maui/VehicleCatalog.Maui.csproj -f net10.0-windows10.0.19041.0` | Launch on Android emulator; verify top bar, two tabs, brand list | Revert `App.xaml.cs` to `CreateWindow` override; delete `AppShell.*`, `MarcasPage.*`; rename `ModelosPage` back to `MainPage` |

---

## Phase 0: Prerequisite — Bring MAUI Source Into Branch

> Lines imported by this merge are NOT authored by this change and do NOT count toward the 400-line budget.

- [x] 0.1 On branch `feature/vehicle-catalog-maui/interfaz-paleta-navegacion`, run `git merge feature/vehicle-catalog-maui/pr-2` and resolve any conflicts.
- [x] 0.2 Verify `src/VehicleCatalog.Maui/` source files are present: `MainPage.xaml`, `MainPage.xaml.cs`, `App.xaml.cs`, `MauiProgram.cs`, `Resources/Styles/Colors.xaml`, `Resources/Styles/Styles.xaml`.
- [x] 0.3 Confirm `dotnet build src/VehicleCatalog.Maui/VehicleCatalog.Maui.csproj -f net10.0-windows10.0.19041.0` exits with zero errors before any edits.

**Acceptance**: Build succeeds; no merge conflicts outstanding.

---

## Phase 1: Color Foundation

- [x] 1.1 Edit `src/VehicleCatalog.Maui/Resources/Styles/Colors.xaml`: remove `Magenta`, `MidnightBlue`, `PrimaryDark` (old purple), `PrimaryDarkText`, `Secondary` (old purple), `SecondaryDarkText`, `Tertiary` entries.
- [x] 1.2 In `Colors.xaml` add the 24 flat `Color` static-resource entries (12 tokens × 2 suffixes — `Light`/`Dark`). Exact hex values:
  - `PrimaryLight=#1A73E8`, `PrimaryDark=#4D9FFF`
  - `PrimaryDarkLight=#1557B0`, `PrimaryDarkDark=#2979CC`
  - `PrimaryLightLight=#E8F0FE`, `PrimaryLightDark=#1E3A5F`
  - `SecondaryLight=#5F6368`, `SecondaryDark=#9AA0A6`
  - `AccentLight=#34A853`, `AccentDark=#57BB75`
  - `BackgroundLight=#FFFFFF`, `BackgroundDark=#121212`
  - `SurfaceLight=#F8F9FA`, `SurfaceDark=#1E1E1E`
  - `TextPrimaryLight=#202124`, `TextPrimaryDark=#E8EAED`
  - `TextSecondaryLight=#5F6368`, `TextSecondaryDark=#9AA0A6`
  - `DangerLight=#EA4335`, `DangerDark=#FF6B6B`
  - `WarningLight=#FBBC04`, `WarningDark=#FDD663`
  - `SuccessLight=#34A853`, `SuccessDark=#57BB75`
- [x] 1.3 Verify `Colors.xaml` contains zero `AppThemeBinding` expressions (grep check); keep `White`, `Black`, `OffBlack`, `Gray100`–`Gray950`, and all `Brush*` entries untouched.

**Acceptance**: `Colors.xaml` has exactly 24 new semantic keys + retained Gray/Brush entries; no `AppThemeBinding` present.

---

## Phase 2: Style System

- [x] 2.1 Edit `src/VehicleCatalog.Maui/Resources/Styles/Styles.xaml`: replace or update the `TargetType="Shell"` implicit style to include all 7 Shell chrome properties using `AppThemeBinding` (see design for exact XML).
- [x] 2.2 In `Styles.xaml` add keyed style `x:Key="LabelTitle"` (TextPrimary token, appropriate font size) and `x:Key="LabelSecondary"` (TextSecondary token).
- [x] 2.3 In `Styles.xaml` add or update `TargetType="Picker"` implicit style: `TextColor="{AppThemeBinding Light={StaticResource TextPrimaryLight}, Dark={StaticResource TextPrimaryDark}}"`, `BackgroundColor="{AppThemeBinding Light={StaticResource SurfaceLight}, Dark={StaticResource SurfaceDark}}"`.
- [x] 2.4 In `Styles.xaml` add a `CollectionView` item `DataTemplate` border-separator style: `Border` with `Stroke="{AppThemeBinding Light={StaticResource SecondaryLight}, Dark={StaticResource SecondaryDark}}"` on bottom edge only.
- [x] 2.5 In `Styles.xaml` update `Headline` and `SubHeadline` implicit label styles to reference `TextPrimary` token (remove any hardcoded color references).
- [x] 2.6 Verify `Styles.xaml` contains no residual Magenta/MidnightBlue references (grep check).

**Acceptance**: All 7 Shell properties bound; LabelTitle, LabelSecondary, Picker, and CollectionView separator styles present; zero template color references remain.

---

## Phase 3: Shell + Navigation Wiring

- [x] 3.1 Create `src/VehicleCatalog.Maui/AppShell.xaml`: `<Shell>` root with `xmlns:local="clr-namespace:VehicleCatalog.Maui"`, `Title="Catálogo de Vehículos"`, one `<TabBar>` with `Tab Title="Modelos"` → `ShellContent ContentTemplate="{DataTemplate local:ModelosPage}"` and `Tab Title="Marcas"` → `ShellContent ContentTemplate="{DataTemplate local:MarcasPage}"`. No Flyout. No custom animations.
- [x] 3.2 Create `src/VehicleCatalog.Maui/AppShell.xaml.cs`: standard `partial class AppShell : Shell` with `InitializeComponent()` constructor only.
- [x] 3.3 Edit `src/VehicleCatalog.Maui/App.xaml.cs`: in constructor set `MainPage = new AppShell();` and remove (or replace) any `CreateWindow` override so it returns `new Window(new AppShell())` if the override must be kept for platform reasons — prefer removing it per design.

**Acceptance**: `AppShell.xaml` parses; `App.xaml.cs` no longer references `MainPage` directly; build passes.

---

## Phase 4: ModelosPage Rename

- [x] 4.1 Rename `src/VehicleCatalog.Maui/MainPage.xaml` → `src/VehicleCatalog.Maui/ModelosPage.xaml`; update root element attribute to `x:Class="VehicleCatalog.Maui.ModelosPage"`.
- [x] 4.2 Rename `src/VehicleCatalog.Maui/MainPage.xaml.cs` → `src/VehicleCatalog.Maui/ModelosPage.xaml.cs`; update class declaration to `public partial class ModelosPage : ContentPage`.
- [x] 4.3 Verify Picker + CollectionView XAML and all filter/load behavior in `ModelosPage.xaml/.cs` is unchanged; apply `LabelSecondary` keyed style to the empty-state label ("No se encontraron modelos").
- [x] 4.4 Confirm no remaining references to `MainPage` exist in `App.xaml.cs` or `AppShell.xaml` (grep check).

**Acceptance**: Rename complete; filter behavior intact; empty-state label uses `LabelSecondary` style; build passes.

---

## Phase 5: MarcasPage Implementation

- [x] 5.1 Create `src/VehicleCatalog.Maui/MarcasPage.xaml`: `ContentPage` with `CollectionView` bound to `Brands`; item `DataTemplate` displays `CarBrand.Name`; uses `Border` separator from Styles.xaml; all colors via `StaticResource` — zero inline hex literals.
- [x] 5.2 Create `src/VehicleCatalog.Maui/MarcasPage.xaml.cs`: `partial class MarcasPage : ContentPage`; field `private readonly CarCatalogService _service = new();`; `public ObservableCollection<CarBrand> Brands { get; } = new();`; `private bool _loaded;`; `OnAppearing` with `_loaded` guard, `await _service.GetBrandsAsync()`, `foreach` add to `Brands`, `catch (HttpRequestException)` → `DisplayAlert("Error", "No se pudo conectar con la API", "OK")`.

**Acceptance**: `MarcasPage.xaml` has no inline colors; `MarcasPage.xaml.cs` matches design pattern exactly; build passes.

---

## Phase 6: Build Verification

- [x] 6.1 Run `dotnet build src/VehicleCatalog.Maui/VehicleCatalog.Maui.csproj -f net10.0-windows10.0.19041.0` — must exit with zero errors and zero warnings about missing resources. *(Note: Android target `-f net10.0-android` requires the Android workload; use Windows target for local CI-free verification.)*
- [x] 6.2 Manual smoke: launch on Android emulator (debug); verify top bar shows "Catálogo de Vehículos" in non-template color; two tabs visible; Modelos tab default-active.
- [x] 6.3 Manual smoke: tap Marcas tab — brand list loads; no crash; repeated tab switches do not reload.
- [x] 6.4 Manual smoke: toggle light/dark mode in emulator — all palette tokens update correctly; no Magenta or MidnightBlue visible.
- [x] 6.5 Manual smoke: disconnect API, navigate to Marcas tab — `DisplayAlert` appears; app does not crash.
- [x] 6.6 Manual smoke: disconnect API, navigate to Modelos tab — empty-state label ("No se encontraron modelos") renders in `TextSecondary` color.

**Acceptance**: All 6 smoke checks pass; zero compilation errors.

---

## Apply Notes

**Batch**: Final (batch 2 of 2 — bookkeeping only)
**Date**: 2026-09-16
**Human-validated smokes**: Tasks 6.2–6.6 marked complete based on human-reported end-to-end validation ("probé la aplicación y está todo correcto").
**API connectivity context (out-of-scope fix)**: `src/API/Program.cs` — `UseHttpsRedirection` disabled in Development environment so the Android emulator can consume HTTP on `10.0.2.2:5023` without the 307→HTTPS redirect. This fix was applied outside the task scope of this change but was required for the emulator smoke tests to reach the API.
