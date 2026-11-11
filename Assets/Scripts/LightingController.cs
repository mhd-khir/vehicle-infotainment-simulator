using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace InfotainmentSystem.Lighting
{
    [DisallowMultipleComponent]
    public sealed class LightingController : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private Button welcomeButton;
        [SerializeField] private Button goodbyeButton;
        [SerializeField] private AmbientLightController ambientController;
        [SerializeField] private TMP_Text scenarioLabel;

        [Header("Exterior Lights")]
        [SerializeField] private LightZone[] frontLights;
        [SerializeField] private LightZone[] rearLights;

        [Header("Colors")]
        [SerializeField]
        private Color frontColor = new Color32(234, 246, 255, 255);

        [SerializeField]
        private Color rearColor = new Color32(255, 51, 77, 255);

        [Header("Timing — Seconds")]
        [SerializeField, Min(0.05f)] private float fadeDuration = 0.8f;
        [SerializeField, Min(0.05f)] private float holdDuration = 0.5f;

        private Coroutine activeScenario; // This reference is used to stop the currently running scenario if a new one is triggered or if the user provides manual input.
        private float frontIntensity;
        private float rearIntensity; 
        private bool isConfigured; // This flag is used to prevent errors when the component is misconfigured in the editor.

        private void Awake()
        {
            isConfigured =
                welcomeButton != null &&
                goodbyeButton != null &&
                welcomeButton != goodbyeButton &&
                ambientController != null &&
                ambientController.IsConfigured &&
                ValidateZones(frontLights) &&
                ValidateZones(rearLights) &&
                !HasSharedZones();

            if (!isConfigured)
            {
                Debug.LogError(
                    "LightingController: Assign distinct buttons " +
                    "and valid, unique exterior light zones.",
                    this);

                enabled = false;
                return;
            }

            RenderExterior();
        }

        private void OnEnable()
        {
            if (!isConfigured)
                return;

            welcomeButton.onClick.AddListener(PlayWelcome);
            goodbyeButton.onClick.AddListener(PlayGoodbye);
            ambientController.ManualInput += HandleManualInput;
            SetStatus("Manual lighting");
        }

        private void OnDisable()
        {
            if (!isConfigured)
                return;

            welcomeButton.onClick.RemoveListener(PlayWelcome);
            goodbyeButton.onClick.RemoveListener(PlayGoodbye);

            ambientController.ManualInput -= HandleManualInput;
            CancelPreview();
        }

        /// <summary>
        /// Handles the event when the user provides manual input in Brightness Bar or Changing Color. This method cancels any active lighting scenario and updates the status label to indicate that manual lighting is in effect.
        /// </summary>
        private void HandleManualInput()
        {
            CancelPreview();
            SetStatus("Manual lighting");
        }

        /// <summary>
        /// Stops any active lighting scenario, resets the exterior light intensities to zero, renders the exterior lights, and restores the ambient lighting controller to manual mode. This method is called when the user provides manual input or when the component is disabled.
        /// </summary>
        private void CancelPreview()
        {
            StopActiveScenario();
            frontIntensity = 0f;
            rearIntensity = 0f;
            RenderExterior();
            ambientController.RestoreManual();
        }

        /// <summary>
        /// Updates the status label with the provided message. This method is used to inform the user about the current lighting scenario or manual lighting state.
        /// </summary>
        /// <param name="message"></param>
        private void SetStatus(string message)
        {
            if (scenarioLabel != null) scenarioLabel.text = message;
        }

        private void PlayWelcome() => StartScenario(isWelcome: true);

        private void PlayGoodbye() => StartScenario(isWelcome: false);

        /// <summary>
        /// Unlocks and starts the specified lighting scenario (welcome or goodbye). This method stops any currently active scenario, updates the status label to indicate the selected scenario, and starts a coroutine to run the scenario's sequence of lighting changes.
        /// </summary>
        /// <param name="isWelcome"></param>
        private void StartScenario(bool isWelcome)
        {
            StopActiveScenario();
            SetStatus(isWelcome ? "Welcome sequence..." : "Goodbye sequence...");
            activeScenario = StartCoroutine(RunScenario(isWelcome));
        }

        /// <summary>
        /// Stops the currently active lighting scenario, if any. This method is called when a new scenario is triggered or when the user provides manual input, ensuring that only one scenario runs at a time.
        /// </summary>
        private void StopActiveScenario()
        {
            if (activeScenario == null)
                return;

            StopCoroutine(activeScenario);
            activeScenario = null;
        }

        private IEnumerator RunScenario(bool isWelcome)
        {
            if (isWelcome) // Chosen Welcome lighting scenario
            {
                // Exterior lights rise while the cabin gently dims.
                yield return FadeTo(1f, 1f, 0.2f);

                // Cabin rises to the user's chosen brightness.
                yield return FadeTo(1f, 1f, 1f);

                yield return Hold();

                // Exterior fades; the selected cabin setting remains.
                yield return FadeTo(0f, 0f, 1f);
            }
            else // Chosen Goodbye lighting scenario
            {
                // Rear lights accompany the cabin fading out.
                yield return FadeTo(0f, 1f, 0f);

                yield return Hold();

                // Finish with all simulated lighting off.
                yield return FadeTo(0f, 0f, 0f);
            }

            activeScenario = null;
            SetStatus(isWelcome
                ? (ambientController.Brightness > 0f
                    ? "Welcome complete · ambient lighting on"
                    : "Welcome complete · brightness is zero")
                : "Goodbye complete · lighting off");
        }

        /// <summary>
        /// Used to smoothly transition the exterior light intensities and the ambient lighting scenario multiplier to the specified target values over a defined duration.
        /// </summary>
        /// <param name="targetFront"></param>
        /// <param name="targetRear"></param>
        /// <param name="targetAmbient"></param>
        /// <returns></returns>
        private IEnumerator FadeTo(
            float targetFront,
            float targetRear,
            float targetAmbient)
        {
            float startFront = frontIntensity;
            float startRear = rearIntensity;
            float startAmbient = ambientController.ScenarioMultiplier;

            float duration = Mathf.Max(0.05f, fadeDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsed / duration);
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

                frontIntensity = Mathf.Lerp(
                    startFront, targetFront, easedProgress);

                rearIntensity = Mathf.Lerp(
                    startRear, targetRear, easedProgress);

                RenderExterior();

                ambientController.SetScenarioMultiplier(
                    Mathf.Lerp(
                        startAmbient,
                        targetAmbient,
                        easedProgress));

                yield return null;
            }

            frontIntensity = targetFront;
            rearIntensity = targetRear;
            RenderExterior();
            ambientController.SetScenarioMultiplier(targetAmbient);
        }

        /// <summary>
        ///  Used to maintain the current lighting state for a specified duration, allowing the user to observe the lighting effect before it transitions to the next state.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Hold()
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, holdDuration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Used to update the exterior light zones with the current front and rear light intensities and colors.
        /// </summary>
        private void RenderExterior()
        {
            foreach (LightZone zone in frontLights)
                zone.Render(frontColor, frontIntensity);

            foreach (LightZone zone in rearLights)
                zone.Render(rearColor, rearIntensity);
        }

        /// <summary>
        /// Used to validate that the provided array of LightZone references is non-null, contains no null elements, and has unique references.
        /// </summary>
        /// <param name="zones"></param>
        /// <returns></returns>
        private static bool ValidateZones(LightZone[] zones)
        {
            if (zones == null || zones.Length == 0)
                return false;

            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] == null)
                    return false;

                for (int j = 0; j < i; j++)
                {
                    if (zones[i] == zones[j])
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Used to check if any LightZone references are shared between the front and rear light arrays.
        /// </summary>
        /// <returns></returns>
        private bool HasSharedZones()
        {
            foreach (LightZone front in frontLights)
            {
                foreach (LightZone rear in rearLights)
                {
                    if (front == rear)
                        return true;
                }
            }

            return false;
        }
    }
}