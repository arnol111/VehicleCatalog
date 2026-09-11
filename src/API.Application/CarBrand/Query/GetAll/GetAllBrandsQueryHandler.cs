using API.Application.Dispatcher;
using API.Application.DTOs;
using API.Domain.Interfaces;

namespace API.Application.CarBrand.Query.GetAll
{
    public class GetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, IReadOnlyList<CarBrandResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllBrandsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<CarBrandResponse>> Handle(GetAllBrandsQuery request)
        {
            var brands = await _unitOfWork.CarBrands.GetAllAsync().ConfigureAwait(false);

            return brands
                .Select(b => new CarBrandResponse(b.IdCarBrand, b.Brand))
                .ToList();
        }
    }
}
