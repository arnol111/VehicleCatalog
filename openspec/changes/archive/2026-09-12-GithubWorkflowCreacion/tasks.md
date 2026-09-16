# Tasks: GitHub Actions CI Workflow & README Rewrite

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~90 additions, ~20 deletions (~110 total) |
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
| 1 | Create `ci.yml` + rewrite `README.md` | PR 1 | Push to feature branch → observe Actions tab green | `dotnet test --no-build --configuration Release --filter "Category=Unit"` locally | Delete `.github/workflows/ci.yml`; `git revert` README commit — no runtime impact |

---

## Phase 1: Verification / Pre-flight

- [x] 1.1 Confirm `[Trait("Category","Unit")]` is present on all test methods in `test/testAPI/Unit/GetAllBrandsQueryHandlerTests.cs` (read-only), `test/testAPI/Unit/GetByIdQueryHandlerTests.cs` (read-only), `test/testAPI/Unit/GetAllModelsQueryHandlerTests.cs` (read-only) — threat mitigation for silent-pass (zero tests matched).
- [x] 1.2 Run `dotnet test --no-build --configuration Release --filter "Category=Unit"` locally and confirm ≥ 1 test discovered and passes.

## Phase 2: CI Workflow Creation

- [x] 2.1 Create `.github/workflows/` directory if it does not exist.
- [x] 2.2 Create `.github/workflows/ci.yml` with triggers `push: branches: [main]` and `pull_request: branches: [main]`; job `build-and-test` on `ubuntu-latest`.
- [x] 2.3 Add steps to `.github/workflows/ci.yml` in order: `actions/checkout@v4`, `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'`, `dotnet restore`, `dotnet build --no-restore --configuration Release`, `dotnet test --no-build --configuration Release --filter "Category=Unit"`.

## Phase 3: README Rewrite

- [x] 3.1 Rewrite `README.md` entirely in neutral professional Spanish with sections: Descripción, Arquitectura, Stack tecnológico, Prerequisitos, Configuración y ejecución local, Endpoints de la API, Documentación interactiva (Scalar), Migraciones, Pruebas, Flujo de ramas, CI.
- [x] 3.2 Add CI badge placeholder in `README.md` referencing workflow file `ci.yml` (format: `![CI](https://github.com/<org>/<repo>/actions/workflows/ci.yml/badge.svg)`).
- [x] 3.3 Add Pruebas section in `README.md` with unit test command (`--filter "Category=Unit"`), integration test command (`--filter "Category=Integration"`), Docker warning for integration tests, and test class coverage table.

## Phase 4: Verification

- [ ] 4.1 Lint `.github/workflows/ci.yml` YAML syntax (use `actionlint` or GitHub's built-in check on push).
- [ ] 4.2 After PR merge, confirm Actions tab shows a green run with unit test names visible and no integration test names or Docker errors in the log.
- [ ] 4.3 Open repository root on GitHub and confirm README renders Spanish content, badge placeholder is visible, and no broken Markdown or raw HTML artifacts.
- [ ] 4.4 Verify CI does NOT trigger on a push to a non-`main` branch (push a test commit to a feature branch and confirm no workflow run is created).
