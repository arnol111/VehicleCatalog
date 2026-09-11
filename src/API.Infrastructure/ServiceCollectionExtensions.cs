using API.Application.CarBrand.Query.GetAll;
using API.Application.CarBrand.Query.GetById;
using API.Application.CarModel.Query.GetAll;
using API.Application.Dispatcher;
using API.Application.DTOs;
using API.Domain.Interfaces;
using API.Infrastructure.Dispatcher;
using API.Infrastructure.Persistence;
using API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<VehicleCatalogDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ICarBrandRepository, CarBrandRepository>();
            services.AddScoped<ICarModelRepository, CarModelRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDispatcher, Dispatcher.Dispatcher>();

            services.AddTransient<IRequestHandler<GetByIdQuery, CarBrandDTO>, GetByIdQueryHandler>();
            services.AddTransient<IRequestHandler<GetAllBrandsQuery, IReadOnlyList<CarBrandResponse>>, GetAllBrandsQueryHandler>();
            services.AddTransient<IRequestHandler<GetAllModelsQuery, IReadOnlyList<CarModelResponse>>, GetAllModelsQueryHandler>();

            return services;
        }
    }
}
