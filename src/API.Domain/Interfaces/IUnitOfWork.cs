namespace API.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        ICarBrandRepository CarBrands { get; }
        ICarModelRepository CarModels { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
