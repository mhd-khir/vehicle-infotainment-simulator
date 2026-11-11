using InfotainmentSystem.Data;
using InfotainmentSystem.ui;
using InfotainmentSystem.Weather;
using UnityEngine;
using UnityEngine.UI;

namespace InfotainmentSystem.UI
{
    [DisallowMultipleComponent]
    public sealed class WeatherController : MonoBehaviour
    {
        [SerializeField] private WeatherUI weatherUI; // Reference to the WeatherUI component that displays weather information.
        [SerializeField] private Button refreshBtn;  // Reference to the button that allows users to refresh the weather data.
        [Header("Location")]
        [SerializeField] private string locationCity = "Gaimersheim"; // The name of the city for which weather data is being displayed.
        [SerializeField, Range(-90f, 90f)] private float latitude = 48.8f; // The latitude coordinate for the location (in degrees).
        [SerializeField, Range(-180f, 180f)] private float longitude = 11.37f; // The longitude coordinate for the location (in degrees).
        private WidgetRequestRunner<WeatherAPI> runner; // A utility class that manages the process of fetching weather data and updating the UI accordingly.

        private void Awake()
        {
            if (weatherUI == null || refreshBtn == null)
            {
                Debug.LogError("WeatherController: Assign all view references.", this);
                enabled = false;
                return;
            }
            // Initialize the WidgetRequestRunner with the WeatherService and callbacks for loading, result handling, and button interactivity.
            runner = new WidgetRequestRunner<WeatherAPI>(this,
                new WeatherService(latitude, longitude), weatherUI.ShowLoading,
                ShowResult, busy => refreshBtn.interactable = !busy);
        }

        private void OnEnable()
        {
            if (runner == null) return;
            refreshBtn.onClick.AddListener(RefreshWeather);
            runner.Activate();
        }

        private void OnDisable()
        {
            if (refreshBtn != null)
                refreshBtn.onClick.RemoveListener(RefreshWeather);
            runner?.Cancel();
        }

        /// <summary>
        /// Used to refresh the weather data by invoking the Refresh method of the WidgetRequestRunner.
        private void RefreshWeather() => runner?.Refresh();

        /// <summary>
        /// Used to handle the result of the weather data request and update the WeatherUI accordingly.
        /// </summary>
        /// <param name="result"></param>
        private void ShowResult(DataResult<WeatherAPI> result)
        {
            switch (result.Status)
            {
                case DataStatus.Success:
                    weatherUI.ShowWeather(locationCity, result.Value.Temperature,
                        WeatherDescriptions.Describe(result.Value.WeatherCode),
                        result.Value.WeatherCode);
                    break;
                case DataStatus.Empty: weatherUI.ShowNoDataCase(); break;
                default: weatherUI.ShowError(); break;
            }
        }
    }
}
