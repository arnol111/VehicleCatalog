# Registro del Proceso de IA — VehicleCatalog

Este documento consolida la evidencia del proceso iterativo de desarrollo asistido por IA (gentle-ai + SDD)
del proyecto VehicleCatalog. Cada iteración cubre los 10 campos del modelo de registro, citando únicamente
artefactos verificados. Los prompts verbatim no están disponibles en el repositorio salvo donde se indica.

---

## Resumen

| # | Cambio | Fecha | Estado | Commit(s) |
|---|--------|-------|--------|-----------|
| 1 | VehicleCatalog-API-Persistence | 2026-09-09 | Archivado | `718a08d` / PR #1 `2be5d69` |
| 2 | ImplementacionEnpoints | 2026-09-11 | Archivado | `79cbf36` `66c67d8` `3609293` `57d8378` |
| 3 | TestProyect | 2026-09-11 | Archivado | `496ae54` `9525c40` |
| 4 | GithubWorkflowCreacion | 2026-09-12 | Archivado | `a01141f` / PR #2 `989dfdf` |
| 5 | vehicle-catalog-mvc | 2026-09-12 | Archivado | `4d2fde6` `d9ca16b` / PR #3 `1170f4b` |
| 6 | vehicle-catalog-maui | 2026-09-14 | Archivado | `19953f9`…`f6c1e48` PR #4 `d56a6f2` / `0ea262c`…`ba80c73` PR #5 `045693f` |
| 7 | mejora-interfaz-maui | 2026-09-16 | Archivado | `9a12b75` `df423ab` `dbcab89` `cd6c692` `d9dc66b` `5f3cc14` / PR #6 `567f65c` |
| 8 | DocumentacionFinal | 2026-09-16 | cambio activo (sin archivar) | `1bfc4fd` / PR #7 `535bcf5` |

---

## Estado inicial del repositorio — pre-SDD

- **Commit de referencia**: `43b7af6` — "Initial commit" (2026-09-09)
- Contenido: entidades de dominio (`CarBrand`, `CarModel`), interfaces de repositorio vacías, stubs `Class1.cs`
- Sin capa de persistencia, sin endpoints funcionales, sin pipeline CI, sin cliente web ni móvil
- Este estado es la línea base anterior al inicio del ciclo SDD; no corresponde a ninguna iteración IA

---

## Iteración 1 — VehicleCatalog-API-Persistence

| Campo | Contenido | Evidencia |
|-------|-----------|-----------|
| 1. Especificación inicial | Capa de persistencia EF Core 10 ausente: sin DbContext, repositorios ni DI | `openspec/changes/archive/2026-09-09-VehicleCatalog-API-Persistence/proposal.md` |
| 2. Prompt utilizado | Objetivo derivado del Intent del proposal (texto verbatim no disponible) | `proposal.md § Intent` |
| 3. Objetivo del prompt | Establecer infraestructura de persistencia completa para CRUD de CarBrand y CarModel con SQL Server | `proposal.md § Intent` |
| 4. Resultado / propuesta IA | Proposal: EF Core 10, `VehicleCatalogDbContext`, Fluent API configs, repositorios, `UnitOfWork`, DI, migración `InitialCreate` | `proposal.md` |
| 5. Implementación | 21/21 tareas completadas; migración generada en `API.Infrastructure/Migrations/` | `openspec/changes/archive/2026-09-09-VehicleCatalog-API-Persistence/tasks.md` |
| 6. Validaciones | Build exit 0, 0 errores, 0 warnings; 19 escenarios PASS; veredicto: PASS_WITH_WARNINGS | `openspec/changes/archive/2026-09-09-VehicleCatalog-API-Persistence/verify-report.md` |
| 7. Problemas identificados | WARNING-001: `GetAllAsync` retorna `IEnumerable<T>` en lugar de `IReadOnlyList<T>`; WARNING-002: migración en `API.Infrastructure/` vs spec `API/`; WARNING-003: nombre `SaveChangesAsync` vs spec `SaveAsync` | `verify-report.md` |
| 8. Ajustes realizados | Migración colocada en `API.Infrastructure/Migrations/` según decisión de diseño (no en `API/`) | commit `718a08d` |
| 9. Decisiones técnicas del desarrollador | D: pin EF Core `10.0.x`; D: `OnDelete(Restrict)` en FK; D: `ApplyConfigurationsFromAssembly`; D: ubicación de migración en Infrastructure [decisiones del desarrollador, citadas de `proposal.md § Risks` y `design.md`] | `proposal.md § Risks` / `openspec/changes/archive/2026-09-09-VehicleCatalog-API-Persistence/design.md` |
| 10. Commit(s) relacionados | `718a08d` / PR #1 `2be5d69` | `git log` |

---

## Iteración 2 — ImplementacionEnpoints

| Campo | Contenido | Evidencia |
|-------|-----------|-----------|
| 1. Especificación inicial | Sin endpoints funcionales; bugs DI: `IDispatcher` Singleton, handlers no registrados | `openspec/changes/archive/2026-09-11-ImplementacionEnpoints/proposal.md` |
| 2. Prompt utilizado | Objetivo derivado del Intent del proposal (texto verbatim no disponible) | `proposal.md § Intent` |
| 3. Objetivo del prompt | Entregar los 3 endpoints de consulta, corregir bugs DI, agregar Scalar UI y README | `proposal.md § Intent` |
| 4. Resultado / propuesta IA | Proposal: `GET /api/carBrands`, `GET /api/carModels`, filtro `?brand=`, fix Singleton→Scoped, registro de handlers, `Scalar.AspNetCore` | `proposal.md` |
| 5. Implementación | 8/8 work units completados incluyendo WU-08 (guardia empty-brand) | `openspec/changes/archive/2026-09-11-ImplementacionEnpoints/tasks.md` |
| 6. Validaciones | 5/5 escenarios runtime PASS (HTTP 200/400/404 en `localhost:5023`); veredicto: PASS | `openspec/changes/archive/2026-09-11-ImplementacionEnpoints/verify-report.md` |
| 7. Problemas identificados | W-01: rutas de tabla vacía, errores DB y render Scalar no ejercitados en runtime (cobertura estática) | `verify-report.md` |
| 8. Ajustes realizados | Guardia empty-brand (`?brand=` → 400) agregada como WU-08 quirúrgico | commits `79cbf36` `66c67d8` `3609293` `57d8378` |
| 9. Decisiones técnicas del desarrollador | D: filtro DB-side con `GetAllByBrandIdAsync` + `Where()`; D: `IDispatcher` → Scoped; D: handlers Transient [decisiones del desarrollador, `proposal.md § Approach`] | `proposal.md § Approach` |
| 10. Commit(s) relacionados | `79cbf36` `66c67d8` `3609293` `57d8378` | `git log` |

---

## Iteración 3 — TestProyect

> Artefactos en ruta no estándar `openspec/TestProyect/`; ruta no estándar; sin explore.md

| Campo | Contenido | Evidencia |
|-------|-----------|-----------|
| 1. Especificación inicial | Sin cobertura de tests; `GetByIdQueryHandler` lanza `Exception` genérica en lugar de `NotFoundException` | `openspec/TestProyect/proposal.md` |
| 2. Prompt utilizado | Objetivo derivado del Intent del proposal (texto verbatim no disponible) | `proposal.md § Intent` |
| 3. Objetivo del prompt | Crear proyecto de tests con 7 unit tests + 9 integration tests vía Testcontainers SQL Server | `proposal.md § Intent` |
| 4. Resultado / propuesta IA | Proposal: `test/testAPI`, NSubstitute unit tests, `WebApplicationFactory` + Testcontainers integration tests, fix handler | `openspec/TestProyect/proposal.md` |
| 5. Implementación | Todas las tareas completadas (6 fases: scaffold, fix, unit tests, fixture, integration tests, docs) | `openspec/TestProyect/tasks.md` |
| 6. Validaciones | 7/7 unit + 9/9 integration = 16/16 PASS; build exit 0; veredicto: PASS | `openspec/TestProyect/verify-report.md` |
| 7. Problemas identificados | NU1603: xunit.runner.visualstudio resuelto a 3.0.0 (warning no bloqueante); OI-01 resuelto: 404 para marca inexistente | `verify-report.md` |
| 8. Ajustes realizados | Controller split de catch: `NotFoundException`→404, `Exception`→500 (aprobado en tasks.md) | commits `496ae54` `9525c40` |
| 9. Decisiones técnicas del desarrollador | D: Testcontainers.MsSql para mantener proveedor SQL Server; D: `EnsureCreated()` en vez de migrations; D: `ICollectionFixture` para container único [decisiones del desarrollador, `proposal.md § Approach`] | `openspec/TestProyect/proposal.md § Approach` |
| 10. Commit(s) relacionados | `496ae54` `9525c40` | `git log` |

---

## Iteración 4 — GithubWorkflowCreacion

| Campo | Contenido | Evidencia |
|-------|-----------|-----------|
| 1. Especificación inicial | Sin pipeline CI; merges a main sin validación automática; README sin contenido en español | `openspec/changes/archive/2026-09-12-GithubWorkflowCreacion/proposal.md` |
| 2. Prompt utilizado | Objetivo derivado del Intent del proposal (texto verbatim no disponible) | `proposal.md § Intent` |
| 3. Objetivo del prompt | Crear workflow CI mínimo (unit tests) y reescribir README en español | `proposal.md § Intent` |
| 4. Resultado / propuesta IA | Proposal: `.github/workflows/ci.yml` con triggers push/PR a main, `dotnet test --filter "Category=Unit"`, README español completo | `proposal.md` |
| 5. Implementación | 7/11 tareas completadas localmente; 4 tareas (4.1–4.4) post-merge no verificables localmente | `openspec/changes/archive/2026-09-12-GithubWorkflowCreacion/tasks.md` |
| 6. Validaciones | 7/7 unit tests PASS localmente; 5/5 reqs CI cubiertos estáticamente; veredicto: PASS WITH WARNINGS | `openspec/changes/archive/2026-09-12-GithubWorkflowCreacion/verify-report.md` |
| 7. Problemas identificados | W1: tareas 4.1–4.4 post-merge pendientes; W2: NU1603 xunit pre-existente; S1: line endings CRLF | `verify-report.md` |
| 8. Ajustes realizados | Nombres de steps en español (desviación aceptada del diseño en inglés); configuración `Release` explícita en `dotnet test` | commit `a01141f` |
| 9. Decisiones técnicas del desarrollador | D: `ubuntu-latest` + `dotnet-version: 10.0.x`; D: filtro `Category=Unit` para excluir integration tests; D: no `global.json` [decisiones del desarrollador, `proposal.md § Approach`] | `proposal.md § Approach` |
| 10. Commit(s) relacionados | `a01141f` / PR #2 `989dfdf` | `git log` |

---

## Iteración 5 — vehicle-catalog-mvc

| Campo | Contenido | Evidencia |
|-------|-----------|-----------|
| 1. Especificación inicial | Solución solo backend REST sin interfaz de usuario; sin proyecto MVC | `openspec/changes/archive/2026-09-12-vehicle-catalog-mvc/proposal.md` |
| 2. Prompt utilizado | Objetivo derivado del Intent del proposal (texto verbatim no disponible) | `proposal.md § Intent` |
| 3. Objetivo del prompt | Agregar app MVC ASP.NET Core read-only que consuma la API y muestre catálogo filtrable | `proposal.md § Intent` |
| 4. Resultado / propuesta IA | Proposal: `VehicleCatalog.Web`, `ICarCatalogService`, `VehiculosController`, Razor view con dropdown y tabla Bootstrap 5 | `proposal.md` |
| 5. Implementación | 23/23 tareas completadas; proyecto registrado en `VehicleCatalog.slnx` | `openspec/changes/archive/2026-09-12-vehicle-catalog-mvc/tasks.md` |
| 6. Validaciones | 6/6 tests PASS; build exit 0; veredicto: PASS WITH WARNINGS | `openspec/changes/archive/2026-09-12-vehicle-catalog-mvc/verify-report.md` |
| 7. Problemas identificados | W-01: `<title>` renderiza "Catálogo de Vehículos - VehicleCatalog.Web" en vez del valor exacto del spec (NFR-04) | `verify-report.md § W-01` |
| 8. Ajustes realizados | `Uri.EscapeDataString` en `GetModelsAsync`; normalización `Todas`→null; bootstrap 5 local (no CDN) | commits `4d2fde6` `d9ca16b` |
| 9. Decisiones técnicas del desarrollador | D: `IHttpClientFactory` typed client; D: `DangerousAcceptAnyServerCertificateValidator` solo en dev; D: `ApiSettings:BaseUrl` desde config [decisiones del desarrollador, `proposal.md § Approach`] | `proposal.md § Approach` |
| 10. Commit(s) relacionados | `4d2fde6` `d9ca16b` / PR #3 `1170f4b` | `git log` |

---

## Iteración 6 — vehicle-catalog-maui

| Campo | Contenido | Evidencia |
|-------|-----------|-----------|
| 1. Especificación inicial | Sin cliente móvil; API solo backend REST | `openspec/changes/archive/2026-09-14-vehicle-catalog-maui/proposal.md` |
| 2. Prompt utilizado | Objetivo derivado del Intent del proposal (texto verbatim no disponible) | `proposal.md § Intent` |
| 3. Objetivo del prompt | Agregar app MAUI pantalla única que consuma API y muestre catálogo filtrable por marca | `proposal.md § Intent` |
| 4. Resultado / propuesta IA | Proposal: `VehicleCatalog.Maui`, `CarCatalogService`, `MainPage` con `Picker`+`CollectionView`, `#if ANDROID` URL guard | `proposal.md` |
| 5. Implementación | 23/23 tareas completadas; builds Windows y Android exit 0 | `openspec/changes/archive/2026-09-14-vehicle-catalog-maui/tasks.md` |
| 6. Validaciones | 11/11 escenarios PASS (build + inspección + smoke manual 2026-09-14); veredicto: PASS WITH WARNINGS | `openspec/changes/archive/2026-09-14-vehicle-catalog-maui/verify-report.md` |
| 7. Problemas identificados | CS0618: `DisplayAlert` obsoleto (aceptado por spec); CS8622: MAUI source-gen (aceptado) | `verify-report.md § Issues` |
| 8. Ajustes realizados | Resolución CD-1: URL Android `http://10.0.2.2:5023/` (HTTP port 5023); `EnsureSuccessStatusCode()` + catch por ruta filter | commits `19953f9`…`f6c1e48` PR #4 / `0ea262c`…`ba80c73` PR #5 |
| 9. Decisiones técnicas del desarrollador | D-1: TFM `net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0`; D-3: code-behind + ObservableCollection sin MVVM; D-6: guard empty-brand sin parámetro [decisiones del desarrollador, `proposal.md § Key Decisions`] | `proposal.md § Key Decisions` |
| 10. Commit(s) relacionados | `19953f9`…`f6c1e48` PR #4 `d56a6f2` / `0ea262c`…`ba80c73` PR #5 `045693f` | `git log` |

---

## Iteración 7 — mejora-interfaz-maui

> Campo 1: sin spec.md en carpeta del cambio — artefactos en `openspec/changes/archive/2026-09-16-mejora-interfaz-maui/specs/`

| Campo | Contenido | Evidencia |
|-------|-----------|-----------|
| 1. Especificación inicial | sin spec.md — colores de plantilla (Magenta, MidnightBlue), sin Shell navigation, una sola página | `openspec/changes/archive/2026-09-16-mejora-interfaz-maui/proposal.md` |
| 2. Prompt utilizado | Objetivo derivado del Intent del proposal (texto verbatim no disponible) | `proposal.md § Intent` |
| 3. Objetivo del prompt | Reemplazar paleta plantilla con diseño automotriz neutro + Shell de 2 tabs (Modelos/Marcas) | `proposal.md § Intent` |
| 4. Resultado / propuesta IA | Proposal: `Colors.xaml` con 24 tokens semánticos, `AppShell.xaml` 2-tab, `ModelosPage`, `MarcasPage` | `proposal.md` |
| 5. Implementación | 27/27 tareas completadas; smoke manual confirmado en emulador Android 2026-09-16 | `openspec/changes/archive/2026-09-16-mejora-interfaz-maui/tasks.md` |
| 6. Validaciones | 17/17 escenarios PASS (13 estáticos + 4 validados por humano); veredicto: PASS WITH WARNINGS | `openspec/changes/archive/2026-09-16-mejora-interfaz-maui/verify-report.md` |
| 7. Problemas identificados | W-002: `ItemsSource="{Binding Brands}"` en MarcasPage.xaml sin BindingContext (redundante, no bloqueante) | `verify-report.md § W-002` |
| 8. Ajustes realizados | `UseHttpsRedirection` deshabilitado en Development para conectividad Android (out-of-scope, requerido para smoke) | commits `9a12b75` `df423ab` `dbcab89` `cd6c692` `d9dc66b` `5f3cc14` |
| 9. Decisiones técnicas del desarrollador | D: claves planas en `Colors.xaml` (no `AppThemeBinding` en Color entries); D: Shell implícito en `Styles.xaml`; D: guardia `_loaded` en `MarcasPage` [decisiones del desarrollador, `proposal.md § Approach`] | `proposal.md § Approach` |
| 10. Commit(s) relacionados | `9a12b75` `df423ab` `dbcab89` `cd6c692` `d9dc66b` `5f3cc14` / PR #6 `567f65c` | `git log` |

---

## Iteración 8 — DocumentacionFinal

> Estado: cambio activo (sin archivar)

| Campo | Contenido | Evidencia |
|-------|-----------|-----------|
| 1. Especificación inicial | Sin instrucciones de ejecución en README; `VehicleCatalog.Maui` no integrado en main ni en `.slnx` | `openspec/changes/DocumentacionFinal/proposal.md` |
| 2. Prompt utilizado | "Se debe primero actualizar main y luego realizar la documentación." [verbatim] | `proposal.md` encabezado confirmado verbatim |
| 3. Objetivo del prompt | Integrar MAUI a main, registrar en `.slnx`, agregar sección "Cómo ejecutar la solución" en README | `proposal.md § Intent` |
| 4. Resultado / propuesta IA | Proposal: merge de `feature/vehicle-catalog-maui/pr-2`, registro `.slnx`, sección README en español con subsecciones de prerrequisitos, DB, migraciones, API, Web, MAUI y tests | `openspec/changes/DocumentacionFinal/proposal.md` |
| 5. Implementación | Fases A y B completas; CI GitHub Actions run #13 `35165164758` PASS (PR #7, head `1bfc4fd`) | `openspec/changes/DocumentacionFinal/tasks.md` |
| 6. Validaciones | 12/12 reqs, 14/14 escenarios PASS; unit tests 7/7; CI verde; veredicto: PASS WITH WARNINGS | `openspec/changes/DocumentacionFinal/verify-report.md` |
| 7. Problemas identificados | W-1: NU1603 pre-existente; W-2: credenciales plain-text en `appsettings*.json` (fuera de scope); W-3: variante EF migration anterior en línea 152 README | `verify-report.md § Issues` |
| 8. Ajustes realizados | `UseHttpsRedirection` ya deshabilitado en Development (iteración 7); README appended después del último `---` del contenido previo | commit `1bfc4fd` |
| 9. Decisiones técnicas del desarrollador | D: User Secrets recomendado (no imprimir credenciales); D: run-order explícito (API primero); D: `http://10.0.2.2:5023/` documentado para emulador Android [decisiones del desarrollador, `proposal.md § Approach`] | `openspec/changes/DocumentacionFinal/proposal.md § Approach` |
| 10. Commit(s) relacionados | `1bfc4fd` / PR #7 `535bcf5` | `git log` |
