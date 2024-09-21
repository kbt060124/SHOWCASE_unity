using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Color normalBackgroundColor; // 完全透過
    [SerializeField] private Color selectedBackgroundColor;
    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color selectedTextColor;

    private Button button;
    private bool isSelected = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        UpdateVisual();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (button != null)
        {
            ColorBlock colors = button.colors;
            Color targetColor = isSelected ? selectedBackgroundColor : normalBackgroundColor;
            
            colors.normalColor = targetColor;
            colors.selectedColor = targetColor;
            colors.pressedColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.disabledColor = targetColor;

            button.colors = colors;

            // ボタンの背景イメージの色を直接設定
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = targetColor;
            }

            //Debug.Log($"Button {gameObject.name} - isSelected: {isSelected}, Color: {targetColor}");
        }

        if (buttonText != null)
        {
            buttonText.color = isSelected ? selectedTextColor : normalTextColor;
        }
    }

    public Button GetButton()
    {
        return button;
    }

    public string CategoryName => gameObject.name;
}
