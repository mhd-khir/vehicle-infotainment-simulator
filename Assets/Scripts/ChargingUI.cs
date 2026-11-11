using InfotainmentSystem.Charging;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace InfotainmentSystem.UI
{
    [DisallowMultipleComponent]
    public class ChargingUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusTxt;
        [SerializeField] private Transform stationContainer;
        [SerializeField] private ChargingStationUI stationUIPrefab;

        private readonly List<ChargingStationUI> stations = new List<ChargingStationUI>();

        public bool isConfigured => statusTxt != null &&
                                    stationContainer != null &&
                                    stationUIPrefab != null &&
                                    stationUIPrefab.isConfigured;

        public void ShowLoading() => ShowStatus("Loading charging stations…");
        public void ShowError() => ShowStatus("Could not load charging stations.\nPlease try again.");
        public void ShowConfigurationError() => ShowStatus("Charging API key is missing.");

        /// <summary>
        /// Used to display the list of charging stations in the UI. If the list is empty, a message will be shown instead.
        /// </summary>
        /// <param name="data"></param>
        public void ShowStation(IReadOnlyList<ChargingStationData> data)
        {
            if (!isConfigured) return;
            if (data == null || data.Count == 0)
            {
                ShowStatus("No charging stations found nearby.");
                return;
            }
            while (stations.Count < data.Count)
            {
                stations.Add(Instantiate(stationUIPrefab, stationContainer));
            }

            for (int i = 0; i < stations.Count; i++)
            {
                bool canShow = i < data.Count;
                stations[i].gameObject.SetActive(canShow);
                if (!canShow) continue;
                ChargingStationData d = data[i];
                stations[i].Render(d.Name, d.Distance, d.MaximumPowerKw);
            }
            statusTxt.gameObject.SetActive(false);
            stationContainer.gameObject.SetActive(true);
        }

        /// <summary>
        /// Used to display a status message in the UI. The list of charging stations will be hidden.
        /// </summary>
        /// <param name="msg"></param>
        private void ShowStatus(string msg)
        {
            if (!isConfigured) return;
            statusTxt.text = msg;
            stationContainer.gameObject.SetActive(false);
            statusTxt.gameObject.SetActive(true);
        }
    }
}
