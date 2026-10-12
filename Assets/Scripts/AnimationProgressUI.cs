using UnityEngine;
using UnityEngine.UI;

public class AnimationProgressUI : MonoBehaviour
{
    [Header("References")]
    public Image progressFill;
    public Animator heatingAnimator;
    public Animator forgingAnimator;
    public Camera arCamera;

    [Header("Animator State Names")]
    public string heatingStateName = "ingotPanas";
    public string forgingStateName = "paluBesi";

    private float displayedProgress = -1f;

    private void Start()
    {
        if(arCamera == null) arCamera = Camera.main;
        SetProgress(0f);
    }

    private void LateUpdate() {
        FaceCamera();

        float progress;
        bool isAnimating = TryGetProgress(heatingAnimator, heatingStateName, out progress) || TryGetProgress(forgingAnimator, forgingStateName, out progress);

        SetProgress(isAnimating ? progress : 0f);
    }

    private bool TryGetProgress(Animator animator, string stateName, out float progress)
    {
        progress = 0f;

        if(animator == null || !animator.gameObject.activeInHierarchy) return false;
        var state = animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName(stateName))
        {
            progress = Mathf.Clamp01(state.normalizedTime);
            return true;
        }

        if (animator.IsInTransition(0))
        {
            var nextState = animator.GetNextAnimatorStateInfo(0);

            if (nextState.IsName(stateName))
            {
                progress = Mathf.Clamp01(nextState.normalizedTime);
                return true;
            }
        }

        return false;
    }

    private void SetProgress(float value)
    {
        if(progressFill == null || Mathf.Approximately(displayedProgress, value)) return;

        progressFill.fillAmount = value;
        displayedProgress = value;
    }

    private void FaceCamera()
    {
        if(arCamera == null) return;

        Vector3 direction = arCamera.transform.position - transform.position;

        if(direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direction, arCamera.transform.up);
    }
}
