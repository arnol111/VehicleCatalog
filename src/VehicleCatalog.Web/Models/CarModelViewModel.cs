namespace VehicleCatalog.Web.Models;

public class CarModelViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public string BrandName { get; set; } = string.Empty;
}
