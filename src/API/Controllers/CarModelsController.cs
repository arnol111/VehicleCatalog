using API.Application.CarModel.Query.GetAll;
using API.Application.Dispatcher;
using API.Application.DTOs;
using API.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/carModels")]
    public class CarModelsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        public CarModelsController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CarModelResponse>>> GetAll([FromQuery] string? brand)
        {
            if (Request.Query.ContainsKey("brand") && string.IsNullOrWhiteSpace(brand))
                return BadRequest("Brand parameter cannot be empty");

            try
            {
                var query = new GetAllModelsQuery(brand);
                var result = await _dispatcher.Send<GetAllModelsQuery, IReadOnlyList<CarModelResponse>>(query).ConfigureAwait(false);
                return Ok(result);
            }
            catch (NotFoundException)
            {
                return NotFound($"Brand '{brand}' not found");
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
