using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class WarehousePanelPositioner : MonoBehaviour
{
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private RectTransform thumbnailsRectTransform;
    [SerializeField] private GridLayoutGroup thumbnailsGridLayoutGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private WarehouseImporter warehouseImporter;

    private void Start()
    {
        PositionPanel();
        SetupThumbnailsGrid();
        LoadInitialThumbnails();
    }

    private void OnRectTransformDimensionsChange()
    {
        PositionPanel();
        SetupThumbnailsGrid();
    }

    private void PositionPanel()
    {
        if (panelRectTransform == null)
        {
            Debug.LogError("パネルのRectTransformが設定されていません。");
            return;
        }

        panelRectTransform.anchorMin = new Vector2(0, 0);
        panelRectTransform.anchorMax = new Vector2(1f/3f, 1);
        panelRectTransform.anchoredPosition = new Vector2(100, -150);
        panelRectTransform.sizeDelta = new Vector2(0, -150);

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

        if (titleText != null)
        {
            RectTransform titleRectTransform = titleText.rectTransform;
            titleRectTransform.anchorMin = new Vector2(0, 1);
            titleRectTransform.anchorMax = new Vector2(1, 1);
            titleRectTransform.anchoredPosition = new Vector2(70, -70);
            titleRectTransform.sizeDelta = new Vector2(-50, 50);
        }
        else
        {
            Debug.LogError("タイトルテキストが設定されていません。");
        }
    }

    private void SetupThumbnailsGrid()
    {
        if (thumbnailsGridLayoutGroup == null)
        {
            Debug.LogError("Thumbnails Grid Layout Groupが設定されていません。");
            return;
        }

        thumbnailsGridLayoutGroup.cellSize = new Vector2(80, 80);
        thumbnailsGridLayoutGroup.spacing = new Vector2(40, 40);
        thumbnailsGridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        thumbnailsGridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        thumbnailsGridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
        thumbnailsGridLayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
        thumbnailsGridLayoutGroup.padding = new RectOffset(80, 80, 0, 0);

        if (titleText != null && thumbnailsRectTransform != null)
        {
            RectTransform titleRectTransform = titleText.rectTransform;
            float titleHeight = titleRectTransform.rect.height;
            float topPadding = 50f;

            thumbnailsRectTransform.anchorMin = new Vector2(0, 0);
            thumbnailsRectTransform.anchorMax = new Vector2(1, 1);
            thumbnailsRectTransform.anchoredPosition = new Vector2(0, -titleHeight - topPadding);
            thumbnailsRectTransform.sizeDelta = new Vector2(-40, -(titleHeight + topPadding));
        }
    }

    private void LoadInitialThumbnails()
    {
        string[] folderPaths = { "Items", "Shelves" };
        foreach (string folderPath in folderPaths)
        {
            LoadThumbnailsFromFolder(folderPath);
        }
    }

    private void LoadThumbnailsFromFolder(string folderPath)
    {
        Debug.Log($"フォルダからサムネイルを読み込み中: {folderPath}");
        Object[] assets = Resources.LoadAll(folderPath);
        Debug.Log($"読み込まれたアセット数: {assets.Length}");

        foreach (Object asset in assets)
        {
            if (asset is Texture2D texture)
            {
                string prefabPath = $"{folderPath}/{Path.GetFileNameWithoutExtension(asset.name)}";
                Debug.Log($"サムネイル作成: {asset.name}, プレハブパス: {prefabPath}");
                CreateThumbnailButton(prefabPath, texture);
            }
        }
    }

    private void CreateThumbnailButton(string prefabPath, Texture2D texture)
    {
        GameObject thumbnailObj = new GameObject("ThumbnailButton");
        thumbnailObj.transform.SetParent(thumbnailsRectTransform, false);

        RectTransform rectTransform = thumbnailObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 80);

        Image thumbnailImage = thumbnailObj.AddComponent<Image>();
        thumbnailImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        thumbnailImage.preserveAspect = true;

        Button button = thumbnailObj.AddComponent<Button>();
        button.onClick.AddListener(() => ImportScene(prefabPath));

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.grey;
        button.colors = colors;

        Debug.Log($"サムネイルボタンが作成されました: {prefabPath}");
    }

    private void ImportScene(string prefabPath)
    {
        if (warehouseImporter != null)
        {
            warehouseImporter.ImportScene(prefabPath);
        }
        else
        {
            Debug.LogError("WarehouseImporterが設定されていません。");
        }
    }
}
