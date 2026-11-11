using System;
using UnityEngine;

namespace InfotainmentSystem.Lighting
{
    [DisallowMultipleComponent]
    public class AmbientLightController : MonoBehaviour
    {
        [SerializeField] private LightZone zone;
        [SerializeField] private Color initialColor = new Color32(34, 211, 238, 255);
        [SerializeField, Range(0f, 1f)] private float initialBrightness = 0.7f;
        private AmbientLightState state; // Backing field for the ambient light state, initialized lazily.
        public bool IsConfigured => zone != null;
        private AmbientLightState State => state ??
            (state = new AmbientLightState(initialColor, initialBrightness));
        public float Brightness => State.Brightness;
        public float ScenarioMultiplier => State.ScenarioMultiplier;
        public event Action Changed;
        public event Action ManualInput;

        private void Reset() => zone = GetComponent<LightZone>();

        private void Awake()
        {
            if (!IsConfigured)
            {
                Debug.LogError("AmbientLightController: Assign a light zone.", this);
                enabled = false;
                return;
            }
            Render();
        }

        /// <summary>
        /// Used to set the user-defined brightness for the ambient light, restoring manual control and updating the rendering accordingly.
        /// </summary>
        /// <param name="value"></param>
        public void SetUserBrightness(float value)
        {
            ManualInput?.Invoke();
            State.RestoreManual();
            State.SetBrightness(value);
            Render();
        }

        /// <summary>
        /// Used to set the user-defined color for the ambient light, restoring manual control and updating the rendering accordingly.
        /// </summary>
        /// <param name="color"></param>
        public void SetUserColor(Color color)
        {
            ManualInput?.Invoke();
            State.RestoreManual();
            State.SetColor(color);
            Render();
        }

        /// <summary>
        /// Used to set the scenario multiplier for the ambient light.
        /// </summary>
        /// <param name="value"></param>
        public void SetScenarioMultiplier(float value)
        {
            State.SetScenarioMultiplier(value);
            Render();
        }

        /// <summary>
        /// Used to restore manual control over the ambient light, resetting the scenario multiplier and updating the rendering accordingly.
        /// </summary>
        public void RestoreManual()
        {
            State.RestoreManual();
            Render();
        }

        /// <summary>
        /// Used to render the ambient light zone based on the current color and effective intensity, invoking the Changed event to notify listeners of the update.
        /// </summary>
        private void Render()
        {
            if (zone != null) zone.Render(State.Color, State.EffectiveIntensity);
            Changed?.Invoke();
        }
    }
}
