using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarBrands",
                columns: table => new
                {
                    IdCarBrand = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Brand = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarBrands", x => x.IdCarBrand);
                });

            migrationBuilder.CreateTable(
                name: "CarModels",
                columns: table => new
                {
                    IdCarModel = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCarBrand = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModels", x => x.IdCarModel);
                    table.ForeignKey(
                        name: "FK_CarModels_CarBrands_IdCarBrand",
                        column: x => x.IdCarBrand,
                        principalTable: "CarBrands",
                        principalColumn: "IdCarBrand",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CarBrands",
                columns: new[] { "IdCarBrand", "Brand" },
                values: new object[,]
                {
                    { 1, "Toyota" },
                    { 2, "Ford" },
                    { 3, "Chevrolet" },
                    { 4, "Honda" },
                    { 5, "Nissan" },
                    { 6, "Volkswagen" },
                    { 7, "BMW" },
                    { 8, "Mercedes-Benz" },
                    { 9, "Hyundai" },
                    { 10, "Kia" }
                });

            migrationBuilder.InsertData(
                table: "CarModels",
                columns: new[] { "IdCarModel", "IdCarBrand", "Model", "Year" },
                values: new object[,]
                {
                    { 1, 1, "Corolla", 2024 },
                    { 2, 1, "Hilux", 2023 },
                    { 3, 1, "RAV4", 2025 },
                    { 4, 1, "Yaris", 2024 },
                    { 5, 1, "Land Cruiser", 2026 },
                    { 6, 2, "Mustang", 2024 },
                    { 7, 2, "Ranger", 2023 },
                    { 8, 2, "Explorer", 2025 },
                    { 9, 2, "F-150", 2024 },
                    { 10, 2, "Focus", 2022 },
                    { 11, 3, "Onix", 2024 },
                    { 12, 3, "Tracker", 2024 },
                    { 13, 3, "Silverado", 2025 },
                    { 14, 3, "Cruze", 2023 },
                    { 15, 3, "Camaro", 2024 },
                    { 16, 4, "Civic", 2024 },
                    { 17, 4, "CR-V", 2025 },
                    { 18, 4, "Accord", 2024 },
                    { 19, 4, "HR-V", 2024 },
                    { 20, 4, "Fit", 2022 },
                    { 21, 5, "Sentra", 2024 },
                    { 22, 5, "Versa", 2024 },
                    { 23, 5, "Frontier", 2023 },
                    { 24, 5, "Kicks", 2025 },
                    { 25, 5, "X-Trail", 2025 },
                    { 26, 6, "Golf", 2024 },
                    { 27, 6, "Jetta", 2024 },
                    { 28, 6, "Tiguan", 2025 },
                    { 29, 6, "Polo", 2023 },
                    { 30, 6, "Amarok", 2024 },
                    { 31, 7, "Serie 3", 2024 },
                    { 32, 7, "X5", 2025 },
                    { 33, 7, "Serie 5", 2024 },
                    { 34, 7, "X3", 2024 },
                    { 35, 7, "M4", 2026 },
                    { 36, 8, "Clase C", 2024 },
                    { 37, 8, "GLC", 2025 },
                    { 38, 8, "Clase E", 2024 },
                    { 39, 8, "GLE", 2025 },
                    { 40, 8, "Clase A", 2023 },
                    { 41, 9, "Elantra", 2024 },
                    { 42, 9, "Tucson", 2025 },
                    { 43, 9, "Santa Fe", 2025 },
                    { 44, 9, "Accent", 2023 },
                    { 45, 9, "Creta", 2024 },
                    { 46, 10, "Sportage", 2025 },
                    { 47, 10, "Rio", 2023 },
                    { 48, 10, "Sorento", 2025 },
                    { 49, 10, "Forte", 2024 },
                    { 50, 10, "Picanto", 2024 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarModels_IdCarBrand",
                table: "CarModels",
                column: "IdCarBrand");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarModels");

            migrationBuilder.DropTable(
                name: "CarBrands");
        }
    }
}
