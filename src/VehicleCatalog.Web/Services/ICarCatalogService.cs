using VehicleCatalog.Web.Models;

namespace VehicleCatalog.Web.Services;

public interface ICarCatalogService
{
    Task<List<CarBrandViewModel>> GetBrandsAsync();
    Task<List<CarModelViewModel>> GetModelsAsync(string? brand = null);
}
