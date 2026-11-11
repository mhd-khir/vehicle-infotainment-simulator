using InfotainmentSystem.Charging;
using InfotainmentSystem.Data;
using InfotainmentSystem.Lighting;
using InfotainmentSystem.Weather;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace InfotainmentSystem.Tests
{
    public sealed class DataAndLightingTests
    {
        private static string Weather(string temperature, string code = "3") =>
            "{\"current\":{\"time\":\"2026-11-10T12:00\",\"temperature_2m\":" +
            temperature + ",\"weather_code\":" + code + "}}";

        private static string Station(int id, string distance, string connections = "[]") =>
            "{\"ID\":" + id + ",\"AddressInfo\":{\"Title\":\"Station\",\"Distance\":" +
            distance + "},\"Connections\":" + connections + "}";

        [TestCase("7.4", 7.4f)]
        [TestCase("0", 0f)]
        [TestCase("-12.5", -12.5f)]
        public void WeatherPreservesValidTemperature(string input, float expected)
        {
            var result = WeatherService.Parse(Weather(input));
            Assert.That(result.Status, Is.EqualTo(DataStatus.Success));
            Assert.That(result.Value.Temperature, Is.EqualTo(expected).Within(0.001f));
            Assert.That(result.Value.WeatherCode, Is.EqualTo(3));
        }

        [TestCase("null")]
        [TestCase("\"7.4\"")]
        [TestCase("true")]
        public void WeatherRejectsInvalidTemperature(string input) =>
            Assert.That(WeatherService.Parse(Weather(input)).Status, Is.EqualTo(DataStatus.Error));

        [TestCase("null")]
        [TestCase("3.5")]
        [TestCase("-1")]
        public void WeatherRejectsInvalidCode(string code) =>
            Assert.That(WeatherService.Parse(Weather("7.4", code)).Status, Is.EqualTo(DataStatus.Error));

        [TestCase("{}")]
        [TestCase("{\"current\":null}")]
        public void WeatherMissingCurrentIsEmpty(string json) =>
            Assert.That(WeatherService.Parse(json).Status, Is.EqualTo(DataStatus.Empty));

        [Test]
        public void WeatherMissingTemperatureIsError() =>
            Assert.That(WeatherService.Parse("{\"current\":{\"time\":\"2026-11-10T12:00\",\"weather_code\":3}}").Status,
                Is.EqualTo(DataStatus.Error));

        [Test]
        public void WeatherInvalidTimeIsError() =>
            Assert.That(WeatherService.Parse(Weather("7.4").Replace("2026-11-10T12:00", "invalid")).Status,
                Is.EqualTo(DataStatus.Error));

        [Test]
        public void MalformedWeatherCannotBecomeSuccess() =>
            Assert.Throws<JsonReaderException>(() => WeatherService.Parse("{broken"));

        [Test]
        public void ChargingEmptyDiffersFromInvalidRecords()
        {
            Assert.That(ChargingAPI.Parse("[]", 3).Status, Is.EqualTo(DataStatus.Empty));
            Assert.That(ChargingAPI.Parse("[{}]", 3).Status, Is.EqualTo(DataStatus.Error));
        }

        [Test]
        public void ChargingZeroDistanceIsValid()
        {
            var result = ChargingAPI.Parse("[" + Station(1, "0") + "]", 3);
            Assert.That(result.Status, Is.EqualTo(DataStatus.Success));
            Assert.That(result.Value[0].Distance, Is.Zero);
        }

        [Test]
        public void ChargingMissingDistanceIsNotZero() =>
            Assert.That(ChargingAPI.Parse("[{\"AddressInfo\":{\"Title\":\"Station\"}}]", 3).Status,
                Is.EqualTo(DataStatus.Error));

        [Test]
        public void ChargingSortsAndLimitsReceivedCandidates()
        {
            var result = ChargingAPI.Parse("[" + Station(1,"5") + "," + Station(2,"1") + "," +
                Station(3,"3") + "," + Station(4,"2") + "]", 3);
            Assert.That(result.Value.Count, Is.EqualTo(3));
            Assert.That(result.Value.ConvertAll(s => s.Distance), Is.EqualTo(new[] {1f,2f,3f}));
        }

        [Test]
        public void ChargingDeduplicatesIds()
        {
            var result = ChargingAPI.Parse("[" + Station(1,"1") + "," + Station(1,"1") + "]", 3);
            Assert.That(result.Value.Count, Is.EqualTo(1));
        }

        [Test]
        public void ChargingSelectsHighestKnownPower()
        {
            var result = ChargingAPI.Parse("[" + Station(1,"1",
                "[{\"PowerKW\":null},{\"PowerKW\":11},{\"PowerKW\":22},{\"PowerKW\":-5}]") + "]", 3);
            Assert.That(result.Value[0].MaximumPowerKw, Is.EqualTo(22f));
        }

        [Test]
        public void ChargingUnknownPowerStaysUnknown()
        {
            var result = ChargingAPI.Parse("[" + Station(1,"1") + "]", 3);
            Assert.That(result.Value[0].MaximumPowerKw, Is.Null);
        }

        [TestCase(-1f, 0f)]
        [TestCase(2f, 1f)]
        public void LightingClampsBrightness(float input, float expected)
        {
            var state = new AmbientLightState(Color.cyan, 0.7f);
            state.SetBrightness(input);
            Assert.That(state.Brightness, Is.EqualTo(expected));
        }

        [Test]
        public void ScenarioDoesNotOverwriteUserBrightness()
        {
            var state = new AmbientLightState(Color.cyan, 0.7f);
            state.SetScenarioMultiplier(0f);
            Assert.That(state.EffectiveIntensity, Is.Zero);
            Assert.That(state.Brightness, Is.EqualTo(0.7f));
            state.RestoreManual();
            Assert.That(state.EffectiveIntensity, Is.EqualTo(0.7f));
        }

        [Test]
        public void ZeroBrightnessRemainsOffDuringWelcome()
        {
            var state = new AmbientLightState(Color.cyan, 0f);
            state.SetScenarioMultiplier(1f);
            Assert.That(state.EffectiveIntensity, Is.Zero);
        }
    }
}
