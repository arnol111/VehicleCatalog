using API.Domain.Entities;
using API.Domain.Interfaces;
using API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Repositories
{
    public class CarModelRepository : ICarModelRepository
    {
        private readonly VehicleCatalogDbContext _context;

        public CarModelRepository(VehicleCatalogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CarModel>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.CarModels.Include(m => m.CarBrand).AsNoTracking().ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<CarModel>> GetAllByBrandIdAsync(int brandId, CancellationToken ct = default)
            => await _context.CarModels
                .Include(m => m.CarBrand)
                .Where(m => m.IdCarBrand == brandId)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<CarModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _context.CarModels.FindAsync([id], cancellationToken);

        public async Task AddAsync(CarModel entity, CancellationToken cancellationToken = default)
            => await _context.CarModels.AddAsync(entity, cancellationToken);

        public void Update(CarModel entity)
            => _context.CarModels.Update(entity);

        public void Delete(CarModel entity)
            => _context.CarModels.Remove(entity);
    }
}
