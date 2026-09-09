using System;
using System.Collections.Generic;
using System.Text;

namespace API.Domain.Entities
{
    public class CarBrand
    {
        public int IdCarBrand { get; private set; }

        public string Brand { get; set; } = string.Empty;

        private CarBrand() { }

        public CarBrand(string brand) 
        {
            Brand = brand;
        }
    }
}
