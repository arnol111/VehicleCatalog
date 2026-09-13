using Microsoft.AspNetCore.Mvc;
using VehicleCatalog.Web.Models;
using VehicleCatalog.Web.Services;

namespace VehicleCatalog.Web.Controllers;

public class VehiculosController : Controller
{
    private readonly ICarCatalogService _service;

    public VehiculosController(ICarCatalogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? marca)
    {
        try
        {
            var marcaNormalizada = string.IsNullOrWhiteSpace(marca) || marca == "Todas"
                ? null
                : marca;

            var modelos = await _service.GetModelsAsync(marcaNormalizada);
            var marcas = await _service.GetBrandsAsync();

            var vm = new VehiculosIndexViewModel
            {
                Modelos = modelos,
                Marcas = marcas,
                MarcaSeleccionada = marca
            };

            return View(vm);
        }
        catch (Exception)
        {
            ViewBag.ErrorMessage = "No se pudo conectar con el servicio de vehículos. Por favor, intentá más tarde.";
            return View("Error");
        }
    }
}
