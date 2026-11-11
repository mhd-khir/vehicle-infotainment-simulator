using System;
using System.Collections;
using System.Globalization;
using InfotainmentSystem.Data;
using Newtonsoft.Json.Linq;

namespace InfotainmentSystem.Weather
{
    /// <summary>
    /// This class represents the weather data retrieved from the Open-Meteo API.
    /// </summary>
    public sealed class WeatherAPI
    {
        public float Temperature { get; }
        public int WeatherCode { get; }

        public WeatherAPI(float temperature, int weatherCode)
        {
            Temperature = temperature;
            WeatherCode = weatherCode;
        }
    }

    /// <summary>
    /// This class is responsible for fetching weather data from the Open-Meteo API based on the provided latitude and longitude.
    /// </summary>
    public sealed class WeatherService : IWidgetDataSource<WeatherAPI>
    {
        private readonly float latitude;
        private readonly float longitude;
        private readonly JsonHttpClient http = new JsonHttpClient();

        public WeatherService(float latitude, float longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }
        
        /// <summary>
        /// Fetches weather data from the Open-Meteo API.
        /// </summary>
        /// <param name="completed">The action to invoke when the request is completed.</param>
        /// <returns>An enumerator for the coroutine.</returns>
        public IEnumerator Fetch(Action<DataResult<WeatherAPI>> completed)
        {
            if (!ValidCoordinates(latitude, longitude))
                return CompleteInvalidCoordinates(completed);
            string url = string.Format(CultureInfo.InvariantCulture,
                "https://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}" +
                "&current=temperature_2m,weather_code&timezone=GMT&temperature_unit=celsius",
                latitude, longitude);
            return http.Get(url, Parse, completed);
        }

        /// <summary>
        /// Uses the Open-Meteo API to fetch weather data based on the provided latitude and longitude.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        private static bool ValidCoordinates(float latitude, float longitude) =>
            !float.IsNaN(latitude) && !float.IsInfinity(latitude) &&
            !float.IsNaN(longitude) && !float.IsInfinity(longitude) &&
            latitude >= -90f && latitude <= 90f && longitude >= -180f && longitude <= 180f;

        /// <summary>
        /// Used to complete the request with an error message when the provided coordinates are invalid.
        /// </summary>
        /// <param name="completed"></param>
        /// <returns></returns>
        private static IEnumerator CompleteInvalidCoordinates(Action<DataResult<WeatherAPI>> completed)
        {
            completed(DataResult<WeatherAPI>.Failure("Weather coordinates are invalid."));
            yield break;
        }

        /// <summary>
        /// Used to cancel the ongoing weather data request.
        /// </summary>
        public void Cancel() => http.Cancel();

        /// <summary>
        /// Used to parse the JSON response from the Open-Meteo API and convert it into a WeatherAPI object.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static DataResult<WeatherAPI> Parse(string json)
        {
            JObject root = JObject.Parse(json);
            JToken currentToken = root["current"];
            if (currentToken == null || currentToken.Type == JTokenType.Null)
                return DataResult<WeatherAPI>.Empty();
            if (!(currentToken is JObject current))
                return DataResult<WeatherAPI>.Failure("Invalid current weather object.");

            if (!JsonNumbers.TryFloat(current["temperature_2m"], out float temperature) ||
                current["weather_code"]?.Type != JTokenType.Integer ||
                current["time"]?.Type != JTokenType.String)
                return DataResult<WeatherAPI>.Failure("Required weather fields are missing or invalid.");

            string time = current["time"].Value<string>();
            if (!DateTime.TryParse(time, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out _))
                return DataResult<WeatherAPI>.Failure("Invalid weather observation time.");

            long code = current["weather_code"].Value<long>();
            if (code < 0 || code > int.MaxValue)
                return DataResult<WeatherAPI>.Failure("Invalid weather code.");
            return DataResult<WeatherAPI>.Success(
                new WeatherAPI(temperature, (int)code));
        }
    }
}
