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

    private Vector3 roomCenter;
    private Vector3 roomSize;

    private string fileName;

    void Start()
    {
        CalculateRoomCenterAndSize();
        LoadThumbnailsFromFolder("Assets/Files");
    }

    private void CalculateRoomCenterAndSize()
    {
        GameObject wallRight = GameObject.Find("wallRight");
        GameObject wallLeft = GameObject.Find("wallLeft");
        GameObject wallBack = GameObject.Find("wallBack");
        GameObject ceiling = GameObject.Find("ceiling");
        GameObject floor = GameObject.Find("floor");

        if (wallRight && wallLeft && wallBack && ceiling && floor)
        {
            roomCenter = new Vector3(
                (wallRight.transform.position.x + wallLeft.transform.position.x) / 2f,
                (ceiling.transform.position.y + floor.transform.position.y) / 2f,
                (wallBack.transform.position.z + Camera.main.transform.position.z) / 2f
            );
            roomSize = new Vector3(
                Vector3.Distance(wallRight.transform.position, wallLeft.transform.position),
                Vector3.Distance(ceiling.transform.position, floor.transform.position),
                Vector3.Distance(wallBack.transform.position, Camera.main.transform.position)
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
        rectTransform.sizeDelta = new Vector2(100, 100); // サイズを切に調整

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
        this.fileName = fileName;

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
        Bounds bounds = CalculateBounds(obj);
        Vector3 originalScale = obj.transform.localScale;

        // スケーリングの計算
        float minAllowedScale = roomSize.y * 0.05f;
        float maxAllowedScale = roomSize.y * 0.5f;
        float scaleFactor = Mathf.Min(
            maxAllowedScale / bounds.size.y,
            roomSize.x / (bounds.size.x * 2),
            roomSize.z / (bounds.size.z * 2)
        );

        // 最小スケールを設定
        scaleFactor = Mathf.Max(scaleFactor, minAllowedScale / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z));

        // Itemタグがついたオブジェクトの場合、スケール係数をさらに半分にする
        if (obj.CompareTag("Item"))
        {
            scaleFactor *= 0.5f;
        }

        // スケールを適用
        obj.transform.localScale = originalScale * scaleFactor;

        // バウンディングボックスを再計算
        bounds = CalculateBounds(obj);

        // オブジェクトを部屋の中心に配置し、床の上に置く
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

    // public void ImportSV15PBasketball()
    // {
    //     Debug.Log("ImportSV15PBasketball called"); // Added debug log
    //     string fileName = "Assets/Files/SV-15P Basketball.fbx"; // Import source path and destination path are the same
    //     ImportScene(fileName);
    // }
}
