using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set;}

    [Header("BGM")]
    public AudioSource bgmSource;
    public AudioClip bgmClip;

    [Range(0f, 1f)]
    public float bgmVolume = 0.7f;

    [Header("SFX Combine")]
    public AudioClip heatingSFX;
    public AudioClip forgingSFX;
    public AudioClip swordSFX;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    private const string BGM_KEY = "BGMVolume";
    private const string SFX_KEY = "SFXVolume";
    private bool settingsDirty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeAudioManager()
    {
        if(Instance != null) return;
        GameObject prefab = Resources.Load<GameObject>("AudioManager");
        if(prefab == null)
        {
            Debug.LogError("AudioManager tidak ditemukan");
            return;
        }
        Instantiate(prefab);
    }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        bgmVolume = PlayerPrefs.GetFloat(BGM_KEY, bgmVolume);
        sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, sfxVolume);

        if(bgmSource == null) bgmSource = GetComponent<AudioSource>();
        if(bgmSource == null) Debug.LogError("AudioSource tidak ditemukan di AudioManager");

        if (bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume;
            bgmSource.mute = false;
            bgmSource.spatialBlend = 0f;
        }
    }

    private void Start()
    {
        if(bgmSource == null) return;
        if(bgmClip == null) return;
        bgmSource.Play();
    }

    public void SetBGMVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        if (Mathf.Approximately(bgmVolume, clampedVolume)) return;

        bgmVolume = clampedVolume;
        ApplyBGMVolume();
        PlayerPrefs.SetFloat(BGM_KEY, bgmVolume);
        settingsDirty = true;
    }

    public void SetSFXVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        if (Mathf.Approximately(sfxVolume, clampedVolume)) return;

        sfxVolume = clampedVolume;
        PlayerPrefs.SetFloat(SFX_KEY, sfxVolume);
        settingsDirty = true;
    }

    public void SaveSettings()
    {
        if (!settingsDirty) return;

        PlayerPrefs.Save();
        settingsDirty = false;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveSettings();
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }

    public void ApplyBGMVolume()
    {
        if(bgmSource != null) bgmSource.volume = bgmVolume;
    }
    public void PlayHeatingSFX()
    {
        PlaySFX(heatingSFX);
    }

    public void PlayForgingSFX()
    {
        PlaySFX(forgingSFX);
    }

    public void PlaySwordSFX()
    {
        PlaySFX(swordSFX);
    }

    public void PlaySFX(AudioClip clip)
    {
        if(clip == null) return;

        Vector3 position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
    }
}
