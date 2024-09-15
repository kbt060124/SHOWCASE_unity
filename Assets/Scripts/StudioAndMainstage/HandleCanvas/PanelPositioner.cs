using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;

public class PanelPositioner : MonoBehaviour
{
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private RectTransform closeButtonRectTransform;
    [SerializeField] private Transform thumbnailsParent;
    [SerializeField] private RectTransform thumbnailsRectTransform;
    [SerializeField] private GridLayoutGroup thumbnailsGridLayoutGroup;
    [SerializeField] private RectTransform categoryButtonsContainer;
    [SerializeField] private Vector2 categoryButtonSize = new Vector2(100, 40); // デフォルトサイズ

    private Dictionary<string, string> categoryPaths = new Dictionary<string, string>
    {
        { "アイテム", "Assets/Files" },
        { "棚", "Assets/Shelves" }
    };

    private void Start()
    {
        PositionPanel();
        PositionCategoryButtons();
        SetupThumbnailsGrid();
        PositionCloseButton();
        SetupCategoryButtons();
    }

    private void OnRectTransformDimensionsChange()
    {
        PositionPanel();
        PositionCategoryButtons();
        SetupThumbnailsGrid();
        PositionCloseButton();
    }

    private void PositionPanel()
    {
        if (panelRectTransform == null)
        {
            Debug.LogError("パネルのRectTransformが設定されていません。");
            return;
        }

        // パネルをWarehouseオブジェクト内で全幅に広げ、下1/3に配置する
        panelRectTransform.anchorMin = new Vector2(0, 0);
        panelRectTransform.anchorMax = new Vector2(1, 1f/4f);
        panelRectTransform.anchoredPosition = Vector2.zero;
        panelRectTransform.sizeDelta = Vector2.zero;

        // Warehouseオブジェクトのサイズを画面全体に合わせる
        RectTransform warehouseRectTransform = panelRectTransform.parent as RectTransform;
        if (warehouseRectTransform != null)
        {
            warehouseRectTransform.anchorMin = Vector2.zero;
            warehouseRectTransform.anchorMax = Vector2.one;
            warehouseRectTransform.anchoredPosition = Vector2.zero;
            warehouseRectTransform.sizeDelta = Vector2.zero;
        }
        else
        {
            Debug.LogError("Warehouseオブジェクトが見つかりません。");
        }
    }

    private void SetupThumbnailsGrid()
    {
        if (thumbnailsGridLayoutGroup == null)
        {
            Debug.LogError("Thumbnails Grid Layout Groupが設定されていません。");
            return;
        }

        // グリッドのセルサイズを設定
        thumbnailsGridLayoutGroup.cellSize = new Vector2(80, 80); // サムネイルのサイズに応じて調整

        // グリッドの間隔を設定
        thumbnailsGridLayoutGroup.spacing = new Vector2(40, 10);

        // グリッドの開始位置を設定
        thumbnailsGridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        thumbnailsGridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;

        // グリッドの配置を設定
        thumbnailsGridLayoutGroup.childAlignment = TextAnchor.MiddleCenter;

        // グリッドの制約を設定（幅に合せて折り返す）
        thumbnailsGridLayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;

        // Thumbnailsオブジェクトのサイズと位置を設定
        if (thumbnailsRectTransform != null)
        {
            thumbnailsRectTransform.anchorMin = new Vector2(0, 0);
            thumbnailsRectTransform.anchorMax = new Vector2(1, 1);
            thumbnailsRectTransform.anchoredPosition = Vector2.zero;
            thumbnailsRectTransform.sizeDelta = Vector2.zero;
        }

        // サムネイルの親オブジェクトを設定
        if (thumbnailsParent == null)
        {
            Debug.LogError("サムネイルの親オブジェクトが設定されていません。");
            return;
        }

        // サムネイルボタンを生成する際の親オブジェクトを設定
        thumbnailsGridLayoutGroup.transform.SetParent(thumbnailsParent, false);
    }

    private void PositionCloseButton()
    {
        if (closeButtonRectTransform == null || panelRectTransform == null)
        {
            Debug.LogError("閉じるボタンまたはパネルのRectTransformが設定されていません。");
            return;
        }

        // ボタンをグリッドレイアウトから完全に除外
        GridLayoutGroup gridLayout = closeButtonRectTransform.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            DestroyImmediate(gridLayout);
        }

        LayoutElement layoutElement = closeButtonRectTransform.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = closeButtonRectTransform.gameObject.AddComponent<LayoutElement>();
        }
        layoutElement.ignoreLayout = true;

        // ボタンの設定
        closeButtonRectTransform.anchorMin = Vector2.one;
        closeButtonRectTransform.anchorMax = Vector2.one;
        closeButtonRectTransform.pivot = Vector2.one;

        // ボタンの位置を設定（右上に配置し、上と右に余白を設定）
        closeButtonRectTransform.anchoredPosition = new Vector2(-20, -20);

        // ボタンを最前面に表示
        closeButtonRectTransform.SetAsLastSibling();

        // 強制的にレイアウトを更新
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
    }

    private void PositionCategoryButtons()
    {
        if (categoryButtonsContainer == null)
        {
            Debug.LogError("カテゴリボタンのコンテナが設定されていません。");
            return;
        }

        // カテゴリボタンコンテナをパネルの左上に配置
        categoryButtonsContainer.anchorMin = new Vector2(0, 1);
        categoryButtonsContainer.anchorMax = new Vector2(0, 1);
        categoryButtonsContainer.pivot = new Vector2(0, 1);
        categoryButtonsContainer.anchoredPosition = new Vector2(100, -30); // 左上からの余白

        // カテゴリボタンのサイズを設定
        foreach (RectTransform buttonRect in categoryButtonsContainer)
        {
            buttonRect.sizeDelta = categoryButtonSize;
        }

        // Horizontal Layout Groupの設定を調整
        HorizontalLayoutGroup layoutGroup = categoryButtonsContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
        }

        // コンテナのサイズを自動調整
        LayoutRebuilder.ForceRebuildLayoutImmediate(categoryButtonsContainer);
    }

    private void SetupCategoryButtons()
    {
        foreach (Transform child in categoryButtonsContainer)
        {
            CategoryButton categoryButton = child.gameObject.AddComponent<CategoryButton>();
            // 必要に応じて追加のセットアップを行う
        }
    }
}