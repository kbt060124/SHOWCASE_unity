using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerManager : MonoBehaviour
{
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private float referenceResolutionWidth = 1920f;
    [SerializeField] private float referenceResolutionHeight = 1080f;
    [SerializeField] [Range(0f, 1f)] private float matchWidthOrHeight = 0.5f;

    private void Start()
    {
        SetupCanvasScaler();
    }

    private void SetupCanvasScaler()
    {
        if (canvasScaler == null)
        {
            Debug.LogError("Canvas Scalerが設定されていません。");
            return;
        }

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(referenceResolutionWidth, referenceResolutionHeight);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
    }
}
