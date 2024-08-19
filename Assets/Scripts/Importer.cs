using Autodesk.Fbx;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class Importer : MonoBehaviour
{

    public Transform panelTransform; // サムネイルを表示するパネルのTransform

    private Dictionary<string, Texture2D> thumbnailCache = new Dictionary<string, Texture2D>();

    void Start()
    {
        LoadThumbnailsFromFolder("Assets/Files");
    }

    public void LoadThumbnailsFromFolder(string folderPath)
    {
        ClearThumbnails();
        string[] fbxFolders = Directory.GetDirectories(folderPath);
        foreach (string subFolderPath in fbxFolders)
        {
            string thumbnailPath = Path.Combine(subFolderPath, "thumbnail.png");
            if (File.Exists(thumbnailPath))
            {
                Texture2D thumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);
                if (thumbnail != null)
                {
                    string fbxPath = Directory.GetFiles(subFolderPath, "*.fbx").FirstOrDefault();
                    if (fbxPath != null)
                    {
                        thumbnailCache[fbxPath] = thumbnail;
                        CreateThumbnailButton(fbxPath, thumbnail);
                        Debug.Log($"Created thumbnail for: {fbxPath}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Thumbnail not found in: {subFolderPath}");
            }
        }
    }

    private void ClearThumbnails()
    {
        thumbnailCache.Clear();
        foreach (Transform child in panelTransform.Find("Thumbnails"))
        {
            Destroy(child.gameObject);
        }
    }

    void CreateThumbnailButton(string fbxPath, Texture2D thumbnail)
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
        rectTransform.sizeDelta = new Vector2(100, 100); // サイズを適切に調整

        // Imageコンポーネントを追加
        Image thumbnailImage = thumbnailObj.AddComponent<Image>();
        thumbnailImage.sprite = Sprite.Create(thumbnail, new Rect(0, 0, thumbnail.width, thumbnail.height), Vector2.zero);

        // Buttonコンポーネントを追加
        Button button = thumbnailObj.AddComponent<Button>();
        button.onClick.AddListener(() => ImportScene(fbxPath));

        // ボタンの色を設定（オプション）
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.grey;
        button.colors = colors;
    }

    protected void ImportScene(string fileName)
    {
        using (FbxManager fbxManager = FbxManager.Create())
        {
            // configure IO settings.
            fbxManager.SetIOSettings(FbxIOSettings.Create(fbxManager, Globals.IOSROOT));

            // Import the scene to make sure file is valid
            using (FbxImporter importer = FbxImporter.Create(fbxManager, "myImporter"))
            {
                // Initialize the importer.
                bool status = importer.Initialize(fileName, -1, fbxManager.GetIOSettings());
                if (!status)
                {
                    Debug.LogError("Failed to initialize importer with file: " + fileName);
                    return;
                }

                // Create a new scene so it can be populated by the imported file.
                FbxScene scene = FbxScene.Create(fbxManager, "myScene");

                // Import the contents of the file into the scene.
                if (!importer.Import(scene))
                {
                    Debug.LogError("Failed to import scene from file: " + fileName);
                    return;
                }

                // Export the scene to the same path
                using (FbxExporter exporter = FbxExporter.Create(fbxManager, "myExporter"))
                {
                    if (!exporter.Initialize(fileName, -1, fbxManager.GetIOSettings()))
                    {
                        Debug.LogError("Failed to initialize exporter with path: " + fileName);
                        return;
                    }

                    if (!exporter.Export(scene))
                    {
                        Debug.LogError("Failed to export scene to path: " + fileName);
                    }
                    else
                    {
                        Debug.Log("Successfully exported scene to path: " + fileName);
                    }
                }
            }
        }

        // FBXファイルをインポートしてPrefabを生成
        AssetDatabase.Refresh();
        GameObject importedObject = AssetDatabase.LoadAssetAtPath<GameObject>(fileName);
        if (importedObject != null)
        {
            // Prefabの保存先パスを設定
            string prefabName = Path.GetFileNameWithoutExtension(fileName) + ".prefab";
            string prefabPath = "Assets/Resources/" + prefabName;

            // Prefabを生成
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(importedObject, prefabPath);
            if (prefab != null)
            {
                // "Objects"という名前のEmpty GameObjectを探すか、なければ新しく作成する
                GameObject parentObject = GameObject.Find("Objects");
                if (parentObject == null)
                {
                    parentObject = new GameObject("Objects");
                }

                // 生成したPrefabを元にGameObjectをインスタンス化
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parentObject.transform) as GameObject;
                instance.name = Path.GetFileNameWithoutExtension(fileName);

                // タグを付ける
                if (fileName.StartsWith("Assets/Shelves"))
                {
                    instance.tag = "Shelf";
                }
                else if (fileName.StartsWith("Assets/Files"))
                {
                    instance.tag = "Item";
                }
                else
                {
                    instance.tag = "SceneObject";
                }

                Debug.Log($"タグを付けました: {instance.name} - タグ: {instance.tag} - パス: {fileName}");

                // オブジェクトの位置を調整
                PositionObjectInFrontOfCamera(instance);

                Debug.Log("Successfully added imported object to the scene: " + instance.name);

                // テクスチャを適用する
                ApplyTextureToObject(instance, fileName);

                // PhysicsAssignerを使用して物理判定を付与する
                PhysicsAssigner physicsAssigner = parentObject.AddComponent<PhysicsAssigner>();
                physicsAssigner.AddPhysicsToChildren();
            }
            else
            {
                Debug.LogError("Failed to create prefab from imported object: " + fileName);
            }
        }
        else
        {
            Debug.LogError("Failed to load imported object from path: " + fileName);
        }
    }

    // テクスチャ適用のためのヘルパーメソッド
    private void ApplyTextureToObject(GameObject obj, string fbxPath)
    {
        string fbmFolderPath = Path.ChangeExtension(fbxPath, "fbm");
        string texturePath = Path.Combine(fbmFolderPath, "texture");
        if (Directory.Exists(fbmFolderPath) && File.Exists(texturePath))
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
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
                Debug.LogWarning("Texture file found but could not be loaded: " + texturePath);
            }
        }
        else
        {
            Debug.LogWarning("Texture file not found for: " + obj.name);
        }
    }

    private void PositionObjectInFrontOfCamera(GameObject obj)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // オブジェクトの境界ボックスを取得
            Bounds bounds = CalculateBounds(obj);

            // カメラの前方4ユニットの位置を計算
            Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * 3f;

            // オブジェクトを新しい位置に移動
            obj.transform.position = targetPosition;

            // オブジェクトをY軸方向に少し上げる（高さを低くするために係数を0.5fに変更）
            obj.transform.position += Vector3.up * (bounds.extents.y * 0.3f);
        }
        else
        {
            Debug.LogWarning("メインカメラが見つかりません。オブジェクトの位置を調整できません。");
        }
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

    // public void ImportSV15PBasketball()
    // {
    //     Debug.Log("ImportSV15PBasketball called"); // Added debug log
    //     string fileName = "Assets/Files/SV-15P Basketball.fbx"; // Import source path and destination path are the same
    //     ImportScene(fileName);
    // }
}
