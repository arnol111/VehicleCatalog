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
        }
    }
}
