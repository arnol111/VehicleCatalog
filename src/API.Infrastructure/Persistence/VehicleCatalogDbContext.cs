using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace API.Infrastructure.Persistence
{
    public class VehicleCatalogDbContext : DbContext
    {
        public VehicleCatalogDbContext(DbContextOptions<VehicleCatalogDbContext> options)
            : base(options)
        {
        }

        public DbSet<CarBrand> CarBrands => Set<CarBrand>();
        public DbSet<CarModel> CarModels => Set<CarModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
