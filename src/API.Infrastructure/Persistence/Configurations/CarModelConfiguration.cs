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
            builder.HasOne(m => m.CarBrand)
                   .WithMany()
                   .HasForeignKey(m => m.IdCarBrand)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                // Toyota (IdCarBrand = 1)
                new CarModel { IdCarModel = 1, IdCarBrand = 1, Model = "Corolla", Year = 2024 },
                new CarModel { IdCarModel = 2, IdCarBrand = 1, Model = "Hilux", Year = 2023 },
                new CarModel { IdCarModel = 3, IdCarBrand = 1, Model = "RAV4", Year = 2025 },
                new CarModel { IdCarModel = 4, IdCarBrand = 1, Model = "Yaris", Year = 2024 },
                new CarModel { IdCarModel = 5, IdCarBrand = 1, Model = "Land Cruiser", Year = 2026 },

                // Ford (IdCarBrand = 2)
                new CarModel { IdCarModel = 6, IdCarBrand = 2, Model = "Mustang", Year = 2024 },
                new CarModel { IdCarModel = 7, IdCarBrand = 2, Model = "Ranger", Year = 2023 },
                new CarModel { IdCarModel = 8, IdCarBrand = 2, Model = "Explorer", Year = 2025 },
                new CarModel { IdCarModel = 9, IdCarBrand = 2, Model = "F-150", Year = 2024 },
                new CarModel { IdCarModel = 10, IdCarBrand = 2, Model = "Focus", Year = 2022 },

                // Chevrolet (IdCarBrand = 3)
                new CarModel { IdCarModel = 11, IdCarBrand = 3, Model = "Onix", Year = 2024 },
                new CarModel { IdCarModel = 12, IdCarBrand = 3, Model = "Tracker", Year = 2024 },
                new CarModel { IdCarModel = 13, IdCarBrand = 3, Model = "Silverado", Year = 2025 },
                new CarModel { IdCarModel = 14, IdCarBrand = 3, Model = "Cruze", Year = 2023 },
                new CarModel { IdCarModel = 15, IdCarBrand = 3, Model = "Camaro", Year = 2024 },

                // Honda (IdCarBrand = 4)
                new CarModel { IdCarModel = 16, IdCarBrand = 4, Model = "Civic", Year = 2024 },
                new CarModel { IdCarModel = 17, IdCarBrand = 4, Model = "CR-V", Year = 2025 },
                new CarModel { IdCarModel = 18, IdCarBrand = 4, Model = "Accord", Year = 2024 },
                new CarModel { IdCarModel = 19, IdCarBrand = 4, Model = "HR-V", Year = 2024 },
                new CarModel { IdCarModel = 20, IdCarBrand = 4, Model = "Fit", Year = 2022 },

                // Nissan (IdCarBrand = 5)
                new CarModel { IdCarModel = 21, IdCarBrand = 5, Model = "Sentra", Year = 2024 },
                new CarModel { IdCarModel = 22, IdCarBrand = 5, Model = "Versa", Year = 2024 },
                new CarModel { IdCarModel = 23, IdCarBrand = 5, Model = "Frontier", Year = 2023 },
                new CarModel { IdCarModel = 24, IdCarBrand = 5, Model = "Kicks", Year = 2025 },
                new CarModel { IdCarModel = 25, IdCarBrand = 5, Model = "X-Trail", Year = 2025 },

                // Volkswagen (IdCarBrand = 6)
                new CarModel { IdCarModel = 26, IdCarBrand = 6, Model = "Golf", Year = 2024 },
                new CarModel { IdCarModel = 27, IdCarBrand = 6, Model = "Jetta", Year = 2024 },
                new CarModel { IdCarModel = 28, IdCarBrand = 6, Model = "Tiguan", Year = 2025 },
                new CarModel { IdCarModel = 29, IdCarBrand = 6, Model = "Polo", Year = 2023 },
                new CarModel { IdCarModel = 30, IdCarBrand = 6, Model = "Amarok", Year = 2024 },

                // BMW (IdCarBrand = 7)
                new CarModel { IdCarModel = 31, IdCarBrand = 7, Model = "Serie 3", Year = 2024 },
                new CarModel { IdCarModel = 32, IdCarBrand = 7, Model = "X5", Year = 2025 },
                new CarModel { IdCarModel = 33, IdCarBrand = 7, Model = "Serie 5", Year = 2024 },
                new CarModel { IdCarModel = 34, IdCarBrand = 7, Model = "X3", Year = 2024 },
                new CarModel { IdCarModel = 35, IdCarBrand = 7, Model = "M4", Year = 2026 },

                // Mercedes-Benz (IdCarBrand = 8)
                new CarModel { IdCarModel = 36, IdCarBrand = 8, Model = "Clase C", Year = 2024 },
                new CarModel { IdCarModel = 37, IdCarBrand = 8, Model = "GLC", Year = 2025 },
                new CarModel { IdCarModel = 38, IdCarBrand = 8, Model = "Clase E", Year = 2024 },
                new CarModel { IdCarModel = 39, IdCarBrand = 8, Model = "GLE", Year = 2025 },
                new CarModel { IdCarModel = 40, IdCarBrand = 8, Model = "Clase A", Year = 2023 },

                // Hyundai (IdCarBrand = 9)
                new CarModel { IdCarModel = 41, IdCarBrand = 9, Model = "Elantra", Year = 2024 },
                new CarModel { IdCarModel = 42, IdCarBrand = 9, Model = "Tucson", Year = 2025 },
                new CarModel { IdCarModel = 43, IdCarBrand = 9, Model = "Santa Fe", Year = 2025 },
                new CarModel { IdCarModel = 44, IdCarBrand = 9, Model = "Accent", Year = 2023 },
                new CarModel { IdCarModel = 45, IdCarBrand = 9, Model = "Creta", Year = 2024 },

                // Kia (IdCarBrand = 10)
                new CarModel { IdCarModel = 46, IdCarBrand = 10, Model = "Sportage", Year = 2025 },
                new CarModel { IdCarModel = 47, IdCarBrand = 10, Model = "Rio", Year = 2023 },
                new CarModel { IdCarModel = 48, IdCarBrand = 10, Model = "Sorento", Year = 2025 },
                new CarModel { IdCarModel = 49, IdCarBrand = 10, Model = "Forte", Year = 2024 },
                new CarModel { IdCarModel = 50, IdCarBrand = 10, Model = "Picanto", Year = 2024 }
            );
        }
    }
}
