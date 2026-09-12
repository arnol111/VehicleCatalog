# Design: TestProyect — VehicleCatalog Test Suite

## Technical Approach

Add a `test/testAPI` project targeting net10.0, referenced in `VehicleCatalog.slnx`. Unit tests instantiate handlers directly against NSubstitute mocks of `IUnitOfWork`. Integration tests boot `WebApplicationFactory<Program>` against a real SQL Server container (Testcontainers.MsSql) — preserving the production EF provider and avoiding any dialect swap.

---

## Architecture Decisions

| Area | Option | Choice | Rationale |
|------|--------|--------|-----------|
| Integration DB | InMemory / SQLite / Testcontainers.MsSql / Testcontainers.PostgreSql | **Testcontainers.MsSql** | Production uses SQL Server; `UseIdentityColumn()` in EF configs is SQL Server-specific — switching provider at test time requires config overrides. MsSql container keeps the exact same EF provider with zero risk. |
| Mock library | Moq / NSubstitute | **NSubstitute** | Cleaner async setup, no `.Object` wrapper, consistent with proposal. |
| Fixture scope | Per-class / per-collection | **ICollectionFixture** (per-collection) | Container startup is slow (~5–10 s). One container per test run via `[Collection("SqlServer")]`. |
| DbContext override | `WithWebHostBuilder` / `ConfigureTestServices` | **`ConfigureTestServices`** | Runs after `AddInfrastructure`, so the SQL Server descriptor can be found and replaced cleanly. |
| Handler fix scope | Leave generic `Exception` / switch to `NotFoundException` | **Switch to `NotFoundException`** | `CarBrandController` catches generic `Exception` → 404. After the fix, the same catch still works. The semantic improvement is that `NotFoundException` is the correct domain signal; a future catch split requires no handler change. |
| Controller catch split | Add `NotFoundException` catch / leave generic catch | **Leave generic catch in `CarBrandController`** | The controller returns 404 for any exception today. Adding a specific `NotFoundException` catch produces identical HTTP behavior. Deferring the catch split to a future refactor avoids scope creep. |

---

## Data Flow

### Unit Test Path

```
xUnit test method
  └─► new Handler(mockUoW)
        └─► mockUoW.CarBrands.GetByIdAsync(id) ──► [NSubstitute returns stub CarBrand or null]
              └─► Handler maps → DTO / throws NotFoundException
                    └─► Assert result / Assert throws
```

### Integration Test Path

```
xUnit test method
  └─► HttpClient (from WebApplicationFactory)
        └─► ASP.NET Core pipeline (full DI)
              └─► Controller ──► IDispatcher ──► Handler
                    └─► IUnitOfWork ──► EF Core (SQL Server provider)
                          └─► MsSqlContainer (Testcontainers)
```

### Fixture Lifecycle

```
SqlServerFixture (IAsyncLifetime)
  InitializeAsync:
    MsSqlContainer.StartAsync()
    WebApplicationFactory.CreateClient()       ← ConfigureTestServices replaces DbContext
    DbContext.Database.EnsureCreated()          ← applies EF model + HasData seed

  [CollectionFixture shared across all Integration test classes]

  Per-test: SeedData() / ClearData() called in test constructor or IAsyncLifetime
```

---

## File Tree

```
VehicleCatalog/
├── test/
│   └── testAPI/
│       ├── testAPI.csproj
│       ├── Unit/
│       │   ├── CarBrand/
│       │   │   ├── GetAllBrandsQueryHandlerTests.cs
│       │   │   └── GetByIdQueryHandlerTests.cs
│       │   └── CarModel/
│       │       └── GetAllModelsQueryHandlerTests.cs
│       └── Integration/
│           ├── Fixtures/
│           │   └── SqlServerFixture.cs          ← IAsyncLifetime + ICollectionFixture
│           ├── CarBrandsControllerTests.cs
│           ├── CarBrandControllerTests.cs
│           └── CarModelsControllerTests.cs
├── src/
│   └── API.Application/CarBrand/Query/GetById/
│       └── GetByIdQueryHandler.cs               ← Modified: Exception → NotFoundException
├── VehicleCatalog.slnx                          ← Modified: add test/testAPI/testAPI.csproj
└── README.md                                    ← Modified: add Spanish test section
```

---

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `test/testAPI/testAPI.csproj` | Create | Test project, net10.0, NuGet refs, project refs |
| `test/testAPI/Unit/CarBrand/GetAllBrandsQueryHandlerTests.cs` | Create | 1 unit test |
| `test/testAPI/Unit/CarBrand/GetByIdQueryHandlerTests.cs` | Create | 2 unit tests |
| `test/testAPI/Unit/CarModel/GetAllModelsQueryHandlerTests.cs` | Create | 4 unit tests |
| `test/testAPI/Integration/Fixtures/SqlServerFixture.cs` | Create | Container + WAF fixture |
| `test/testAPI/Integration/CarBrandsControllerTests.cs` | Create | 2 integration tests |
| `test/testAPI/Integration/CarBrandControllerTests.cs` | Create | 3 integration tests |
| `test/testAPI/Integration/CarModelsControllerTests.cs` | Create | 4 integration tests |
| `src/API.Application/CarBrand/Query/GetById/GetByIdQueryHandler.cs` | Modify | `Exception` → `NotFoundException` |
| `VehicleCatalog.slnx` | Modify | Add test project under `/test/` folder |
| `README.md` | Modify | Add Spanish test documentation section |

---

## Interfaces / Contracts

### testAPI.csproj (key structure)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="NSubstitute" Version="5.*" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.*" />
    <PackageReference Include="Testcontainers.MsSql" Version="4.*" />
    <PackageReference Include="FluentAssertions" Version="6.*" />
    <PackageReference Include="coverlet.collector" Version="6.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\API\API.csproj" />
    <ProjectReference Include="..\..\src\API.Application\API.Application.csproj" />
    <ProjectReference Include="..\..\src\API.Domain\API.Domain.csproj" />
    <ProjectReference Include="..\..\src\API.Infrastructure\API.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

### VehicleCatalog.slnx change

```xml
<Solution>
  <Folder Name="/src/">
    <!-- existing entries unchanged -->
  </Folder>
  <Folder Name="/test/">
    <Project Path="test/testAPI/testAPI.csproj" />
  </Folder>
</Solution>
```

### SqlServerFixture (key signatures)

```csharp
// test/testAPI/Integration/Fixtures/SqlServerFixture.cs
[CollectionDefinition("SqlServer")]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture> { }

public class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();
    public HttpClient Client { get; private set; } = null!;
    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove SQL Server DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<VehicleCatalogDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<VehicleCatalogDbContext>(options =>
                    options.UseSqlServer(_container.GetConnectionString()));
            });
        });
        Client = _factory.CreateClient();

        // Apply EF model and HasData seed (no migrations needed)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VehicleCatalogDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    // Call in tests that need a clean state
    public async Task ClearDataAsync(VehicleCatalogDbContext db)
    {
        db.CarModels.RemoveRange(db.CarModels);
        db.CarBrands.RemoveRange(db.CarBrands);
        await db.SaveChangesAsync();
    }

    // Call to restore seed state after a ClearData
    public async Task SeedDataAsync(VehicleCatalogDbContext db)
    {
        // Insert known test rows explicitly (do NOT rely on HasData after clear)
        db.CarBrands.Add(new CarBrand { IdCarBrand = 1, Brand = "Toyota" });
        // ... add other seed rows
        await db.SaveChangesAsync();
    }
}
```

### Unit test class structure

```csharp
// test/testAPI/Unit/CarBrand/GetByIdQueryHandlerTests.cs
public class GetByIdQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICarBrandRepository _brandRepo = Substitute.For<ICarBrandRepository>();
    private readonly GetByIdQueryHandler _handler;

    public GetByIdQueryHandlerTests()
    {
        _unitOfWork.CarBrands.Returns(_brandRepo);
        _handler = new GetByIdQueryHandler(_unitOfWork);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task getBrandById_WithValidId_ShouldReturnBrand()
    {
        _brandRepo.GetByIdAsync(1).Returns(new CarBrand { IdCarBrand = 1, Brand = "Toyota" });
        var result = await _handler.Handle(new GetByIdQuery(1));
        result.IdCarBrand.Should().Be(1);
        result.Brand.Should().Be("Toyota");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task getBrandById_WithInvalidId_ShouldThrowNotFoundException()
    {
        _brandRepo.GetByIdAsync(999).Returns((CarBrand?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new GetByIdQuery(999)));
    }
}
```

**Critical pattern for CarModel unit tests** — `CarModelResponse` accesses `m.CarBrand!.Brand`, so the `CarBrand` navigation property MUST be populated on every mock-returned `CarModel`:

```csharp
// REQUIRED: always set CarBrand on CarModel mock instances
var model = new CarModel
{
    IdCarModel = 1,
    Name = "Corolla",
    Year = 2022,
    IdCarBrand = 1,
    CarBrand = new CarBrand { IdCarBrand = 1, Brand = "Toyota" }  // ← mandatory
};
```

### Integration test class structure

```csharp
// test/testAPI/Integration/CarBrandControllerTests.cs
[Collection("SqlServer")]
public class CarBrandControllerTests
{
    private readonly HttpClient _client;
    public CarBrandControllerTests(SqlServerFixture fixture) => _client = fixture.Client;

    [Fact]
    [Trait("Category", "Integration")]
    public async Task getById_WithValidId_ShouldReturn200()
    {
        var response = await _client.GetAsync("/carBrand?id=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task getById_WithInvalidId_ShouldReturn404()
    {
        var response = await _client.GetAsync("/carBrand?id=9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task getById_WithNonIntId_ShouldReturn400()
    {
        var response = await _client.GetAsync("/carBrand?id=abc");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

---

## Handler Fix Decision

**Change:** Replace `throw new Exception("La marca de carro no existe")` with `throw new NotFoundException("La marca de carro no existe")` in `GetByIdQueryHandler.cs`.

**Controller impact:** `CarBrandController` catches `catch (Exception e)` → `NotFound(e.Message)`. Since `NotFoundException : Exception`, this catch still fires. HTTP behavior is **identical** before and after the fix.

**Decision on catch split:** Leave `CarBrandController`'s generic catch as-is. Adding a specific `NotFoundException` catch produces the same 404 response and is not required for correctness. A future controller refactor can split catches when other exception types need different HTTP codes.

**One-line diff:**
```diff
- throw new Exception("La marca de carro no existe");
+ throw new NotFoundException("La marca de carro no existe");
```

---

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | Handler logic, null guard, case-insensitive filter, nav-prop access | Direct instantiation + NSubstitute; `[Trait("Category","Unit")]` |
| Integration | Full HTTP round-trip, 200/404/400 codes, empty DB scenario, filtered queries | WAF + MsSqlContainer; `[Trait("Category","Integration")]` |
| E2E | N/A — out of scope for this change | — |

Run commands:
```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"   # requires Docker
dotnet test test/testAPI                       # all tests
```

---

## Threat Matrix

N/A — no routing boundary changes, shell commands, subprocesses, VCS/PR automation, executable-file classification, or process-integration boundary in this change.

---

## Migration / Rollout

No migration required. The test project is purely additive. The handler fix is a one-line change in a single file with no behavioral change at the HTTP layer.

---

## Open Questions

- [ ] Confirm `Testcontainers.MsSql` v4.x supports net10.0 (expected yes — library targets netstandard2.1)
- [ ] Confirm latest `Microsoft.AspNetCore.Mvc.Testing` 10.0.* stable version is published on NuGet at implementation time
