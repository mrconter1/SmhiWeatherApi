using Microsoft.AspNetCore.Mvc;
using SmhiWeatherApi.Services;

namespace SmhiWeatherApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class StationsController : ControllerBase
    {
        private readonly ILogger<StationsController> _logger;
        private readonly ISmhiService _smhiService;

        public StationsController(ILogger<StationsController> logger, ISmhiService smhiService)
        {
            _logger = logger;
            _smhiService = smhiService;
        }

        [HttpGet("stations/{stationId}")]
        public async Task<ActionResult> GetStationReading(string stationId)
        {
            var stationReading = await _smhiService.GetStationReadingAsync(stationId);
            
            if (stationReading == null)
            {
                return BadRequest("Failed to retrieve station data from SMHI API");
            }

            return Ok(stationReading);
        }
    }
}
