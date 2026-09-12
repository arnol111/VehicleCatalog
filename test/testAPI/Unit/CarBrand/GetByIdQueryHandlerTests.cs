using Xunit;
using API.Application.CarBrand.Query.GetById;
using API.Application.DTOs;
using API.Domain.Entities;
using API.Domain.Exceptions;
using API.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace testAPI.Unit.CarBrand;

public class GetByIdQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICarBrandRepository _brandRepo = Substitute.For<ICarBrandRepository>();
    private readonly GetByIdQueryHandler _handler;

    public GetByIdQueryHandlerTests()
    {
        _unitOfWork.CarBrands.Returns(_brandRepo);
        _handler = new GetByIdQueryHandler(_unitOfWork);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task getBrandById_WithValidId_ShouldReturnBrand()
    {
        // Arrange
        _brandRepo.GetByIdAsync(1).Returns(new API.Domain.Entities.CarBrand { IdCarBrand = 1, Brand = "Toyota" });

        // Act
        var result = await _handler.Handle(new GetByIdQuery(1));

        // Assert
        result.IdCarBrand.Should().Be(1);
        result.Brand.Should().Be("Toyota");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task getBrandById_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        _brandRepo.GetByIdAsync(9999).Returns((API.Domain.Entities.CarBrand?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new GetByIdQuery(9999)));
    }
}
