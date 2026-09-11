using API.Application.CarBrand.Query.GetAll;
using API.Application.Dispatcher;
using API.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/carBrands")]
    public class CarBrandsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        public CarBrandsController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CarBrandResponse>>> GetAll()
        {
            try
            {
                var query = new GetAllBrandsQuery();
                var result = await _dispatcher.Send<GetAllBrandsQuery, IReadOnlyList<CarBrandResponse>>(query).ConfigureAwait(false);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
