using UnityEngine;
using Vuforia;

public class CombineManager : MonoBehaviour
{
    [Header("Marker Image")]
    public ObserverBehaviour targetApi;
    public ObserverBehaviour targetIngot;
    public ObserverBehaviour targetHammer;

    [Header("Model")]
    public GameObject modelApi;
    public GameObject modelIngot;
    public GameObject modelHammer;
    public GameObject modelIngotPanas;
    public GameObject modelPaluBesi;

    [Header("Jarak dalam satuan Unity")]
    public float combineDistance = 0.21f;
    public float separateDistance = 0.22f;

    [Header("Animation")]
    public Animator ingotPanasAnimator;
    public Animator paluBesiAnimator;
    public Animator hammerAnimator;

    [Header("Audio")]
    public AudioManager audioManager;

    [Header("Ingot Asli")]
    public Renderer ingotRenderer;
    public Color warnaPanas = Color.red;
    public Color warnaAsli = Color.red;

    [Header("Forging Gameplay")]
    public ForgingManager forgingManager;

    private bool isCombined = false;
    private bool ingotSudahPanas = false;
    private bool ingotSudahTempa = false;
    private MaterialPropertyBlock ingotMaterialProperties;
    public bool IsIngotHotForUI => ingotSudahPanas;
    public bool IsCombinedForUI => isCombined;
    
    private enum CombineType { None, ApiIngot, HammerIngot}
    private CombineType currentCombineType = CombineType.None;

    void Start() 
    {
        if (audioManager == null) audioManager = AudioManager.Instance;

        modelIngotPanas.SetActive(false);
        modelPaluBesi.SetActive(false);

        modelApi.SetActive(true);
        modelIngot.SetActive(true);
        modelHammer.SetActive(true);
    }

    void Update()
    {
        bool apiTracked = IsTracked(targetApi);
        bool ingotTracked = IsTracked(targetIngot);
        bool hammerTracked = IsTracked(targetHammer);

        // NOT COMBINED YET
        if (!isCombined)
        {
            if(forgingManager != null && forgingManager.isSwordComplete) return;
            // ===============================
            // STATE 1 : Api + Ingot
            // ===============================
            if (apiTracked && ingotTracked)
            {
                float distanceApiIngotSqr = (targetApi.transform.position - targetIngot.transform.position).sqrMagnitude;

                LogDistance("Jarak Api - Ingot", distanceApiIngotSqr);
                if(distanceApiIngotSqr < combineDistance * combineDistance)
                {
                    SetCombined(true, CombineType.ApiIngot);
                    return;
                }
            }
            // ===============================
            // STATE 2 : Palu + Ingot
            // ===============================
            if (hammerTracked && ingotTracked)
            {
                float distanceHammerIngotSqr = (targetHammer.transform.position - targetIngot.transform.position).sqrMagnitude;

                LogDistance("Jarak Hammer - Ingot", distanceHammerIngotSqr);
                if(distanceHammerIngotSqr < combineDistance * combineDistance)
                {
                    SetCombined(true, CombineType.HammerIngot);
                    return;
                }
            }
        }
        // COMBINED
        else
        {
            if(currentCombineType == CombineType.ApiIngot)
            {
                if(!apiTracked || !ingotTracked)
                {
                    SetCombined(false);
                    return;
                }

                float distanceApiIngotSqr = (targetApi.transform.position - targetIngot.transform.position).sqrMagnitude;

                LogDistance("Jarak Api - Ingot", distanceApiIngotSqr);
                if(distanceApiIngotSqr > separateDistance * separateDistance)
                {
                    SetCombined(false);
                    return;
                }
            }else if(currentCombineType == CombineType.HammerIngot)
            {
                if(!hammerTracked || !ingotTracked)
                {
                    SetCombined(false);
                    return;
                }

                float distanceHammerIngotSqr = (targetHammer.transform.position - targetIngot.transform.position).sqrMagnitude;

                LogDistance("Jarak Hammer - Ingot", distanceHammerIngotSqr);
                if(distanceHammerIngotSqr > separateDistance * separateDistance)
                {
                    SetCombined(false);
                    return;
                }
            }
        }
        
    }

    bool IsTracked(ObserverBehaviour target)
    {
        if (target == null) return false;

        var status = target.TargetStatus.Status;
        return status == Status.TRACKED || status == Status.EXTENDED_TRACKED;
    }

    private void LogDistance(string label, float distanceSquared)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(label + " : " + Mathf.Sqrt(distanceSquared));
#endif
    }

    void SetCombined(bool combined, CombineType combineType = CombineType.None)
    {
        if (combined)
        {
            isCombined = true;
            currentCombineType = combineType;

            if(audioManager != null)
            {
                if(combineType == CombineType.ApiIngot) audioManager.PlayHeatingSFX();
                else if(combineType == CombineType.HammerIngot && ingotSudahPanas && !ingotSudahTempa) audioManager.PlayForgingSFX();
            }
            // State 1 - Api + Ingot
            if(combineType == CombineType.ApiIngot)
            {
                modelApi.SetActive(false);
                modelIngot.SetActive(false);
                modelIngotPanas.SetActive(true);
                
                // Case gabung pertama kali
                if(!ingotSudahPanas) ingotPanasAnimator.Play("IngotPanas", 0, 0f);
                // Case gabung kembali
                else ingotPanasAnimator.Play("heated", 0, 0f);
            }
            // State 2 - Hammer + Ingot
            else if (combineType == CombineType.HammerIngot)
            {
                modelHammer.SetActive(false);
                modelIngot.SetActive(false);
                modelPaluBesi.SetActive(true);
                
                // Case gabung pertama kali
                if(ingotSudahPanas && !ingotSudahTempa) paluBesiAnimator.Play("paluBesi", 0, 0f);
                // Case gabung kembali
                else
                {
                    paluBesiAnimator.Play("forged", 0, 0f);
                    hammerAnimator.Play("hammerIdle", 0, 0f);
                }
            }
        }
        else
        {
            CombineType previousCombineType = currentCombineType;

            isCombined = false;
            currentCombineType = CombineType.None;

            if(previousCombineType == CombineType.ApiIngot)
            {
                modelApi.SetActive(true);
                modelIngot.SetActive(true);
                modelIngotPanas.SetActive(false);
            }else if (previousCombineType == CombineType.HammerIngot)
            {
                modelHammer.SetActive(true);
                modelPaluBesi.SetActive(false);

                if(forgingManager == null || !forgingManager.isSwordComplete) modelIngot.SetActive(true);
                else modelIngot.SetActive(false);
            }
        }
    }

    public string IsCombinedForDebug
    {
        get
        {
            if (!isCombined) return "Tidak ada";
            return currentCombineType.ToString();
        }
    }

    public void FinishHeating()
    {
        ingotSudahPanas = true;
        ingotSudahTempa = false;

        SetIngotColor(warnaPanas);
        if(ingotPanasAnimator != null) ingotPanasAnimator.SetBool("isHeated", true);
    }

    public void FinishForging()
    {
        bool validForge = isCombined && currentCombineType == CombineType.HammerIngot && ingotSudahPanas && !ingotSudahTempa;
        if(validForge && forgingManager != null) forgingManager.RegisterForgeCompleted();

        ingotSudahTempa = true;
        ingotSudahPanas = false;

        SetIngotColor(warnaAsli);
        if(paluBesiAnimator != null) paluBesiAnimator.SetBool("isForged", true);
        if(hammerAnimator != null) hammerAnimator.SetBool("isForged", true);
    }

    private void SetIngotColor(Color color)
    {
        if (ingotRenderer == null) return;

        if (ingotMaterialProperties == null) ingotMaterialProperties = new MaterialPropertyBlock();
        ingotRenderer.GetPropertyBlock(ingotMaterialProperties);
        ingotMaterialProperties.SetColor("_Color", color);
        ingotRenderer.SetPropertyBlock(ingotMaterialProperties);
    }
}
