# maui-marcas-page Specification

## Purpose

Defines the Marcas brand-list page: data loading, presentation, error handling, and styling constraints for the new tab in the MAUI Shell.

## Requirements

### Requirement: Brand List Loading

`MarcasPage` MUST load the full brand list by calling `CarCatalogService.GetBrandsAsync()` inside `OnAppearing`. The result MUST be bound to an `ObservableCollection<CarBrand>` that backs a `CollectionView`. No filter or search is applied.

#### Scenario: Brands load on tab open

- GIVEN the API is reachable and returns brands
- WHEN the user navigates to the Marcas tab
- THEN the CollectionView displays the complete brand list returned by the service

#### Scenario: Repeated tab navigation does not double-load

- GIVEN the brand list has already been loaded
- WHEN the user switches away and back to the Marcas tab
- THEN `GetBrandsAsync()` is not called again (guard via `_loaded` flag or collection count check)

---

### Requirement: Error Handling on Load Failure

If `GetBrandsAsync()` throws, `MarcasPage` MUST catch the exception and display a `DisplayAlert` with a user-readable error message. The page MUST NOT crash.

#### Scenario: API unreachable shows alert

- GIVEN the API is unreachable
- WHEN `OnAppearing` executes and `GetBrandsAsync()` throws
- THEN a `DisplayAlert` dialog is shown with an error message and the CollectionView remains empty without crashing

---

### Requirement: Service Instantiation

`MarcasPage` code-behind MUST instantiate `CarCatalogService` directly (`new CarCatalogService()`). No dependency injection or third-party frameworks are used.

#### Scenario: Service created in code-behind

- GIVEN `MarcasPage` is initialized
- WHEN `OnAppearing` runs
- THEN `CarCatalogService` is available as a field and `GetBrandsAsync()` is callable

---

### Requirement: Zero Hardcoded Colors on MarcasPage

All color values in `MarcasPage.xaml` MUST reference `StaticResource` keys from `Colors.xaml`. No inline hex or named color literals are permitted.

#### Scenario: Audit of MarcasPage finds no inline colors

- GIVEN `MarcasPage.xaml` is opened
- WHEN all color-bearing attributes are inspected
- THEN every color value is a `{StaticResource ...}` reference

---

### Requirement: MarcasPage Accessible from Shell Tab

The "Marcas" tab in `AppShell.xaml` MUST route to `MarcasPage` via `ShellContent`. Tapping the tab MUST display `MarcasPage` and trigger `OnAppearing`.

#### Scenario: Tab navigation triggers page load

- GIVEN the app is on the Modelos tab
- WHEN the user taps "Marcas"
- THEN `MarcasPage` is displayed and brand data is loaded (or already cached)
