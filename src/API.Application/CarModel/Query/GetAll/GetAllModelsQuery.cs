using API.Application.Dispatcher;
using API.Application.DTOs;

namespace API.Application.CarModel.Query.GetAll
{
    public class GetAllModelsQuery : IRequest<IReadOnlyList<CarModelResponse>>
    {
        public string? BrandName { get; init; }

        public GetAllModelsQuery(string? brandName = null) => BrandName = brandName;
    }
}
