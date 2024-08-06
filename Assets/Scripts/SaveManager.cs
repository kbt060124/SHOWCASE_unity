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
        Debug.Log($"Application.persistentDataPath: {Application.persistentDataPath}");
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

        string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        try
        {
            string json = JsonUtility.ToJson(sceneData);
            File.WriteAllText(savePath, json);

            Debug.Log("シーンが保存されました");
            Debug.Log($"保存ファイルのパス: {savePath}");
            Debug.Log($"保存されたJSONデータ: {json}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ファイルの保存中にエラーが発生しました: {e.Message}");
        }

        if (File.Exists(savePath))
        {
            Debug.Log($"ファイルが正に作成されました: {savePath}");
        }
        else
        {
            Debug.LogError($"ファイルの作成に失敗しました: {savePath}");
        }
    }

    private void SaveObjectRecursively(GameObject obj, SceneData sceneData, int parentIndex = -1)
    {
        string prefabPath = GetPrefabPath(obj);

        ObjectData objData = new ObjectData
        {
            prefabPath = prefabPath,
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

    private string GetPrefabPath(GameObject obj)
    {
        // オブジェクト名から(Clone)を削除
        string objName = obj.name.Replace("(Clone)", "");

        // Resourcesフォルダからの相対パスを取得
        string path = objName;
        Transform parent = obj.transform.parent;
        while (parent != null && parent.name != "Objects")
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        // "Resources/"プレフィックスを追加
        return "Resources/" + path;
    }

    public void LoadScene()
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SceneData sceneData = JsonUtility.FromJson<SceneData>(json);

            Debug.Log($"読み込まれたJSONデータ: {json}");

            GameObject existingObjectsContainer = GameObject.Find("Objects");
            if (existingObjectsContainer != null)
            {
                Destroy(existingObjectsContainer);
            }

            GameObject newObjectsContainer = new GameObject("Objects");
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
                    instantiatedObjects.Add(newObj);
                }
                else
                {
                    Debug.LogWarning($"プレハブが見つかりません: {objData.prefabPath}");
                    instantiatedObjects.Add(null);
                }
            }

            for (int i = 0; i < sceneData.objects.Count; i++)
            {
                if (instantiatedObjects[i] != null)
                {
                    if (sceneData.objects[i].parentIndex == -1)
                    {
                        instantiatedObjects[i].transform.SetParent(newObjectsContainer.transform);
                    }
                    else
                    {
                        instantiatedObjects[i].transform.SetParent(instantiatedObjects[sceneData.objects[i].parentIndex].transform);
                    }
                }
            }

            Debug.Log("シーンが読み込まれました");
            Debug.Log($"読み込まれたオブジェクト数: {instantiatedObjects.Count}");
            Debug.Log($"'Objects'コンテナの子オブジェクト数: {newObjectsContainer.transform.childCount}");

            // オブジェクトの生成が完了した後に物理判定を追加
            StartCoroutine(AddPhysicsToLoadedObjectsDelayed());
        }
        else
        {
            Debug.LogWarning($"保存されたシーンが見つかりません。パス: {path}");
        }
    }

    private IEnumerator AddPhysicsToLoadedObjectsDelayed()
    {
        // 1フレーム待機して、オブジェクトの生成が確実に完了するのを待つ
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