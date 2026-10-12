using UnityEngine;

public class ingotAnimationEvent : MonoBehaviour {
    public CombineManager combineManager;
    public void FinishHeating()
    {
        combineManager.FinishHeating();
    }

    public void FinishForging()
    {
        combineManager.FinishForging();
    }
}