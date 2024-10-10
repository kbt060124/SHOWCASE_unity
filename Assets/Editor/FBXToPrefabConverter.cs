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
            string folderName = new DirectoryInfo(subFolder).Name;

            // フォルダ名が数字であることを確認
            if (!int.TryParse(folderName, out _))
            {
                Debug.LogWarning($"数字以外の名前のフォルダをスキップします: {subFolder}");
                continue;
            }

            foreach (string fbxFile in fbxFiles)
            {
                string relativeFbxPath = fbxFile.Replace(Application.dataPath, "Assets");
                string relativeSourceFolder = sourceFolder.Replace(Application.dataPath, "Assets");

                string prefabFolder = Path.Combine(destinationFolder, relativeSourceFolder.Substring(7));
                string prefabPath = Path.Combine(prefabFolder, folderName + ".prefab");

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
                    Debug.LogError($"FBXの読み込みに失敗しました: {relativeFbxPath}");
                    continue;
                }

                // プレハブを作成または更新
                GameObject prefabObject = PrefabUtility.SaveAsPrefabAsset(fbxObject, prefabPath);
                if (prefabObject == null)
                {
                    Debug.LogError($"プレハブの作成に失敗しました: {prefabPath}");
                }
                else
                {
                    Debug.Log($"プレハブを作成または更新しました: {prefabPath}");

                    // 同じ名前のjpgまたはpngファイルを探してコピー
                    string[] imageExtensions = { ".jpg", ".png" };
                    foreach (string ext in imageExtensions)
                    {
                        string sourceImagePath = Path.Combine(subFolder, folderName + ext);
                        if (File.Exists(sourceImagePath))
                        {
                            string destImagePath = Path.Combine(prefabFolder, folderName + ext);
                            File.Copy(sourceImagePath, destImagePath, true);
                            Debug.Log($"画像ファイルをコピーしました: {destImagePath}");
                            break; // 最初に見つかった画像ファイルのみをコピー
                        }
                    }
                }
            }
        }

        AssetDatabase.Refresh();
    }
}
