namespace SmhiWeatherApi.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private const string ApiKeyHeaderName = "X-API-Key";

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for Swagger endpoints
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            // Check if API key header exists
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key is missing");
                return;
            }

            // Get valid keys from configuration
            var validApiKeys = _configuration.GetSection("ApiKeys").Get<string[]>();

            // Validate the key (convert StringValues to string for comparison)
            var apiKey = extractedApiKey.ToString();
            if (validApiKeys == null || !validApiKeys.Contains(apiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            // Key is valid, continue to next middleware
            await _next(context);
        }
    }
}
