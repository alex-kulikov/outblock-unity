using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

namespace OutBlock
{

    /// <summary>
    /// This class manages all saved settings and UI.
    /// </summary>
    public class Settings : MonoBehaviour
    {

        [SerializeField]
        private GameObject window = null;
        [SerializeField]
        private Slider sensitivitySlider = null;
        [SerializeField]
        private Text sensitivityValue = null;
        [SerializeField]
        private Slider joySensitivitySlider = null;
        [SerializeField]
        private Text joySensitivityValue = null;
        [SerializeField]
        private Toggle aimAssistToggle = null;
        [SerializeField]
        private Slider aimAssistCoefficientSlider = null;
        [SerializeField]
        private Text aimAssistCoefficientValue = null;
        [SerializeField]
        private Slider volumeSlider = null;
        [SerializeField]
        private Text volumeText = null;
        [SerializeField]
        private Text qualityText = null;
        [SerializeField]
        private Text resolutionText = null;
        [SerializeField, Header("Keybinding")]
        private GameObject keybindingWindow = null;

        private const float minSensitivity = 0.1f;
        private const float maxSensitivity = 3f;

        private const string prefsSensitivityKey = "Sensitivity";
        private const string prefsJoySensitivityKey = "JoySensitivity";
        private const string prefsAimAssistKey = "AimAssist";
        private const string prefsAimAssistCoefficientKey = "AimAssistCoefficient";
        private const string volumePrefsKey = "Volume";
        private const string qualityPrefsKey = "UnityGraphicsQuality";

        private const int minQuality = 0;
        private const int maxQuality = 2;

        private bool initialized;

        private static Settings current;
        private static bool settingsLoaded;

        private static float sensitivity = 1;
        /// <summary>
        /// Mouse sensitivity.
        /// </summary>
        public static float Sensitivity
        {
            get
            {
                if (!settingsLoaded)
                    LoadSettings();

                return sensitivity;
            }
            private set
            {
                sensitivity = Mathf.Clamp(value, minSensitivity, maxSensitivity);
                if (current)
                    current.sensitivityValue.text = sensitivity.ToString("F1");
            }
        }

        private static float joySensitivity = 1;
        /// <summary>
        /// Gamepad sensitivity.
        /// </summary>
        public static float JoySensitivity
        {
            get
            {
                if (!settingsLoaded)
                    LoadSettings();

                return joySensitivity;
            }
            private set
            {
                joySensitivity = Mathf.Clamp(value, minSensitivity, maxSensitivity);
                if (current)
                    current.joySensitivityValue.text = joySensitivity.ToString("F1");
            }
        }

        private static bool aimAssist = true;
        /// <summary>
        /// Aim assist.
        /// </summary>
        public static bool AimAssist
        {
            get
            {
                if (!settingsLoaded)
                    LoadSettings();

                return aimAssist;
            }
            private set
            {
                aimAssist = value;
            }
        }

        private static float aimAssistCoefficient = 0.5f;
        /// <summary>
        /// Aim assist coefficient.
        /// </summary>
        public static float AimAssistCoefficient
        {
            get
            {
                if (!settingsLoaded)
                    LoadSettings();

                return aimAssistCoefficient;
            }
            private set
            {
                aimAssistCoefficient = Mathf.Clamp(value, 0, 1);
                if (current)
                    current.aimAssistCoefficientValue.text = aimAssistCoefficient.ToString("F1");
            }
        }

        private static float volume = 0.8f;
        /// <summary>
        /// Audio volume.
        /// </summary>
        public static float Volume
        {
            get
            {
                if (!settingsLoaded)
                    LoadSettings();

                return volume;
            }
            private set
            {
                volume = Mathf.Clamp(value, 0.0001f, 1);

                Utils.Mixer?.SetFloat("Volume", Mathf.Log10(volume) * 20);
                if (current)
                    current.volumeText.text = volume.ToString("F1");
            }
        }

        private static int quality = 0;
        /// <summary>
        /// Graphics quality.
        /// </summary>
        public static int Quality
        {
            get
            {
                if (!settingsLoaded)
                    LoadSettings();

                return quality;
            }
            private set
            {
                quality = value;
                if (quality < minQuality)
                    quality = maxQuality;
                if (quality > maxQuality)
                    quality = minQuality;

                QualitySettings.SetQualityLevel(quality);
                if (current)
                    current.qualityText.text = qualityNames[quality];
            }
        }
        private static string[] qualityNames = new string[3] { "Low", "Medium", "High" };

        private static List<Resolution> availableResolutions = new List<Resolution>();
        /// <summary>
        /// Available screen resolutions.
        /// </summary>
        public static List<Resolution> AvailableResolutions
        {
            get
            {
                if (availableResolutions.Count <= 0)
                {
                    availableResolutions = new List<Resolution>();
                    Resolution[] resolutions = Screen.resolutions;
                    for (int i = 0; i < resolutions.Length; i++)
                    {
                        if (!ResolutionContains(ref availableResolutions, resolutions[i]))
                            availableResolutions.Add(resolutions[i]);
                    }
                }

                return availableResolutions;
            }
        }

        private static int currentResolution;
        /// <summary>
        /// Index of the current screen resolution in the <see cref="AvailableResolutions"/>
        /// </summary>
        public static int CurrentResolution
        {
            get
            {
                return currentResolution;
            }
            private set
            {
                currentResolution = value;
                if (currentResolution >= AvailableResolutions.Count)
                    currentResolution = 0;
                if (currentResolution < 0)
                    currentResolution = AvailableResolutions.Count - 1;

                Screen.SetResolution(AvailableResolutions[currentResolution].width, AvailableResolutions[currentResolution].height, Screen.fullScreen);
                if (current)
                    current.resolutionText.text = string.Format("{0}X{1}", AvailableResolutions[currentResolution].width, AvailableResolutions[currentResolution].height);
            }
        }

        private void InitSettings()
        {
            initialized = true;

            if (!settingsLoaded)
                LoadSettings();

            sensitivityValue.text = sensitivity.ToString("F1");
            joySensitivityValue.text = joySensitivity.ToString("F1");
            aimAssistCoefficientValue.text = aimAssistCoefficient.ToString("F1");
            volumeText.text = volume.ToString("F1");
            qualityText.text = qualityNames[quality];
            resolutionText.text = string.Format("{0}X{1}", AvailableResolutions[currentResolution].width, AvailableResolutions[currentResolution].height);

            sensitivitySlider.minValue = joySensitivitySlider.minValue = minSensitivity;
            sensitivitySlider.maxValue = joySensitivitySlider.maxValue = maxSensitivity;
            sensitivitySlider.value = Sensitivity;
            joySensitivitySlider.value = JoySensitivity;
            aimAssistCoefficientSlider.value = AimAssistCoefficient;

            aimAssistToggle.isOn = AimAssist;

            volumeSlider.value = Volume;

            int width = PlayerPrefs.GetInt("Screenmanager Resolution Width", Screen.currentResolution.width);
            int height = PlayerPrefs.GetInt("Screenmanager Resolution Height", Screen.currentResolution.height);
            for (int i = 0; i < AvailableResolutions.Count; i++)
            {
                if (width == AvailableResolutions[i].width && height == AvailableResolutions[i].height)
                    CurrentResolution = i;
            }
        }

        private static void LoadSettings()
        {
            Sensitivity = PlayerPrefs.GetFloat(prefsSensitivityKey, 1);
            JoySensitivity = PlayerPrefs.GetFloat(prefsJoySensitivityKey, 1);
            AimAssist = Convert.ToBoolean(PlayerPrefs.GetInt(prefsAimAssistKey, 1));
            AimAssistCoefficient = PlayerPrefs.GetFloat(prefsAimAssistCoefficientKey, 0.5f);
            Volume = PlayerPrefs.GetFloat(volumePrefsKey, 0.8f);
            Quality = PlayerPrefs.GetInt(qualityPrefsKey, 1);


            settingsLoaded = true;
        }

        private static bool ResolutionContains(ref List<Resolution> list, Resolution check)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].width == check.width && list[i].height == check.height)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Select UI element.
        /// </summary>
        public void SelectObject(GameObject gameObject)
        {
            GameUI.Instance().SelectObject(gameObject);
        }

        /// <summary>
        /// Open settings window.
        /// </summary>
        public void Open()
        {
            window.SetActive(!window.activeSelf);

            current = window.activeSelf ? this : null;

            if (!initialized)
                InitSettings();

            if (!window.activeSelf)
            {
                PlayerPrefs.SetFloat(prefsSensitivityKey, Sensitivity);
                PlayerPrefs.SetFloat(prefsJoySensitivityKey, JoySensitivity);
                PlayerPrefs.SetInt(prefsAimAssistKey, AimAssist ? 1 : 0);
                PlayerPrefs.SetFloat(prefsAimAssistCoefficientKey, AimAssistCoefficient);
                PlayerPrefs.SetFloat(volumePrefsKey, Volume);
                PlayerPrefs.SetInt(qualityPrefsKey, Quality);
                PlayerPrefs.SetInt("Screenmanager Resolution Width", AvailableResolutions[currentResolution].width);
                PlayerPrefs.SetInt("Screenmanager Resolution Height", AvailableResolutions[currentResolution].height);
            }
        }

        /// <summary>
        /// On mouse sensitivity slider changed.
        /// </summary>
        public void ChangeSensitivity()
        {
            if (window.activeSelf)
            {
                Sensitivity = sensitivitySlider.value;
            }
        }

        /// <summary>
        /// On gamepad sensitivity slider changed.
        /// </summary>
        public void ChangeJoySensitivity()
        {
            if (window.activeSelf)
            {
                JoySensitivity = joySensitivitySlider.value;
            }
        }

        /// <summary>
        /// On aim assist slider toggle.
        /// </summary>
        public void ChangeAimAssist()
        {
            if (window.activeSelf)
            {
                AimAssist = aimAssistToggle.isOn;
            }
        }

        /// <summary>
        /// On aim assist coefficient slider changed.
        /// </summary>
        public void ChangeAimAssistCoefficient()
        {
            if (window.activeSelf)
            {
                AimAssistCoefficient = aimAssistCoefficientSlider.value;
            }
        }

        /// <summary>
        /// On audio volume slider changed.
        /// </summary>
        public void ChangeVolume()
        {
            if (window.activeSelf)
            {
                Volume = volumeSlider.value;
            }
        }

        /// <summary>
        /// Change quality.
        /// </summary>
        public void ChangeQuality(int i)
        {
            if (window.activeSelf)
            {
                Quality += i;
            }
        }

        /// <summary>
        /// Change screen resolution.
        /// </summary>
        public void ChangeResolution(int i)
        {
            if (window.activeSelf)
            {
                CurrentResolution += i;
            }
        }

        /// <summary>
        /// Open keybindings window.
        /// </summary>
        public void KeybindingOpen()
        {
            keybindingWindow.SetActive(true);
        }

        /// <summary>
        /// Close keybindings window. Use this instead of just disabling window(Cancels all <see cref="KeyChanger"/> and saves key settings.
        /// </summary>
        public void KeybindingClose()
        {
            keybindingWindow.SetActive(false);
            KeyChanger.CancelAll();
            InputManager.Instance().Save();
        }

        /// <summary>
        /// Reset keybindings to default.
        /// </summary>
        public void ResetControlsToDefaults()
        {
            KeyChanger.ResetToDefault();
        }

    }
}