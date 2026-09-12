using Xunit;
using API.Application.CarModel.Query.GetAll;
using API.Application.DTOs;
using API.Domain.Entities;
using API.Domain.Exceptions;
using API.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace testAPI.Unit.CarModel;

public class GetAllModelsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICarBrandRepository _brandRepo = Substitute.For<ICarBrandRepository>();
    private readonly ICarModelRepository _modelRepo = Substitute.For<ICarModelRepository>();
    private readonly GetAllModelsQueryHandler _handler;

    public GetAllModelsQueryHandlerTests()
    {
        _unitOfWork.CarBrands.Returns(_brandRepo);
        _unitOfWork.CarModels.Returns(_modelRepo);
        _handler = new GetAllModelsQueryHandler(_unitOfWork);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task getAllModels_ShouldReturnModelsWithDetails()
    {
        // Arrange
        var toyotaBrand = new API.Domain.Entities.CarBrand { IdCarBrand = 1, Brand = "Toyota" };
        var fordBrand = new API.Domain.Entities.CarBrand { IdCarBrand = 2, Brand = "Ford" };
        var models = new List<API.Domain.Entities.CarModel>
        {
            new() { IdCarModel = 1, Model = "Corolla", Year = 2022, IdCarBrand = 1, CarBrand = toyotaBrand },
            new() { IdCarModel = 2, Model = "Mustang", Year = 2021, IdCarBrand = 2, CarBrand = fordBrand }
        };
        _modelRepo.GetAllAsync().Returns(models);

        // Act
        var result = await _handler.Handle(new GetAllModelsQuery());

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new CarModelResponse(1, "Corolla", 2022, "Toyota"));
        result[1].Should().BeEquivalentTo(new CarModelResponse(2, "Mustang", 2021, "Ford"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task getModelsByBrand_WithExactName_ShouldFilterCorrectly()
    {
        // Arrange
        var toyotaBrand = new API.Domain.Entities.CarBrand { IdCarBrand = 1, Brand = "Toyota" };
        _brandRepo.GetAllAsync().Returns(new List<API.Domain.Entities.CarBrand> { toyotaBrand });
        var toyotaModels = new List<API.Domain.Entities.CarModel>
        {
            new() { IdCarModel = 1, Model = "Corolla", Year = 2022, IdCarBrand = 1, CarBrand = toyotaBrand }
        };
        _modelRepo.GetAllByBrandIdAsync(1).Returns(toyotaModels);

        // Act
        var result = await _handler.Handle(new GetAllModelsQuery("Toyota"));

        // Assert
        result.Should().HaveCount(1);
        result[0].BrandName.Should().Be("Toyota");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task getModelsByBrand_CaseInsensitive_ShouldReturnFilteredModels()
    {
        // Arrange
        var toyotaBrand = new API.Domain.Entities.CarBrand { IdCarBrand = 1, Brand = "Toyota" };
        _brandRepo.GetAllAsync().Returns(new List<API.Domain.Entities.CarBrand> { toyotaBrand });
        var toyotaModels = new List<API.Domain.Entities.CarModel>
        {
            new() { IdCarModel = 1, Model = "Corolla", Year = 2022, IdCarBrand = 1, CarBrand = toyotaBrand }
        };
        _modelRepo.GetAllByBrandIdAsync(1).Returns(toyotaModels);

        // Act
        var result = await _handler.Handle(new GetAllModelsQuery("tOyOtA"));

        // Assert
        result.Should().HaveCount(1);
        result[0].BrandName.Should().Be("Toyota");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task getModelsByBrand_NonExistentBrand_ShouldThrowNotFoundException()
    {
        // Arrange
        _brandRepo.GetAllAsync().Returns(new List<API.Domain.Entities.CarBrand>
        {
            new() { IdCarBrand = 1, Brand = "Toyota" }
        });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetAllModelsQuery("MarcaInexistente")));
    }
}
