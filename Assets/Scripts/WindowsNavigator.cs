using System;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace InfotainmentSystem.UI
{
    [DisallowMultipleComponent]
    public sealed class WindowsNavigator : MonoBehaviour
    {
        [Serializable]
        private class Window // Represents a single window and its associated button in the navigator.
        {
            public GameObject content;
            public Button button;

            [NonSerialized] public UnityAction clickHandler;
        }

        [Header("Windows")]
        [SerializeField] private Window[] windows; // The windows to navigate between, each with its associated button.
        [SerializeField, Min(0)] private int initialWindowIndex;

        [Header("Navigation Colors")]
        [SerializeField]
        private Color activeColor = new Color32(14, 116, 144, 255);

        [SerializeField]
        private Color inactiveColor = new Color32(30, 41, 59, 255);

        private Image[] indicators;
        private bool isConfigured; // flag to indicate whether the navigator is properly configured and ready to use
        private int currentWindowIndex = -1;

        private void Awake()
        {
            isConfigured = ValidateConfiguration();

            if (!isConfigured)
            {
                enabled = false;
                return;
            }

            indicators = new Image[windows.Length];
            for (int i = 0; i < windows.Length; i++)
            {
                indicators[i] = CreateIndicator(windows[i].button); // Create an indicator for each button
                int index = i;
                windows[i].clickHandler = () => ShowWindow(index); // Assign a click handler to each button that shows the corresponding window
            }
        }

        private void OnEnable()
        {
            if (!isConfigured)
                return;

            foreach (Window entry in windows)
                entry.button.onClick.AddListener(entry.clickHandler);

            int screenToRestore = currentWindowIndex >= 0
                ? currentWindowIndex
                : initialWindowIndex;

            currentWindowIndex = -1;
            ShowWindow(screenToRestore);

        }

        private void OnDisable()
        {
            if (!isConfigured)
                return;

            foreach (Window entry in windows)
            {
                if (entry.button != null)
                    entry.button.onClick.RemoveListener(entry.clickHandler);
            }
        }

        /// <summary>
        /// Used to show a specific window by index. If the index is invalid or the window is already active, it does nothing. Otherwise, it activates the specified window and deactivates all others, updating the button colors and indicators accordingly.
        /// </summary>
        /// <param name="index"></param>
        private void ShowWindow(int index)
        {
            if (currentWindowIndex == index)
                return;

            if (index < 0 || index >= windows.Length) return;
            currentWindowIndex = index;

            for (int i = 0; i < windows.Length; i++)
            {
                bool isActive = i == index;

                windows[i].content.SetActive(isActive);
                UpdateButton(windows[i].button, isActive);
                indicators[i].enabled = isActive;
            }
        }

        /// <summary>
        /// Used to update the visual state of a button based on whether it is active or inactive.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="isActive"></param>
        private void UpdateButton(Button button, bool isActive)
        {
            Color baseColor = isActive ? activeColor : inactiveColor;
            if (button.targetGraphic != null) button.targetGraphic.color = Color.white;
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = isActive
                ? new Color32(103, 232, 249, 255) : new Color32(226, 232, 240, 255);
            ColorBlock colors = button.colors;

            colors.normalColor = baseColor;
            colors.selectedColor = baseColor;
            colors.highlightedColor =
                Color.Lerp(baseColor, Color.white, 0.15f);
            colors.pressedColor =
                Color.Lerp(baseColor, Color.black, 0.2f);

            button.colors = colors;
        }

        /// <summary>
        /// Used to create an indicator image for a button, which visually indicates the active window.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private static Image CreateIndicator(Button button)
        {
            var indicator = new GameObject("Active indicator", typeof(RectTransform), typeof(Image));
            indicator.transform.SetParent(button.transform, false);
            RectTransform rect = (RectTransform)indicator.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(12f, 0f);
            rect.offsetMax = new Vector2(-12f, 3f);
            Image image = indicator.GetComponent<Image>();
            image.color = new Color32(34, 211, 238, 255);
            image.raycastTarget = false;
            return image;
        }

        /// <summary>
        /// Used to validate the configuration of the navigator, ensuring that all windows and buttons are properly assigned and that there are no duplicate or invalid references.
        /// </summary>
        /// <returns></returns>
        private bool ValidateConfiguration()
        {
            if (windows == null || windows.Length == 0)
                return ConfigurationError("Assign at least one window.");

            if (initialWindowIndex < 0 ||
                initialWindowIndex >= windows.Length)
            {
                return ConfigurationError(
                    "Initial window index is outside the windows list.");
            }

            for (int i = 0; i < windows.Length; i++)
            {
                Window entry = windows[i];

                if (entry == null ||
                    entry.content == null ||
                    entry.button == null)
                {
                    return ConfigurationError(
                        $"Window entry {i} has missing references.");
                }

                if (transform.IsChildOf(entry.content.transform))
                {
                    return ConfigurationError(
                        "Place the navigator outside the Windows it controls.");
                }

                for (int j = 0; j < i; j++)
                {
                    if (entry.content == windows[j].content ||
                        entry.button == windows[j].button ||
                        entry.content.transform.IsChildOf(windows[j].content.transform) ||
                        windows[j].content.transform.IsChildOf(entry.content.transform))
                    {
                        return ConfigurationError(
                            $"Window entry {i} repeats a Window or button.");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Used to log a configuration error message and return false, indicating that the navigator is not properly configured.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool ConfigurationError(string message)
        {
            Debug.LogError($"WindowsNavigator: {message}", this);
            return false;
        }

    }
}