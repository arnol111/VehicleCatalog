using Xunit;
using API.Domain.Entities;
using API.Infrastructure.Persistence;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace testAPI.Integration.Fixtures;

[CollectionDefinition("SqlServer")]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture> { }

public class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient Client { get; private set; } = null!;
    public IServiceProvider Services => _factory.Services;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContextOptions<VehicleCatalogDbContext> registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<VehicleCatalogDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Re-register with container's connection string
                services.AddDbContext<VehicleCatalogDbContext>(options =>
                    options.UseSqlServer(_container.GetConnectionString()));
            });
        });

        Client = _factory.CreateClient();

        // Apply EF model — no migrations needed
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VehicleCatalogDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Inserts a known set of test rows. Call before tests that need data.
    /// </summary>
    public async Task SeedDataAsync(VehicleCatalogDbContext db)
    {
        db.CarBrands.Add(new CarBrand { Brand = "Toyota" });
        db.CarBrands.Add(new CarBrand { Brand = "Ford" });
        await db.SaveChangesAsync();

        var toyota = db.CarBrands.Local.First(b => b.Brand == "Toyota");
        var ford = db.CarBrands.Local.First(b => b.Brand == "Ford");

        db.CarModels.Add(new CarModel { Model = "Corolla", Year = 2022, IdCarBrand = toyota.IdCarBrand });
        db.CarModels.Add(new CarModel { Model = "Camry", Year = 2021, IdCarBrand = toyota.IdCarBrand });
        db.CarModels.Add(new CarModel { Model = "Mustang", Year = 2022, IdCarBrand = ford.IdCarBrand });
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes all test data. Respects FK — CarModels first, then CarBrands.
    /// </summary>
    public async Task ClearDataAsync(VehicleCatalogDbContext db)
    {
        db.CarModels.RemoveRange(db.CarModels);
        await db.SaveChangesAsync();

        db.CarBrands.RemoveRange(db.CarBrands);
        await db.SaveChangesAsync();
    }
}
