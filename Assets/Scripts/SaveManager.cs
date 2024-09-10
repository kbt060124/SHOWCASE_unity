using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;  // Added this line

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
}

[System.Serializable]
public class SceneData
{
    public List<ObjectData> objects = new List<ObjectData>();
}

public class SaveManager : MonoBehaviour
{
    private const string SAVE_FILE_NAME = "scene_data.json";

    public void SaveScene()
    {
        SceneData sceneData = new SceneData();
        GameObject objectsContainer = GameObject.Find("Objects");

        if (objectsContainer != null)
        {
            Debug.Log($"'Objects'コンテナが見つかりました。子オブジェクト数: {objectsContainer.transform.childCount}");

            foreach (Transform child in objectsContainer.transform)
            {
                SaveObjectRecursively(child.gameObject, sceneData);
            }
        }
        else
        {
            Debug.LogWarning("'Objects'コンテナが見つかりません。");
        }

        string json = JsonUtility.ToJson(sceneData);
        string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        File.WriteAllText(savePath, json);

        Debug.Log("シーンが保存されました");
        Debug.Log($"保存されたJSONデータ: {json}");
        Debug.Log($"保存先: {savePath}");
    }

    private void SaveObjectRecursively(GameObject obj, SceneData sceneData, int parentIndex = -1)
    {
        // フォルダオブジェクトをスキップ
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
                prefabPath = prefabPath.Replace("Resources/", ""),
                position = new SerializableVector3(obj.transform.localPosition),
                rotation = new SerializableQuaternion(obj.transform.localRotation),
                scale = new SerializableVector3(obj.transform.localScale),
                parentIndex = parentIndex
            };
            int currentIndex = sceneData.objects.Count;
            sceneData.objects.Add(objData);
            Debug.Log($"オブジェクト '{obj.name}' を保存: プレハブパス {objData.prefabPath}, 位置 {obj.transform.localPosition}, 回転 {obj.transform.localRotation.eulerAngles}, スケール {obj.transform.localScale}");

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

    public void LoadScene()
    {
        string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        if (!File.Exists(savePath))
        {
            Debug.Log("保存されたシーンデータがありません。");
            return;
        }

        string json = File.ReadAllText(savePath);
        SceneData sceneData = JsonUtility.FromJson<SceneData>(json);

        // 既存のObjectsを取得
        GameObject existingObjects = GameObject.Find("Objects");
        if (existingObjects == null)
        {
            existingObjects = new GameObject("Objects");
        }

        // 既存のItems, Shelvesフォルダを取得または作成
        GameObject itemsFolder = existingObjects.transform.Find("Items")?.gameObject;
        if (itemsFolder == null)
        {
            itemsFolder = new GameObject("Items");
            itemsFolder.transform.SetParent(existingObjects.transform);
        }

        GameObject shelvesFolder = existingObjects.transform.Find("Shelves")?.gameObject;
        if (shelvesFolder == null)
        {
            shelvesFolder = new GameObject("Shelves");
            shelvesFolder.transform.SetParent(existingObjects.transform);
        }

        List<GameObject> instantiatedObjects = new List<GameObject>();

        foreach (ObjectData objData in sceneData.objects)
        {
            GameObject prefab = Resources.Load<GameObject>(objData.prefabPath.Replace("Resources/", ""));
            if (prefab != null)
            {
                GameObject newObj = Instantiate(prefab);
                newObj.name = Path.GetFileName(objData.prefabPath);
                newObj.transform.localPosition = objData.position.ToVector3();
                newObj.transform.localRotation = objData.rotation.ToQuaternion();
                newObj.transform.localScale = objData.scale.ToVector3();

                // タグを付与
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
                    else
                    {
                        Debug.LogWarning($"親オブジェクトが見つかりません。インデックス: {sceneData.objects[i].parentIndex}。現在の親を維持します。");
                    }
                }
                else
                {
                    Debug.LogWarning($"無効な親インデックス: {sceneData.objects[i].parentIndex}。現在の親を維持します。");
                }
            }
        }

        Debug.Log("シーンを読み込みました。");
    }

    private IEnumerator AddPhysicsToLoadedObjectsDelayed()
    {
        // 1フレーム待機して、オブジェクトの生成が確実に完了すのを待つ
        yield return null;

        AddPhysicsToLoadedObjects();
    }

    private void AddPhysicsToLoadedObjects()
    {
        PhysicsAssigner physicsAssigner = GetComponent<PhysicsAssigner>();
        if (physicsAssigner == null)
        {
            physicsAssigner = gameObject.AddComponent<PhysicsAssigner>();
            Debug.Log("PhysicsAssignerコンポーネントが追加されました。");
        }
        physicsAssigner.AddPhysicsToChildren();
        Debug.Log("読み込まれたオブジェクトに物理判定の追加を試みました。");
    }
}
