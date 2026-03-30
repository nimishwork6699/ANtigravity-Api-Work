using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace WeatherAPI.Controllers
{
    [ApiController]             // Indicates this is an API controller with built-in model validation
    [Route("api/weather")]      // Explicit route /api/weather 
    public class WeatherController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        // Inject HttpClient which we configured in Program.cs
        public WeatherController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET endpoint: /api/weather or /api/weather?city=Mumbai
        [HttpGet]
        public async Task<IActionResult> GetWeather([FromQuery] string city = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(city))
                {
                    city = "London";
                }

                // Use Open-Meteo geocoding (no API key required)
                string geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1";
                HttpResponseMessage geoResponse = await _httpClient.GetAsync(geoUrl);
                if (!geoResponse.IsSuccessStatusCode)
                {
                    string errorBody = await geoResponse.Content.ReadAsStringAsync();
                    return BadRequest(new { error = $"Could not geocode '{city}'. Status Code: {geoResponse.StatusCode}", providerMessage = errorBody });
                }

                string geoData = await geoResponse.Content.ReadAsStringAsync();
                JObject geoJson = JObject.Parse(geoData);
                var results = geoJson["results"];
                if (results == null || !results.HasValues)
                {
                    return BadRequest(new { error = $"Could not find coordinates for '{city}'." });
                }

                var first = results[0];
                double lat = first["latitude"]?.Value<double>() ?? 0;
                double lon = first["longitude"]?.Value<double>() ?? 0;
                string resolvedName = first["name"]?.ToString() ?? city;

                // Fetch current weather from Open-Meteo
                string weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true&timezone=auto";
                HttpResponseMessage response = await _httpClient.GetAsync(weatherUrl);
                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { error = $"Could not fetch weather for '{city}'. Status Code: {response.StatusCode}", providerMessage = errorBody });
                }

                string responseData = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseData);

                var current = json["current_weather"];
                if (current == null)
                {
                    return StatusCode(500, new { error = "No current weather data returned by provider." });
                }

                string temp = current["temperature"]?.ToString() ?? "Unknown";
                string windspeed = current["windspeed"]?.ToString() ?? "Unknown";
                string weathercode = current["weathercode"]?.ToString() ?? "Unknown";

                var result = new
                {
                    city = resolvedName,
                    temperature = temp + " °C",
                    windspeed = windspeed + " km/h",
                    weathercode = weathercode
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An internal server error occurred.", details = ex.Message });
            }
        }
    }
}
