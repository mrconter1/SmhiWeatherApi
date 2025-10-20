using SmhiWeatherApi.Models;
using SmhiWeatherApi.Models.External;
using System.Globalization;
using System.Text.Json;

namespace SmhiWeatherApi.Services
{
    public interface ISmhiService
    {
        Task<StationReading?> GetStationReadingAsync(string stationId);
        Task<List<StationReading>> GetAllStationsReadingAsync();
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

        public async Task<List<StationReading>> GetAllStationsReadingAsync()
        {
            try
            {
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };

                // Fetch all stations temperature data
                var tempUrl = $"{SmhiBaseUrl}/parameter/{TemperatureParameter}/station-set/all/period/latest-hour/data.json";
                var tempResponse = await _httpClient.GetAsync(tempUrl);
                tempResponse.EnsureSuccessStatusCode();
                var tempJson = await tempResponse.Content.ReadAsStringAsync();
                var tempData = JsonSerializer.Deserialize<SmhiStationSetResponse>(tempJson, options);

                // Fetch all stations wind gust data
                var windUrl = $"{SmhiBaseUrl}/parameter/{WindGustParameter}/station-set/all/period/latest-hour/data.json";
                var windResponse = await _httpClient.GetAsync(windUrl);
                windResponse.EnsureSuccessStatusCode();
                var windJson = await windResponse.Content.ReadAsStringAsync();
                var windData = JsonSerializer.Deserialize<SmhiStationSetResponse>(windJson, options);

                // Validate we got data
                if (tempData?.Station == null || !tempData.Station.Any() ||
                    windData?.Station == null || !windData.Station.Any())
                {
                    _logger.LogWarning("Invalid response from SMHI for all stations endpoint");
                    return new List<StationReading>();
                }

                // Create a dictionary for wind data by station key for quick lookup
                var windByStation = windData.Station
                    .Where(s => s.Value != null && s.Value.Any())
                    .ToDictionary(s => s.Key, s => s);

                // Map temperature and wind data for stations that have both
                var stationReadings = new List<StationReading>();
                
                foreach (var tempStation in tempData.Station)
                {
                    // Only process if temperature has values
                    if (tempStation.Value == null || !tempStation.Value.Any())
                        continue;

                    // Check if this station also has wind data
                    if (!windByStation.TryGetValue(tempStation.Key, out var windStation))
                        continue;

                    // Extract values
                    var tempValue = tempStation.Value.First();
                    var windValue = windStation.Value!.First();

                    var temperature = double.Parse(tempValue.Value, CultureInfo.InvariantCulture);
                    var windGust = double.Parse(windValue.Value, CultureInfo.InvariantCulture);
                    var timestamp = ConvertUnixTimestamp(tempValue.Date);

                    var stationReading = new StationReading
                    {
                        StationId = int.Parse(tempStation.Key!),
                        Temperature = temperature,
                        WindGust = windGust,
                        Timestamp = timestamp
                    };

                    stationReadings.Add(stationReading);
                }

                _logger.LogInformation("Retrieved {Count} stations with both temperature and wind gust data", stationReadings.Count);
                return stationReadings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all stations data from SMHI");
                return new List<StationReading>();
            }
        }
    }
}
