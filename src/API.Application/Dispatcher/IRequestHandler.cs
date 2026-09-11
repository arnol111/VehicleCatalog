using System;
using System.Collections.Generic;
using System.Text;

namespace API.Application.Dispatcher
{

    public interface IRequestHandler<TRequest, TResult> where TRequest : IRequest<TResult>
    {
        Task<TResult> Handle(TRequest request);
    }
}
