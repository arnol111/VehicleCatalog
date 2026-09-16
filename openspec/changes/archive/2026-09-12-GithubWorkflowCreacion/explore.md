# Exploration: GithubWorkflowCreacion

## Current State

The repository `arnol111/VehicleCatalog` is a .NET 10 / ASP.NET Core Clean Architecture solution with four source projects and one test project. There is **no `.github/` directory** and **no existing CI/CD configuration** of any kind. A ruleset "Require a pull request before merging" has been added on GitHub, meaning all changes must arrive via PR from a branch.

---

## Project Structure Summary

```
VehicleCatalog/
├── src/
│   ├── API/                    — ASP.NET Core host (net10.0), Controllers, DI root
│   ├── API.Application/        — Use cases, handlers, DTOs (net10.0)
│   ├── API.Domain/             — Entities, repository interfaces, domain exceptions (net10.0)
│   └── API.Infrastructure/     — EF Core DbContext, repository implementations (net10.0)
└── test/
    └── testAPI/                — xUnit test suite (net10.0)
```

No `global.json` found — the SDK version is pinned only via `<TargetFramework>net10.0</TargetFramework>` in each `.csproj`.

---

## Affected Areas

- `.github/workflows/ci.yml` — **must be created** (does not exist)
- `README.md` — exists, well-documented in English (with a Spanish "Proyecto de Pruebas" section), no CI badge yet

---

## Existing CI/CD Setup

**None.** No `.github/` directory, no workflow YAML, no badge in README. This is a greenfield CI setup.

---

## README Current State

- Language: English (primary), Spanish (test section)
- Content: well-structured — architecture overview, tech stack, setup/run instructions, endpoint reference, migration commands, test matrix
- Missing: CI badge (can be added after workflow is created)

---

## Test Infrastructure

| Package | Purpose |
|---------|---------|
| `xunit 2.9.3` | Test framework |
| `xunit.runner.visualstudio 2.9.3` | VS/CLI runner |
| `Microsoft.NET.Test.Sdk 17.14.1` | dotnet test host |
| `NSubstitute 5.3.0` | Mocking |
| `Microsoft.AspNetCore.Mvc.Testing 10.0.12` | Integration — WebApplicationFactory |
| `Testcontainers.MsSql 4.15.0` | Integration — spins up SQL Server container (requires Docker) |
| `FluentAssertions 6.12.2` | Assertions |
| `coverlet.collector 6.0.4` | Code coverage |

**Key constraint**: integration tests require Docker Desktop running to spin up `Testcontainers.MsSql`. Unit tests (`--filter "Category=Unit"`) have no external dependencies.

---

## .NET Version Confirmed

- **Target framework**: `net10.0` (all projects)
- **No `global.json`** — `actions/setup-dotnet@v4` must specify `dotnet-version: '10.0.x'` explicitly

---

## Approaches

### 1. **Minimal CI — Unit tests only** (recommended for immediate unblock)
Run only unit tests in CI; skip integration tests that require Docker.

- Pros: No Docker-in-GitHub-Actions setup needed; fast feedback; immediately safe with the new branch protection rule; zero external service dependencies
- Cons: Integration tests not verified in CI; gaps in confidence on DB layer
- Effort: Low

### 2. **Full CI — Unit + Integration tests with Docker**
Enable Docker in the GitHub Actions runner and run all tests.

- Pros: Full test coverage in CI including DB integration
- Cons: Integration tests are slower; `ubuntu-latest` runners have Docker but it needs to be confirmed available; Testcontainers pulls SQL Server image (~500 MB) on each run increasing job time; risk of flaky container startup
- Effort: Medium

### 3. **Split jobs — Unit fast / Integration on schedule**
One job runs unit tests on every push/PR; a second job runs integration tests only on schedule or on `main` merge.

- Pros: Fast PR feedback + full coverage on merge; mirrors a realistic pipeline
- Cons: More complex YAML; integration failures may not block PRs
- Effort: Medium-High

---

## Recommendation

**Start with Approach 1 (unit tests only)** to immediately satisfy the branch protection rule requirement with a working, reliable pipeline. The workflow should:

- Trigger on `push` (all branches) and `pull_request` (targeting `main`)
- Use `ubuntu-latest`
- Steps: `actions/checkout@v4` → `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'` → `dotnet restore` → `dotnet build --no-restore` → `dotnet test --no-build --filter "Category=Unit"`

Approach 2 can be layered in a follow-up change once Docker availability is confirmed on the runner tier in use.

---

## Risks

- **No `global.json`**: `dotnet-version: '10.0.x'` must be explicit in the workflow — `.NET 10` is still in preview/RC cadence so `x` must resolve correctly on `setup-dotnet@v4`.
- **Integration tests require Docker**: running all tests without filtering will fail in a standard runner if `Testcontainers` cannot pull the SQL Server image (network, Docker daemon availability).
- **README CI badge**: currently missing — should be added after the workflow is created and has its first successful run.
- **No solution file found at root**: `dotnet build` should target `src/` or pass the `.sln` path explicitly — confirm whether a `.sln` exists at root level before writing the workflow.

> **Note**: A solution file scan was performed to depth 3; no `.sln` was returned in the file listing. The build step should use `dotnet build src/API/API.csproj` or a wildcard restore, unless a `.sln` is present at root.

---

## Ready for Proposal

**Yes.** All facts needed to write the workflow YAML are confirmed:
- Repo: `arnol111/VehicleCatalog`
- SDK: `net10.0` → `dotnet-version: '10.0.x'`
- Test runner: xUnit via `dotnet test`
- Unit test filter: `--filter "Category=Unit"`
- No existing CI to migrate or conflict with
- Recommended trigger: `push` + `pull_request` on `main`
