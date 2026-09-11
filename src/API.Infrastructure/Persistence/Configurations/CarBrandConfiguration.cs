using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations
{
    public class CarBrandConfiguration : IEntityTypeConfiguration<CarBrand>
    {
        public void Configure(EntityTypeBuilder<CarBrand> builder)
        {
            builder.ToTable("CarBrands");
            builder.HasKey(e => e.IdCarBrand);
            builder.Property(e => e.IdCarBrand).UseIdentityColumn();
            builder.Property(e => e.Brand)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasData(
            new CarBrand { IdCarBrand = 1, Brand = "Toyota" },
            new CarBrand { IdCarBrand = 2, Brand = "Ford" },
            new CarBrand { IdCarBrand = 3, Brand = "Chevrolet" },
            new CarBrand { IdCarBrand = 4, Brand = "Honda" },
            new CarBrand { IdCarBrand = 5, Brand = "Nissan" },
            new CarBrand { IdCarBrand = 6, Brand = "Volkswagen" },
            new CarBrand { IdCarBrand = 7, Brand = "BMW" },
            new CarBrand { IdCarBrand = 8, Brand = "Mercedes-Benz" },
            new CarBrand { IdCarBrand = 9, Brand = "Hyundai" },
            new CarBrand { IdCarBrand = 10, Brand = "Kia" }
            );
        }
    }
}
