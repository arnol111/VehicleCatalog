using API.Application.Dispatcher;
using API.Application.DTOs;
using API.Domain.Exceptions;
using API.Domain.Interfaces;

namespace API.Application.CarModel.Query.GetAll
{
    public class GetAllModelsQueryHandler : IRequestHandler<GetAllModelsQuery, IReadOnlyList<CarModelResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllModelsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<CarModelResponse>> Handle(GetAllModelsQuery request)
        {
            if (request.BrandName is null)
            {
                var models = await _unitOfWork.CarModels.GetAllAsync().ConfigureAwait(false);

                return models
                    .Select(m => new CarModelResponse(
                        m.IdCarModel,
                        m.Model,
                        m.Year,
                        m.CarBrand!.Brand))
                    .ToList();
            }

            var brands = await _unitOfWork.CarBrands.GetAllAsync().ConfigureAwait(false);
            var brand = brands.FirstOrDefault(b =>
                string.Equals(b.Brand, request.BrandName, StringComparison.OrdinalIgnoreCase));

            if (brand is null)
                throw new NotFoundException($"Brand '{request.BrandName}' not found.");

            var filteredModels = await _unitOfWork.CarModels
                .GetAllByBrandIdAsync(brand.IdCarBrand)
                .ConfigureAwait(false);

            return filteredModels
                .Select(m => new CarModelResponse(
                    m.IdCarModel,
                    m.Model,
                    m.Year,
                    m.CarBrand!.Brand))
                .ToList();
        }
    }
}
