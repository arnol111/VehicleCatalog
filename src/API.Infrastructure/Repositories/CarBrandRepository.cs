using API.Domain.Entities;
using API.Domain.Interfaces;
using API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Repositories
{
    public class CarBrandRepository : ICarBrandRepository
    {
        private readonly VehicleCatalogDbContext _context;

        public CarBrandRepository(VehicleCatalogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CarBrand>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.CarBrands.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<CarBrand?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _context.CarBrands.FindAsync([id], cancellationToken);

        public async Task AddAsync(CarBrand entity, CancellationToken cancellationToken = default)
            => await _context.CarBrands.AddAsync(entity, cancellationToken);

        public void Update(CarBrand entity)
            => _context.CarBrands.Update(entity);

        public void Delete(CarBrand entity)
            => _context.CarBrands.Remove(entity);
    }
}
