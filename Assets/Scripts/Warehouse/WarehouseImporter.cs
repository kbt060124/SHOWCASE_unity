using UnityEngine;
using System.IO;
using System.Linq;

public class WarehouseImporter : MonoBehaviour
{
    [SerializeField] private WarehouseObjectRotator objectRotator;
    [SerializeField] private ItemPanel itemPanel;

    private string selectedThumbnailPath;

    public string GetSelectedThumbnailPath()
    {
        return selectedThumbnailPath;
    }

    private void Start()
    {
        if (itemPanel == null)
        {
            itemPanel = FindObjectOfType<ItemPanel>();
            if (itemPanel == null)
            {
                Debug.LogError("ItemPanelが見つかりません。シーンにItemPanelを追加してください。");
            }
        }
    }

    public void ImportScene(string thumbnailPath)
    {
        Debug.Log($"ImportScene called with thumbnailPath: {thumbnailPath}");

        selectedThumbnailPath = thumbnailPath;
        
        if (string.IsNullOrEmpty(thumbnailPath))
        {
            Debug.LogError("無効なサムネイルパスです。");
            return;
        }

        string[] pathParts = thumbnailPath.Split('/');
        string category = pathParts[0]; // "Items" または "Shelves"
        string folderName = pathParts[1]; // 数字のフォルダ名

        Debug.Log($"Category: {category}, FolderName: {folderName}");

        GameObject parentObject = GameObject.Find("Objects");
        if (parentObject == null)
        {
            parentObject = new GameObject("Objects");
        }

        // 両方のカテゴリフォルダ内の既存のプレハブインスタンスを削除
        ClearCategoryFolder(parentObject, "Items");
        ClearCategoryFolder(parentObject, "Shelves");

        // カテゴリフォルダを取得または作成
        GameObject categoryFolder = GetOrCreateFolder(parentObject, category);

        // フォルダ内のすべてのアセットを取得
        UnityEngine.Object[] assets = Resources.LoadAll($"{category}/{folderName}");
        Debug.Log($"Loaded assets count: {assets.Length}");
        
        // プレハブを見つける
        GameObject prefab = null;
        foreach (UnityEngine.Object asset in assets)
        {
            GameObject go = asset as GameObject;
            if (go != null && IsPrefabLike(go))
            {
                prefab = go;
                Debug.Log($"Found prefab: {go.name}");
                break;
            }
        }

        if (prefab == null)
        {
            Debug.LogError($"プレハブが見つかりません: {category}/{folderName}");
            return;
        }

        GameObject instance = Instantiate(prefab, categoryFolder.transform);
        instance.name = prefab.name;
        instance.tag = category == "Shelves" ? "Shelf" : "Item";

        Debug.Log($"タグを付けました: {instance.name} - タグ: {instance.tag} - パス: {category}/{folderName}/{prefab.name}");

        SetupImportedObject(instance, prefab.name, folderName);

        UpdateItemPanelThumbnail();

        ObjectInfo objectInfo = ObjectInfoManager.GetObjectInfoByFolderName(folderName);
        if (objectInfo != null)
        {
            UpdateItemPanel(objectInfo);
        }
        else
        {
            UpdateItemPanel(null);
        }
    }

    private void ClearCategoryFolder(GameObject parent, string categoryName)
    {
        Transform categoryTransform = parent.transform.Find(categoryName);
        if (categoryTransform != null)
        {
            foreach (Transform child in categoryTransform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private bool IsPrefabLike(GameObject go)
    {
        return go.name != "thumbnail" && 
               (go.transform.childCount > 0 || 
                go.GetComponent<MeshRenderer>() != null || 
                go.GetComponent<SkinnedMeshRenderer>() != null);
    }

    private void SetupImportedObject(GameObject obj, string prefabName, string folderName)
    {
        GameObject parentObject = GameObject.Find("Objects") ?? new GameObject("Objects");

        GameObject folder = prefabName.StartsWith("Shelf") 
            ? GetOrCreateFolder(parentObject, "Shelves") 
            : GetOrCreateFolder(parentObject, "Items");

        obj.transform.SetParent(folder.transform);
        obj.tag = prefabName.StartsWith("Shelf") ? "Shelf" : "Item";

        ObjectFolderInfo folderInfo = obj.AddComponent<ObjectFolderInfo>();
        folderInfo.folderName = folderName;

        obj.name = prefabName;

        ApplyTextureToObject(obj, prefabName);
        AddPhysicsToObject(obj);
        AdjustObjectSize(obj);

        if (objectRotator != null)
        {
            objectRotator.SetTargetObject(obj);
        }
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

    private void ApplyTextureToObject(GameObject obj, string prefabName)
    {
        string texturePath = prefabName.StartsWith("Shelf") 
            ? $"Shelves/{obj.name}/texture" 
            : $"Items/{obj.name}/texture";

        Texture2D texture = Resources.Load<Texture2D>(texturePath);
        if (texture != null)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.mainTexture = texture;
            }
            Debug.Log("Applied texture to imported object: " + obj.name);
        }
        else
        {
            Debug.LogWarning("Texture file not found for: " + obj.name);
        }
    }

    private void AddPhysicsToObject(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        Collider collider = obj.GetComponent<Collider>();
        if (collider == null)
        {
            MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
            meshCollider.convex = true;
        }
    }

    private void AdjustObjectSize(GameObject obj)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("メインカメラが見つかりません。");
            return;
        }

        Bounds bounds = CalculateBounds(obj);

        Vector3 screenCenter = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, mainCamera.nearClipPlane));

        float maxHeight = mainCamera.orthographicSize * 1.6f;
        float maxWidth = mainCamera.orthographicSize * mainCamera.aspect / 3f;

        float scaleFactor = Mathf.Min(
            maxHeight / bounds.size.y,
            maxWidth / bounds.size.x,
            maxWidth / bounds.size.z
        );

        Vector3 originalScale = obj.transform.localScale;
        obj.transform.localScale = originalScale * scaleFactor;

        bounds = CalculateBounds(obj);

        Vector3 targetPosition = screenCenter + mainCamera.transform.forward * 5f;

        Vector3 offset = bounds.center - obj.transform.position;
        obj.transform.position = targetPosition - offset;
    }

    private Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(obj.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private void UpdateItemPanelThumbnail()
    {
        if (itemPanel != null)
        {
            string thumbnailPath = selectedThumbnailPath.Replace("Assets/Resources/", "").Replace(".png", "");
            Texture2D thumbnail = Resources.Load<Texture2D>(thumbnailPath);
            if (thumbnail != null)
            {
                itemPanel.UpdateThumbnail(thumbnail);
            }
            else
            {
                Debug.LogWarning($"サムネイルの読み込みに失敗しました: {thumbnailPath}");
            }
        }
    }

    private void UpdateItemPanel(ObjectInfo info)
    {
        if (itemPanel != null)
        {
            itemPanel.UpdateInfo(info);
        }
    }
}
