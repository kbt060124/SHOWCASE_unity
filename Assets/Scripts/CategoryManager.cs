using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class CategoryButtonManager : MonoBehaviour
{
    [SerializeField] private List<CategoryButton> categoryButtons;

    private void Start()
    {
        foreach (var button in categoryButtons)
        {
            button.GetButton().onClick.AddListener(() => OnCategoryButtonClicked(button));
        }
    }

    private void OnCategoryButtonClicked(CategoryButton clickedButton)
    {
        foreach (var button in categoryButtons)
        {
            button.SetSelected(button == clickedButton);
        }
    }
}