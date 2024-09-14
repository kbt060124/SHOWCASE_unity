using UnityEngine;

public class SideMenuController : MonoBehaviour
{
    public GameObject sideMenuPanel; // サイドメニューのパネル
    public GameObject openButton; // メニューを開くボタン
    public GameObject closeButton; // メニューを閉じるボタン

    private bool isOpen = false;

    public void OpenMenu()
    {
        isOpen = true;
        sideMenuPanel.SetActive(true);
        openButton.SetActive(false);
        closeButton.SetActive(true);
    }

    public void CloseMenu()
    {
        isOpen = false;
        sideMenuPanel.SetActive(false);
        openButton.SetActive(true);
        closeButton.SetActive(false);
    }
}