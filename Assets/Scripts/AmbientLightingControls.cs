using InfotainmentSystem.Lighting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfotainmentSystem.UI
{
    [DisallowMultipleComponent]
    public sealed class AmbientLightingControls : MonoBehaviour
    {
        [SerializeField] private Slider brightnessBar;
        [SerializeField] private TMP_Text brightnessLabel;
        [SerializeField] private AmbientLightController lightController;
        private bool isConfigured; // Flag to indicate if the component is properly configured.

        private void Awake()
        {
            isConfigured = brightnessBar != null && brightnessLabel != null &&
                lightController != null && lightController.IsConfigured;
            if (!isConfigured)
            {
                Debug.LogError("AmbientLightingControls: Assign all references.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (!isConfigured) return;
            brightnessBar.onValueChanged.AddListener(HandleBrightnessChanged);
            lightController.Changed += RefreshView;
            RefreshView();
        }

        private void OnDisable()
        {
            if (brightnessBar != null)
                brightnessBar.onValueChanged.RemoveListener(HandleBrightnessChanged);
            if (lightController != null)
                lightController.Changed -= RefreshView;
        }

        /// <summary>
        /// Used to handle changes in the brightness slider's value, updating the ambient light controller's brightness accordingly.
        /// </summary>
        /// <param name="value"></param>
        private void HandleBrightnessChanged(float value) =>
            lightController.SetUserBrightness(brightnessBar.normalizedValue);

        /// <summary>
        /// Used to refresh the UI elements to reflect the current state of the ambient light controller, including updating the brightness slider and label.
        /// </summary>
        private void RefreshView()
        {
            brightnessBar.SetValueWithoutNotify(Mathf.Lerp(
                brightnessBar.minValue, brightnessBar.maxValue, lightController.Brightness));
            brightnessLabel.SetText("Brightness  {0}%", Mathf.RoundToInt(lightController.Brightness * 100f));
        }

        /// <summary>
        /// Used to set the color of the ambient light through the ambient light controller, allowing for user-defined color adjustments.
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color color)
        {
            if (lightController != null) lightController.SetUserColor(color);
        }
    }
}
