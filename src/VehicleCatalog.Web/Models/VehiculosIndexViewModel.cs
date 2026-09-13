using System.Collections.Generic;

namespace VehicleCatalog.Web.Models;

public class VehiculosIndexViewModel
{
    public List<CarModelViewModel> Modelos { get; set; } = new();
    public List<CarBrandViewModel> Marcas { get; set; } = new();
    public string? MarcaSeleccionada { get; set; }
}
