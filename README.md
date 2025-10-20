# SMHI Weather API

A simple API that allows for intuitive fetching of temperature and wind gust data from SMHI.

## Endpoints

The API provides two endpoints for retrieving weather station data:

### 1. Get All Stations

```
GET /api/stations
```

Returns a list of **all stations with both temperature and wind gust data** for the latest hour.

**Important:** Only stations that have values for both temperature (parameter 1) and wind gust (parameter 21) are included in the response. Stations missing either value are filtered out.

**Response:**
```json
[
  {
    "stationId": 159880,
    "temperature": -0.7,
    "windGust": 2.1,
    "timestamp": "2024-10-20T12:00:00"
  }
]
```

### 2. Get Specific Station

```
GET /api/stations/{stationId}
```

Returns temperature and wind gust data for a specific station. Supports `hour` (default) and `day` periods.

**Query Parameter:** `?period={hour|day}`
```
hour (default) - ~1 reading from latest hour
day            - ~24 readings from latest day (limited station support)
```

**Examples:**
```
GET /api/stations/159880?period=hour        # latest hour (~1 reading)
GET /api/stations/159880?period=day         # latest day (~24 readings, if available)
GET /api/stations/159880                    # defaults to period=hour
```

**Response (array of readings):**
```json
[
  {
    "stationId": 159880,
    "temperature": -0.7,
    "windGust": 2.1,
    "timestamp": "2024-10-20T12:00:00"
  }
]
```

## Data Source

- **Provider:** SMHI (Swedish Meteorological and Hydrological Institute)
- **API Version:** 1.0
- **Base URL:** https://opendata-download-metobs.smhi.se/api/version/1.0
- **Parameters:**
  - Temperature (ID: 1) - Celsius
  - Wind Gust (ID: 21) - Meters per second
- **Parameter Info:** https://opendata.smhi.se/metobs/resources/parameter

## Project Structure

```
SmhiWeatherApi/
├── Controllers/
│   └── StationsController.cs          # API endpoints
├── Services/
│   └── SmhiService.cs                 # SMHI API integration
├── Models/
│   ├── StationReading.cs              # Response model
│   └── External/
│       ├── SmhiResponse.cs            # Single station response deserialization
│       ├── SmhiStationSetResponse.cs  # All stations response deserialization
│       ├── SmhiValue.cs               # Measurement value
│       └── SmhiStation.cs             # Station metadata
└── Program.cs                          # Configuration & DI setup
```

## Error Handling

- Invalid/missing data returns empty list
- Errors are logged with detailed context
- API returns HTTP 400 for failed requests
