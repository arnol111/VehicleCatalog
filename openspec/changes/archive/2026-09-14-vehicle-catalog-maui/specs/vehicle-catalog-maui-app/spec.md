# Spec: vehicle-catalog-maui-app

> Change: `vehicle-catalog-maui`
> Domain: `vehicle-catalog-maui-app` (New Capability)
> Date: 2026-09-14

## Purpose

Single-screen .NET MAUI application that consumes the existing VehicleCatalog REST API
and presents a filterable vehicle model catalog on Android and Windows.

---

## Requirements

### REQ-1: Full Model List on App Open

On `OnAppearing()` the app MUST load all vehicle brands from `GET /api/carBrands`
and all vehicle models from `GET /api/carModels` (no `brand` param).
Each cell in the `CollectionView` MUST display model name, brand name, and year.

#### Scenario: SCEN-1.1 — App opens with API reachable

- GIVEN the API is reachable and returns a non-empty model list
- WHEN the `MainPage` appears for the first time
- THEN the `CollectionView` is populated with all models (name, brand, year per cell)
- AND the `Picker` is populated with "Todas las marcas" as first item followed by every brand name
- AND the `ActivityIndicator` is visible during the HTTP calls and hidden once they complete

#### Scenario: SCEN-1.2 — App opens with empty model list

- GIVEN the API returns an empty array for `GET /api/carModels`
- WHEN `OnAppearing()` completes
- THEN the `CollectionView` is empty
- AND the empty-state `Label` "No se encontraron modelos" is visible
- AND no exception is thrown

---

### REQ-2: Brand Filter via Picker

Selecting a brand in the `Picker` MUST call `GET /api/carModels?brand=<name>` using
the exact brand name string. The `brand` parameter MUST NOT be empty or null.

#### Scenario: SCEN-2.1 — User selects a specific brand

- GIVEN the `Picker` is populated and the user selects a brand (e.g. "Toyota")
- WHEN `SelectedIndexChanged` fires
- THEN the app calls `GET /api/carModels?brand=Toyota`
- AND the `CollectionView` is refreshed with only models belonging to that brand
- AND the `ActivityIndicator` is visible during the call

#### Scenario: SCEN-2.2 — Brand with no models

- GIVEN the user selects a brand that exists but has no associated models
- WHEN the API returns an empty array
- THEN the `CollectionView` is empty
- AND the empty-state `Label` "No se encontraron modelos" is visible

---

### REQ-3: "Todas las marcas" Restores Full List

When the `Picker` selection is "Todas las marcas" (index 0), the app MUST call
`GET /api/carModels` without any `brand` query parameter.

#### Scenario: SCEN-3.1 — User resets filter to all brands

- GIVEN a brand filter is currently applied
- WHEN the user selects "Todas las marcas" in the `Picker`
- THEN the app calls `GET /api/carModels` with no `brand` parameter
- AND the `CollectionView` displays the full unfiltered model list

---

### REQ-4: Activity Indicator During Loads

An `ActivityIndicator` MUST be shown (`IsRunning = true`, `IsVisible = true`)
for the entire duration of every HTTP call and hidden immediately after the call
completes (success or failure).

#### Scenario: SCEN-4.1 — Indicator shown during brand load

- GIVEN the app is loading brands on `OnAppearing()`
- WHEN the HTTP call is in flight
- THEN `ActivityIndicator.IsRunning` is `true` and the spinner is visible
- AND it is hidden once the call resolves

---

### REQ-5: API Error Handling

All HTTP calls MUST be wrapped in `try/catch`. On any network error or non-2xx
response the app MUST call `DisplayAlert("Error", "No se pudo conectar con la API", "OK")`.
The `CollectionView` and `Picker` MUST remain in a consistent (possibly empty) state —
the app MUST NOT crash.

#### Scenario: SCEN-5.1 — API is unreachable on app open

- GIVEN the API host is unreachable (network error)
- WHEN `OnAppearing()` attempts to load brands and models
- THEN a `DisplayAlert` with title "Error" and message "No se pudo conectar con la API" is shown
- AND the `ActivityIndicator` is hidden
- AND the UI remains functional (no crash)

#### Scenario: SCEN-5.2 — API returns non-2xx on brand filter

- GIVEN the user selects a brand and the API returns a non-2xx response (e.g. 404 unknown brand)
- WHEN `SelectedIndexChanged` processes the HTTP response
- THEN a `DisplayAlert` with title "Error" and message "No se pudo conectar con la API" is shown
- AND the `CollectionView` is not modified with invalid data

---

### REQ-6: Platform Base URL via Compile-Time Constant

`CarCatalogService` MUST define `BaseUrl` as a compile-time constant using `#if ANDROID`:

- Android: `http://10.0.2.2:5023/`
- All other targets: `https://localhost:7294/`

No runtime configuration or DI injection is used for this value.

#### Scenario: SCEN-6.1 — Android build uses Android base URL

- GIVEN the project is compiled with `net10.0-android`
- WHEN `CarCatalogService` is instantiated
- THEN `BaseUrl` resolves to `http://10.0.2.2:5023/`

#### Scenario: SCEN-6.2 — Windows build uses HTTPS base URL

- GIVEN the project is compiled with `net10.0-windows10.0.19041.0`
- WHEN `CarCatalogService` is instantiated
- THEN `BaseUrl` resolves to `https://localhost:7294/`

---

### REQ-7: JSON Deserialization Case Insensitivity

`HttpClient` responses MUST be deserialized with `PropertyNameCaseInsensitive = true`
so that camelCase API responses map correctly to PascalCase C# model properties.

#### Scenario: SCEN-7.1 — API returns camelCase JSON

- GIVEN the API returns `[{"id":1,"name":"Toyota","year":2023,"brandName":"Toyota"}]`
- WHEN `CarCatalogService` deserializes the response
- THEN the `CarModel` object has `Name = "Toyota"`, `Year = 2023`, `BrandName = "Toyota"`

---

## Non-Functional Requirements

| ID     | Requirement |
|--------|-------------|
| NFR-01 | TFM MUST be `net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0` |
| NFR-02 | A single `HttpClient` instance MUST be used inside `CarCatalogService` (no per-request allocation) |
| NFR-03 | `CarCatalogService` MUST be instantiated directly in code-behind — no DI container |
| NFR-04 | No third-party MVVM framework or DI library MAY be added |
| NFR-05 | `CollectionView` MUST bind to `ObservableCollection<CarModel>` defined in code-behind |
| NFR-06 | The solution MUST build for at least one target (`net10.0-android` or `net10.0-windows10.0.19041.0`) without errors |
