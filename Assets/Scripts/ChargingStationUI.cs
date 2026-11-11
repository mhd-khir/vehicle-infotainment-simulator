using System.Globalization;
using TMPro;
using UnityEngine;

namespace InfotainmentSystem.UI
{
    [DisallowMultipleComponent]
    public class ChargingStationUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameTxt;
        [SerializeField] private TMP_Text detailsTxt;

        public bool isConfigured => nameTxt != null && detailsTxt != null;

        /// <summary>
        /// Used to display the name, distance and maximum power of a charging station in the UI. If any of the values are missing, a default message will be shown instead.
        /// </summary>
        /// <param name="stationName"></param>
        /// <param name="distanceKm"></param>
        /// <param name="maximumPowerKw"></param>
        public void Render(string stationName, float distanceKm, float? maximumPowerKw) 
        {
            if(!isConfigured)
            {
                Debug.LogError("ChargingStationUI: Assign both text references.", this);
                return;
            }
            nameTxt.text = string.IsNullOrWhiteSpace(stationName) ? "Unknown Name" : stationName;
            string distance = distanceKm.ToString("0.0", CultureInfo.InvariantCulture);
            string power = maximumPowerKw.HasValue ? maximumPowerKw.Value.ToString("0.#", CultureInfo.InvariantCulture) + " kW" : "Power Unknown";
            detailsTxt.text= $"{distance} km away - {power}";
        }
    }
}
