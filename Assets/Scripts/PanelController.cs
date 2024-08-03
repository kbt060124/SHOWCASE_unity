using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject panel;
    public Button closeButton;
    public Button showButton;

    private void Start()
    {
        closeButton.onClick.AddListener(HidePanel);
        showButton.onClick.AddListener(ShowPanel);
    }

    private void HidePanel()
    {
        panel.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    private void ShowPanel()
    {
        panel.SetActive(true);
        closeButton.gameObject.SetActive(true);
    }
}
