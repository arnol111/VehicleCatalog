
using API.Application.CarBrand.Query.GetById;
using API.Application.Dispatcher;
using API.Application.DTOs;
using API.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CarBrandController : ControllerBase
    {
        private IDispatcher _dispatcher;
        public CarBrandController(IDispatcher dispatcher)
        {
               _dispatcher = dispatcher;
        }

        [HttpGet]
        public async Task<ActionResult<CarBrandDTO>> GetById([FromQuery] int id)
        {
            var carBrandDto = new CarBrandDTO();
            try
            {
                var query = new GetByIdQuery(id);
               
                carBrandDto = await _dispatcher.Send<GetByIdQuery, CarBrandDTO>(query).ConfigureAwait(false);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
            return Ok(carBrandDto);
        }
    }
}

