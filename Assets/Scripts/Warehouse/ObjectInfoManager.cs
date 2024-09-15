using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class ObjectInfo
{
    public string name;
    public string category;
    public string memo;
}

public class ObjectInfoData
{
    public Dictionary<string, ObjectInfo> objects;
}

public class ObjectInfoManager
{
    private static ObjectInfoData objectInfoData;

    public static void LoadObjectInfo()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("object_info");
        if (jsonFile != null)
        {
            string jsonText = jsonFile.text;
            try
            {
                objectInfoData = JsonConvert.DeserializeObject<ObjectInfoData>(jsonText);
                if (objectInfoData == null || objectInfoData.objects == null)
                {
                    Debug.LogError("JSONの解析に失敗しました。objectInfoDataまたはobjectsがnullです。");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSONの解析中にエラーが発生しました: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("object_info.jsonファイルが見つかりません。");
        }
    }

    public static ObjectInfo GetObjectInfoByFolderName(string folderName)
    {
        if (objectInfoData == null || objectInfoData.objects == null)
        {
            LoadObjectInfo();
        }

        if (objectInfoData == null || objectInfoData.objects == null)
        {
            Debug.LogError("オブジェクト情報が正しく読み込まれていません。");
            return null;
        }

        if (objectInfoData.objects.TryGetValue(folderName, out ObjectInfo info))
        {
            return info;
        }

        return null;
    }
}
