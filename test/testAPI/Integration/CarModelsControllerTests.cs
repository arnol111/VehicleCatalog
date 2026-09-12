using System.Net;
using System.Text.Json;
using Xunit;
using testAPI.Integration.Fixtures;
using API.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace testAPI.Integration;

[Collection("SqlServer")]
public class CarModelsControllerTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private readonly HttpClient _client;
    private VehicleCatalogDbContext _db = null!;
    private IServiceScope _scope = null!;

    public CarModelsControllerTests(SqlServerFixture fixture)
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

    /// <summary>SCEN-I-03: GET /api/carModels with seeded data → 200 OK, items include id, name, year, brandName.</summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task getAll_WithSeededData_ShouldReturn200AndModelList()
    {
        var response = await _client.GetAsync("/api/carModels");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var models = JsonSerializer.Deserialize<List<JsonElement>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(models);
        Assert.True(models!.Count >= 1, "Expected at least 1 model in response");

        foreach (var model in models)
        {
            Assert.True(model.TryGetProperty("id", out _), "Each model must have 'id'");
            Assert.True(model.TryGetProperty("name", out _), "Each model must have 'name'");
            Assert.True(model.TryGetProperty("year", out _), "Each model must have 'year'");
            Assert.True(model.TryGetProperty("brandName", out _), "Each model must have 'brandName'");
        }
    }

    /// <summary>SCEN-I-04: GET /api/carModels?brand=Toyota → 200 OK, only Toyota models returned.</summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task getAll_FilteredByToyota_ShouldReturnOnlyToyotaModels()
    {
        var response = await _client.GetAsync("/api/carModels?brand=Toyota");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var models = JsonSerializer.Deserialize<List<JsonElement>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(models);
        Assert.True(models!.Count >= 1, "Expected at least 1 Toyota model");

        foreach (var model in models)
        {
            Assert.True(model.TryGetProperty("brandName", out var brandProp));
            Assert.Equal("Toyota", brandProp.GetString());
        }

        // Ensure no Ford models appear
        Assert.DoesNotContain(models, m =>
            m.TryGetProperty("brandName", out var bp) && bp.GetString() == "Ford");
    }

    /// <summary>SCEN-I-05: GET /api/carModels?brand=tOyOtA → 200 OK, same result as brand=Toyota (case-insensitive).</summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task getAll_FilteredByCaseInsensitiveToyota_ShouldReturnSameAsToyota()
    {
        var responseMixedCase = await _client.GetAsync("/api/carModels?brand=tOyOtA");
        var responseExact = await _client.GetAsync("/api/carModels?brand=Toyota");

        Assert.Equal(HttpStatusCode.OK, responseMixedCase.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responseExact.StatusCode);

        var jsonMixed = await responseMixedCase.Content.ReadAsStringAsync();
        var jsonExact = await responseExact.Content.ReadAsStringAsync();

        var modelsMixed = JsonSerializer.Deserialize<List<JsonElement>>(jsonMixed, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var modelsExact = JsonSerializer.Deserialize<List<JsonElement>>(jsonExact, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(modelsMixed);
        Assert.NotNull(modelsExact);
        Assert.Equal(modelsExact!.Count, modelsMixed!.Count);

        foreach (var model in modelsMixed)
        {
            Assert.True(model.TryGetProperty("brandName", out var brandProp));
            Assert.Equal("Toyota", brandProp.GetString());
        }
    }

    /// <summary>SCEN-I-06: GET /api/carModels?brand=MarcaInexistente → 404 Not Found (OI-01 confirmed).</summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task getAll_FilteredByNonExistentBrand_ShouldReturn404()
    {
        var response = await _client.GetAsync("/api/carModels?brand=MarcaInexistente");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
