using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace InfotainmentSystem.ui
{
    [DisallowMultipleComponent]
    public class WeatherUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusTxt;
        [SerializeField] private GameObject weatherData;
        [SerializeField] private TMP_Text locationTxt;
        [SerializeField] private TMP_Text temperatureTxt;
        [SerializeField] private TMP_Text descriptionTxt;

        [Header("Weather icons (optional)")]
        [SerializeField] private Image weatherIcon;
        [SerializeField] private Sprite clearIcon;
        [SerializeField] private Sprite partlyCloudyIcon;
        [SerializeField] private Sprite cloudyIcon;
        [SerializeField] private Sprite fogIcon;
        [SerializeField] private Sprite rainIcon;
        [SerializeField] private Sprite snowIcon;
        [SerializeField] private Sprite thunderstormIcon;
        [SerializeField] private Sprite unknownIcon;

        private bool isConfigured;

        private void Awake()
        {
            isConfigured =
                statusTxt != null &&
                weatherData != null &&
                locationTxt != null &&
                temperatureTxt != null &&
                descriptionTxt != null;

            SetIcon(null);

            if(!isConfigured)
            {
                Debug.LogError("WeatherUI: Assign all references.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// Shows a loading message while weather data is being fetched.
        /// </summary>
        public void ShowLoading() => ShowStatus("Loading Weather Data...");

        /// <summary>
        /// Shows an error message when weather data could not be loaded.
        /// </summary>
        public void ShowError() => ShowStatus("Could not load weather.\nPlease try again.");
        
        /// <summary>
        /// Shows a message when no weather data is available.
        /// </summary>
        public void ShowNoDataCase() => ShowStatus("No Weather Data is available!");

        /// <summary>
        /// Shows the weather information including location, temperature, description, and an optional weather icon based on the provided weather code.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="temperatureDegree"></param>
        /// <param name="description"></param>
        /// <param name="weatherCode"></param>
        public void ShowWeather(string location, float temperatureDegree, string description, int weatherCode = -1)
        {
            if (!isConfigured) return;
            locationTxt.text = location;
            temperatureTxt.SetText("{0:0.0} °C", temperatureDegree);
            SetIcon(ResolveIcon(weatherCode));
            descriptionTxt.text = description; 

            statusTxt.gameObject.SetActive(false);
            weatherData.SetActive(true);
        }

        /// <summary>
        /// Shows a status message.
        /// </summary>
        /// <param name="message"></param>
        private void ShowStatus(string message)
        {
            if (!isConfigured) return;
            SetIcon(null);
            statusTxt.text = message;
            statusTxt.gameObject.SetActive(true);
            weatherData.SetActive(false);
        }

        /// <summary>
        /// Sets the weather icon based on the provided sprite. If the sprite is null, the icon will be hidden.
        /// </summary>
        /// <param name="sprite"></param>
        private void SetIcon(Sprite sprite)
        {
            if (weatherIcon == null) return;
            weatherIcon.sprite = sprite;
            weatherIcon.enabled = sprite != null;
        }

        /// <summary>
        /// Used to resolve the appropriate weather icon based on the provided weather code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private Sprite ResolveIcon(int code)
        {
            Sprite icon;
            switch (code)
            {
                case 0: case 1: icon = clearIcon; break;
                case 2: icon = partlyCloudyIcon; break;
                case 3: icon = cloudyIcon; break;
                case 45: case 48: icon = fogIcon; break;
                case 51: case 53: case 55: case 56: case 57:
                case 61: case 63: case 65: case 66: case 67:
                case 80: case 81: case 82: icon = rainIcon; break;
                case 71: case 73: case 75: case 77:
                case 85: case 86: icon = snowIcon; break;
                case 95: case 96: case 99: icon = thunderstormIcon; break;
                default: icon = unknownIcon; break;
            }
            return icon != null ? icon : unknownIcon;
        }
    }
}
