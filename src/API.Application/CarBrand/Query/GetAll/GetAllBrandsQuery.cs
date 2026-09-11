using API.Application.Dispatcher;
using API.Application.DTOs;

namespace API.Application.CarBrand.Query.GetAll
{
    public class GetAllBrandsQuery : IRequest<IReadOnlyList<CarBrandResponse>>
    {
    }
}
