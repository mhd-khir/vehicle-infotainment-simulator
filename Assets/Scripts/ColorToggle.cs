using UnityEngine;
using UnityEngine.UI;

namespace InfotainmentSystem.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Toggle))]
    public class ColorToggle : MonoBehaviour
    {
        [SerializeField] private AmbientLightingControls lightingControls;
        [SerializeField] private Color presetColor = Color.white;

        private Toggle toggle;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            if(lightingControls == null)
            {
                Debug.LogError("ColorToggle: Assign a reference to AmbientLightingControls.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (lightingControls == null) 
                return;

            toggle.onValueChanged.AddListener(HandleToggleChanged);
            ApplyIfSelected();
        }

        private void OnDisable()
        {
            if(toggle != null)
                toggle.onValueChanged.RemoveListener(HandleToggleChanged);
        }
        /// <summary>
        /// A Callback for the Toggle's onValueChanged event.
        /// </summary>
        /// <param name="isOn"></param>
        private void HandleToggleChanged(bool isOn)
        {
            if (isOn)
                ApplyIfSelected();
        }

        /// <summary>
        /// Updates the lighting controls with the preset color if this toggle is selected.
        /// </summary>
        private void ApplyIfSelected()
        {
            if (toggle.isOn)
                lightingControls.SetColor(presetColor);
        }
    }
}