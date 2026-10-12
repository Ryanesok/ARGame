using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    private AudioManager audioManager;
    
    [Header("Slider")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        if(audioManager == null)
        {
            Debug.LogError("SettingPanel: AudioManager tidak ditemukan");
            return;
        }
        if(bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
            bgmSlider.SetValueWithoutNotify(AudioSettingsData.BGMVolume);
            bgmSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if(sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.SetValueWithoutNotify(AudioSettingsData.SFXVolume);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if(audioManager != null) audioManager.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if(audioManager != null) audioManager.SetSFXVolume(value);
    }

    private void OnDestroy() {
        if(bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if(sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        if(audioManager != null) audioManager.SaveSettings();
    }
}
