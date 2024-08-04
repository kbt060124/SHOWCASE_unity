using UnityEngine;

public class PanelPositioner : MonoBehaviour
{
    [SerializeField] private RectTransform panelRectTransform;

    private void Start()
    {
        PositionPanel();
    }

    private void OnRectTransformDimensionsChange()
    {
        PositionPanel();
    }

    private void PositionPanel()
    {
        if (panelRectTransform == null)
        {
            Debug.LogError("パネルのRectTransformが設定されていません。");
            return;
        }

        // パネルを画面の下1/3に配置
        panelRectTransform.anchorMin = new Vector2(0, 0);
        panelRectTransform.anchorMax = new Vector2(1, 1f/3f);
        panelRectTransform.anchoredPosition = Vector2.zero;
        panelRectTransform.sizeDelta = Vector2.zero;
    }
}