using API.Domain.Entities;

namespace API.Domain.Interfaces
{
    public interface ICarBrandRepository
    {
        Task<IEnumerable<CarBrand>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<CarBrand?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(CarBrand entity, CancellationToken cancellationToken = default);
        void Update(CarBrand entity);
        void Delete(CarBrand entity);
    }
}
