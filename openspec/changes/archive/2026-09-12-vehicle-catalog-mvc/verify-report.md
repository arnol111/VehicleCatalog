```yaml
change: vehicle-catalog-mvc
date: "2026-09-12"
mode: full
verdict: PASS WITH WARNINGS
status: passed

build:
  command: "dotnet build src/VehicleCatalog.Web/VehicleCatalog.Web.csproj"
  exit_code: 0
  result: pass
  warnings: 0
  errors: 0

tests:
  command: "dotnet test tests/VehicleCatalog.Web.Tests/VehicleCatalog.Web.Tests.csproj"
  exit_code: 0
  result: pass
  total: 6
  passed: 6
  failed: 0
  skipped: 0
  suite:
    - name: CarCatalogServiceTests.GetModelsAsync_NullBrand_RequestsUrlWithoutQueryString
      result: PASS
    - name: CarCatalogServiceTests.GetModelsAsync_WithBrand_AppendsBrandQueryParam
      result: PASS
    - name: CarCatalogServiceTests.GetModelsAsync_BrandWithSpaces_EncodesQueryParam
      result: PASS
    - name: VehiculosControllerTests.Index_ReturnsViewWithPopulatedViewModel
      result: PASS
    - name: VehiculosControllerTests.Index_WhenServiceThrows_ReturnsErrorViewWithMessage
      result: PASS
    - name: VehiculosControllerTests.Index_MarcaTodasNormalizesToNull
      result: PASS

tasks:
  total: 23
  completed: 23
  incomplete: 0

spec_counts:
  requirements: 6
  scenarios: 6

acceptance_criteria:
  - id: REQ-01
    title: "Full model list on landing — GetModelsAsync(null) called with no brand"
    status: PASS
    evidence: >
      Controller normalizes null/empty marca to null before calling GetModelsAsync.
      Test Index_ReturnsViewWithPopulatedViewModel passes with marca=null and asserts MarcaSeleccionada is null.
      Test GetModelsAsync_NullBrand_RequestsUrlWithoutQueryString confirms no query string.

  - id: REQ-02
    title: "Brand filter — GetModelsAsync(brand) called with encoded brand name"
    status: PASS
    evidence: >
      CarCatalogService.GetModelsAsync uses Uri.EscapeDataString(brand) when brand is non-null.
      Tests GetModelsAsync_WithBrand_AppendsBrandQueryParam and GetModelsAsync_BrandWithSpaces_EncodesQueryParam pass.

  - id: REQ-03
    title: "Filter reset — 'Todas' or empty normalizes to GetModelsAsync(null)"
    status: PASS
    evidence: >
      Controller: string.IsNullOrWhiteSpace(marca) || marca == 'Todas' → null.
      Test Index_MarcaTodasNormalizesToNull verifies mock called with null, passes.

  - id: REQ-04
    title: "Brand persists in dropdown — MarcaSeleccionada bound and rendered"
    status: PASS
    evidence: >
      Controller sets vm.MarcaSeleccionada = marca (original value before normalization).
      Index.cshtml renders selected option when Model.MarcaSeleccionada == m.Name.
      Note: when marca is null/empty, MarcaSeleccionada is null/empty → 'Todas las marcas' is shown selected. Correct behavior.

  - id: REQ-05
    title: "Empty state alert shown when Model.Modelos is empty"
    status: PASS
    evidence: >
      Index.cshtml line 43: @if (!Model.Modelos.Any()) renders alert-info with 'No se encontraron modelos para esta marca.'
      Matches spec text exactly.

  - id: REQ-06
    title: "Error handling — try/catch, ViewBag.ErrorMessage set, no stack trace in Error.cshtml"
    status: PASS
    evidence: >
      VehiculosController wraps entire Index body in try/catch(Exception).
      Sets ViewBag.ErrorMessage on error; returns View('Error').
      Error.cshtml renders ViewBag.ErrorMessage inside alert-danger; no stack trace output present.
      Test Index_WhenServiceThrows_ReturnsErrorViewWithMessage passes.

  - id: REQ-07
    title: "URL encoding — Uri.EscapeDataString used in GetModelsAsync"
    status: PASS
    evidence: >
      CarCatalogService.cs line 24: Uri.EscapeDataString(brand) confirmed in source.
      Test GetModelsAsync_BrandWithSpaces_EncodesQueryParam asserts 'General%20Motors' encoding — passes.

  - id: NFR-01
    title: "Bootstrap 5 responsive layout — local lib, not CDN"
    status: PASS
    evidence: >
      _Layout.cshtml references ~/lib/bootstrap/dist/css/bootstrap.min.css and ~/lib/bootstrap/dist/js/bootstrap.bundle.min.js.
      No CDN URLs (cdn.jsdelivr.net, stackpath, etc.) present.
      Build confirms wwwroot/lib/bootstrap/ assets accepted.

  - id: NFR-02
    title: "No third-party HTTP libraries — only IHttpClientFactory typed client"
    status: PASS
    evidence: >
      Program.cs uses AddHttpClient<ICarCatalogService, CarCatalogService> typed registration.
      CarCatalogService injects HttpClient directly (typed client pattern).
      No RestSharp, Flurl, or other HTTP library references found.

  - id: NFR-03
    title: "No authentication and no direct database access from MVC layer"
    status: PASS
    evidence: >
      Program.cs has no AddAuthentication, AddDbContext, or ORM registrations.
      No auth middleware or database packages referenced.

  - id: NFR-04
    title: "Page title must be 'Catálogo de Vehículos'"
    status: WARNING
    evidence: >
      Index.cshtml sets ViewData["Title"] = "Catálogo de Vehículos".
      _Layout.cshtml renders: <title>@ViewData["Title"] - VehicleCatalog.Web</title>
      Rendered output: "Catálogo de Vehículos - VehicleCatalog.Web" — does NOT match spec exactly.
      Spec says MUST be "Catálogo de Vehículos" (no suffix).

design_coherence:
  - check: "Typed HttpClient registered via AddHttpClient<ICarCatalogService, CarCatalogService>"
    status: PASS
  - check: "Dev-only SSL bypass behind IsDevelopment()"
    status: PASS
  - check: "Default route points to Vehiculos/Index"
    status: PASS
  - check: "PropertyNameCaseInsensitive = true in JSON deserialization"
    status: PASS
  - check: "ApiSettings:BaseUrl sourced from appsettings.json"
    status: PASS

critical_issues: []

warnings:
  - id: W-01
    severity: WARNING
    location: "src/VehicleCatalog.Web/Views/Shared/_Layout.cshtml:6"
    description: >
      Page <title> renders as "Catálogo de Vehículos - VehicleCatalog.Web" instead of the
      spec-required "Catálogo de Vehículos". The layout appends " - VehicleCatalog.Web" suffix.
    fix: >
      Change _Layout.cshtml line 6 from:
        <title>@ViewData["Title"] - VehicleCatalog.Web</title>
      to:
        <title>@ViewData["Title"]</title>
      This makes the rendered title exactly match NFR-04.

suggestions:
  - id: S-01
    description: >
      Consider adding an integration test or a view test that asserts the rendered HTML contains
      the exact <title> text to catch layout regressions like W-01 automatically.
  - id: S-02
    description: >
      The empty-state alert uses alert-info (blue). Consider alert-warning (yellow) to better
      signal "no results found" vs. an informational message. Non-blocking.

next_recommended: apply
next_recommended_reason: >
  One WARNING exists (NFR-04 page title suffix). No CRITICAL issues. All 6 tests pass,
  build is clean. Fix W-01 in _Layout.cshtml, then proceed to archive.
```
