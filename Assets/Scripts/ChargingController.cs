using System.Collections.Generic;
using InfotainmentSystem.Charging;
using InfotainmentSystem.Data;
using UnityEngine;
using UnityEngine.UI;

namespace InfotainmentSystem.UI
{
    [DisallowMultipleComponent]
    public sealed class ChargingController : MonoBehaviour
    {
        [Header("Widget")]
        [SerializeField] private ChargingUI widget;
        [SerializeField] private Button refreshButton;
        [Header("API — dedicated evaluation key")]
        [SerializeField] private string apiKey;
        [Header("Search Location")]
        [SerializeField, Range(-90f, 90f)] private float latitude = 48.8f;
        [SerializeField, Range(-180f, 180f)] private float longitude = 11.37f;
        [SerializeField, Min(0.1f)] private float radiusKm = 10f;
        [SerializeField, Range(1, 3)] private int maximumResults = 3;
        private WidgetRequestRunner<List<ChargingStationData>> runner; // The runner is responsible for managing the data fetching and UI updates.

        private void Awake()
        {
            if (widget == null || !widget.isConfigured || refreshButton == null)
            {
                Debug.LogError("ChargingController: Assign all view references.", this);
                enabled = false;
                return;
            }
            // Create a new instance of the ChargingAPI with the specified parameters and assign it to the runner.
            runner = new WidgetRequestRunner<List<ChargingStationData>>(this,
                new ChargingAPI(apiKey, latitude, longitude, Mathf.Max(0.1f, radiusKm),
                    Mathf.Clamp(maximumResults, 1, 3)),
                widget.ShowLoading, ShowResult,
                busy => refreshButton.interactable = !busy);
        }

        private void OnEnable()
        {
            if (runner == null) return;
            refreshButton.onClick.AddListener(Refresh);
            runner.Activate();
        }

        private void OnDisable()
        {
            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(Refresh);
            runner?.Cancel();
        }

        /// <summary>
        /// Used to refresh the list of charging stations. If the API key is missing, a configuration error will be shown instead.
        /// </summary>
        public void Refresh()
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                if (widget != null) widget.ShowConfigurationError();
                return;
            }
            runner?.Refresh();
        }

        /// <summary>
        /// Used to display the result of the data fetching in the UI. If there was an error, an error message will be shown instead.
        /// </summary>
        /// <param name="result"></param>
        private void ShowResult(DataResult<List<ChargingStationData>> result)
        {
            if (result.Status == DataStatus.Error)
            {
                if (string.IsNullOrWhiteSpace(apiKey)) widget.ShowConfigurationError();
                else widget.ShowError();
            }
            else widget.ShowStation(result.Value);
        }
    }
}
