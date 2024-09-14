using UnityEngine;
using System.IO;
using UnityEditor;
using System;

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
        ImportFirstThumbnail();
    }

    private void ImportFirstThumbnail()
    {
        string[] folderPaths = { "Assets/Files", "Assets/Shelves" };
        foreach (string folderPath in folderPaths)
        {
            string[] thumbnailFiles = Directory.GetFiles(folderPath, "thumbnail.png", SearchOption.AllDirectories);
            if (thumbnailFiles.Length > 0)
            {
                ImportScene(thumbnailFiles[0]);
                return;
            }
        }
        Debug.LogWarning("サムネイルが見つかりません。");
    }

    public void ImportScene(string thumbnailPath)
    {
        selectedThumbnailPath = thumbnailPath;
        
        // 既存のオブジェクトを削除
        GameObject existingObject = GameObject.Find("Objects");
        if (existingObject != null)
        {
            DestroyImmediate(existingObject);
        }

        if (string.IsNullOrEmpty(thumbnailPath))
        {
            Debug.LogError("無効なサムネイルパスです。");
            return;
        }

        string directoryPath = Path.GetDirectoryName(thumbnailPath);
        string[] fbxFiles = Directory.GetFiles(directoryPath, "*.fbx");

        if (fbxFiles.Length == 0)
        {
            Debug.LogError($"FBXファイルが見つかりません: {directoryPath}");
            return;
        }

        string fbxPath = fbxFiles[0]; // 最初に見つかったFBXファイルを使用

        GameObject prefab = CreatePrefabFromFBX(fbxPath);
        if (prefab == null)
        {
            Debug.LogError($"プレハブの作成に失敗しました: {fbxPath}");
            return;
        }

        GameObject instance = Instantiate(prefab);
        instance.name = prefab.name;

        // フォルダ名（数字）を取得
        string folderName = new DirectoryInfo(directoryPath).Name;

        SetupImportedObject(instance, fbxPath, folderName);

        // ItemPanelのThumbnailを更新
        UpdateItemPanelThumbnail();

        // ObjectInfoManagerを使用してオブジェクト情報を取得
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

    private GameObject CreatePrefabFromFBX(string fbxPath)
    {
        AssetDatabase.Refresh();
        GameObject importedObject = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (importedObject == null)
        {
            return null;
        }

        string prefabName = Path.GetFileNameWithoutExtension(fbxPath) + ".prefab";
        string prefabPath = fbxPath.StartsWith("Assets/Files") 
            ? "Assets/Resources/Items/" + prefabName 
            : "Assets/Resources/Shelves/" + prefabName;

        Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(importedObject, prefabPath);
        return prefab;
    }

    private void SetupImportedObject(GameObject obj, string fbxPath, string folderName)
    {
        GameObject parentObject = GameObject.Find("Objects") ?? new GameObject("Objects");

        GameObject folder = fbxPath.StartsWith("Assets/Shelves") 
            ? GetOrCreateFolder(parentObject, "Shelves") 
            : GetOrCreateFolder(parentObject, "Items");

        obj.transform.SetParent(folder.transform);
        obj.tag = fbxPath.StartsWith("Assets/Shelves") ? "Shelf" : "Item";

        // ObjectFolderInfoコンポーネントを追加し、フォルダ名を設定
        ObjectFolderInfo folderInfo = obj.AddComponent<ObjectFolderInfo>();
        folderInfo.folderName = folderName;

        obj.name = Path.GetFileNameWithoutExtension(fbxPath);

        ApplyTextureToObject(obj, fbxPath);
        AddPhysicsToObject(obj);
        AdjustObjectSize(obj);

        // WarehouseObjectRotatorに新しいオブジェクトを通知
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

    private void ApplyTextureToObject(GameObject obj, string fbxPath)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            string texturePath = Path.ChangeExtension(fbxPath, ".png");
            Texture2D texture = LoadTexture(texturePath);
            if (texture != null)
            {
                renderer.material.mainTexture = texture;
            }
        }
    }

    private Texture2D LoadTexture(string filePath)
    {
        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                return texture;
            }
        }
        return null;
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

        // オブジェクトのバウンディングボックスを取得
        Bounds bounds = CalculateBounds(obj);

        // 画面心位置を計算
        Vector3 screenCenter = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, mainCamera.nearClipPlane));

        // 画面の高さの0.8倍と幅の1/3を計算（高さの制限を少し厳しくする）
        float maxHeight = mainCamera.orthographicSize * 1.6f;
        float maxWidth = mainCamera.orthographicSize * mainCamera.aspect / 3f;

        // スケール係数を計算（高さと幅の制限を考慮）
        float scaleFactor = Mathf.Min(
            maxHeight / bounds.size.y,
            maxWidth / bounds.size.x,
            maxWidth / bounds.size.z
        );

        // オブジェクトのスケールを調整
        Vector3 originalScale = obj.transform.localScale;
        obj.transform.localScale = originalScale * scaleFactor;

        // バウンディングボックスを再計算
        bounds = CalculateBounds(obj);

        // オブジェクトの位置を画面中央に設定
        Vector3 targetPosition = screenCenter + mainCamera.transform.forward * 5f; // カメラの5単位前方

        // オブジェクトの中心が画面中央に来るように位置を調整
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
            string thumbnailPath = Path.ChangeExtension(selectedThumbnailPath, ".png");
            itemPanel.UpdateThumbnail(thumbnailPath);
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
