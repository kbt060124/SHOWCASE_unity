using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class Importer : MonoBehaviour
{
    public Transform panelTransform;

    private Dictionary<string, Texture2D> thumbnailCache = new Dictionary<string, Texture2D>();
    private Vector3 roomCenter;
    private Vector3 roomSize;
    private string fileName;

    void Start()
    {
        CalculateRoomCenterAndSize();
        LoadThumbnailsFromResources("Items");
    }

    private void CalculateRoomCenterAndSize()
    {
        GameObject wallRight = GameObject.Find("WallRight");
        GameObject wallLeft = GameObject.Find("WallLeft");
        GameObject wallBack = GameObject.Find("WallBack");
        GameObject wallFront = GameObject.Find("WallFront");
        GameObject ceiling = GameObject.Find("Ceiling");
        GameObject floor = GameObject.Find("Floor");

        if (wallRight && wallLeft && wallBack && wallFront && ceiling && floor)
        {
            roomCenter = new Vector3(
                (wallRight.transform.position.x + wallLeft.transform.position.x) / 2f,
                (ceiling.transform.position.y + floor.transform.position.y) / 2f,
                (wallBack.transform.position.z + wallFront.transform.position.z) / 2f
            );
            roomSize = new Vector3(
                Vector3.Distance(wallRight.transform.position, wallLeft.transform.position),
                Vector3.Distance(ceiling.transform.position, floor.transform.position),
                Vector3.Distance(wallBack.transform.position, wallFront.transform.position)
            );
            Debug.Log($"計算されたroomCenter: {roomCenter}");
            Debug.Log($"計算されたroomSize: {roomSize}");
        }
        else
        {
            Debug.LogWarning("部屋のオブジェクトが見つかりません。デフォルト値を使用します。");
            roomCenter = Vector3.zero;
            roomSize = new Vector3(10f, 5f, 10f);
            Debug.Log($"デフォルトのroomSize: {roomSize}");
        }
        Debug.Log($"最終的なroomSize: x={roomSize.x}, y={roomSize.y}, z={roomSize.z}");
    }

    public void LoadThumbnailsFromResources(string folderPath)
    {
        ClearThumbnails();
        LoadThumbnailsFromFolder(folderPath);
    }

    private void ClearThumbnails()
    {
        thumbnailCache.Clear();
        foreach (Transform child in panelTransform.Find("Thumbnails"))
        {
            Destroy(child.gameObject);
        }
    }

    public void LoadThumbnailsFromFolder(string folderPath)
    {
        Texture2D[] thumbnails = Resources.LoadAll<Texture2D>(folderPath);
        foreach (Texture2D thumbnail in thumbnails)
        {
            string prefabPath = $"{folderPath}/{Path.GetFileNameWithoutExtension(thumbnail.name)}";
            CreateThumbnailButton(prefabPath, thumbnail);
        }
    }

    void CreateThumbnailButton(string prefabPath, Texture2D thumbnail)
    {
        if (panelTransform == null)
        {
            Debug.LogError("Panel Transform is not assigned in the Inspector!");
            return;
        }

        // Thumbnailsオブジェクトを探す
        Transform thumbnailsTransform = panelTransform.Find("Thumbnails");
        if (thumbnailsTransform == null)
        {
            Debug.LogError("Thumbnails object not found under Panel!");
            return;
        }

        // サムネイルボタンを動的生成
        GameObject thumbnailObj = new GameObject("ThumbnailButton");
        thumbnailObj.transform.SetParent(thumbnailsTransform, false);

        // RectTransformを追加
        RectTransform rectTransform = thumbnailObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 100);

        // Imageコンポーネントを追加
        Image thumbnailImage = thumbnailObj.AddComponent<Image>();
        thumbnailImage.sprite = Sprite.Create(thumbnail, new Rect(0, 0, thumbnail.width, thumbnail.height), Vector2.zero);

        // Buttonコンポーネントを追加
        Button button = thumbnailObj.AddComponent<Button>();
        button.onClick.AddListener(() => ImportScene(prefabPath));

        // ボタンの色を設定（オプション）
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.grey;
        button.colors = colors;
    }

    protected void ImportScene(string prefabPath)
    {
        this.fileName = prefabPath;

        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Failed to load prefab: {prefabPath}");
            return;
        }

        GameObject parentObject = GameObject.Find("Objects");
        if (parentObject == null)
        {
            parentObject = new GameObject("Objects");
        }

        GameObject itemsFolder = GetOrCreateFolder(parentObject, "Items");
        GameObject shelvesFolder = GetOrCreateFolder(parentObject, "Shelves");

        GameObject instance = Instantiate(prefab);
        instance.name = Path.GetFileNameWithoutExtension(prefabPath);

        if (prefabPath.StartsWith("Shelves/"))
        {
            instance.tag = "Shelf";
            instance.transform.SetParent(shelvesFolder.transform);
        }
        else if (prefabPath.StartsWith("Items/"))
        {
            instance.tag = "Item";
            instance.transform.SetParent(itemsFolder.transform);
        }
        else
        {
            instance.tag = "SceneObject";
            instance.transform.SetParent(parentObject.transform);
        }

        Debug.Log($"タグを付けました: {instance.name} - タグ: {instance.tag} - パス: {prefabPath}");

        PositionObjectInFrontOfCamera(instance);

        Debug.Log($"Successfully added imported object to the scene: {instance.name}");

        PhysicsAssigner physicsAssigner = parentObject.GetComponent<PhysicsAssigner>();
        if (physicsAssigner == null)
        {
            physicsAssigner = parentObject.AddComponent<PhysicsAssigner>();
        }
        physicsAssigner.AddPhysicsToChildren();
    }

    private GameObject GetOrCreateFolder(GameObject parent, string folderName)
    {
        Transform folderTransform = parent.transform.Find(folderName);
        if (folderTransform == null)
        {
            GameObject folder = new GameObject(folderName);
            folder.transform.SetParent(parent.transform);
            return folder;
        }
        return folderTransform.gameObject;
    }

    private void PositionObjectInFrontOfCamera(GameObject obj)
    {
        Bounds bounds = CalculateBounds(obj);
        Vector3 originalScale = obj.transform.localScale;

        float minAllowedScale = roomSize.y * 0.05f;
        float maxAllowedScale = roomSize.y * 0.5f;
        float scaleFactor = Mathf.Min(
            maxAllowedScale / bounds.size.y,
            roomSize.x / (bounds.size.x * 2),
            roomSize.z / (bounds.size.z * 2)
        );

        scaleFactor = Mathf.Max(scaleFactor, minAllowedScale / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z));

        if (obj.CompareTag("Item"))
        {
            scaleFactor *= 0.5f;
        }

        obj.transform.localScale = originalScale * scaleFactor;

        bounds = CalculateBounds(obj);

        float yOffset = bounds.extents.y;
        obj.transform.position = new Vector3(roomCenter.x, roomCenter.y - roomSize.y / 2 + yOffset, roomCenter.z);

        Debug.Log($"オブジェクト '{obj.name}' の最終位置: {obj.transform.position}, スケール: {obj.transform.localScale}, 元のスケール: {originalScale}, 適用されたスケール係数: {scaleFactor}, タグ: {obj.tag}");
    }

    private Bounds CalculateBounds(GameObject obj)
    {
        Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
}
