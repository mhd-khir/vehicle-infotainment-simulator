using UnityEngine;
using UnityEngine.UI;


namespace InfotainmentSystem.Lighting
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class LightZone : MonoBehaviour
    {
        [SerializeField] private Image lightImg; // Reference to the Image component used for rendering the light zone.

        [Tooltip("Maximum overlay opacity at full intensity.")]
        [SerializeField, Range(0f, 1f)]
        private float maximumOpacity = 0.45f;

        private void Reset()
        {
            lightImg = GetComponent<Image>();
        }

        public void Render(Color color, float intensity)
        {
            if (lightImg == null)
                lightImg = GetComponent<Image>();

            color.a = Mathf.Clamp01(intensity) * maximumOpacity; // Adjust the alpha based on intensity and maximum opacity.
            lightImg.color = color;
        }
    }
}