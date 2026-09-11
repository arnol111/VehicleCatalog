using API.Application.Dispatcher;
using API.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace API.Application.CarBrand.Query.GetById
{
    public class GetByIdQuery : IRequest<CarBrandDTO>
    {
        public int Request { get; set; }
        public GetByIdQuery(int request)
        {
            Request = request;
        }
    }
}
