# GitHub CI Workflow & README Specification

## Purpose

Defines behavior for the new CI pipeline and the README rewrite introduced by the GithubWorkflowCreacion change.

---

## ADDED Requirements

### Requirement: CI Workflow Triggers

The CI workflow MUST trigger on `push` events to `main` and on `pull_request` events targeting `main`. It MUST NOT trigger on pushes to other branches.

#### Scenario: Push to main triggers CI

- GIVEN a commit is pushed to the `main` branch
- WHEN GitHub Actions evaluates the event
- THEN the `ci.yml` workflow starts and a job run is created

#### Scenario: PR targeting main triggers CI

- GIVEN a pull request is opened or updated targeting `main`
- WHEN GitHub Actions evaluates the event
- THEN the `ci.yml` workflow starts for that PR's head commit

#### Scenario: Push to non-main branch does not trigger CI

- GIVEN a commit is pushed to any branch other than `main`
- WHEN GitHub Actions evaluates the event
- THEN the `ci.yml` workflow does NOT start

---

### Requirement: CI Job Steps

The CI job MUST execute these steps in order: checkout, setup-dotnet (10.0.x), `dotnet restore`, `dotnet build --no-restore --configuration Release`, `dotnet test --no-build --filter "Category=Unit"`. Each step MUST succeed before the next step runs.

#### Scenario: All steps pass on green build

- GIVEN the repository compiles cleanly and all unit tests pass
- WHEN the CI job runs
- THEN all five steps complete with exit code 0 and the job reports success

#### Scenario: Build failure stops pipeline

- GIVEN a compilation error exists in the codebase
- WHEN `dotnet build` runs
- THEN the step exits non-zero and subsequent steps (`dotnet test`) do NOT run

#### Scenario: Unit test failure fails the job

- GIVEN one or more unit tests fail
- WHEN `dotnet test --no-build --filter "Category=Unit"` runs
- THEN the step exits non-zero and the job reports failure

---

### Requirement: Unit Test Isolation

The CI pipeline MUST execute only tests carrying `[Trait("Category", "Unit")]`. Tests tagged `Category=Integration` MUST NOT run. Tests with no `Category` trait MUST NOT run under this filter.

#### Scenario: Unit tests run; integration tests are excluded

- GIVEN unit test classes carry `[Trait("Category","Unit")]` and integration test classes carry `[Trait("Category","Integration")]`
- WHEN `dotnet test --no-build --filter "Category=Unit"` executes
- THEN only unit tests are discovered and run; integration tests are skipped
- AND no Docker or network dependency is required

#### Scenario: Missing trait causes test to be excluded

- GIVEN a test method has no `[Trait("Category","Unit")]` attribute
- WHEN the filter is applied
- THEN that test is not executed by the CI job

---

### Requirement: .NET Version Pin

The workflow MUST declare `dotnet-version: '10.0.x'` explicitly in `actions/setup-dotnet@v4`. No `global.json` SHALL be required for the CI to resolve the SDK version.

#### Scenario: SDK resolves without global.json

- GIVEN the repository has no `global.json`
- WHEN `actions/setup-dotnet@v4` runs with `dotnet-version: '10.0.x'`
- THEN the runner installs a .NET 10.0.x SDK and `dotnet --version` outputs a 10.0.x version

---

### Requirement: README Spanish Content

The README.md MUST be written entirely in neutral professional Spanish and MUST contain: project description, directory structure, prerequisites, local run instructions, test execution instructions, branch workflow, and a CI badge placeholder referencing `ci.yml`.

#### Scenario: README renders correctly on GitHub

- GIVEN the updated README.md is merged to `main`
- WHEN a user visits the repository root on GitHub
- THEN the page renders Spanish content with no broken Markdown, no raw HTML artifacts, and a visible (possibly pending) CI badge

#### Scenario: Badge placeholder references correct workflow

- GIVEN the CI badge markdown references the workflow file name `ci.yml`
- WHEN the workflow has run at least once
- THEN the badge resolves to the actual workflow status (pass/fail/pending)

---

## Constraints

| # | Constraint |
|---|-----------|
| C1 | Integration tests MUST NOT run in CI (no Docker-in-Docker requirement) |
| C2 | No CD pipeline, coverage reporting, or branch protection changes are in scope |
| C3 | All unit test classes in `test/testAPI/Unit/` MUST carry `[Trait("Category","Unit")]` before merge (verified: trait already present as of spec date) |
| C4 | `dotnet-version` MUST be explicit — no implicit SDK resolution via global.json |

---

## Verification Approach

1. **Trait coverage**: Confirm all files under `test/testAPI/Unit/` have `[Trait("Category","Unit")]` — already verified in `GetAllBrandsQueryHandlerTests.cs`, `GetByIdQueryHandlerTests.cs`, `GetAllModelsQueryHandlerTests.cs`.
2. **Workflow file lint**: Validate `ci.yml` YAML syntax before push (e.g., `actionlint` or GitHub's built-in check).
3. **First run green**: After merging, verify the Actions tab shows a green run with only unit tests in the test output and no integration test names present.
4. **README render**: Open the repository root on GitHub and confirm Spanish content, badge placeholder visibility, and no broken Markdown.
