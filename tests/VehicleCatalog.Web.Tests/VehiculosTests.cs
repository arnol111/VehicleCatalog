using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VehicleCatalog.Web.Controllers;
using VehicleCatalog.Web.Models;
using VehicleCatalog.Web.Services;

namespace VehicleCatalog.Web.Tests;

// ────────────────────────────────────────────────────────────
// CarCatalogService — URL construction tests
// ────────────────────────────────────────────────────────────

[Trait("Category", "Unit")]
public class CarCatalogServiceTests
{
    private static (CarCatalogService service, List<Uri> capturedUris) BuildService(string responseJson)
    {
        var capturedUris = new List<Uri>();

        var handler = new FakeHttpMessageHandler(request =>
        {
            capturedUris.Add(request.RequestUri!);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7294/")
        };

        return (new CarCatalogService(httpClient), capturedUris);
    }

    [Fact]
    public async Task GetModelsAsync_NullBrand_RequestsUrlWithoutQueryString()
    {
        var (service, uris) = BuildService("[]");

        await service.GetModelsAsync(null);

        Assert.Single(uris);
        Assert.Equal("/api/carModels", uris[0].AbsolutePath);
        Assert.Equal(string.Empty, uris[0].Query);
    }

    [Fact]
    public async Task GetModelsAsync_WithBrand_AppendsBrandQueryParam()
    {
        var (service, uris) = BuildService("[]");

        await service.GetModelsAsync("Ford");

        Assert.Single(uris);
        Assert.Equal("/api/carModels", uris[0].AbsolutePath);
        Assert.Equal("?brand=Ford", uris[0].Query);
    }

    [Fact]
    public async Task GetModelsAsync_BrandWithSpaces_EncodesQueryParam()
    {
        var (service, uris) = BuildService("[]");

        await service.GetModelsAsync("General Motors");

        Assert.Single(uris);
        Assert.Equal("/api/carModels", uris[0].AbsolutePath);
        Assert.Equal("?brand=General%20Motors", uris[0].Query);
    }
}

// ────────────────────────────────────────────────────────────
// VehiculosController — behaviour tests
// ────────────────────────────────────────────────────────────

[Trait("Category", "Unit")]
public class VehiculosControllerTests
{
    private static Mock<ICarCatalogService> BuildServiceMock(
        List<CarModelViewModel>? modelos = null,
        List<CarBrandViewModel>? marcas = null)
    {
        var mock = new Mock<ICarCatalogService>();
        mock.Setup(s => s.GetModelsAsync(It.IsAny<string?>()))
            .ReturnsAsync(modelos ?? new List<CarModelViewModel>());
        mock.Setup(s => s.GetBrandsAsync())
            .ReturnsAsync(marcas ?? new List<CarBrandViewModel>());
        return mock;
    }

    [Fact]
    public async Task Index_ReturnsViewWithPopulatedViewModel()
    {
        var modelos = new List<CarModelViewModel>
        {
            new() { Id = 1, Name = "Fiesta", BrandName = "Ford", Year = 2022 }
        };
        var marcas = new List<CarBrandViewModel>
        {
            new() { Id = 1, Name = "Ford" }
        };

        var mock = BuildServiceMock(modelos, marcas);
        var controller = new VehiculosController(mock.Object);

        var result = await controller.Index(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<VehiculosIndexViewModel>(viewResult.Model);
        Assert.Single(vm.Modelos);
        Assert.Single(vm.Marcas);
        Assert.Null(vm.MarcaSeleccionada);
    }

    [Fact]
    public async Task Index_WhenServiceThrows_ReturnsErrorViewWithMessage()
    {
        var mock = new Mock<ICarCatalogService>();
        mock.Setup(s => s.GetModelsAsync(It.IsAny<string?>()))
            .ThrowsAsync(new HttpRequestException("connection refused"));

        var controller = new VehiculosController(mock.Object);

        var result = await controller.Index(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
        Assert.NotNull(controller.ViewBag.ErrorMessage);
    }

    [Fact]
    public async Task Index_MarcaTodasNormalizesToNull()
    {
        var mock = BuildServiceMock();
        var controller = new VehiculosController(mock.Object);

        await controller.Index("Todas");

        // Service should have been called with null (not "Todas")
        mock.Verify(s => s.GetModelsAsync(null), Times.Once);
    }
}

// ────────────────────────────────────────────────────────────
// Test helpers
// ────────────────────────────────────────────────────────────

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        => _handler = handler;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        => Task.FromResult(_handler(request));
}
