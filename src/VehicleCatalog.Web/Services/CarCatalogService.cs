using System.Text.Json;
using VehicleCatalog.Web.Models;

namespace VehicleCatalog.Web.Services;

public class CarCatalogService : ICarCatalogService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public CarCatalogService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<CarBrandViewModel>> GetBrandsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<CarBrandViewModel>>("api/carBrands", _jsonOptions);
        return result ?? new List<CarBrandViewModel>();
    }

    public async Task<List<CarModelViewModel>> GetModelsAsync(string? brand = null)
    {
        var url = string.IsNullOrWhiteSpace(brand)
            ? "api/carModels"
            : $"api/carModels?brand={Uri.EscapeDataString(brand)}";
        var result = await _httpClient.GetFromJsonAsync<List<CarModelViewModel>>(url, _jsonOptions);
        return result ?? new List<CarModelViewModel>();
    }
}
