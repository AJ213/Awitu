using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer = default;
    [SerializeField] TMP_Dropdown resolutionDropdown = default;
    [SerializeField] TMP_Dropdown graphicsDropdown = default;
    [SerializeField] Toggle fullscreenToggle = default;
    [SerializeField] Slider volume = default;
    [SerializeField] Slider sensitivityX = default;
    [SerializeField] Slider sensitivityY = default;
    [SerializeField] Slider fov = default;

    Resolution[] resolutions;
    int currentResolutionIndex;
    int currentQualityIndex;
    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        currentResolutionIndex = 0;
        List<string> options = new List<string>();

        for(int i = 0; i < resolutions.Length; i++)
        {
            string option =  resolutions[i].width + " x " + resolutions[i].height + ", " + resolutions[i].refreshRate + "hz";
            options.Add(option);

            if(Screen.width == resolutions[i].width && Screen.height == resolutions[i].height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        LoadSettings();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        currentResolutionIndex = resolutionIndex;
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Master Volume", Mathf.Log10(volume)*20);
    }

    public void SetSensitivityX(float sensitivityX)
    {
        CameraLook.MouseSensitivityX = sensitivityX;
    }
    public void SetSensitivityY(float sensitivityY)
    {
        CameraLook.MouseSensitivityY = sensitivityY;
    }

    public void SetFOV(float fov)
    {
        Camera.main.fieldOfView = fov;
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        currentQualityIndex = qualityIndex;
    }

    public void SetFullScreeen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("resolution", currentResolutionIndex);

        float volume = 0;
        audioMixer.GetFloat("Master Volume", out volume);
        PlayerPrefs.SetFloat("volume", volume);

        PlayerPrefs.SetFloat("sensitivityX", CameraLook.MouseSensitivityX);
        PlayerPrefs.SetFloat("sensitivityY", CameraLook.MouseSensitivityY);
        PlayerPrefs.SetFloat("fov", Camera.main.fieldOfView);
        PlayerPrefs.SetInt("qualityIndex", currentQualityIndex);
        PlayerPrefs.SetInt("isFullscreen", Convert.ToInt32(Screen.fullScreen));
        PlayerPrefs.Save();
    }
    public void LoadSettings()
    {
        SetResolution(PlayerPrefs.GetInt("resolution", currentResolutionIndex));
        resolutionDropdown.value = currentResolutionIndex;
        audioMixer.SetFloat("Master Volume", Mathf.Log10(PlayerPrefs.GetFloat("volume", 0)) * 20);
        volume.value = Mathf.Pow(10, PlayerPrefs.GetFloat("volume", 0)/20);
        CameraLook.MouseSensitivityX = PlayerPrefs.GetFloat("sensitivityX", 300);
        sensitivityX.value = CameraLook.MouseSensitivityX;
        CameraLook.MouseSensitivityY = PlayerPrefs.GetFloat("sensitivityY", 300);
        sensitivityY.value = CameraLook.MouseSensitivityY;
        Camera.main.fieldOfView = PlayerPrefs.GetFloat("fov", 90);
        fov.value = Camera.main.fieldOfView;
        SetQuality(PlayerPrefs.GetInt("qualityIndex", 0));
        graphicsDropdown.value = PlayerPrefs.GetInt("qualityIndex", 0);
        Screen.fullScreen = Convert.ToBoolean(PlayerPrefs.GetInt("isFullscreen", 1));
        fullscreenToggle.isOn = Screen.fullScreen;
    }

}
