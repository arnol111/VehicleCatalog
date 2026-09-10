using API.Domain.Interfaces;
using API.Infrastructure.Repositories;

namespace API.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VehicleCatalogDbContext _context;

        public UnitOfWork(VehicleCatalogDbContext context,
                          ICarBrandRepository carBrands,
                          ICarModelRepository carModels)
        {
            _context = context;
            CarBrands = carBrands;
            CarModels = carModels;
        }

        public ICarBrandRepository CarBrands { get; }
        public ICarModelRepository CarModels { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);
    }
}
