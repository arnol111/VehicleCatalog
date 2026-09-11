using API.Application.Dispatcher;
using Microsoft.Extensions.DependencyInjection;


namespace API.Infrastructure.Dispatcher
{
    public class Dispatcher :IDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public Dispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResult> Send<TRequest, TResult>(TRequest request) where TRequest : IRequest<TResult>
        {
            var handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResult>>();

            return await handler.Handle(request);
        }
    }
}
