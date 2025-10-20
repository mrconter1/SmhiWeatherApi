using Microsoft.AspNetCore.Mvc;
using SmhiWeatherApi.Services;

namespace SmhiWeatherApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class StationsController : ControllerBase
    {
        private readonly ISmhiService _smhiService;

        public StationsController(ISmhiService smhiService)
        {
            _smhiService = smhiService;
        }

        /// <summary>
        /// Get weather readings for a specific station
        /// </summary>
        /// <param name="stationId">Station ID (default: 159880 - Arvidsjaur A). Other examples: 97280, 92410, 167710, 188790</param>
        /// <param name="period">Time period: "hour" (default) or "day"</param>
        /// <returns>List of station readings with temperature and wind gust data</returns>
        [HttpGet("stations/{stationId}")]
        public async Task<ActionResult> GetStationReading(
            [FromRoute] string stationId = "159880", 
            [FromQuery] string? period = "hour")
        {
            if (string.IsNullOrWhiteSpace(stationId))
            {
                return BadRequest("Station ID cannot be empty");
            }

            var stationReadings = await _smhiService.GetStationReadingAsync(stationId, period);
            return Ok(stationReadings);
        }

        /// <summary>
        /// Get weather readings for all stations (latest hour)
        /// </summary>
        /// <remarks>
        /// Returns all stations with both temperature and wind gust data for the latest hour.
        /// Only stations that have values for both parameters are included.
        /// 
        /// Common station IDs:
        /// - 159880: Arvidsjaur A
        /// - 97280: Adelsö A
        /// - 92410: Arvika A
        /// - 167710: Arjeplog A
        /// - 188790: Abisko Aut
        /// </remarks>
        /// <returns>List of all stations with readings</returns>
        [HttpGet("stations")]
        public async Task<ActionResult> GetAllStations()
        {
            var stationReadings = await _smhiService.GetAllStationsReadingAsync();
            return Ok(stationReadings);
        }
    }
}
