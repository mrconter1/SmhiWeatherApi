using SmhiWeatherApi.Models;
using SmhiWeatherApi.Models.External;
using System.Globalization;
using System.Text.Json;

namespace SmhiWeatherApi.Services
{
    public interface ISmhiService
    {
        Task<StationReading?> GetStationReadingAsync(string stationId);
    }

    public class SmhiService : ISmhiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SmhiService> _logger;
        private const string SmhiBaseUrl = "https://opendata-download-metobs.smhi.se/api/version/latest";
        
        // Parameter IDs - see https://opendata.smhi.se/metobs/resources/parameter for available parameters
        private const int TemperatureParameter = 1;
        private const int WindGustParameter = 21;

        public SmhiService(HttpClient httpClient, ILogger<SmhiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<StationReading?> GetStationReadingAsync(string stationId)
        {
            try
            {
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };

                // Fetch temperature data
                var tempUrl = $"{SmhiBaseUrl}/parameter/{TemperatureParameter}/station/{stationId}/period/latest-hour/data.json";
                var tempResponse = await _httpClient.GetAsync(tempUrl);
                tempResponse.EnsureSuccessStatusCode();
                var tempJson = await tempResponse.Content.ReadAsStringAsync();
                var tempData = JsonSerializer.Deserialize<SmhiResponse>(tempJson, options);

                // Fetch wind gust data
                var windUrl = $"{SmhiBaseUrl}/parameter/{WindGustParameter}/station/{stationId}/period/latest-hour/data.json";
                var windResponse = await _httpClient.GetAsync(windUrl);
                windResponse.EnsureSuccessStatusCode();
                var windJson = await windResponse.Content.ReadAsStringAsync();
                var windData = JsonSerializer.Deserialize<SmhiResponse>(windJson, options);

                // Validate we got what we need
                if (tempData == null || tempData.Value == null || !tempData.Value.Any() ||
                    windData == null || windData.Value == null || !windData.Value.Any())
                {
                    _logger.LogWarning("Invalid response from SMHI for station {StationId}", stationId);
                    return null;
                }

                // After this point, we KNOW these are not null
                var tempValue = tempData.Value.First();
                var windValue = windData.Value.First();

                var temperature = double.Parse(tempValue.Value, CultureInfo.InvariantCulture);
                var windGust = double.Parse(windValue.Value, CultureInfo.InvariantCulture);
                var timestamp = ConvertUnixTimestamp(tempValue.Date);

                var stationReading = new StationReading
                {
                    StationId = int.Parse(stationId),
                    Temperature = temperature,
                    WindGust = windGust,
                    Timestamp = timestamp
                };

                return stationReading;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data for station {StationId}", stationId);
                return null;
            }
        }

        private DateTime ConvertUnixTimestamp(long timestamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(timestamp).ToLocalTime();
            return dateTime;
        }
    }
}
