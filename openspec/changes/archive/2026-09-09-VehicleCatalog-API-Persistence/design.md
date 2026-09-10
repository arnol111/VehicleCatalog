# Design: VehicleCatalog-API-Persistence

## Architecture Overview

```
┌─────────────────────────────────────────────────┐
│  API (Host)                                      │
│  Program.cs  ──→  AddInfrastructure(config)      │
│  appsettings.json  (ConnectionStrings)           │
└───────────────────────┬─────────────────────────┘
                        │ project ref
┌───────────────────────▼─────────────────────────┐
│  API.Infrastructure                              │
│  Persistence/                                    │
│    VehicleCatalogDbContext                       │
│    UnitOfWork                                    │
│    Configurations/                               │
│      CarBrandConfiguration                      │
│      CarModelConfiguration                      │
│  Repositories/                                   │
│    CarBrandRepository                            │
│    CarModelRepository                            │
│  ServiceCollectionExtensions (AddInfrastructure) │
└───────────────────────┬─────────────────────────┘
                        │ project ref
┌───────────────────────▼─────────────────────────┐
│  API.Domain                                      │
│  Entities: CarBrand, CarModel                    │
│  Interfaces: IUnitOfWork,                        │
│    ICarBrandRepository, ICarModelRepository      │
└─────────────────────────────────────────────────┘
```

## Technology Decisions

| Decision | Choice | Rationale |
|---|---|---|
| ORM | EF Core 10.x | Matches net10.0 TFM; LTS alignment |
| Provider | Microsoft.EntityFrameworkCore.SqlServer 10.0.x | SQL Server target; same major as EF Core |
| Design API | Fluent API via `IEntityTypeConfiguration<T>` | Keeps entities clean; no data annotations |
| Migrations | EF Core Migrations (code-first) | Audit trail, repeatable schema evolution |
| DI pattern | `AddInfrastructure(IConfiguration)` extension | Encapsulates infra wiring; host stays thin |

## File / Class Map

| Action | File Path | Class / Type |
|---|---|---|
| **Create** | `API.Infrastructure/Persistence/VehicleCatalogDbContext.cs` | `VehicleCatalogDbContext : DbContext` |
| **Create** | `API.Infrastructure/Persistence/Configurations/CarBrandConfiguration.cs` | `CarBrandConfiguration : IEntityTypeConfiguration<CarBrand>` |
| **Create** | `API.Infrastructure/Persistence/Configurations/CarModelConfiguration.cs` | `CarModelConfiguration : IEntityTypeConfiguration<CarModel>` |
| **Create** | `API.Infrastructure/Persistence/UnitOfWork.cs` | `UnitOfWork : IUnitOfWork` |
| **Create** | `API.Infrastructure/Repositories/CarBrandRepository.cs` | `CarBrandRepository : ICarBrandRepository` |
| **Create** | `API.Infrastructure/Repositories/CarModelRepository.cs` | `CarModelRepository : ICarModelRepository` |
| **Create** | `API.Infrastructure/ServiceCollectionExtensions.cs` | `static ServiceCollectionExtensions` |
| **Modify** | `API.Domain/Interfaces/ICarBrandRepository.cs` | Add CRUD members |
| **Modify** | `API.Domain/Interfaces/ICarModelRepository.cs` | Add CRUD members |
| **Modify** | `API.Infrastructure/API.Infrastructure.csproj` | Add EF Core packages + Domain project ref |
| **Modify** | `API/API.csproj` | Add EF Core.SqlServer + Infrastructure project ref |
| **Modify** | `API/Program.cs` | Call `AddInfrastructure`; remove MVC leftover |
| **Modify** | `API/appsettings.json` | Add `ConnectionStrings:DefaultConnection` |
| **Modify** | `API/appsettings.Development.json` | Override connection string for local dev |
| **Modify** | `API/Controllers/*.cs` | Change base class from `Controller` → `ControllerBase` |
| **Delete** | `API.Infrastructure/Class1.cs` | Stub — no longer needed |
| **Delete** | `API.Application/Class1.cs` | Stub — no longer needed |
| **Generate** | `API/Migrations/` | `InitialCreate` migration via `dotnet ef migrations add` |

## Interface Signatures

```csharp
// API.Domain/Interfaces/ICarBrandRepository.cs
public interface ICarBrandRepository
{
    Task<IEnumerable<CarBrand>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CarBrand?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(CarBrand entity, CancellationToken cancellationToken = default);
    void Update(CarBrand entity);
    void Delete(CarBrand entity);
}

// API.Domain/Interfaces/ICarModelRepository.cs
public interface ICarModelRepository
{
    Task<IEnumerable<CarModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CarModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(CarModel entity, CancellationToken cancellationToken = default);
    void Update(CarModel entity);
    void Delete(CarModel entity);
}

// API.Domain/Interfaces/IUnitOfWork.cs  (existing — verify shape)
public interface IUnitOfWork
{
    ICarBrandRepository CarBrands { get; }
    ICarModelRepository CarModels { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

## Entity Configurations

```csharp
// CarBrandConfiguration
builder.ToTable("CarBrands");
builder.HasKey(e => e.Id);
builder.Property(e => e.Id).UseIdentityColumn();
builder.Property(e => e.Brand)
       .IsRequired()
       .HasMaxLength(200);

// CarModelConfiguration
builder.ToTable("CarModels");
builder.HasKey(e => e.Id);
builder.Property(e => e.Id).UseIdentityColumn();
builder.Property(e => e.Model)
       .IsRequired()
       .HasMaxLength(200);
builder.Property(e => e.CarBrandId).IsRequired();
builder.HasOne<CarBrand>()           // no navigation property on CarModel
       .WithMany()
       .HasForeignKey(e => e.CarBrandId)
       .OnDelete(DeleteBehavior.Restrict);
```

## DI Registration Pattern

```csharp
// API.Infrastructure/ServiceCollectionExtensions.cs
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

        return services;
    }
}

// API/Program.cs — single call
builder.Services.AddInfrastructure(builder.Configuration);
```

## Configuration Schema

```jsonc
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=VehicleCatalog;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}

// appsettings.Development.json  (override for local dev)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VehicleCatalog_Dev;Trusted_Connection=True;"
  }
}
```

Environment variable override follows .NET convention:
`ConnectionStrings__DefaultConnection=<value>`

## Migration Strategy

1. Ensure `Microsoft.EntityFrameworkCore.Design` is referenced in the **host** (API) project.
2. Set `API` as the startup project and `API.Infrastructure` as the migrations project.
3. Run:
   ```
   dotnet ef migrations add InitialCreate --project API.Infrastructure --startup-project API
   dotnet ef database update --startup-project API
   ```
4. Migrations folder lands at `API.Infrastructure/Migrations/` (recommended) or `API/Migrations/` — choose one and keep it consistent. Preference: `API.Infrastructure/Migrations/` to keep infra concerns collocated.

## Threat Matrix

N/A — no routing, shell, subprocess, VCS/PR automation, executable-file classification, or process-integration boundary introduced by this change.

## Open Questions

- [ ] Confirm target SQL Server version (localdb / full SQL Server) — affects connection string defaults.
- [ ] Confirm whether `CarModel` needs a navigation property to `CarBrand` in the future (current design: none, Fluent API only).
