using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using InfotainmentSystem.Data;
using Newtonsoft.Json.Linq;

namespace InfotainmentSystem.Charging
{
    public sealed class ChargingAPI : IWidgetDataSource<List<ChargingStationData>>
    {
        private readonly string apiKey;
        private readonly float latitude;
        private readonly float longitude;
        private readonly float radiusKm;
        private readonly int maximumResults;
        private readonly JsonHttpClient http = new JsonHttpClient();

        /// <summary>
        /// Creates a new instance of ChargingAPI with the specified parameters.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="radiusKm"></param>
        /// <param name="maximumResults"></param>
        public ChargingAPI(string apiKey, float latitude, float longitude,
            float radiusKm, int maximumResults)
        {
            this.apiKey = apiKey;
            this.latitude = latitude;
            this.longitude = longitude;
            this.radiusKm = radiusKm;
            this.maximumResults = maximumResults;
        }

        /// <summary>
        /// Used to fetch the list of charging stations from the Open Charge Map API.
        /// </summary>
        /// <param name="completed"></param>
        /// <returns></returns>
        public IEnumerator Fetch(Action<DataResult<List<ChargingStationData>>> completed)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                completed(DataResult<List<ChargingStationData>>.Failure("Charging API key is missing."));
                yield break;
            }
            if (float.IsNaN(latitude) || float.IsInfinity(latitude) ||
                float.IsNaN(longitude) || float.IsInfinity(longitude) ||
                latitude < -90f || latitude > 90f || longitude < -180f || longitude > 180f ||
                float.IsNaN(radiusKm) || float.IsInfinity(radiusKm) || radiusKm <= 0f)
            {
                completed(DataResult<List<ChargingStationData>>.Failure("Charging search settings are invalid."));
                yield break;
            }
            // Retrieve a bounded candidate set, then rank locally by straight-line distance.
            string url = string.Format(CultureInfo.InvariantCulture,
                "https://api.openchargemap.io/v3/poi/?output=json&latitude={0}&longitude={1}" +
                "&distance={2}&distanceunit=km&maxresults=100&compact=true&verbose=false" +
                "&camelcase=false&client=InfotainmentSimulator&opendata=true",
                latitude, longitude, radiusKm);
            yield return http.Get(url, json => Parse(json, maximumResults), completed,
                request => request.SetRequestHeader("X-API-Key", apiKey.Trim()));
        }

        /// <summary>
        /// Used to cancel the current data fetching request.
        /// </summary>
        public void Cancel() => http.Cancel();

        /// <summary>
        /// Used to parse the JSON response from the Open Charge Map API and convert it into a list of ChargingStationData objects. If no usable records are found, an error result will be returned instead.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="maximumResults"></param>
        /// <returns></returns>
        public static DataResult<List<ChargingStationData>> Parse(string json, int maximumResults)
        {
            JArray records = JArray.Parse(json);
            if (records.Count == 0)
                return DataResult<List<ChargingStationData>>.Empty();

            var stations = new List<ChargingStationData>();
            var seenIds = new HashSet<long>();
            foreach (JToken record in records)
            {
                if (!(record is JObject item) || !(item["AddressInfo"] is JObject address) ||
                    !JsonNumbers.TryFloat(address["Distance"], out float distance) || distance < 0f)
                    continue;

                if (item["ID"]?.Type == JTokenType.Integer &&
                    !seenIds.Add(item["ID"].Value<long>()))
                    continue;

                float? maximumPower = null;
                if (item["Connections"] is JArray connections)
                {
                    foreach (JToken connection in connections)
                    {
                        if (connection is JObject details &&
                            JsonNumbers.TryFloat(details["PowerKW"], out float power) && power > 0f &&
                            (!maximumPower.HasValue || power > maximumPower.Value))
                            maximumPower = power;
                    }
                }
                string name = address["Title"]?.Type == JTokenType.String
                    ? address["Title"].Value<string>() : null;
                stations.Add(new ChargingStationData(name, distance, maximumPower));
            }
            if (stations.Count == 0)
                return DataResult<List<ChargingStationData>>.Failure("No usable station records were returned.");
            stations.Sort((left, right) => left.Distance.CompareTo(right.Distance));
            int count = Math.Max(1, Math.Min(3, maximumResults));
            if (stations.Count > count)
                stations.RemoveRange(count, stations.Count - count);
            return DataResult<List<ChargingStationData>>.Success(stations);
        }
    }
}
