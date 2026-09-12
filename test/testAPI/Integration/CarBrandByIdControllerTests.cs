using System.Net;
using System.Text.Json;
using Xunit;
using testAPI.Integration.Fixtures;
using API.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace testAPI.Integration;

[Collection("SqlServer")]
public class CarBrandByIdControllerTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private readonly HttpClient _client;
    private VehicleCatalogDbContext _db = null!;
    private IServiceScope _scope = null!;

    public CarBrandByIdControllerTests(SqlServerFixture fixture)
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

    /// <summary>SCEN-I-07: GET /carBrand?id={existingId} → 200 OK, correct brand object with idCarBrand and brand.</summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task getById_WithExistingId_ShouldReturn200AndBrandObject()
    {
        // Discover a real ID from the seeded data via the list endpoint
        var listResponse = await _client.GetAsync("/api/carBrands");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listJson = await listResponse.Content.ReadAsStringAsync();
        var brands = JsonSerializer.Deserialize<List<JsonElement>>(listJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(brands);
        Assert.True(brands!.Count >= 1, "Expected at least 1 seeded brand");
        var existingId = brands[0].GetProperty("id").GetInt32();

        var response = await _client.GetAsync($"/carBrand?id={existingId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var brand = JsonSerializer.Deserialize<JsonElement>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.True(brand.TryGetProperty("idCarBrand", out var idProp), "Response must contain 'idCarBrand'");
        Assert.Equal(existingId, idProp.GetInt32());

        Assert.True(brand.TryGetProperty("brand", out var brandProp), "Response must contain 'brand'");
        Assert.False(string.IsNullOrEmpty(brandProp.GetString()), "'brand' must not be empty");
    }

    /// <summary>SCEN-I-08: GET /carBrand?id=9999 (non-existent) → 404 Not Found.</summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task getById_WithNonExistentId_ShouldReturn404()
    {
        var response = await _client.GetAsync("/carBrand?id=9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>SCEN-I-09: GET /carBrand?id=abc (invalid format) → 400 Bad Request.</summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task getById_WithNonIntegerId_ShouldReturn400()
    {
        var response = await _client.GetAsync("/carBrand?id=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
