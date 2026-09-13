# Spec: vehicle-catalog-web

> Change: `vehicle-catalog-mvc`
> Domain: `vehicle-catalog-web` (New Capability)
> Date: 2026-09-12

## Purpose

MVC frontend that displays and filters vehicle models via the existing REST API.

---

## Requirements

### Requirement: Full Model List on Landing

The page MUST display all vehicle models on initial load by calling `GET /api/carModels`.
Each row MUST show model name, brand, and year.

#### Scenario: User visits the catalog root

- GIVEN the API is reachable and returns a non-empty model list
- WHEN the user navigates to `/`
- THEN a table with columns "Modelo", "Marca", "Año" is rendered
- AND every model returned by the API appears as a table row

---

### Requirement: Brand Filter Dropdown

The page MUST render a dropdown populated from `GET /api/carBrands`.
Submitting the dropdown with a brand selected MUST reload the page calling `GET /api/carModels?brand={encodedBrand}`.

#### Scenario: User selects a brand and submits

- GIVEN the brand dropdown is populated and the user selects a specific brand
- WHEN the user submits the filter form
- THEN the table shows only models belonging to that brand
- AND the selected brand remains selected in the dropdown

---

### Requirement: Filter Reset via "Todas las marcas"

The dropdown MUST include a "Todas las marcas" option as its first entry.
Selecting it and submitting MUST reload the full model list without a `brand` query parameter.

#### Scenario: User resets filter to all brands

- GIVEN a brand filter is currently applied
- WHEN the user selects "Todas las marcas" and submits
- THEN the full model list is displayed
- AND no brand query parameter is present in the request

---

### Requirement: Filter Persistence After Reload

The dropdown MUST reflect the currently filtered brand after the page reloads.

#### Scenario: Page reloads with brand query parameter

- GIVEN the page URL contains `?brand=Toyota`
- WHEN the page renders
- THEN "Toyota" is the selected option in the dropdown

---

### Requirement: Empty State Message

When the API returns an empty list for a selected brand, the page MUST display
"No se encontraron modelos para esta marca" instead of an empty table.

#### Scenario: Brand with no models selected

- GIVEN the user selects a brand that has no associated models
- WHEN the filter is submitted
- THEN the table is replaced by the message "No se encontraron modelos para esta marca"

---

### Requirement: API Error Handling

If the API is unreachable or returns an HTTP error, the page MUST show a
user-friendly error message. The page MUST NOT expose stack traces or exception
details to the user.

#### Scenario: API is down on page load

- GIVEN the API is unreachable (network error or non-2xx response)
- WHEN the user navigates to the catalog
- THEN a friendly error message is displayed
- AND no exception page or stack trace is shown

---

### Requirement: URL-Encoded Brand Query Parameter

The brand value MUST be encoded with `Uri.EscapeDataString` before being appended
to the API query string.

#### Scenario: Brand name contains special characters

- GIVEN a brand name contains spaces or reserved URI characters
- WHEN the filter is submitted
- THEN the query string sent to the API uses the percent-encoded form of the brand name

---

## Non-Functional Requirements

| ID | Requirement |
|----|-------------|
| NFR-01 | Responsive layout using Bootstrap 5 (local, from MVC template) |
| NFR-02 | HTTP calls MUST use `IHttpClientFactory` typed client — no third-party HTTP libraries |
| NFR-03 | No authentication and no direct database access from the MVC layer |
| NFR-04 | Page `<title>` MUST be "Catálogo de Vehículos" |
