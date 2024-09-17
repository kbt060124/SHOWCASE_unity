using UnityEngine;
using System.IO;

public class ImportObj : MonoBehaviour
{
    public string objResourcePath = "Hands/Hands";
    public string mtlResourcePath = "Hands/Hands_material";
    public string bmpResourcePath = "Hands/Hands_texture";

    public void ImportObjFile()
    {
        // OBJファイルをインポート
        GameObject importedObj = Resources.Load<GameObject>(objResourcePath);
        if (importedObj == null)
        {
            Debug.LogError("OBJファイルが見つかりません: " + objResourcePath);
            return;
        }

        // MTLファイルをインポート（マテリアル情報）
        Material importedMaterial = Resources.Load<Material>(mtlResourcePath);
        if (importedMaterial == null)
        {
            Debug.LogWarning("MTLファイルが見つかりません: " + mtlResourcePath);
        }

        // BMPファイルをインポート（テクスチャ）
        Texture2D importedTexture = Resources.Load<Texture2D>(bmpResourcePath);
        if (importedTexture == null)
        {
            Debug.LogWarning("BMPファイルが見つかりません: " + bmpResourcePath);
        }

        // インポートしたオブジェクトをシーンに配置
        GameObject instance = Instantiate(importedObj);

        // マテリアルとテクスチャを適用
        ApplyMaterialAndTexture(instance, importedMaterial, importedTexture);
    }

    private void ApplyMaterialAndTexture(GameObject obj, Material material, Texture2D texture)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (material != null)
            {
                // マテリアルのインスタンスを作成
                Material instanceMaterial = new Material(material);
                renderer.material = instanceMaterial;

                if (texture != null)
                {
                    // テクスチャを適用
                    instanceMaterial.mainTexture = texture;
                }
            }
            else if (texture != null)
            {
                // マテリアルがない場合、新しいマテリアルを作成してテクスチャを適用
                Material newMaterial = new Material(Shader.Find("Standard"));
                newMaterial.mainTexture = texture;
                renderer.material = newMaterial;
            }
        }
    }
}
