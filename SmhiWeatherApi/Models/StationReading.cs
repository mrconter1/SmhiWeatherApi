namespace SmhiWeatherApi.Models
{
    public class StationReading
    {
        public int StationId { get; set; }

        /// <summary>Temperature in Celsius</summary>
        public double Temperature { get; set; }

        /// <summary>Wind gust speed in meters per second</summary>
        public double WindGust { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
