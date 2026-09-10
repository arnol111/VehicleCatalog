using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations
{
    public class CarModelConfiguration : IEntityTypeConfiguration<CarModel>
    {
        public void Configure(EntityTypeBuilder<CarModel> builder)
        {
            builder.ToTable("CarModels");
            builder.HasKey(e => e.IdCarModel);
            builder.Property(e => e.IdCarModel).UseIdentityColumn();
            builder.Property(e => e.Model)
                   .IsRequired()
                   .HasMaxLength(200);
            builder.Property(e => e.IdCarBrand).IsRequired();
            builder.HasOne<CarBrand>()
                   .WithMany()
                   .HasForeignKey(e => e.IdCarBrand)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
