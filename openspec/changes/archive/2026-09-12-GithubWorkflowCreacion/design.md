# Design: GitHub Actions CI Workflow & README Rewrite

## Technical Approach

Add a GitHub Actions workflow that triggers on push/PR to `main`, builds the solution in Release configuration, and runs only unit tests (filtered by the `[Trait("Category","Unit")]` attribute already present in all unit test classes). Rewrite `README.md` fully in Spanish, preserving existing technical content and adding CI badge, test execution instructions, and branch workflow section.

No application code changes. Both files are additive/replacement; no schema, API, or dependency changes required.

## Architecture Decisions

| Option | Tradeoff | Decision |
|--------|----------|----------|
| Run all tests in CI | Simple, but integration tests require Docker-in-Docker on ubuntu-latest — flaky/unsupported | ❌ Rejected |
| Filter `Category=Unit` only | Safe, fast, zero Docker dependency; integration tests deferred to local/CD stage | ✅ Chosen |
| `windows-latest` runner | Native SQL Server support but slower and costlier | ❌ Rejected |
| `ubuntu-latest` runner | Standard for .NET OSS CI; .NET 10 available at GA | ✅ Chosen |
| `dotnet-version: '10.0.x'` (no global.json) | Explicit pin needed since no `global.json` exists; `x` resolves latest patch | ✅ Chosen |
| Branch prefix `feature/` | Standard Git Flow prefix; compatible with GitHub ruleset (PR required before merging) | ✅ Chosen |

## Data Flow

```
push / pull_request → main
        │
        ▼
  ubuntu-latest runner
        │
  actions/checkout@v4
        │
  actions/setup-dotnet@v4  (dotnet-version: 10.0.x)
        │
  dotnet restore
        │
  dotnet build --no-restore --configuration Release
        │
  dotnet test --no-build --filter "Category=Unit"
        │
  ✅ pass  /  ❌ fail → PR blocked by ruleset
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `.github/workflows/ci.yml` | Create | CI pipeline: checkout, setup-dotnet 10.0.x, restore, build Release, unit test filter |
| `README.md` | Modify | Full rewrite in neutral professional Spanish |

## Interfaces / Contracts

### `.github/workflows/ci.yml` — complete content

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Run unit tests
        run: dotnet test --no-build --configuration Release --filter "Category=Unit"
```

### `README.md` — section outline (Spanish)

```
# VehicleCatalog REST API
[![CI](badge-placeholder)]

## Descripción
## Arquitectura
  - tabla: proyecto | responsabilidad
## Stack tecnológico
## Prerequisitos
## Configuración y ejecución local
  1. clonar, 2. connection string, 3. migrations, 4. dotnet run
## Endpoints de la API
  - tabla métodos + response shapes + códigos de estado
## Documentación interactiva (Scalar)
## Migraciones
## Pruebas
  - tabla: clase | tipo | cobertura
  - comandos: unit filter, integration filter, all
  - advertencia Docker para integración
## Flujo de ramas
  - convención: feature/<nombre>, fix/<nombre>
  - regla: PR obligatorio antes de merge a main
## CI
  - descripción del workflow, qué ejecuta, dónde ver resultados
```

## xUnit Trait Status

**No setup required.** All unit test classes already carry `[Trait("Category", "Unit")]` at method level (verified in `GetAllBrandsQueryHandlerTests`, `GetByIdQueryHandlerTests`, `GetAllModelsQueryHandlerTests`). Integration tests carry `[Trait("Category", "Integration")]`. The filter `"Category=Unit"` will match exactly the three unit test classes and exclude all Testcontainers tests.

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Workflow syntax | Valid YAML, correct trigger/job structure | Push to feature branch → observe Actions tab |
| Unit test filter | Only `Category=Unit` tests run; no Docker error | CI green on first push |
| Integration exclusion | No Testcontainers test runs in CI | Absence of Docker errors in CI log |
| README render | Spanish content, badge placeholder, no broken links | Preview in GitHub after PR |

## Threat Matrix

This change introduces VCS/PR automation (GitHub Actions workflow). Applicable assessment:

| Threat | Applicable | Safe Behavior | RED Test |
|--------|-----------|---------------|----------|
| Workflow triggered on untrusted fork PRs leaking secrets | N/A — no secrets used; workflow only reads code and runs tests | No secrets in env; read-only runner | — |
| Test filter matches zero tests (silent pass) | **Applicable** | `dotnet test` exits 0 with 0 tests run — misleading green | Verify trait presence before merge (manual check; documented in proposal risks) |
| Integration tests accidentally included by filter | N/A | Filter is explicit `Category=Unit`; integration trait is `Category=Integration` | — |

> Mitigation for "zero tests matched": the three unit test classes are verified to carry `[Trait("Category","Unit")]` — filter will match ≥ 6 test methods on first run.

## Migration / Rollout

No migration required. Workflow is additive. README is a full replace via PR. Rollback: delete `ci.yml` or `git revert` the README commit.

## Open Questions

- [ ] Confirm `.NET 10.0.x` is available on `ubuntu-latest` at time of first push (monitor Actions tab after merge)
- [ ] Decide whether to add `--configuration Release` to `dotnet test` command for consistency (currently `--no-build` relies on the Release build artifact)
