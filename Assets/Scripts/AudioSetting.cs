using UnityEngine;

public static class AudioSettingsData
{
    public static float BGMVolume
    {
        get => PlayerPrefs.GetFloat("BGMVolume", 0.7f);
        set
        {
            PlayerPrefs.SetFloat("BGMVolume", Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }

    public static float SFXVolume
    {
        get => PlayerPrefs.GetFloat("SFXVolume", 1f);
        set
        {
            PlayerPrefs.SetFloat("SFXVolume", Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }
}