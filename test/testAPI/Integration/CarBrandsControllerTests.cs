using System.Net;
using System.Text.Json;
using Xunit;
using testAPI.Integration.Fixtures;
using API.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace testAPI.Integration;

[Collection("SqlServer")]
public class CarBrandsControllerTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private readonly HttpClient _client;
    private VehicleCatalogDbContext _db = null!;
    private IServiceScope _scope = null!;

    public CarBrandsControllerTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    public async Task InitializeAsync()
    {
        _scope = _fixture.Services.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<VehicleCatalogDbContext>();
        await _fixture.ClearDataAsync(_db);
        await _fixture.SeedDataAsync(_db);
    }

    public async Task DisposeAsync()
    {
        await _fixture.ClearDataAsync(_db);
        await _fixture.SeedDataAsync(_db);
        _scope.Dispose();
    }

    /// <summary>SCEN-I-01: GET /api/carBrands with seeded data → 200 OK, non-empty array with id+name.</summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task getAll_WithSeededData_ShouldReturn200AndBrandList()
    {
        var response = await _client.GetAsync("/api/carBrands");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var brands = JsonSerializer.Deserialize<List<JsonElement>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(brands);
        Assert.True(brands!.Count >= 1, "Expected at least 1 brand in response");

        foreach (var brand in brands)
        {
            Assert.True(brand.TryGetProperty("id", out var idProp), "Each brand must have 'id' field");
            Assert.True(brand.TryGetProperty("name", out var nameProp), "Each brand must have 'name' field");
            Assert.True(idProp.GetInt32() > 0, "'id' must be a positive integer");
            Assert.False(string.IsNullOrEmpty(nameProp.GetString()), "'name' must not be empty");
        }
    }

    /// <summary>SCEN-I-02: GET /api/carBrands with empty DB → 200 OK, empty array [].</summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task getAll_WithEmptyDatabase_ShouldReturn200AndEmptyArray()
    {
        await _fixture.ClearDataAsync(_db);

        var response = await _client.GetAsync("/api/carBrands");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var brands = JsonSerializer.Deserialize<List<JsonElement>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(brands);
        Assert.Empty(brands!);

        // Restore seed for other tests
        await _fixture.SeedDataAsync(_db);
    }
}
