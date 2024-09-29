using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using System.Linq;

[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class SerializableQuaternion
{
    public float x, y, z, w;

    public SerializableQuaternion(Quaternion quaternion)
    {
        x = quaternion.x;
        y = quaternion.y;
        z = quaternion.z;
        w = quaternion.w;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }
}

[System.Serializable]
public class ObjectData
{
    public string prefabPath;
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 scale;
    public int parentIndex;
    public string texturePath;
    public string mtlPath;
}

[System.Serializable]
public class SceneData
{
    public List<ObjectData> objects = new List<ObjectData>();
}

public class SaveManager : MonoBehaviour
{
    private const string SAVE_FILE_NAME = "scene_data.json";
    [SerializeField] private GameObject saveCompletedPopup;
    [SerializeField] private Canvas studioCanvas;
    private CanvasGroup popupCanvasGroup;

    private void Start()
    {
        // ポップアップのCanvasGroupを取得
        popupCanvasGroup = saveCompletedPopup.GetComponent<CanvasGroup>();
        if (popupCanvasGroup == null)
        {
            popupCanvasGroup = saveCompletedPopup.AddComponent<CanvasGroup>();
        }
        saveCompletedPopup.SetActive(false);
    }

    public void SaveScene()
    {
        try
        {
            SceneData sceneData = new SceneData();
            GameObject objectsContainer = GameObject.Find("Objects");

            if (objectsContainer != null)
            {
                SaveObjectRecursively(objectsContainer, sceneData);
            }
            else
            {
                Debug.LogWarning("'Objects'コンテナが見つかりません。");
            }

            string json = JsonUtility.ToJson(sceneData, true);
            string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            File.WriteAllText(savePath, json);

            Debug.Log($"シーンが保存されました: {savePath}");
            ShowSaveCompletedPopup();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"シーンの保存中にエラーが発生しました: {e.Message}");
            // エラーが発生した場合、ここでエラーメッセージを表示するなどの処理を追加できます
        }
    }

    private void SaveObjectRecursively(GameObject obj, SceneData sceneData, int parentIndex = -1)
    {
        if (obj.name == "Items" || obj.name == "Shelves")
        {
            int currentIndex = parentIndex;
            foreach (Transform child in obj.transform)
            {
                SaveObjectRecursively(child.gameObject, sceneData, currentIndex);
            }
            return;
        }

        string prefabPath = GetPrefabPath(obj);

        // プレハブパスが有効な場合のみオブジェクトを保存
        if (!string.IsNullOrEmpty(prefabPath))
        {
            ObjectData objData = new ObjectData
            {
                prefabPath = prefabPath,
                position = new SerializableVector3(obj.transform.localPosition),
                rotation = new SerializableQuaternion(obj.transform.localRotation),
                scale = new SerializableVector3(obj.transform.localScale),
                parentIndex = parentIndex,
                texturePath = GetTexturePath(obj),
                mtlPath = GetMtlPath(obj)
            };
            int currentIndex = sceneData.objects.Count;
            sceneData.objects.Add(objData);

            foreach (Transform child in obj.transform)
            {
                SaveObjectRecursively(child.gameObject, sceneData, currentIndex);
            }
        }
    }

    private string GetPrefabPath(GameObject obj)
    {
        // オブジェクト名から(Clone)を削除
        string objName = obj.name.Replace("(Clone)", "");

        // タグに基づいてパスを設定
        if (obj.CompareTag("Shelf"))
        {
            return "Shelves/" + objName;
        }
        else if (obj.CompareTag("Item"))
        {
            return "Items/" + objName;
        }
        else
        {
            // タグが一致しない場合は空文字列を返す
            return "";
        }
    }

    private string GetTexturePath(GameObject obj)
    {
        string objName = obj.name.Replace("(Clone)", "");
        return objName;
    }

    private string GetMtlPath(GameObject obj)
    {
        string objName = obj.name.Replace("(Clone)", "");
        return objName;
    }

    public void LoadScene()
    {
        // 既存のオブジェクトをクリア
        GameObject existingObjects = GameObject.Find("Objects");
        if (existingObjects != null)
        {
            DestroyImmediate(existingObjects);
        }

        // 新しいObjectsコンテナを作成
        existingObjects = new GameObject("Objects");

        string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        if (!File.Exists(savePath))
        {
            Debug.Log("保存されたシーンデータがありません。");
            return;
        }

        string json = File.ReadAllText(savePath);
        SceneData sceneData = JsonUtility.FromJson<SceneData>(json);

        GameObject itemsFolder = GetOrCreateFolder(existingObjects, "Items");
        GameObject shelvesFolder = GetOrCreateFolder(existingObjects, "Shelves");

        List<GameObject> instantiatedObjects = new List<GameObject>();

        foreach (ObjectData objData in sceneData.objects)
        {
            GameObject prefab = Resources.Load<GameObject>(objData.prefabPath);
            if (prefab != null)
            {
                GameObject newObj = Instantiate(prefab);
                newObj.name = Path.GetFileName(objData.prefabPath);
                newObj.transform.localPosition = objData.position.ToVector3();
                newObj.transform.localRotation = objData.rotation.ToQuaternion();
                newObj.transform.localScale = objData.scale.ToVector3();

                // レイヤーをDefaultに設定
                newObj.layer = LayerMask.NameToLayer("Default");

                if (objData.prefabPath.StartsWith("Items/"))
                {
                    newObj.tag = "Item";
                    newObj.transform.SetParent(itemsFolder.transform);
                }
                else if (objData.prefabPath.StartsWith("Shelves/"))
                {
                    newObj.tag = "Shelf";
                    newObj.transform.SetParent(shelvesFolder.transform);
                }
                else
                {
                    newObj.tag = "SceneObject";
                    newObj.transform.SetParent(existingObjects.transform);
                }

                ApplyTextureAndMaterialToObject(newObj, objData.texturePath, objData.mtlPath);

                if (newObj.GetComponent<Collider>() == null)
                {
                    newObj.AddComponent<BoxCollider>();
                }

                instantiatedObjects.Add(newObj);
            }
            else
            {
                Debug.LogWarning($"プレハブが見つかりません: {objData.prefabPath}");
                instantiatedObjects.Add(null);
            }
        }

        // 親子関係を設定
        for (int i = 0; i < sceneData.objects.Count; i++)
        {
            if (instantiatedObjects[i] != null && sceneData.objects[i].parentIndex != -1)
            {
                if (sceneData.objects[i].parentIndex >= 0 && sceneData.objects[i].parentIndex < instantiatedObjects.Count)
                {
                    GameObject parentObject = instantiatedObjects[sceneData.objects[i].parentIndex];
                    if (parentObject != null)
                    {
                        instantiatedObjects[i].transform.SetParent(parentObject.transform);
                    }
                }
            }
        }

        Debug.Log("シーンを読み込みました。");
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

    private void ApplyTextureAndMaterialToObject(GameObject obj, string texturePath, string mtlPath)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // マテリアルを適用
            Material material = Resources.Load<Material>(mtlPath);
            if (material != null)
            {
                renderer.material = new Material(material);
            }
            else
            {
                Debug.LogWarning($".mtlファイルが見つかりません: {mtlPath}");
            }

            // テクスチャを適用
            Texture2D texture = Resources.Load<Texture2D>(texturePath);
            if (texture != null)
            {
                renderer.material.mainTexture = texture;
            }
            else
            {
                Debug.LogWarning($"テクスチャが見つかりません: {texturePath}");
            }
        }
    }

    private void ShowSaveCompletedPopup()
    {
        saveCompletedPopup.SetActive(true);
        StartCoroutine(FadeInOutPopup());
    }

    private System.Collections.IEnumerator FadeInOutPopup()
    {
        // フェードインとフェードアウトの処理
        float duration = 0.5f;
        float time = 0;
        while (time < duration)
        {
            popupCanvasGroup.alpha = Mathf.Lerp(0, 1, time / duration);
            time += Time.deltaTime;
        // 1フレーム待機して、オブジェクトの生成が確実に完了すのを待つ
            yield return null;
        }
        popupCanvasGroup.alpha = 1;

        // 表示時間
        yield return new WaitForSeconds(2f);

        // フェードアウト
        time = 0;
        while (time < duration)
        {
            popupCanvasGroup.alpha = Mathf.Lerp(1, 0, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        popupCanvasGroup.alpha = 0;

        saveCompletedPopup.SetActive(false);
        yield break;
    }

    private IEnumerator AddPhysicsToLoadedObjectsDelayed()
        // 現在のシーン名を取得
    {
        yield return null;
        // Warehouseシーンでない場合のみSaveManagerを生成
        AddPhysicsToLoadedObjects();
    }

    private void AddPhysicsToLoadedObjects()
    {
        PhysicsAssigner physicsAssigner = GetComponent<PhysicsAssigner>();
        if (physicsAssigner == null)
        {
            physicsAssigner = gameObject.AddComponent<PhysicsAssigner>();
        }
        physicsAssigner.AddPhysicsToChildren();
    }

    private void Awake()
    {
        LoadScene();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterSceneLoadRuntimeMethod()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentSceneName != "Warehouse")
        {
            SaveManager saveManager = FindObjectOfType<SaveManager>();
            if (saveManager == null)
            {
                GameObject saveManagerObject = new GameObject("SaveManager");
                saveManager = saveManagerObject.AddComponent<SaveManager>();
            }
            saveManager.LoadScene();
        }
    }
}