using API.Domain.Entities;

namespace API.Domain.Interfaces
{
    public interface ICarModelRepository
    {
        Task<IEnumerable<CarModel>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<CarModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(CarModel entity, CancellationToken cancellationToken = default);
        void Update(CarModel entity);
        void Delete(CarModel entity);
    }
}
