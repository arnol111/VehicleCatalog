using Xunit;
using API.Application.CarBrand.Query.GetAll;
using API.Application.DTOs;
using API.Domain.Entities;
using API.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace testAPI.Unit.CarBrand;

public class GetAllBrandsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICarBrandRepository _brandRepo = Substitute.For<ICarBrandRepository>();
    private readonly GetAllBrandsQueryHandler _handler;

    public GetAllBrandsQueryHandlerTests()
    {
        _unitOfWork.CarBrands.Returns(_brandRepo);
        _handler = new GetAllBrandsQueryHandler(_unitOfWork);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task getAllBrands_ShouldReturnList()
    {
        // Arrange
        var brands = new List<API.Domain.Entities.CarBrand>
        {
            new() { IdCarBrand = 1, Brand = "Toyota" },
            new() { IdCarBrand = 2, Brand = "Ford" }
        };
        _brandRepo.GetAllAsync().Returns(brands);

        // Act
        var result = await _handler.Handle(new GetAllBrandsQuery());

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new CarBrandResponse(1, "Toyota"));
        result[1].Should().BeEquivalentTo(new CarBrandResponse(2, "Ford"));
    }
}
