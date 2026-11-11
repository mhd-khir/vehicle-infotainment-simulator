using UnityEngine;

namespace InfotainmentSystem.Lighting
{
    public sealed class AmbientLightState
    {
        public float Brightness { get; private set; }
        public Color Color { get; private set; }
        public float ScenarioMultiplier { get; private set; } = 1f;
        public float EffectiveIntensity => Brightness * ScenarioMultiplier;

        /// <summary>
        /// Creates a new instance of the AmbientLightState class with the specified color and brightness.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="brightness"></param>
        public AmbientLightState(Color color, float brightness)
        {
            Color = color;
            Brightness = Mathf.Clamp01(brightness);
        }

        public void SetBrightness(float value) => Brightness = Mathf.Clamp01(value);
        public void SetColor(Color value) => Color = value;
        public void SetScenarioMultiplier(float value) => ScenarioMultiplier = Mathf.Clamp01(value);
        public void RestoreManual() => ScenarioMultiplier = 1f;
    }
}
