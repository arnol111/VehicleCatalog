using System.Net.Http.Json;
using System.Text.Json;
using VehicleCatalog.Maui.Models;

namespace VehicleCatalog.Maui.Services;

public class CarCatalogService
{
#if ANDROID
    private const string BaseUrl = "http://10.0.2.2:5023/";
#else
    private const string BaseUrl = "https://localhost:7294/";
#endif

    private readonly HttpClient _client;

    public CarCatalogService()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public async Task<List<CarBrand>> GetBrandsAsync()
    {
        var response = await _client.GetAsync("api/carBrands");
        response.EnsureSuccessStatusCode();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return await response.Content.ReadFromJsonAsync<List<CarBrand>>(options) ?? new List<CarBrand>();
    }

    public async Task<List<CarModel>> GetModelsAsync(string? brand = null)
    {
        var url = "api/carModels";
        if (!string.IsNullOrEmpty(brand))
        {
            url += $"?brand={Uri.EscapeDataString(brand)}";
        }

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return await response.Content.ReadFromJsonAsync<List<CarModel>>(options) ?? new List<CarModel>();
    }
}
