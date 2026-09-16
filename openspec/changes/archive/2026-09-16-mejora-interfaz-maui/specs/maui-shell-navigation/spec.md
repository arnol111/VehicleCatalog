# maui-shell-navigation Specification

## Purpose

Defines the Shell entry point, two-tab navigation, and semantic color/style system for the .NET MAUI app. Replaces single-window MainPage wiring and template palette.

## Requirements

### Requirement: Shell Entry Point

The app MUST launch via `AppShell` as its root page. `App.xaml.cs` MUST set `MainPage = new AppShell()` and MUST NOT override `CreateWindow`.

#### Scenario: App launches with Shell

- GIVEN the app starts on any platform
- WHEN `App.xaml.cs` initializes
- THEN `MainPage` is an `AppShell` instance and the top nav bar is visible with title "Catálogo de Vehículos"

---

### Requirement: Two-Tab Navigation

`AppShell.xaml` MUST declare exactly one `TabBar` containing two `Tab` entries: "Modelos" (pointing to `ModelosPage`) and "Marcas" (pointing to `MarcasPage`). Flyout navigation MUST NOT be used. No custom transition animations are permitted.

#### Scenario: Modelos tab is active by default

- GIVEN the app has launched
- WHEN the Shell renders
- THEN the "Modelos" tab is selected and `ModelosPage` content is visible

#### Scenario: User taps Marcas tab

- GIVEN the app is on the Modelos tab
- WHEN the user taps the "Marcas" tab
- THEN `MarcasPage` content is displayed and no animation plays

---

### Requirement: ModelosPage Rename

`MainPage` MUST be renamed to `ModelosPage` (file, class, and XAML root). All Shell routes and references MUST use `ModelosPage`.

#### Scenario: ModelosPage loads filter and list

- GIVEN the Shell has navigated to the Modelos tab
- WHEN `ModelosPage` appears
- THEN the brand filter Picker and vehicle model CollectionView are visible and functional as before the rename

---

### Requirement: Semantic Color Palette

`Colors.xaml` MUST define the following flat static keys with separate light and dark values (no `AppThemeBinding` inside `Colors.xaml`):

| Token | Light | Dark |
|-------|-------|------|
| Primary | `#1A73E8` | `#4D9FFF` |
| PrimaryDark | `#1557B0` | `#2979CC` |
| PrimaryLight | `#E8F0FE` | `#1E3A5F` |
| Secondary | `#5F6368` | `#9AA0A6` |
| Accent | `#34A853` | `#57BB75` |
| Background | `#FFFFFF` | `#121212` |
| Surface | `#F8F9FA` | `#1E1E1E` |
| TextPrimary | `#202124` | `#E8EAED` |
| TextSecondary | `#5F6368` | `#9AA0A6` |
| Danger | `#EA4335` | `#FF6B6B` |
| Warning | `#FBBC04` | `#FDD663` |
| Success | `#34A853` | `#57BB75` |

#### Scenario: Flat keys defined for both modes

- GIVEN `Colors.xaml` is loaded
- WHEN the resource dictionary is inspected
- THEN each token has a `Light`-suffixed and `Dark`-suffixed key (e.g. `BackgroundLight`, `BackgroundDark`) and no `Color` entry itself contains `AppThemeBinding`

---

### Requirement: AppThemeBinding in Styles Only

`AppThemeBinding` MUST appear exclusively in `Styles.xaml` setters. `Colors.xaml` MUST contain only flat `Color` static resources.

#### Scenario: Light/dark toggle applies palette variants

- GIVEN the device switches between light and dark mode
- WHEN the app re-renders
- THEN all themed elements update to the correct palette variant without restart

---

### Requirement: Shell and TabBar Color Customization

`Styles.xaml` MUST apply palette tokens to the Shell chrome via `AppThemeBinding`:

| Property | Bound Token |
|----------|-------------|
| `Shell.BackgroundColor` | Primary |
| `Shell.ForegroundColor` | Background |
| `Shell.TitleColor` | Background |
| `Shell.TabBarBackgroundColor` | Surface |
| `Shell.TabBarForegroundColor` | Primary |
| `Shell.TabBarTitleColor` | Primary |
| `Shell.TabBarUnselectedColor` | Secondary |

#### Scenario: Custom colors visible on Modelos tab

- GIVEN the app is open
- WHEN the user views any tab
- THEN the top bar background is Primary, title is Background-colored, and tab bar uses Surface/Primary/Secondary tokens — no template Magenta or MidnightBlue colors are visible

---

### Requirement: Zero Hardcoded Colors in Page XAML

All color values in page XAML files (AppShell, ModelosPage, MarcasPage) MUST reference `StaticResource` keys from `Colors.xaml`. Inline hex or named color literals MUST NOT appear in page XAML.

#### Scenario: Audit finds no inline colors

- GIVEN any page XAML file is opened
- WHEN all color-bearing attributes are inspected
- THEN every value is a `{StaticResource ...}` reference; no hex strings or named colors are present

---

### Requirement: Label Hierarchy via Palette Tokens

Title labels MUST use `TextPrimary` token. Secondary/descriptive labels MUST use `TextSecondary` token. Empty-state labels (e.g. "No se encontraron modelos") MUST use `TextSecondary`.

#### Scenario: Empty state label color

- GIVEN the model list is empty
- WHEN the empty-state label is rendered
- THEN its text color resolves to the `TextSecondary` token value for the active mode

---

### Requirement: CollectionView Item Styling

CollectionView item templates MUST apply a subtle visual separator (alternating background or row border) using palette tokens from `Styles.xaml`. No hardcoded color values are permitted in item templates.

#### Scenario: Alternating or bordered rows visible

- GIVEN a CollectionView with multiple items
- WHEN the list is rendered
- THEN each item row is visually distinct from adjacent rows via background or border styled with palette tokens

---

### Requirement: Picker Styled with Palette Tokens

The brand filter Picker MUST apply `TextPrimary` for its text color and `Surface` for its background via `Styles.xaml` tokens. No hardcoded colors in the Picker element.

#### Scenario: Picker respects theme switch

- GIVEN the device is in dark mode
- WHEN the Modelos tab Picker is rendered
- THEN Picker text is `TextPrimary` dark variant and background is `Surface` dark variant
