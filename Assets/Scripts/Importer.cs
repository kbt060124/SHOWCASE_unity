using Autodesk.Fbx;
using UnityEngine;
using UnityEditor;

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
                }
                else
                {
                    Debug.Log("Successfully imported scene from file: " + fileName);
                }
            }
        }
    }

    public void ImportSV15PBasketball()
    {
        Debug.Log("ImportSV15PBasketball called"); // Added debug log
        string fileName = "Assets/Files/SV-15P Basketball.fbx";
        ImportScene(fileName);
    }
}