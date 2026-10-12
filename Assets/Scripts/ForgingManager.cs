using UnityEngine;

public class ForgingManager : MonoBehaviour
{
    [Header("Model")]
    [Tooltip("Visual Model Ingot yang berada di dalam objek PaluBesi")]
    [SerializeField] private Transform forgingIngotVisual;

    [Tooltip("Model Ingot Asli")]
    [SerializeField] private GameObject originalIngotModel;

    [Tooltip("Model pedang")]
    [SerializeField] private GameObject swordModel;

    [Header("Pengaturan Scaling")]
    [SerializeField] private float panjangPerTahap = 0.08f;
    [SerializeField] private float tinggiBerkurangPerTahap = 0.15f;

    [Header("Audio")]
    public AudioManager audioManager;
    private Vector3 initialScale;
    private bool initialScaleSaved;

    [Header("Status")]
    public GameObject statusForging;
    public int ForgingCount { get; private set; } = 0;
    public int MaxForgingCount { get; private set; } = 5;

    public bool isSwordComplete => ForgingCount >= MaxForgingCount;

    private void Start()
    {
        if (audioManager == null) audioManager = AudioManager.Instance;

        if (forgingIngotVisual != null)
        {
            initialScale = forgingIngotVisual.localScale;
            initialScaleSaved = true;
        } else Debug.LogError("ForgingManager: Forging Ingot Visual belum dihubungkan");

        if(swordModel != null) swordModel.SetActive(false);
        else Debug.LogError("ForgingManager: Sword Model belum terhubung");
    }

    public void RegisterForgeCompleted()
    {
        if (isSwordComplete) return;
        ForgingCount++;
        Debug.Log($"Penempaan Selesai: {ForgingCount}/{MaxForgingCount}");

        if(ForgingCount < MaxForgingCount) UpdateIngotShape();
        else CompleteSword();
    }

    private void UpdateIngotShape()
    {
        if(forgingIngotVisual == null || !initialScaleSaved) return;
        
        float lengthMultiplier = 1f + (ForgingCount * panjangPerTahap);
        float heightMultiplier = Mathf.Max(0.2f, 1f - (ForgingCount * tinggiBerkurangPerTahap));

        forgingIngotVisual.localScale = new Vector3 (
            initialScale.x * lengthMultiplier,
            initialScale.y,
            initialScale.z * heightMultiplier
        );
    }

    private void CompleteSword()
    {
        if (originalIngotModel != null) originalIngotModel.SetActive(false);
        if(swordModel != null)
        {
            swordModel.SetActive(true);
            statusForging.SetActive(false);
            if (audioManager != null) audioManager.PlaySwordSFX();
        }

        Debug.Log("Pedang selesai");
    }
}
