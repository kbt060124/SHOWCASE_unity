using Autodesk.Fbx;
using UnityEngine;
using UnityEditor;
using System.IO;

public class Importer : MonoBehaviour
{
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

        // Load the imported asset and instantiate it in the scene
        AssetDatabase.Refresh();
        GameObject importedObject = AssetDatabase.LoadAssetAtPath<GameObject>(fileName);
        if (importedObject != null)
        {
            Instantiate(importedObject);
            Debug.Log("Successfully added imported object to the scene.");
        }
        else
        {
            Debug.LogError("Failed to load imported object from path: " + fileName);
        }
    }

    public void ImportSV15PBasketball()
    {
        Debug.Log("ImportSV15PBasketball called"); // Added debug log
        string fileName = "Assets/Files/SV-15P Basketball.fbx"; // Import source path and destination path are the same
        ImportScene(fileName);
    }
}