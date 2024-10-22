using UnityEngine;

public class SideMenuController : MonoBehaviour
{
    public GameObject sideMenuPanel; // サイドメニューのパネル
    public GameObject openButton; // メニューを開くボタン
    public GameObject closeButton; // メニューを閉じるボタン
    public GameObject presetButton; // プリセットボタン

    private bool isOpen = false;

    public void OpenMenu()
    {
        isOpen = true;
        sideMenuPanel.SetActive(true);
        presetButton.SetActive(false);
        openButton.SetActive(false);
        closeButton.SetActive(true);
    }

    public void CloseMenu()
    {
        isOpen = false;
        sideMenuPanel.SetActive(false);
        presetButton.SetActive(true);
        openButton.SetActive(true);
        closeButton.SetActive(false);
    }
}