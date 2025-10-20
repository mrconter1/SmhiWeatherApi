# SMHI Weather API

A .NET 8 API for fetching real-time temperature and wind gust data from Swedish meteorological stations (SMHI).

## Quick Start

All endpoints require `X-API-Key` header. Dev keys: `solita-ai-1`, `solita-development`

In Swagger UI, click 🔒 Authorize at the top and enter a key.

## Endpoints

### GET /api/stations
Returns all stations with temperature and wind data for the latest hour. Only returns stations with both measurements.

Example response:
```json
[{
  "stationId": 159880,
  "temperature": -0.7,
  "windGust": 2.1,
  "timestamp": "2024-10-20T12:00:00"
}]
```

### GET /api/stations/{stationId}
Returns data for a specific station. Supports `period` query parameter: `hour` (default) or `day`.

```
GET /api/stations/159880              # latest hour
GET /api/stations/159880?period=day   # latest day (~24 readings)
```

## Key Features

**Timestamp validation:** When combining temperature and wind measurements, timestamps are compared. If they differ, a warning is logged with the millisecond difference and the latest timestamp is used.

**API key authentication:** All endpoints secured with API key validation. Swagger/Swagger UI automatically included without auth.

**Error handling:** Invalid or missing data returns an empty list. All errors are logged with context.

## Testing

The project includes a comprehensive test suite in `SmhiWeatherApi.Tests` using xUnit and Moq. Tests cover happy path, error handling, and empty data scenarios.

Run tests via Test Explorer in Visual Studio or use `dotnet test`.

## Architecture

**Services** - `SmhiService` handles API integration, data fetching, and timestamp comparison logic

**Controllers** - `StationsController` exposes REST endpoints

**Middleware** - `ApiKeyMiddleware` validates API keys on all requests (except Swagger)

**Models** - DTOs for external SMHI responses and internal `StationReading` model

## Shortcuts & Trade-offs

- **Sequential HTTP calls** Could parallelize with `Task.WhenAll()` but minimal impact with only 2 API calls
- **Single timestamp per reading** Simpler API contract, separate timestamps would require client logic to handle mismatches
- **Skips stations missing either parameter** Returns consistent, complete datasets; partial data complicates client consumption
- **No caching** Not needed for demonstration, would add complexity without clear requirements
- **No pagination on all-stations** Dataset is manageable (~200-400 stations), unnecessary complexity at this scale
- **Quality field ignored** No client requirements defined for quality thresholds, should discuss with stakeholder first
- **Inconsistent error responses** Single-station returns 400 on empty; all-stations returns 200. Works but inconsistent design
- **Limited test coverage** Only service layer tested, controller/middleware testing would require more mock setup time
- **No retry/rate limiting** Depends on actual usage and SMHI API limits, needs production monitoring to justify

## Data Source

**Provider:** SMHI (Swedish Meteorological and Hydrological Institute)

**Base URL:** https://opendata-download-metobs.smhi.se/api/version/1.0

**Parameters:** Temperature (ID: 1), Wind Gust (ID: 21)

**More info about parameters:** https://opendata.smhi.se/metobs/resources/parameter
