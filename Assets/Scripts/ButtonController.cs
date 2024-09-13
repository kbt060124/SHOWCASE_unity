using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public Button resizeButton;
    public Button rotateButton;
    public Button axisButton;
    public Button saveButton;
    public Button deleteButton;

    // ObjectSelected関数を追加
    public void ObjectSelected()
    {
        resizeButton.gameObject.SetActive(true);
        rotateButton.gameObject.SetActive(true);
        axisButton.gameObject.SetActive(true);
        deleteButton.gameObject.SetActive(true);
        saveButton.gameObject.SetActive(false);
    }

    // 新しいObjectDeselected関数を追加
    public void ObjectDeselected()
    {
        resizeButton.gameObject.SetActive(false);
        rotateButton.gameObject.SetActive(false);
        axisButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(true);
    }
}
