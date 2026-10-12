using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GamePlayUI : MonoBehaviour
{
    [Header("References")]
    public ForgingManager forgingManager;
    public CombineManager combineManager;

    [Header("Text")]
    public TMP_Text forgingCountText;
    public TMP_Text ingotStatusText;
    public TMP_Text processStatusText;

    [Header("Setting")]
    public GameObject settingPanel;
    public Slider bgmSlider;
    public Slider sfxSlider;

    private int displayedForgingCount = -1;
    private bool displayedSwordComplete;
    private bool displayedCombined;
    private bool displayedIngotHot;
    private bool displayStateInitialized;
    
    private void Start()
    {
        if(settingPanel != null) settingPanel.SetActive(false);
    }

    private void Update()
    {
        if (forgingManager == null || combineManager == null) return;

        int count = forgingManager.ForgingCount;
        bool swordComplete = forgingManager.isSwordComplete;
        bool combined = combineManager.IsCombinedForUI;
        bool ingotHot = combineManager.IsIngotHotForUI;

        if (!displayStateInitialized ||
            count != displayedForgingCount ||
            swordComplete != displayedSwordComplete ||
            combined != displayedCombined ||
            ingotHot != displayedIngotHot)
        {
            if(forgingCountText != null) forgingCountText.text = $"Penempaan: {count}/{forgingManager.MaxForgingCount}";
            if(ingotStatusText != null) ingotStatusText.text = swordComplete ? "Pedang selesai" : ingotHot ? "Ingot Panas" : "Ingot Dingin";

            if(processStatusText != null)
            {
                if (swordComplete) processStatusText.text = "Pedang selesai ditempa!";
                else if(!combined) processStatusText.text = "Dekatkan marker untuk melanjutkan";
                else if(ingotHot) processStatusText.text = "Ingot Sudah panas, siap ditempa!";
                else processStatusText.text = "Panaskan Ingot terlebih dahulu";
            }

            displayedForgingCount = count;
            displayedSwordComplete = swordComplete;
            displayedCombined = combined;
            displayedIngotHot = ingotHot;
            displayStateInitialized = true;
        }
    }

    public void BackToMain()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSetting()
    {
        if(settingPanel == null) return;
        settingPanel.SetActive(true);

        AudioManager audioManager = AudioManager.Instance;
        if(audioManager == null)
        {
            Debug.LogError("AudioManager tidak ditemukan");
            return;
        }
        if(bgmSlider != null) bgmSlider.SetValueWithoutNotify(audioManager.bgmVolume);
        if(sfxSlider != null) sfxSlider.SetValueWithoutNotify(audioManager.sfxVolume);
    }

    public void CloseSetting()
    {
        if(settingPanel != null) settingPanel.SetActive(false);
    }
}
