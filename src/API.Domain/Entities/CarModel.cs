using System;
using System.Collections.Generic;
using System.Text;

namespace API.Domain.Entities
{
    public class CarModel
    {
        public int IdCarModel { get; private set; }
        public int IdCarBrand { get; set; }

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        private CarModel() { }

        public CarModel(int idCarBrand, string model, int year  )
        {
            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentNullException("El modelo no puede ser vacio");
            }

            if (year <=0)
            {
                throw new ArgumentNullException("Año no es valido ");
            }

            if (idCarBrand <= 0)
            {
                throw new ArgumentNullException("El modelo no es valido ");
            }

            IdCarBrand = idCarBrand;
            Model = model;
            Year = year;
        }
    }
}
