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
        panelRectTransform.anchoredPosition = new Vector2(0, -150);
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
        thumbnailsGridLayoutGroup.spacing = new Vector2(40, 10);
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
        string[] folderPaths = { "Assets/Files", "Assets/Shelves" };
        foreach (string folderPath in folderPaths)
        {
            LoadThumbnailsFromFolder(folderPath);
        }
    }

    private void LoadThumbnailsFromFolder(string folderPath)
    {
        string[] thumbnailPaths = Directory.GetFiles(folderPath, "thumbnail.png", SearchOption.AllDirectories);
        foreach (string thumbnailPath in thumbnailPaths)
        {
            Texture2D thumbnail = LoadTexture(thumbnailPath);
            if (thumbnail != null)
            {
                string fbxPath = Path.ChangeExtension(thumbnailPath, ".fbx");
                CreateThumbnailButton(fbxPath, thumbnail);
            }
        }
    }

    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            return texture;
        }
        return null;
    }

    private void CreateThumbnailButton(string thumbnailPath, Texture2D thumbnail)
    {
        GameObject thumbnailObj = new GameObject("ThumbnailButton");
        thumbnailObj.transform.SetParent(thumbnailsRectTransform, false);

        RectTransform rectTransform = thumbnailObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 80);

        Image thumbnailImage = thumbnailObj.AddComponent<Image>();
        thumbnailImage.sprite = Sprite.Create(thumbnail, new Rect(0, 0, thumbnail.width, thumbnail.height), Vector2.zero);

        Button button = thumbnailObj.AddComponent<Button>();
        button.onClick.AddListener(() => ImportScene(thumbnailPath));

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.grey;
        button.colors = colors;
    }

    private void ImportScene(string thumbnailPath)
    {
        if (warehouseImporter != null)
        {
            string fbxPath = Path.ChangeExtension(thumbnailPath, ".fbx");
            warehouseImporter.ImportScene(thumbnailPath);
        }
        else
        {
            Debug.LogError("WarehouseImporterが設定されていません。");
        }
    }
}
