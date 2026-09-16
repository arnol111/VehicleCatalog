using System.Collections.ObjectModel;
using VehicleCatalog.Maui.Models;
using VehicleCatalog.Maui.Services;

namespace VehicleCatalog.Maui;

public partial class ModelosPage : ContentPage
{
    private readonly CarCatalogService _service = new();
    public ObservableCollection<CarModel> Modelos { get; } = new();
    private bool _isLoadingBrands;

    public ModelosPage()
    {
        InitializeComponent();
        modelsCollection.ItemsSource = Modelos;
    }

    // REQ-1 / SCEN-1.1 / SCEN-1.2 / SCEN-5.1
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        SetLoading(true);
        try
        {
            // Load brands into Picker
            var brands = await _service.GetBrandsAsync();
            _isLoadingBrands = true;
            brandPicker.Items.Clear();
            brandPicker.Items.Add("Todas las marcas");
            foreach (var b in brands)
            {
                brandPicker.Items.Add(b.Name);
            }
            _isLoadingBrands = false;

            // Load all models (no brand filter)
            var models = await _service.GetModelsAsync();
            RefreshModelos(models);
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Error", "No se pudo conectar con la API", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    // REQ-2 / REQ-3 / SCEN-2.1 / SCEN-2.2 / SCEN-3.1 / SCEN-5.2
    private async void BrandPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Guard: skip when Picker is being populated programmatically
        if (_isLoadingBrands)
            return;

        var index = brandPicker.SelectedIndex;
        if (index < 0)
            return;

        SetLoading(true);
        try
        {
            List<CarModel> models;
            if (index == 0)
            {
                // "Todas las marcas" — no brand param
                models = await _service.GetModelsAsync();
            }
            else
            {
                var selectedBrand = brandPicker.Items[index];
                models = await _service.GetModelsAsync(selectedBrand);
            }
            RefreshModelos(models);
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Error", "No se pudo conectar con la API", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    // REQ-4 / SCEN-4.1
    private void SetLoading(bool loading)
    {
        activityIndicator.IsRunning = loading;
        activityIndicator.IsVisible = loading;
    }

    // REQ-1 / REQ-2 / REQ-3 / SCEN-1.2 / SCEN-2.2
    private void RefreshModelos(List<CarModel> models)
    {
        Modelos.Clear();
        foreach (var m in models)
        {
            Modelos.Add(m);
        }
        emptyLabel.IsVisible = Modelos.Count == 0;
    }
}
