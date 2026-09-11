using API.Application.Dispatcher;
using API.Application.DTOs;
using API.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace API.Application.CarBrand.Query.GetById
{
    public class GetByIdQueryHandler : IRequestHandler<GetByIdQuery, CarBrandDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public async Task<CarBrandDTO> Handle(GetByIdQuery request)
        {
            var id = request.Request;
            var carBrandFromDB = await _unitOfWork.CarBrands.GetByIdAsync(id).ConfigureAwait(false);
            
            if (carBrandFromDB == null)
            {
                throw new Exception("La marca de carro no existe");
            }
            var carBrandDto = new CarBrandDTO() { Brand = carBrandFromDB.Brand, IdCarBrand = carBrandFromDB.IdCarBrand };

            return carBrandDto;
        }
    }
}
