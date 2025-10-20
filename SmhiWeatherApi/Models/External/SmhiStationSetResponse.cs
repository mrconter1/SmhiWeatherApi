namespace SmhiWeatherApi.Models.External
{
    public class SmhiStationSetResponse
    {
        public long Updated { get; set; }

        public SmhiParameter? Parameter { get; set; }

        public SmhiPeriod? Period { get; set; }

        public List<SmhiLink>? Link { get; set; }

        public List<SmhiStationData>? Station { get; set; }
    }

    public class SmhiStationData
    {
        public string? Key { get; set; }

        public string? Name { get; set; }

        public string? Owner { get; set; }

        public string? OwnerCategory { get; set; }

        public string? MeasuringStations { get; set; }

        public long From { get; set; }

        public long To { get; set; }

        public double Height { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public List<SmhiValue>? Value { get; set; }
    }
}
