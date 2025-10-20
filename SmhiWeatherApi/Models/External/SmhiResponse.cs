namespace SmhiWeatherApi.Models.External
{
    public class SmhiResponse
    {
        public long Updated { get; set; }

        public SmhiParameter? Parameter { get; set; }

        public SmhiStation? Station { get; set; }

        public SmhiPeriod? Period { get; set; }

        public List<SmhiPosition>? Position { get; set; }

        public List<SmhiLink>? Link { get; set; }

        public List<SmhiValue>? Value { get; set; }
    }

    public class SmhiParameter
    {
        public string? Key { get; set; }

        public string? Name { get; set; }

        public string? Summary { get; set; }

        public string? Unit { get; set; }
    }

    public class SmhiPeriod
    {
        public string? Key { get; set; }

        public long From { get; set; }

        public long To { get; set; }

        public string? Summary { get; set; }

        public string? Sampling { get; set; }
    }

    public class SmhiPosition
    {
        public long From { get; set; }

        public long To { get; set; }

        public double Height { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class SmhiLink
    {
        public string? Href { get; set; }

        public string? Rel { get; set; }

        public string? Type { get; set; }
    }
}
