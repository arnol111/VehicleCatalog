using System.Collections.ObjectModel;
using VehicleCatalog.Maui.Models;
using VehicleCatalog.Maui.Services;

namespace VehicleCatalog.Maui;

public partial class MarcasPage : ContentPage
{
    private readonly CarCatalogService _service = new();
    public ObservableCollection<CarBrand> Brands { get; } = new();
    private bool _loaded;

    public MarcasPage()
    {
        InitializeComponent();
        brandsCollection.ItemsSource = Brands;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded) return;
        _loaded = true;
        try
        {
            var brands = await _service.GetBrandsAsync();
            foreach (var b in brands) Brands.Add(b);
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Error", "No se pudo conectar con la API", "OK");
        }
    }
}
