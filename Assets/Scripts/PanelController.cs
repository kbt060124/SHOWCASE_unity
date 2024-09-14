using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject panel;
    public Button closeButton;
    public Button showButton;

    private bool isPanelVisible = false;

    private void Start()
    {
        closeButton.onClick.AddListener(HidePanel);
        showButton.onClick.AddListener(ShowPanel);
        Debug.Log("PanelController: 初期化完了");

        HidePanel();
    }

    private void HidePanel()
    {
        isPanelVisible = false;
        panel.SetActive(isPanelVisible);
        closeButton.gameObject.SetActive(false);
        showButton.gameObject.SetActive(true);
        Debug.Log("PanelController:" + isPanelVisible);
    }

    private void ShowPanel()
    {
        isPanelVisible = true;
        panel.SetActive(isPanelVisible);
        closeButton.gameObject.SetActive(true);
        showButton.gameObject.SetActive(false);
        Debug.Log("PanelController:" + isPanelVisible);
    }

    public bool IsPanelVisible()
    {
        return isPanelVisible;
    }
}
