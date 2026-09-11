using System.Threading.Tasks;

namespace API.Application.Dispatcher
{
    public interface IDispatcher
    {
        Task<TResult> Send<TRequest, TResult>(TRequest request) where TRequest : IRequest<TResult>;
    }
}
