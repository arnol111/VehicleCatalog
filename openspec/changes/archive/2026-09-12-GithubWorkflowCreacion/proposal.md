# Proposal: GitHub Actions CI Workflow & README Rewrite

## Intent

VehicleCatalog has no CI pipeline and no Spanish README. Merges to `main` are unguarded by automated checks despite an active branch ruleset requiring PRs. This change introduces a minimal CI workflow (unit tests only) and rewrites the README in Spanish to match the team's language.

## Scope

### In Scope
- `.github/workflows/ci.yml` — push + PR triggers on `main`; jobs: checkout, setup-dotnet 10.0.x, restore, build, unit test filter
- `README.md` — full rewrite in Spanish: project structure, local run instructions, test instructions, branch workflow, CI badge placeholder

### Out of Scope
- Integration test execution in CI (Testcontainers requires Docker-in-Docker; deferred)
- CD / deployment pipeline
- Code coverage reporting or thresholds
- Branch protection rule changes (ruleset already active)
- Multi-environment matrix builds

## Capabilities

### New Capabilities
- `github-ci-workflow`: Automated CI that validates unit tests on every push/PR to `main`

### Modified Capabilities
None

## Approach

1. Create `.github/workflows/ci.yml` using `actions/checkout@v4` and `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'`. Run `dotnet restore`, `dotnet build --no-restore --configuration Release`, `dotnet test --no-build --filter "Category=Unit"`.
2. Rewrite `README.md` entirely in neutral professional Spanish: sections for description, project structure, prerequisites, local run, test execution, branch workflow, and a CI badge placeholder referencing the new workflow.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `.github/workflows/ci.yml` | New | CI pipeline (~30 lines) |
| `README.md` | Modified | Full rewrite in Spanish (~60 lines) |

**Estimated changed lines**: ~90 additions, ~20 deletions (README prior content). Well within 400-line budget.

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| `dotnet-version: 10.0.x` not yet available on `ubuntu-latest` runners | Low | Pin to `10.0.x`; GitHub adds .NET versions promptly at GA. Verify after first push. |
| Unit test filter `Category=Unit` matches zero tests if trait is missing | Med | Verify trait usage in `test/testAPI` before merging; add `[Trait("Category","Unit")]` to unit test classes if absent |
| Integration tests accidentally picked up by filter | Low | Filter is explicit; Testcontainers tests should use `Category=Integration` trait |

## Rollback Plan

- **CI workflow**: Delete `.github/workflows/ci.yml`. No runtime impact — workflow is additive only.
- **README**: `git revert <commit>` or restore from `git show HEAD~1:README.md > README.md`.

## Dependencies

- GitHub Actions runners with .NET 10.0.x support (available on `ubuntu-latest`)
- Unit tests must carry `[Trait("Category","Unit")]` attribute — verify before apply

## Success Criteria

- [ ] `ci.yml` triggers on push to `main` and on PRs targeting `main`
- [ ] CI job completes green with unit tests passing
- [ ] Integration tests are NOT executed in CI (no Docker dependency)
- [ ] README renders correctly in GitHub with Spanish content and badge placeholder
- [ ] No broken markdown links or formatting issues in README
