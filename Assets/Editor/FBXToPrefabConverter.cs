using UnityEngine;
using UnityEditor;
using System.IO;

public class FBXToPrefabConverter : EditorWindow
{
    private string[] sourceFolders = new string[] { "Assets/Items", "Assets/Shelves" };
    private string destinationFolder = "Assets/Resources";

    [MenuItem("Tools/FBX to Prefab Converter")]
    public static void ShowWindow()
    {
        GetWindow<FBXToPrefabConverter>("FBX to Prefab Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("FBX to Prefab Converter", EditorStyles.boldLabel);

        if (GUILayout.Button("Convert FBX to Prefab"))
        {
            ConvertFBXToPrefab();
        }
    }

    private void ConvertFBXToPrefab()
    {
        foreach (string sourceFolder in sourceFolders)
        {
            ProcessFolder(sourceFolder);
        }

        AssetDatabase.Refresh();
    }

    private void ProcessFolder(string sourceFolder)
    {
        string[] subFolders = Directory.GetDirectories(sourceFolder);

        foreach (string subFolder in subFolders)
        {
            string[] fbxFiles = Directory.GetFiles(subFolder, "*.fbx", SearchOption.TopDirectoryOnly);

            foreach (string fbxFile in fbxFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(fbxFile);
                string relativeFbxPath = fbxFile.Replace(Application.dataPath, "Assets");
                string relativeSourceFolder = sourceFolder.Replace(Application.dataPath, "Assets");
                string subFolderName = new DirectoryInfo(subFolder).Name;

                string prefabFolder = Path.Combine(destinationFolder, relativeSourceFolder.Substring(7), subFolderName);
                string prefabPath = Path.Combine(prefabFolder, fileName + ".prefab");

                // フォルダが存在しない場合は作成
                if (!Directory.Exists(prefabFolder))
                {
                    Directory.CreateDirectory(prefabFolder);
                }

                // FBXファイルをプロジェクトにインポート（既にインポート済みの場合は更新）
                AssetDatabase.ImportAsset(relativeFbxPath, ImportAssetOptions.ForceUpdate);

                // FBXからGameObjectを作成
                GameObject fbxObject = AssetDatabase.LoadAssetAtPath<GameObject>(relativeFbxPath);
                if (fbxObject == null)
                {
                    Debug.LogError($"Failed to load FBX: {relativeFbxPath}");
                    continue;
                }

                // プレハブを作成または更新
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (existingPrefab != null)
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(fbxObject, prefabPath, InteractionMode.AutomatedAction);
                    Debug.Log($"Updated prefab: {prefabPath}");
                }
                else
                {
                    GameObject prefabObject = PrefabUtility.SaveAsPrefabAsset(fbxObject, prefabPath);
                    if (prefabObject == null)
                    {
                        Debug.LogError($"Failed to create prefab: {prefabPath}");
                        continue;
                    }
                    Debug.Log($"Created prefab: {prefabPath}");
                }
            }
        }
    }
}
